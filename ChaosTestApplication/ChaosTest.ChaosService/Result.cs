// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Collections.Generic;

    public class Result
    {
        public string TotalRuntime;
        public string CurrentState;
        public SortedList<long, ChaosEntry> ChaosLog;
    }
}