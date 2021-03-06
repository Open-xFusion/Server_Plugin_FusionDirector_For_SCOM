//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace FusionDirectorPlugin.Model.Event
{
    public class AlarmData : ICloneable
    {
        public AlarmData()
        {
        }

        public AlarmData(EventInfo eventInfo)
        {
            this.AlarmId = eventInfo.EventID;
            this.AlarmName = eventInfo.EventName;
            this.ResourceId = eventInfo.EventSource + "_" + eventInfo.EventSubject;
            this.ResourceIdName = eventInfo.EventSource + "_" + eventInfo.EventSubject;
            this.Moc = string.Empty;
            this.Sn = eventInfo.SerialNumber;
            this.Category = GetCategory(eventInfo.Severity);
            this.Severity = GetSeverity(eventInfo.Severity);
            this.OccurTime = eventInfo.FirstOccurTime;
            this.ClearTime = eventInfo.ClearTime;
            this.ClearType = eventInfo.ClearType;
            this.IsClear = string.Empty;
            this.Status = eventInfo.Status;
            this.Additional = eventInfo.Description;
            this.DeviceId = eventInfo.DeviceID;
            this.EventCategory = eventInfo.EventCategory;
            this.EventSubject = eventInfo.EventSubject;
            this.EventDescriptionArgs = eventInfo.EventDescriptionArgs;
            this.PossibleCause = eventInfo.PossibleCause;
            this.Suggestion = eventInfo.HandingSuggesstion;
            this.Effect = eventInfo.Effect;

        }

        public AlarmData(EventSummary eventInfo)
        {
            this.AlarmId = eventInfo.EventID;
            this.AlarmName = eventInfo.EventName;
            this.ResourceId = eventInfo.EventSource + "_" + eventInfo.EventSubject;
            this.ResourceIdName = eventInfo.EventSource + "_" + eventInfo.EventSubject;
            this.Moc = string.Empty;
            this.Sn = eventInfo.SerialNumber;
            this.Category = GetCategory(eventInfo.Severity);
            this.Severity = GetSeverity(eventInfo.Severity);
            this.OccurTime = eventInfo.FirstOccurTime;
            this.ClearTime = eventInfo.ClearTime;
            this.ClearType = eventInfo.ClearType;
            this.IsClear = string.Empty;
            this.Status = eventInfo.Status;
            this.Additional = eventInfo.EventDescription;
            this.DeviceId = eventInfo.DeviceID;
            this.EventCategory = eventInfo.EventCategory;
            this.EventSubject = eventInfo.EventSubject;
            this.EventDescriptionArgs = eventInfo.EventDescriptionArgs;
            this.PossibleCause = string.Empty;
            this.Suggestion = string.Empty;
            this.Effect = string.Empty;
        }

        /// <summary>
        /// ?????????????????? 1???Critical 2???Major 3???Warning 4???OK
        /// </summary>
        /// <param name="eventInfoSeverity">The event information severity.</param>
        /// <returns>System.String.</returns>
        private string GetSeverity(string eventInfoSeverity)
        {
            switch (eventInfoSeverity.ToUpper())
            {
                case "1":
                case "CRITICAL":
                    return "1";
                case "2":
                case "MAJOR":
                    return "2";
                case "3":
                case "WARNING":
                    return "3";
                case "4":
                case "OK":
                    return "4";
                default:
                    return "4";
            }
        }

        /// <summary>
        /// ?????????????????? 1????????? 2????????? 3?????????
        /// </summary>
        /// <param name="eventInfoSeverity">The event information severity.</param>
        /// <returns>System.String.</returns>
        private string GetCategory(string eventInfoSeverity)
        {
            switch (eventInfoSeverity.ToUpper())
            {
                case "1":
                case "CRITICAL":
                case "2":
                case "MAJOR":
                case "3":
                case "WARNING":
                    return "1";
                case "4":
                case "OK":
                    return "3";
                default:
                    return "3";
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// ?????????
        /// </summary>
        [JsonProperty("alarmid")]
        public string AlarmId { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        [JsonProperty("alarmname")]
        public string AlarmName { get; set; }

        /// <summary>
        /// ??????IP_????????????
        /// </summary>
        [JsonProperty("resourceid")]
        public string ResourceId { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        [JsonProperty("resourceidname")]
        public string ResourceIdName { get; set; }

        /// <summary>
        /// ??????MOC??????
        /// </summary>
        [JsonProperty("moc")]
        public string Moc { get; set; }

        /// <summary>
        /// ?????????
        /// </summary>
        [JsonProperty("sn")]
        public int Sn { get; set; }

        /// <summary>
        /// ???????????? 1????????? 2????????? 3?????????
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// ?????? 1???Critical 2???Major 3???Warning 4:OK
        /// </summary>
        [JsonProperty("severity")]
        public string Severity { get; set; }

        /// <summary>
        /// ??????????????????
        /// </summary>
        [JsonProperty("occurtime")]
        public string OccurTime { get; set; }

        /// <summary>
        /// ??????????????????
        /// </summary>
        [JsonProperty("cleartime")]
        public string ClearTime { get; set; }

        /// <summary>
        /// ??????????????????0???????????????  2???????????????
        /// </summary>
        [JsonProperty("cleartype")]
        public string ClearType { get; set; }

        /// <summary>
        /// ????????????????????????0?????? 1??????
        /// </summary>
        [JsonProperty("isclear")]
        public string IsClear { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        [JsonProperty("additional")]
        public string Additional { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        //[JsonProperty("cause")]
        //public string Cause { get; set; }

        /// <summary>
        /// ??????deviceid
        /// </summary>
        [JsonProperty("deviceid")]
        public string DeviceId { get; set; }

        /// <summary>
        /// ???????????? BMC Enclosure FusionDirector Switch
        /// </summary>
        [JsonProperty("eventcategory")]
        public string EventCategory { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        [JsonProperty("eventsubject")]
        public string EventSubject { get; set; }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        [JsonProperty("eventdescriptionargs")]
        [XmlIgnore]
        public object EventDescriptionArgs { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        [JsonProperty("cause")]
        public string PossibleCause { get; set; }

        /// <summary>
        /// ????????????
        /// </summary>
        [JsonProperty("suggestion")]
        public string Suggestion { get; set; }

        /// <summary>
        /// ??????
        /// </summary>
        [JsonProperty("effect")]
        public string Effect { get; set; }

        /// <summary>
        /// ???????????????Uncleared??????????????????Cleared???????????????
        /// </summary>
        /// <value>The status.</value>
        public string Status { get; set; }
    }
}