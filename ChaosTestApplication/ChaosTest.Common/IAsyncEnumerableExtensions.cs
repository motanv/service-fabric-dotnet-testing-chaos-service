using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace ChaosTest.Common
{
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
