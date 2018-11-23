using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace standard_lib
{
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Users of that method should not throw.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="defaultValue"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<T> RunUntillAsync<T>(this Task<T> task, T defaultValue, int timeout = 10000)
        {
            if (await Task.WhenAny(Task.Delay(timeout), task).ConfigureAwait(false) == task) return task.Result;
            return defaultValue;
        }

        /// <summary>
        ///     Users of that method should not throw.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<T> RunUntillAsync<T>(this Task<T> task, int timeout = 10000)
        {
            if (await Task.WhenAny(Task.Delay(timeout), task).ConfigureAwait(false) == task) return task.Result;
            return default(T);
        }

        public static async Task SuppressExceptions(this Task task, Action<Exception> action = null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                action?.Invoke(ex);
            }
        }

        public static async Task<T> SuppressExceptions<T>(this Task<T> task, Action<Exception> action = null)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                action?.Invoke(ex);
            }

            return default(T);
        }

        /// <summary>
        ///     Hides exception throwed and throws given exception instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="predicate"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static T FirstOrCustomException<T>(this IEnumerable<T> self, Func<T, bool> predicate,
            Exception exception)
        {
            try
            {
                var result = self.First(predicate);
                return result;
            }
            catch (Exception ex)
            {
                throw exception;
            }
        }
    }
}