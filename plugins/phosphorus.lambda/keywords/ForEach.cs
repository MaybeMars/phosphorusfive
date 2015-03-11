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
    ///     Class wrapping the pf.lambda [for-each] keyword.
    /// 
    ///     The [for-each] keyword allows you to iterate of the results of an expression.
    /// </summary>
    public static class ForEach
    {
        /// <summary>
        ///     The [for-each] keyword allows you to iterate over the results of expressions.
        /// 
        ///     The [for-each] keyword allows you to loop over the iterated results of an 
        ///     <see cref="phosphorus.expressions.Expression">Expression</see>. Consider the following code, that changes all values of 
        ///     the [_data] node's children nodes to <em>"x_success"</em>, where "x" is the name of your node.
        /// 
        ///     <pre>_data
        ///   foo1:error
        ///   foo2:error
        ///   foo3:error
        /// for-each:@/-/"*"?name
        ///   set:@/./-/"*"/{0}?value
        ///     :@/././"*"/__dp?value
        ///     source:{0}_success
        ///       :@/./././"*"/__dp?value</pre>
        /// 
        ///     The [for-each] statement, will create a child node of itself, named [__dp], which is the result of the currently iterated value,
        ///     for each iteration. For instance, in the above code, during the first iteration of the [for-each], then the [__dp] node's value will
        ///     be <em>"foo1"</em>. The second iteration, it will be <em>"foo2"</em>, and so on.
        /// 
        ///     [for-each] can iterate over anything an expression can return, though iterating over a 'count' type of expression, probably doesn't
        ///     make much sense. When you iterate over a 'node' type of expression, then the node the [for-each] is currently iterating over, 
        ///     will be put into the value of the [__dp] node. This means that if you wish to use that node, you must use it through a 
        ///     <see cref="phosphorus.expressions.iterators.IteratorReference">reference iterator</see>. Consider this code, which does the same as
        ///     the code above, only iterating over the node, and not its name;
        /// 
        ///     <pre>_data
        ///   foo1:error
        ///   foo2:error
        ///   foo3:error
        /// for-each:@/-/"*"?node
        ///   set:@/./"*"/__dp/#?value
        ///     source:{0}_success
        ///       :@/./././"*"/#?name</pre>
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
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