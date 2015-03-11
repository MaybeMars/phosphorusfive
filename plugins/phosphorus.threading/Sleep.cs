/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System.Threading;
using phosphorus.core;
using phosphorus.expressions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.threading
{
    /// <summary>
    ///     Class wrapping the [sleep] keyword.
    /// 
    ///     The [sleep] keyword, allows you to have the thread you're invoking it for, stop execution for a period, given as a specified amount of 
    ///     milliseconds.
    /// </summary>
    public static class Sleep
    {
        /// <summary>
        ///     Sleeps the thread you invoke it for.
        /// 
        ///     Sleeps the current thread for n milliseconds, where n is the parameter given to [sleep].
        /// 
        ///     See the <see cref="phosphorus.threading.Wait.lambda_wait">[wait]</see> Active Event for an example of how to create multiple threads,
        ///     and a deeper explanation of how threading works.
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "sleep")]
        private static void lambda_sleep (ApplicationContext context, ActiveEventArgs e)
        {
            var milliseconds = XUtil.Single<int> (e.Args, context);
            Thread.Sleep (milliseconds);
        }
    }
}