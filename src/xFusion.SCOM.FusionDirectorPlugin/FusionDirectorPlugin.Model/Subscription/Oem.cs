//**************************************************************************  
//Copyright (C) 2019 xFusion Digital Technologies Co., Ltd. All rights reserved.
//This program is free software; you can redistribute it and/or modify
//it under the terms of the MIT license.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//MIT license for more detail.
//*************************************************************************  
using Newtonsoft.Json;

namespace FusionDirectorPlugin.Model
{
    public partial class Oem
    {
        [JsonProperty("xFusion")]
        public xFusion xFusion { get; set; } = new xFusion();
    }

}
