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

namespace FusionDirectorPlugin.Dal.Model
{
    public class TaskResult
    {

        public TaskResult(string message)
        {
            this.Message = message;
        }

        public TaskResult()
        {
            this.SyncResults = new List<SyncResult>();
        }
        public string Message { get; set; }

        public List<SyncResult> SyncResults { get; set; } 
    }

    public class SyncResult
    {
        public Exception Exception { get; set; }

        public string DN { get; set; }

        public string ServerType { get; set; }
        public bool Success { get; set; }
    }
}
