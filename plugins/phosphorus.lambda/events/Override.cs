/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System.Collections.Generic;
using phosphorus.core;
using phosphorus.expressions;
using phosphorus.expressions.exceptions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.lambda.events
{
    /// <summary>
    ///     Class wrapping [override] keyword.
    /// 
    ///     This class encapsulates the [override] keyword, and its associated helper methods.
    /// </summary>
    public static class Override
    {
        /// <summary>
        ///     Overrides zero or more events
        /// 
        ///     Overrides the given Active Event(s), with the given [super] Active Event(s).
        /// 
        ///     Example;
        /// 
        ///     <pre>event:bar
        ///   lambda
        ///     set:@/./.?value
        ///       source:OVERRIDDEN!
        /// override:foo
        ///   super:bar
        /// bar</pre>
        /// 
        /// Please notice though, that if you execute the above code twice, you'll end up creating two overrides, with the exact same
        ///     base event and super event. To avoid this, you might want to consider using [remove-override] before you override your Active Events
        ///     with the [override] keyword.
        /// 
        ///     If you wish to invoke the <em>"base event"</em>, you can do such using 
        ///     <see cref="phosphorus.lambda.Lambda.lambda_invoke">[lambda.invoke]</see>.
        /// 
        ///     The Active Event you override using [override] does not have to exist. When invoking the base Active Event, your override will still
        ///     be invoked automatically.
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "override")]
        private static void lambda_override (ApplicationContext context, ActiveEventArgs e)
        {
            // syntax checking
            var overrideNodes = new List<Node> (e.Args.FindAll (idx => idx.Name == "super"));
            if (overrideNodes.Count != 1)
                throw new LambdaException ("[override] requires exactly one [super] parameter", e.Args, context);

            // retrieving all overrides
            var overrides = new List<string> (XUtil.Iterate<string> (overrideNodes [0], context));

            // iterating through each override event, creating our override
            foreach (var idxBase in XUtil.Iterate<string> (e.Args, context)) {
                foreach (var idxSuper in overrides) {
                    EventsCommon.CreateOverride (context, idxBase, idxSuper);
                }
            }
        }
    }
}
