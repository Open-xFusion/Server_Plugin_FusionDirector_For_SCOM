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
// Created          : 12-26-2018
//
// Last Modified By : mike
// Last Modified On : 12-26-2018
// ***********************************************************************
// <copyright file="NetworkAdapterSummary.cs" company="xFusion Digital Technologies Co., Ltd">
//     Copyright ©  2018
// </copyright>
// <summary></summary>
// ***********************************************************************
using Newtonsoft.Json;

namespace FusionDirectorPlugin.Model
{
    /// <summary>
    /// Class NetworkAdapterSummary.
    /// </summary>
    public class NetworkAdapterSummary
    {
        /// <summary>
        /// 指定Node资源网卡个数。
        /// </summary>
        /// <value>The count.</value>
        [JsonProperty("Count")]
        public int? Count { get; set; }
    }
}