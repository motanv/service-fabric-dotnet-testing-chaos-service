// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.Common
{
    using System;

    public static class Constants
    {
        public static readonly int HistoryLength = 30;
        public static readonly int ServiceRequestRetryCount = 3;
        public static readonly TimeSpan IntervalBetweenLoopIteration = TimeSpan.FromSeconds(5.0);
    }
}