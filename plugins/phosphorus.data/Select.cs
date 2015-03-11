/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using phosphorus.core;
using phosphorus.expressions;
using phosphorus.data.helpers;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.data
{
    /// <summary>
    ///     Class wrapping [pf.data.select].
    /// 
    ///     Encapsulates the [pf.data.select] Active Event, and its associated supporting methods.
    /// </summary>
    public static class Select
    {
        /// <summary>
        ///     Selects nodes from your database.
        /// 
        ///     Selects items from your database, according to expression given as value of node, and returns the matches
        ///     as children nodes.
        /// 
        ///     The database stores its nodes as the root node being the database itself, and beneath the root node, are
        ///     all file nodes. This means that your expressions should start with; <em>@/*/*</em>, before the rest of
        ///     your expression, referring to your actual data nodes.
        /// 
        ///     The node used as the "root node" for most database expressions, except [pf.data.insert] though, is the 
        ///     root node of your database, and not your execution tree root node.
        /// 
        ///     Example that will select all items from your database, having a type, containing the string "foo";
        /// 
        ///     <pre>
        /// pf.data.select:@/*/*/"/foo/"?node</pre>
        /// </summary>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.data.select")]
        private static void pf_data_select (ApplicationContext context, ActiveEventArgs e)
        {
            if (e.Args.Value == null)
                return; // nothing to do here ...

            // acquiring lock on database
            lock (Common.Lock) {
                // verifying syntax
                if (!XUtil.IsExpression (e.Args.Value))
                    throw new ArgumentException ("[pf.data.select] requires an expression to select items from database");

                // making sure database is initialized
                Common.Initialize (context);

                // used to signal if we performed any iterations 
                var foundHit = false;

                // iterating through each result from database node tree
                foreach (var idxMatch in XUtil.Iterate (e.Args, Common.Database, e.Args, context)) {
                    // signaling we've been here
                    foundHit = true;

                    // aborting iteration early if it is a 'count' expression
                    if (idxMatch.TypeOfMatch == Match.MatchType.count) {
                        e.Args.Add (new Node (string.Empty, idxMatch.Match.Count));
                        return;
                    }

                    // dependent upon type of expression, we either return a bunch of nodes, flat, with
                    // name being string.Empty, and value being matched value, or we append node itself back
                    // to caller. this allows us to select using expressions which are not of type 'node'
                    e.Args.Add (idxMatch.TypeOfMatch != Match.MatchType.node ? new Node (string.Empty, idxMatch.Value) : idxMatch.Node.Clone ());
                }

                // special case for 'count' expression where there are no matches
                if (!foundHit && XUtil.ExpressionType (e.Args, context) == Match.MatchType.count) {
                    // no hits, returning ZERO
                    e.Args.Add (new Node (string.Empty, 0));
                }
            }
        }
    }
}
