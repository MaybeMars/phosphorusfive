/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using phosphorus.core;
using phosphorus.expressions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.lambda.keywords
{
    /// <summary>
    ///     class wrapping execution engine keyword "for-each",
    ///     which allows for iteratively executing code for every instance in an expression
    /// </summary>
    public static class ForEach
    {
        /// <summary>
        ///     [for-each] keyword for execution engine,
        ///     allowing for iterating over an expression returning a list of nodes
        /// </summary>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "for-each")]
        private static void lambda_for_each (ApplicationContext context, ActiveEventArgs e)
        {
            foreach (var idxSource in XUtil.Iterate (e.Args, context)) {
                var dp = new Node ("__dp", idxSource.Value);
                e.Args.Insert (0, dp);

                // checking to see if there are any [lambda.xxx] children beneath [for-each]
                // at which case we execute these nodes
                var executed = false;
                foreach (var idxExe in e.Args.FindAll (idxChild => idxChild.Name.StartsWith ("lambda"))) {
                    executed = true;
                    context.Raise (idxExe.Name, idxExe);
                }

                // if there were no [lambda.xxx] children, we default to executing everything
                // inside of [for-each] as immutable
                if (!executed)
                    context.Raise ("lambda.immutable", e.Args);
                e.Args [0].UnTie ();
            }
        }
    }
}