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
// Assembly         : FusionDirectorPlugin.Model
// Author           : panwei
// Created          : 12-27-2018
//
// Last Modified By : panwei
// Last Modified On : 12-27-2018
// ***********************************************************************
// <copyright file="SwitchSlot.cs" company="xFusion Digital Technologies Co., Ltd">
//     Copyright ©  2018
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Linq;
using Newtonsoft.Json;

namespace FusionDirectorPlugin.Model
{
    /// <summary>
    /// 交换槽位信息。
    /// </summary>
    public partial class SwitchSlot
    {
        /// <summary>
        /// Gets the union identifier.
        /// </summary>
        /// <value>The union identifier.</value>
        public string UnionId => $"{FusionDirectorIp}-{this.UUID}";

        /// <summary>
        /// Id
        /// ResourceURL中截取
        /// </summary>
        /// <value>The identifier.</value>
        public string UUID => this.ResourceURL.Split('/').Last();

        /// <summary>
        /// Gets or sets the FusionDirector ip.
        /// </summary>
        /// <value>The FusionDirector ip.</value>
        public string FusionDirectorIp { get; set; }

        /// <summary>
        /// 物理UUID。
        /// </summary>
        /// <value>The physical UUID.</value>
        [JsonProperty("PhysicalUUID")]
        public string PhysicalUUID { get; set; }

        /// <summary>
        /// 槽位号。
        /// </summary>
        /// <value>The index.</value>
        [JsonProperty("Index")]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        [JsonProperty("State")]

        public SlotState SlotState { get; set; }

        /// <summary>
        /// 产品名。
        /// </summary>
        /// <value>The name of the product.</value>
        [JsonProperty("ProductName")]
        public string ProductName { get; set; }

        /// <summary>
        /// 序列号。
        /// </summary>
        /// <value>The serial number.</value>
        [JsonProperty("SerialNumber")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 设备详细信息的链接。
        /// </summary>
        /// <value>The resource URL.</value>
        [JsonProperty("ResourceURL")]
        public string ResourceURL { get; set; }

        #region 从node接口中查询再赋值
        /// <summary>
        /// 名称
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Node的生产厂商信息。
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer { get; set; }

        /// <summary>
        /// ip address.
        /// </summary>
        /// <value>The ip address.</value>
        public string IPAddress { get; set; }

        /// <summary>
        /// 指定Switch的型号。
        /// </summary>
        /// <value>The model.</value>
        [JsonProperty("Model")]
        public string Model { get; set; }

        /// <summary>
        /// 指定Switch的主机名。
        /// </summary>
        /// <value>The name of the host.</value>
        [JsonProperty("HostName")]
        public string HostName { get; set; }

        /// <summary>
        /// 交换板状态。
        /// </summary>
        /// <value>The state of the switch.</value>
        [JsonProperty("SwitchState")]
        public string SwitchState { get; set; }

        #endregion

        /// <summary>
        /// 健康状态-需手动再次查询
        /// </summary>
        /// <value>The health.</value>
        [JsonProperty("Health")]
        public Health Health { get; set; } = Health.Warning;

    }
}