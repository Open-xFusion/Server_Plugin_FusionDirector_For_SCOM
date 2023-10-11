//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
// ***********************************************************************
// Assembly         : FusionDirectorPlugin.Service
// Author           : yayun
// Created          : 01-04-2019
//
// Last Modified By : yayun
// Last Modified On : 01-04-2019
// ***********************************************************************
// <copyright file="FusionDirectorSyncInstance.cs" company="xFusion Digital Technologies Co., Ltd">
//     Copyright ©  2018
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FusionDirectorPlugin.Api;
using FusionDirectorPlugin.Core;
using FusionDirectorPlugin.Dal.Model;
using FusionDirectorPlugin.LogUtil;
using FusionDirectorPlugin.Model;

using System.Linq;
using System.Threading;
using FusionDirectorPlugin.Model.Event;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;
using FusionDirectorPlugin.Dal;

namespace FusionDirectorPlugin.Service
{
    /// <summary>
    ///     Class FusionDirectorSyncInstance.
    /// </summary>
    public partial class FusionDirectorSyncInstance
    {
        #region field

        private readonly List<Task> taskList = new List<Task>();

        private FusionDirectorLogger logger;

        private PluginConfig pluginConfig;

        private Timer pollingPerformanceTimer;

        private Timer keepEventTimer;

        private readonly TaskFactory taskFactory;

        private CancellationTokenSource cts = new CancellationTokenSource();

        private Timer waitSyncTaskFinishedTimer;

        #endregion

        #region Constuctor
        /// <summary>
        /// Initializes a new instance of the <see cref="FusionDirectorSyncInstance"/> class.
        /// </summary>
        /// <param name="fusionDirector">
        /// The e sight.
        /// </param>
        public FusionDirectorSyncInstance(FusionDirector fusionDirector)
        {
            this.IsRunning = true;
            this.FusionDirector = fusionDirector;
            logger = new FusionDirectorLogger(fusionDirector.HostIP);
            this.enclosureService = new EnclosureService(fusionDirector);
            this.fusionDirectorService = new FusionDirectorService(fusionDirector);
            this.nodePoolService = new NodePoolService(fusionDirector);
            this.eventService = new EventService(fusionDirector);
            this.metricsService = new MetricsService(fusionDirector);
            this.pluginConfig = ConfigHelper.GetPluginConfig();
            this.AlarmDatas = new Queue<AlarmData>();
            this.AlarmQueue = new Queue<AlarmData>();
            this.StartAlarmEventProcessor();

            this.UpdateServerTasks = new List<UpdateTask<Server>>();
            this.UpdateEnclosureTasks = new List<UpdateTask<Enclosure>>();
            this.RunPerformanceCollectTask();
            this.RunKeepEventTask();

            var scheduler = new LimitedConcurrencyLevelTaskScheduler(pluginConfig.ThreadCount);
            taskFactory = new TaskFactory(cts.Token, TaskCreationOptions.HideScheduler, TaskContinuationOptions.HideScheduler, scheduler);
        }

        public void UpdateFusionDirector(FusionDirector fusionDirector)
        {
            this.FusionDirector = fusionDirector;
            this.enclosureService = new EnclosureService(fusionDirector);
            this.fusionDirectorService = new FusionDirectorService(fusionDirector);
            this.nodePoolService = new NodePoolService(fusionDirector);
            this.eventService = new EventService(fusionDirector);
            this.metricsService = new MetricsService(fusionDirector);
        }
        #endregion

        #region Property

        public EnclosureService enclosureService { get; set; }

        public FusionDirectorService fusionDirectorService { get; set; }

        public NodePoolService nodePoolService { get; set; }

        public EventService eventService { get; set; }

        public MetricsService metricsService { get; set; }

        public FusionDirector FusionDirector { get; set; }

        /// <summary>
        ///     The is complete.
        /// </summary>
        /// <value><c>true</c> if this instance is complete; otherwise, <c>false</c>.</value>
        public bool IsComplete
        {
            get
            {
                foreach (var task in this.taskList)
                {
                    if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                    {
                        continue;
                    }
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the e sight ip.
        /// </summary>
        /// <value>The e sight ip.</value>
        public string FusionDirectorIp => this.FusionDirector.HostIP;

        /// <summary>
        /// 是否允许
        /// </summary>
        public bool IsRunning { get; set; }

        #endregion

        public void Sync()
        {
            string taskId = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            logger.Polling.Info($"[{taskId}]Start Polling Task.");
            logger.Sdk.Info($"[{taskId}]Start Polling Task.");
            if (!this.IsComplete)
            {
                logger.Polling.Warn($"[{taskId}] The last task was not completed, and the task was given up.");
                return;
            }
            var testResult = fusionDirectorService.TestLinkFd();
            if (!testResult.Success)
            {
                OnPollingError($"[{taskId}] Can not connect the FusionDirector {this.FusionDirectorIp}, and the task was given up.");
                return;
            }
            // 清除完成的任务
            this.taskList.Clear();
            this.SyncEnclosureList(taskId);
            this.SyncServerList(taskId);
            WaitAndStartSyncOpenAlertsIfNeccessary();
        }

        private void WaitAndStartSyncOpenAlertsIfNeccessary()
        {
            if (this.pluginConfig.IsEnableAlert)
            {
                if (waitSyncTaskFinishedTimer == null)
                {
                    // 开启一个timer来监听本次轮询任务是否执行完
                    waitSyncTaskFinishedTimer = new Timer(3000)
                    {
                        AutoReset = false,
                        Enabled = true,
                    };

                    waitSyncTaskFinishedTimer.Elapsed += (sender, e) =>
                    {
                        try
                        {
                            if (IsComplete && IsRunning)
                            {
                                logger.Polling.Info("All sync task has finished, sync open alarms now.");
                                SyncFusionDirectorOpenAlarms();
                            }
                        }
                        finally
                        {
                            if (!IsComplete && IsRunning)
                            {
                                waitSyncTaskFinishedTimer.Start(); // Restart timer for next tick's checking
                            }
                        }
                    };
                }

                waitSyncTaskFinishedTimer.Start();
            }
        }

        #region 机框

        private void SyncEnclosureList(string taskId)
        {
            logger.Polling.Info($"[{taskId}]Start Polling Enclosure Task.");
            logger.Sdk.Info($"[{taskId}]Start Polling Enclosure Task.");
            try
            {
                var result = this.enclosureService.GetEnclosureCollectionAsync(null, null, null, null).Result;

                //删除不存在的Enclosure
                var newDeviceIds = result.Members.Select(x => $"{this.FusionDirectorIp}-{x.UUID}").ToList();
                logger.Polling.Debug($"[{taskId}]SyncEnclosureList Finish.[{string.Join(",", newDeviceIds).Replace($"{this.FusionDirectorIp}-", "")}]");
                EnclosureConnector.Instance.CompareDataOnSync(this.FusionDirectorIp, newDeviceIds);

                //开始同步详情
                foreach (var x in result.Members)
                {
                    var task = taskFactory.StartNew(() =>
                    {
                        var enclosure = this.QueryEnclosureDetails(x).Result;
                        logger.Polling.Debug($"[{taskId}]Query Enclosure Finish:[{JsonConvert.SerializeObject(enclosure)}].");
                        EnclosureConnector.Instance.Sync(enclosure).Wait();
                    }, cts.Token);
                    this.taskList.Add(task);
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                }
            }
            catch (Exception ex)
            {
                OnPollingError($"[{taskId}]SyncEnclosureList Error.FusionDirector:{this.FusionDirectorIp} ", ex);
            }
        }

        private async Task<Enclosure> QueryEnclosureDetails(EnclosureSummary enclosureSummary)
        {
            var enclosureId = enclosureSummary.UUID;
            try
            {
                return await QueryEnclosureDetailsById(enclosureId);
            }
            catch (Exception ex)
            {
                OnPollingError($"QueryEnclosureDetial Error:{enclosureId}", ex);
                var defaultEnclosure = new Enclosure(enclosureSummary);
                defaultEnclosure.MakeDetails(this.FusionDirectorIp);
                return defaultEnclosure;
            }
        }

        /// <summary>
        /// Queries the Enclosure detial.
        /// </summary>
        /// <returns>Task&lt;Enclosure&gt;.</returns>
        private async Task<Enclosure> QueryEnclosureDetailsById(string deviceId)
        {
            var enclosure = await this.enclosureService.GetEnclosureAsync(deviceId);
            enclosure.MakeDetails(this.FusionDirectorIp);
            return enclosure;
        }

        #endregion

        #region Server
        private void SyncServerList(string taskId)
        {
            logger.Polling.Info($"[{taskId}]Start Polling Server Task.");
            logger.Sdk.Info($"[{taskId}]Start Polling Server Task.");

            try
            {
                // 限制数量
                int limitQuantity = FusionDirectorPluginService.MAX_SERVER_COUNT - FusionDirectorPluginService.dealServerCount();
                List<ServerSummary> serverList = new List<ServerSummary>();
                if (limitQuantity <= 0)
                {
                    logger.Sdk.Warn($"[{taskId}]SyncServerList limitQuantity:{limitQuantity} <= 0, FusionDirector:{this.FusionDirectorIp}");
                }
                else
                {
                    ServerList result = this.nodePoolService.GetAllServerAsync(null, null, null, limitQuantity).Result;
                    serverList = result.Members;
                    logger.Sdk.Info($"[{taskId}]SyncServerList start.[LimitQuantity:{limitQuantity}, ServerCount:{serverList.Count}, TotalCount:{result.Membersodatacount}]");
                }

                FusionDirectorPluginService.dealServerCount("add", this.FusionDirectorIp, serverList.Count());
                // 删除不存在的server
                var newDeviceIds = serverList.Select(selector: x => $"{this.FusionDirectorIp}-{x.Id}").ToList();
                ServerConnector.Instance.CompareDataOnSync(this.FusionDirectorIp, newDeviceIds);
                foreach (var x in serverList)
                {
                    var task = taskFactory.StartNew(async () =>
                    {
                        Server server = await QueryServerDetails(x);
                        await ServerConnector.Instance.Sync(server);
                    }, cts.Token);
                    this.taskList.Add(item: task);
                    Thread.Sleep(timeout: TimeSpan.FromMilliseconds(200));
                }
                logger.Sdk.Info($"[{taskId}]SyncServerList end.[Server Count:{serverList.Count}, FusionDirector:{this.FusionDirectorIp}]");
            }
            catch (Exception ex)
            {
                OnPollingError($"[{taskId}]SyncServerList Error.FusionDirector:{this.FusionDirectorIp} ", ex);
            }
        }

        /// <summary>
        /// Queries the Server detial.
        /// </summary>
        /// <param name="serverSummary">The server summary.</param>
        /// <returns>Task&lt;Server&gt;.</returns>
        private async Task<Server> QueryServerDetails(ServerSummary serverSummary)
        {
            var serverId = serverSummary.Id;
            try
            {
                return await QueryServerDetailsById(serverId);
            }
            catch (Exception ex)
            {
                OnPollingError($"QueryServerDetial Error:{serverId}", ex);
                var defaultServer = new Server(serverSummary);
                defaultServer.MakeDetails(this.FusionDirectorIp);
                return defaultServer;
            }
        }

        private async Task<Server> QueryServerDetailsById(string serverId)
        {
            var server = await this.nodePoolService.GetServerInfoAsync(serverId);
            server.MakeDetails(this.FusionDirectorIp);
            var manager = await QueryManager(serverId);
            if (manager != null)
            {
                server.BMCVersion = manager.FirmwareVersion;
                server.HostName = manager.EthernetInterfaces.FirstOrDefault()?.HostName ?? string.Empty;//如果为null ,取空
            }
            return server;
        }

        #endregion

        private async Task<ServerManagerCollection> QueryManager(string serverId)
        {
            try
            {
                var manager = await this.nodePoolService.GetServerManagerCollectionAsync(serverId);
                return manager;
            }
            catch (Exception)
            {
                logger.Polling.Error($"GetServerManager Error:[ServerId:{serverId}]");
                //throw;
                return null;
            }
        }

        #region 性能视图

        /// <summary>
        /// Runs the performance collect task.
        /// </summary>
        private void RunPerformanceCollectTask()
        {
            logger.Polling.Info($"RunPerformanceCollectTask.");
            this.pollingPerformanceTimer = new Timer(30 * 60 * 1000)
            {
                Enabled = true,
                AutoReset = true,
            };
            this.pollingPerformanceTimer.Elapsed += (s, e) =>
            {
                this.RunSyncServerPerformance();
            };
            this.pollingPerformanceTimer.Start();
        }

        /// <summary>
        /// Runs the synchronize server performance.
        /// </summary>
        private async void RunSyncServerPerformance()
        {
            logger.Polling.Info($"Start Server Performance Collect Task.");
            logger.Sdk.Info($"Start Server Performance Collect Task.");
            try
            {
                ServerList result = await this.nodePoolService.GetAllServerAsync(null, null, null, FusionDirectorPluginService.MAX_SERVER_COUNT);
                var serverList = result.Members.Where(x => x.State.ToUpper() == "READY").ToList();
                logger.Sdk.Info($"RunSyncServerPerformance start.[Server Count:{serverList.Count}, totalCount:{result.Membersodatacount}]");
                foreach (var x in serverList)
                {
                    await DealServerPerformance(x);
                    Thread.Sleep(timeout: TimeSpan.FromMilliseconds(200));
                }
                logger.Sdk.Info($"RunSyncServerPerformance end.[Server Count:{serverList.Count}, totalCount:{result.Membersodatacount}]");
            }
            catch (Exception ex)
            {
                OnPollingError($"Sync Server Performance Error.FusionDirector:{this.FusionDirectorIp} ", ex);
            }
        }

        /// <summary>
        /// Deals the server performance.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>Task.</returns>
        private async Task DealServerPerformance(ServerSummary server)
        {
            try
            {
                var unionId = $"{FusionDirectorIp}-{server.Id}";
                var realTimeData = await this.metricsService.GetServerRealTimePerformanceAsync(server.Id);
                var objectName = $"{this.FusionDirectorIp}_{server.IPv4Address.Address}";
                ServerConnector.Instance.InsertPerformanceData(unionId, objectName, realTimeData);
            }
            catch (Exception ex)
            {
                OnPollingError($"DealServerPerformance Error:{server.Id}", ex);
            }
        }
        #endregion

        /// <summary>
        /// CheckFd
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>Task.</returns>
        public ApiResult CheckFd()
        {
            var testResult = fusionDirectorService.TestLinkFd();
            return testResult;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            HWLogger.Service.Info($"Delete FusionDirector SyncInstance: {this.FusionDirectorIp}");
            this.IsRunning = false;

            // we could not use subscribe id in memory directly. should reload from db.
            if (!string.IsNullOrEmpty(FusionDirector.SubscribeId))
            {
                try
                {
                    this.eventService.DeleteGivenSubscriptions(FusionDirector.SubscribeId);
                    FusionDirectorDal.Instance.UpdateSubscribeStatus(this.FusionDirectorIp, SubscribeStatus.NotSubscribed, "", "");
                    HWLogger.Service.Info($"Unsubscribe event notification success.");
                }
                catch (Exception ex)
                {
                    FusionDirectorDal.Instance.UpdateSubscribeStatus(this.FusionDirectorIp, SubscribeStatus.Error, ex.Message, "");
                    HWLogger.Service.Info(ex, $"Failed to unsubscribe event notification for FusionDirector: {FusionDirectorIp}");
                }
            }

            this.cts.Cancel(false);
            this.UpdateServerTasks.Clear();
            this.UpdateEnclosureTasks.Clear();
            this.keepEventTimer.Stop();
            this.AlarmQueue.Clear();
            this.pollingPerformanceTimer.Stop();
            DisposeFusionDirectorClients();
        }

        private void DisposeFusionDirectorClients()
        {
            this.eventService?.Dispose();
            this.enclosureService?.Dispose();
            this.fusionDirectorService?.Dispose();
            this.nodePoolService?.Dispose();
            this.metricsService?.Dispose();
        }

        /// <summary>
        /// Called when [polling error].
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="ex">The ex.</param>
        private void OnPollingError(string msg, Exception ex = null)
        {
            logger.Polling.Error(ex, msg);
        }
    }
}