/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System.Threading;
using phosphorus.core;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.threading
{
    /// <summary>
    ///     wraps [wait] keyword
    /// </summary>
    public static class Wait
    {
        /// <summary>
        ///     waits for all [lambda.fork] children to finish their work, before allowing
        ///     execution to flow out of scope. the [wait] node will also be passed into all
        ///     [lambda.thread] active events beneath itself, as a reference node, as value
        ///     of [_wait]. however, if you access the [_wait] node inside your thread, you
        ///     should probably use the [lock] statement, since access to [_wait] is shared
        ///     among all threads being children of [wait]
        /// </summary>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "wait")]
        private static void lambda_wait (ApplicationContext context, ActiveEventArgs e)
        {
            try {
                context.Raise ("lambda", e.Args);
                if (e.Args ["__threads"] != null) {
                    foreach (var idx in e.Args ["__threads"].Children) {
                        var thread = idx.Get<Thread> (context);
                        thread.Join ();
                    }
                }
            } finally {
                if (e.Args ["__threads"] != null) {
                    e.Args ["__threads"].UnTie ();
                }
            }
        }
    }
}