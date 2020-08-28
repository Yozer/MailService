using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailService.Api.Tests.Integration
{
    public static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> YieldAsync<T>(this IEnumerable<ValueTask<T>> input)
        {
            foreach (var task in input)
            {
                yield return await task.ConfigureAwait(false);
            }
        }

        public static async IAsyncEnumerable<T> YieldAsync<T>(this IEnumerable<Task<T>> input)
        {
            foreach (var task in input)
            {
                yield return await task.ConfigureAwait(false);
            }
        }
    }
}