/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using phosphorus.core;
using phosphorus.expressions;
using phosphorus.expressions.exceptions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.lambda.keywords
{
    /// <summary>
    ///     class wrapping execution engine keyword "append",
    ///     which allows for appending nodes into a node's list of children
    /// </summary>
    public static class Append
    {
        /// <summary>
        ///     [add] keyword for execution engine. allows adding nodes from one part of your tree to another part
        /// </summary>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "append")]
        private static void lambda_append (ApplicationContext context, ActiveEventArgs e)
        {
            // figuring out source type, for then to execute the corresponding logic
            if (e.Args.Count > 0 && (e.Args.LastChild.Name == "rel-source" || e.Args.LastChild.Name == "rel-src")) {
                // relative source
                AppendRelativeSource (e.Args, context);
            } else {
                // static source
                AppendStaticSource (e.Args, context);
            }
        }

        /*
         * source is static
         */

        private static void AppendStaticSource (Node node, ApplicationContext context)
        {
            // retrieving source before we start iterating destination,
            // in case destination and source overlaps
            var sourceNodes = XUtil.SourceNodes (node, context);

            // making sure there is a source
            if (sourceNodes == null)
                return;

            // looping through every destination node
            var isFirst = true; // since source is already cloned, we avoid cloning on our first run
            foreach (var idxDestination in XUtil.Iterate (node, context)) {
                // verifying destination actually is a node
                var curDest = idxDestination.Value as Node;
                if (curDest == null)
                    throw new LambdaException ("cannot [append] into something that's not a node", node, context);

                // minor optimization trick, since source already is cloned upon first run
                if (isFirst) {
                    // we don't clone on the first run-through, since node-set is already cloned
                    curDest.AddRange (sourceNodes);
                    isFirst = false;
                } else {
                    // cloning on all consecutive run-throughs
                    foreach (var idxSource in sourceNodes) {
                        curDest.Add (idxSource.Clone ());
                    }
                }
            }
        }

        /*
         * relative source
         */

        private static void AppendRelativeSource (Node node, ApplicationContext context)
        {
            foreach (var idxDestination in XUtil.Iterate (node, context)) {
                // verifying destination actually is a node
                var curDest = idxDestination.Value as Node;
                if (curDest == null)
                    throw new LambdaException ("cannot [append] into something that's not a node", node, context);

                foreach (var idxSource in XUtil.SourceNodes (node, idxDestination.Node, context)) {
                    curDest.Add (idxSource.Clone ());
                }
            }
        }
    }
}