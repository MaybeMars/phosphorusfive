/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using phosphorus.core;
using phosphorus.expressions;
using phosphorus.data.helpers;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.data
{
    /// <summary>
    ///     Class wrapping [pf.data.remove].
    /// 
    ///     Class containing the [pf.data.remove] Active Event, and its associated helper methods.
    /// </summary>
    public static class Remove
    {
        /// <summary>
        ///     Removes nodes from your database.
        /// 
        ///     Removes all nodes matching your given <see cref="phosphorus.expressions.Expression">Expression</see>
        ///     from your database.
        /// 
        ///     The database stores its nodes as the root node being the database itself, and beneath the root node, are
        ///     all file nodes. This means that your expressions should start with; <em>@/*/*</em>, before the rest of
        ///     your expression, referring to your actual data nodes.
        /// 
        ///     The node used as the "root node" for most database expressions, except [pf.data.insert] though, is the 
        ///     root node of your database, and not your execution tree root node.
        /// 
        ///     Example that will remove all items from your database, having a type, containing the string "foo";
        /// 
        ///     <pre>
        /// pf.data.remove:@/*/*/"/foo/"?node</pre>
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "pf.data.remove")]
        private static void pf_data_remove (ApplicationContext context, ActiveEventArgs e)
        {
            if (e.Args.Value == null)
                return;
            if (!XUtil.IsExpression (e.Args.Value))
                throw new ArgumentException ("[pf.data.remove] requires an expression as its value");

            // acquiring lock on database
            lock (Common.Lock) {
                // making sure database is initialized
                Common.Initialize (context);

                // looping through database matches and removing nodes while storing which files have been changed
                var changed = new List<Node> ();
                foreach (var idxDest in XUtil.Iterate (e.Args, Common.Database, e.Args, context)) {
                    // making sure "file nodes" and "root node" cannot be remove
                    if (idxDest.Node.Path.Count < 2)
                        throw new ArgumentException ("You cannot remove the actual file node, or root node from your database");

                    // figuring out which file Node updated belongs to, and storing in changed list
                    Common.AddNodeToChanges (idxDest.Node, changed);

                    // setting value to null, which works if user chooses to remove "value", "name" and/or "node"
                    idxDest.Value = null;
                }

                // saving all affected files
                Common.SaveAffectedFiles (context, changed);
            }
        }
    }
}
