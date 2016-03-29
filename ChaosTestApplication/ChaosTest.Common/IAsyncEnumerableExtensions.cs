// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;

    public static class IAsyncEnumerableExtensions
    {
        public static async Task ForeachAsync<T>(this IAsyncEnumerable<T> instance, CancellationToken token, Action<T> doSomething)
        {
            IAsyncEnumerator<T> e = instance.GetAsyncEnumerator();

            try
            {
                goto Check;

                Resume:
                T i = e.Current;
                {
                    doSomething(i);
                }

                Check:
                if (await e.MoveNextAsync(token))
                {
                    goto Resume;
                }
            }
            finally
            {
                if (e != null)
                {
                    e.Dispose();
                }
            }
        }
    }
}