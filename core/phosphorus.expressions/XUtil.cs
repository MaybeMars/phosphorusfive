/*
 * phosphorus five, copyright 2014 - Mother Earth, Jannah, Gaia
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using phosphorus.core;
using phosphorus.expressions.exceptions;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable CoVariantArrayConversion

namespace phosphorus.expressions
{
    /// <summary>
    ///     contains useful helper methods for dealing with pf.lambda expressions
    /// </summary>
    public static class XUtil
    {
        /// <summary>
        ///     returns true if value is an expression
        /// </summary>
        /// <returns><c>true</c> if value is an expression; otherwise, <c>false</c></returns>
        /// <param name="value">value to check</param>
        public static bool IsExpression (object value)
        {
            return IsExpression (value as string);
        }

        /// <summary>
        ///     returns true if value is an expression
        /// </summary>
        /// <returns><c>true</c> if value is an expression; otherwise, <c>false</c></returns>
        /// <param name="value">value to check</param>
        public static bool IsExpression (string value)
        {
            // TODO: simplify, needs support for expressions on multiple lines, having first iterator on second line
            return value != null &&
                   value.StartsWith ("@") &&
                   value.Length >= 4 && // "@{0}" is the shortest possible expression, and has 4 characters
                   // an expression must have an iterator, referenced expression, string formatter, 
                   // or a type declaration as its second character
                   (value [1] == '?' || value [1] == '/' || value [1] == '{' || value [1] == '@');
        }

        // TODO: refactor, too complex, also contains overlapping functionality with Expression.cs
        /// <summary>
        ///     returns type of expression
        /// </summary>
        /// <returns>type of expression</returns>
        /// <param name="expressionNode">node containing expression to check, will be formatted if necessary</param>
        /// <param name="context">application context</param>
        public static Match.MatchType ExpressionType (
            Node expressionNode,
            ApplicationContext context)
        {
            // checking if we're actually given an expression
            if (!IsExpression (expressionNode.Value))
                throw new ExpressionException (
                    expressionNode.Value as string,
                    "ExpressionType must be given an actual expression",
                    expressionNode,
                    context);

            var exp = TryFormat<string> (expressionNode, context);
            var type = exp.Substring (exp.LastIndexOf ('?') + 1);
            if (type.Contains ("."))
                type = type.Substring (0, type.IndexOf ('.'));

            // some additional code, to be able to provide intelligent errors back to caller, if something goes wrong ...
            Match.MatchType matchType;
            switch (type) {
                case "node":
                case "value":
                case "count":
                case "name":
                case "path":
                    matchType = (Match.MatchType) Enum.Parse (typeof (Match.MatchType), type);
                    break;
                default:
                    throw new ExpressionException (
                        exp,
                        string.Format ("'{0}' is an unknown type declaration for your expression", type),
                        expressionNode,
                        context);
            }

            // returning type back to caller
            return matchType;
        }

        /// <summary>
        ///     returns true if given node contains formatting parameters
        /// </summary>
        /// <returns><c>true</c> if node contains formatting parameters; otherwise, <c>false</c></returns>
        /// <param name="node">node to check</param>
        public static bool IsFormatted (Node node)
        {
            // a formatted node is defined as having one or more children with string.Empty as name
            // and a value which is of type string
            return node.Value is string && node.FindAll (string.Empty).GetEnumerator ().MoveNext ();
        }

        /// <summary>
        ///     formats the node's value as a string.Format expression, using each child node
        ///     with a string.Empty name as indexed formatting parameters
        /// </summary>
        /// <returns>formatted string</returns>
        /// <param name="node">node containing formatting expression and formatting children nodes</param>
        /// <param name="context">application context</param>
        public static string FormatNode (
            Node node,
            ApplicationContext context)
        {
            return FormatNode (node, node, context);
        }

        /// <summary>
        ///     formats the node's value as a string.Format expression, using each child node
        ///     with a string.Empty name as indexed formatting parameters, using dataSource node
        ///     as the root node for any expressions within node's formatting children values
        /// </summary>
        /// <returns>formatted string</returns>
        /// <param name="node">node containing formatting expression and formatting children nodes</param>
        /// <param name="dataSource">node to use as dataSource for any expressions within formatting parameters</param>
        /// <param name="context">application context</param>
        public static string FormatNode (
            Node node,
            Node dataSource,
            ApplicationContext context)
        {
            // making sure node contains formatting values
            if (!IsFormatted (node))
                throw new ExpressionException (
                    (node.Value ?? "").ToString (),
                    "Cannot format node, no formatting nodes exists, or node's value is not a string",
                    node,
                    context);

            // retrieving all "formatting values"
            var childrenValues = new List<string> (node.ConvertChildren (
                delegate (Node idx) {
                    // we only use nodes who's names are empty as "formatting nodes"
                    if (idx.Name == string.Empty) {
                        // recursively format and evaluate expressions of children nodes
                        return FormatNodeRecursively (idx, dataSource == node ? idx : dataSource, context) ?? "";
                    }

                    // this is not a part of the formatting values for our formating expression,
                    // since it doesn't have an empty name, hence we return null, to signal to 
                    // ConvertChildren that this is to be excluded from list
                    return null;
                }));

            // returning node's value, after being formatted, according to its children node's values
            // PS, at this point all childrenValues have already been converted by the engine itself to string values
            return string.Format (CultureInfo.InvariantCulture, node.Get<string> (context), childrenValues.ToArray ());
        }

        /// <summary>
        ///     checks if node is formatted, and if it is, it will format the node, and return the
        ///     formatted value, as type T
        /// </summary>
        /// <returns>the value of the node</returns>
        /// <param name="node">node that might be formatted</param>
        /// <param name="context">application context</param>
        /// <param name="defaultValue">default value</param>
        /// <typeparam name="T">the type you wish to convert the node's value into</typeparam>
        public static T TryFormat<T> (
            Node node,
            ApplicationContext context,
            T defaultValue = default(T))
        {
            return TryFormat (node, node, context, defaultValue);
        }

        /// <summary>
        ///     checks if node is formatted, and if it is, it will format the node, and return the
        ///     formatted value, as type T
        /// </summary>
        /// <returns>the value of the node</returns>
        /// <param name="node">node that might be formatted</param>
        /// <param name="dataSource">data source to use for formatting operation</param>
        /// <param name="context">application context</param>
        /// <param name="defaultValue">default value</param>
        /// <typeparam name="T">the type you wish to convert the node's value into</typeparam>
        public static T TryFormat<T> (
            Node node,
            Node dataSource,
            ApplicationContext context,
            T defaultValue = default(T))
        {
            return Utilities.Convert (IsFormatted (node) ? FormatNode (node, dataSource, context) : node.Value, context, defaultValue);
        }

        /// <summary>
        ///     returns a single value of type T from the constant or expression in node's value. if node's value
        ///     is an expression, then expression will be evaluated, and result of expression converted to T. if
        ///     expression yields multiple results, then the results will be concatenated into a string, in order
        ///     evaluated, before string is converted to T and returned. if expression returns one result, or
        ///     node's value is a constant, then no conversion will be performed, unless necessary due to different
        ///     types in expression's result or constant. if node contains formatting children, these will be
        ///     evaluated as a formatting expression before expression is created, or constant is returned
        /// </summary>
        /// <param name="node">node who's value will be evaluated</param>
        /// <param name="context">application context</param>
        /// <param name="defaultValue">default value to return if expression or constant yields null</param>
        /// <typeparam name="T">type of object to return</typeparam>
        public static T Single<T> (
            Node node,
            ApplicationContext context,
            T defaultValue = default (T))
        {
            return Single (node, node, context, defaultValue);
        }

        /// <summary>
        ///     returns a single value of type T from the constant or expression in node's value. if node's value
        ///     is an expression, then expression will be evaluated, and result of expression converted to T. if
        ///     expression yields multiple results, then the results will be concatenated into a string, in order
        ///     evaluated, before string is converted to T and returned. if expression returns one result, or
        ///     node's value is a constant, then no conversion will be performed, unless necessary due to different
        ///     types in expression's result or constant. if node contains formatting children, these will be
        ///     evaluated as a formatting expression before expression is created, or constant is returned
        /// </summary>
        /// <param name="node">node who's value will be evaluated</param>
        /// <param name="dataSource">
        ///     node that will be used as data source for any expressions within formatting
        ///     paramaters of node's value
        /// </param>
        /// <param name="context">application context</param>
        /// <param name="defaultValue">default value to return if expression or constant yields null</param>
        /// <typeparam name="T">type of object to return</typeparam>
        public static T Single<T> (
            Node node,
            Node dataSource,
            ApplicationContext context,
            T defaultValue = default (T))
        {
            return SingleImplementation (() => Iterate<T> (node, dataSource, context), context, defaultValue);
        }

        /// <summary>
        ///     returns a single value of type T from the result of the expression given. if
        ///     expression yields multiple results, then the results will be concatenated into a string, in order
        ///     evaluated, before string is converted to T and returned. if expression returns one result, or
        ///     node's value is a constant, then no conversion will be performed, unless necessary due to different
        ///     types in expression's result or constant. if node contains formatting children, these will be
        ///     evaluated as a formatting expression before expression is created, or constant is returned
        /// </summary>
        /// <param name="expressionOrConstant">expression or constant to evaluate</param>
        /// <param name="dataSource">node to use as start node for expression</param>
        /// <param name="context">application context</param>
        /// <param name="defaultValue">default value to return if expression or constant yields null</param>
        /// <typeparam name="T">type of object to return</typeparam>
        public static T Single<T> (
            object expressionOrConstant,
            Node dataSource,
            ApplicationContext context,
            T defaultValue = default (T))
        {
            return SingleImplementation (() => Iterate<T> (expressionOrConstant, dataSource, context), context, defaultValue);
        }

        /// <summary>
        ///     iterates the given node's value, which might be either an expression or a constant. if node's
        ///     value is a constant, then this constant will be converted if necessary to T before returned. if
        ///     node's value is an expression, then this expression will be evaluated, and all results converted
        ///     to T before returned to caller. node's value can contain formatting parameters, which will be
        ///     evaluated if existing. if node contains formatting parameters, these will be evaluated before
        ///     expression is evaluated
        /// </summary>
        /// <param name="node">node who's value will be evaluated</param>
        /// <param name="context">application context</param>
        /// <param name="iterateChildren">if true, and results are converted from string, then the children of that generated node
        /// will be iterated, and not the automatically generated root node, created during string conversion</param>
        /// <typeparam name="T">type of object you wish to retrieve</typeparam>
        public static IEnumerable<T> Iterate<T> (
            Node node, 
            ApplicationContext context, 
            bool iterateChildren = false)
        {
            return Iterate<T> (node, node, context, iterateChildren);
        }

        /// <summary>
        ///     iterates the given node's value, which might be either an expression or a constant. if node's
        ///     value is a constant, then this constant will be converted if necessary to T before returned. if
        ///     node's value is an expression, then this expression will be evaluated, and all results converted
        ///     to T before returned to caller. node's value can contain formatting parameters, which will be
        ///     evaluated if existing. if node contains formatting parameters, these will be evaluated before
        ///     expression is evaluated
        /// </summary>
        /// <param name="node">node who's value will be evaluated</param>
        /// <param name="dataSource">node to use as start node for any expressions within formatting parameters</param>
        /// <param name="context">application context</param>
        /// <param name="iterateChildren">if true, and results are converted from string, then the children of that generated node
        /// will be iterated, and not the automatically generated root node, created during string conversion</param>
        /// <typeparam name="T">type of object you wish to retrieve</typeparam>
        public static IEnumerable<T> Iterate<T> (
            Node node,
            Node dataSource,
            ApplicationContext context,
            bool iterateChildren = false)
        {
            return node.Value != null ? 
                Iterate<T> (TryFormat<object> (node, dataSource, context), dataSource, context, iterateChildren) : 
                IterateChildren<T> (node, context);
        }

        /// <summary>
        ///     iterates the given expression or constant on the given dataSource node, and converts each result to
        ///     type T before yielding results back to caller
        /// </summary>
        /// <param name="expressionOrConstant">expression to run on dataSource</param>
        /// <param name="dataSource">node to use as start node for any expressions within formatting parameters</param>
        /// <param name="context">application context</param>
        /// <param name="iterateChildren">if true, and results are converted from string, then the children of that generated node
        /// will be iterated, and not the automatically generated root node, created during string conversion</param>
        /// <typeparam name="T">type of object you wish to retrieve</typeparam>
        public static IEnumerable<T> Iterate<T> (
            object expressionOrConstant,
            Node dataSource,
            ApplicationContext context,
            bool iterateChildren = false)
        {
            // ending early, if we're given nothing to iterate
            if (expressionOrConstant == null)
                return new T [] {};

            // checking if node's value is an expression
            if (IsExpression (expressionOrConstant)) {

                // we have an expression, making sure our expression iterator overload is invoked
                return Iterate<T> (Utilities.Convert<string> (expressionOrConstant, context), dataSource, context, iterateChildren);
            }

            // checking to see if user requests "children of conversions"
            if (iterateChildren && typeof (T) == typeof (Node)) {
                // user requests to iterate children, therefor we
                // iterate the children of our constant node, instead of the node itself
                return (IEnumerable<T>) Utilities.Convert<Node> (expressionOrConstant, context).Children;
            }
            return new [] { Utilities.Convert<T> (expressionOrConstant, context) };
        }

        /// <summary>
        ///     iterates the given expression or constant on the given dataSource node, and converts each result to
        ///     type T before yielding results back to caller
        /// </summary>
        /// <param name="expression">expression to run on dataSource</param>
        /// <param name="dataSource">node to use as start node for any expressions within formatting parameters</param>
        /// <param name="context">application context</param>
        /// <param name="iterateChildren">if true, and results are converted from string, then the children of that generated node
        /// will be iterated, and not the automatically generated root node, created during string conversion</param>
        /// <typeparam name="T">type of object you wish to retrieve</typeparam>
        public static IEnumerable<T> Iterate<T> (
            string expression,
            Node dataSource,
            ApplicationContext context,
            bool iterateChildren = false)
        {
            if (!IsExpression (expression))
                throw new ExpressionException (expression, "Iterate was not given a valid expression", dataSource, context);

            // we have an expression, creating a match object
            var match = Expression.Create (expression).Evaluate (dataSource, context);

            // checking type of match
            if (match.TypeOfMatch == Match.MatchType.count) {
                // if expression is of type 'count', we return 'count', possibly triggering
                // a conversion, returning count as type T, hence only iterating once
                yield return Utilities.Convert<T> (match.Count, context);
            } else {
                // caller requested anything but 'count', we return it as type T, possibly triggering
                // a conversion
                foreach (var idx in match) {
                    if (iterateChildren && typeof (T) == typeof (Node)) {
                        // user requested to iterateChildren, and since current match triggers a conversion,
                        // we iterate the children of that conversion, and not the automatically generated
                        // root node
                        foreach (var idxInner in Utilities.Convert<Node> (idx.Value, context).Children) {
                            yield return Utilities.Convert<T> (idxInner, context);
                        }
                    } else {
                        yield return Utilities.Convert<T> (idx.Value, context);
                    }
                }
            }
        }

        /// <summary>
        ///     returns all matches from expression in node. node may contain formatting parameters which will
        ///     be evaluated before expression
        /// </summary>
        /// <param name="node">node being both expression node and data source node</param>
        /// <param name="context">application context</param>
        public static IEnumerable<MatchEntity> Iterate (
            Node node,
            ApplicationContext context)
        {
            return Iterate (node, node, context);
        }

        /// <summary>
        ///     returns all matches from expression in node. node may contain formatting parameters, which will
        ///     be evaluated first, using dataSource as start node, for any expressions within formatting expression
        ///     parameters
        /// </summary>
        /// <param name="node">node being expression node</param>
        /// <param name="dataSource">node being data source node</param>
        /// <param name="context">application context</param>
        public static IEnumerable<MatchEntity> Iterate (
            Node node,
            Node dataSource,
            ApplicationContext context)
        {
            var exp = TryFormat<string> (node, dataSource, context);
            return Iterate (exp, dataSource, context);
        }

        // TODO: the next one is only used in [pf.data.select], try to rethink logic of this part, somehow
        /// <summary>
        ///     returns all matches from expression in node. node may contain formatting parameters, which will
        ///     be evaluated before expression, using formattingSource as start node for any expressions within
        ///     formatting, while using dataSource as source for evaluating expression
        ///     parameters
        /// </summary>
        /// <param name="node">node being expression node</param>
        /// <param name="dataSource">node being data source node</param>
        /// <param name="formattingSource">node being data source node for formatting expressions</param>
        /// <param name="context">application context</param>
        public static IEnumerable<MatchEntity> Iterate (
            Node node,
            Node dataSource,
            Node formattingSource,
            ApplicationContext context)
        {
            var exp = TryFormat<string> (node, formattingSource, context);
            return Iterate (exp, dataSource, context);
        }

        /// <summary>
        ///     returns all matches from given expression
        /// </summary>
        /// <param name="expression">expression</param>
        /// <param name="dataSource">node being data source node</param>
        /// <param name="context">application context</param>
        public static IEnumerable<MatchEntity> Iterate (
            string expression,
            Node dataSource,
            ApplicationContext context)
        {
            // syntax checking
            if (!IsExpression (expression))
                throw new ExpressionException (expression, dataSource, context);

            // creating a match to iterate over
            var match = Expression.Create (expression).Evaluate (dataSource, context);

            // iterating over each MatchEntity in Match
            return match;
        }

        /// <summary>
        ///     retrieves the value of [source], [rel-source], [src] or [re-src] child node, converted into T. returns null if no source exists.
        ///     does not care about whether or not there are multiple values, and will return a List if there are, though
        ///     will attempt to return only one value if it can, such as when there's a list containing only one value
        /// </summary>
        /// <param name="node">node where [source], [rel-source], [rel-src] or [src] is expected to be a child</param>
        /// <param name="context">application context</param>
        public static object Source (Node node, ApplicationContext context)
        {
            return Source (node, node.LastChild, context);
        }

        // TODO: refactor these next buggers, they're too complex
        /// <summary>
        ///     retrieves the value of [source], [rel-source], [src] or [rel-src] child node, converted into T. returns null if no source exists.
        ///     does not care about whether or not there are multiple values, and will return a List if there are multiple values, though
        ///     will attempt to return only one value, if it can, such as when there's a list of only one value for instance
        /// </summary>
        /// <param name="node">node where [source], [rel-source], [rel-src] or [src] is expected to be a child</param>
        /// <param name="dataSource">node used as data source for expressions</param>
        /// <param name="context">application context</param>
        private static object Source (Node node, Node dataSource, ApplicationContext context)
        {
            object source = null;
            if (node.LastChild != null &&
                (node.LastChild.Name == "source" || node.LastChild.Name == "src" ||
                 node.LastChild.Name == "rel-source" || node.LastChild.Name == "rel-src")) {
                // we have a [source] or [src] parameter here, figuring out what it points to, or contains
                if (IsExpression (node.LastChild.Value)) {
                    // this is an expression which might lead to multiple results, trying to return one result,
                    // but will resort to returning List of objects if necssary
                    var tmpList = new List<object> (Iterate<object> (node.LastChild, dataSource, context));
                    switch (tmpList.Count) {
                        case 0:
                            // no source values
                            break;
                        case 1:
                            // one single object in list, returning only that single object
                            source = tmpList [0];
                            break;
                        default:
                            source = tmpList;
                            break;
                    }
                } else if (node.LastChild.Value != null) {
                    // source is a constant, might still be formatted
                    source = TryFormat<object> (node.LastChild, dataSource, context);
                    
                    // making sure we support "escaped expressions"
                    // else if source is a node, we make sure we clone it, in case source and destination overlaps
                    if (source is string && (source as string).StartsWith ("\\"))
                        source = (source as string).Substring (1);
                    else if (source is Node)
                        source = (source as Node).Clone ();
                } else {
                    // there are no value in [src] node, trying to create source out of [src]'s children
                    if (node.LastChild.Count == 1) {
                        // source is a constant node, making sure we clone it, in case source and destination overlaps
                        source = node.LastChild.FirstChild.Clone ();
                    } else {
                        // more than one source, making sure we clone them, before we return the clones
                        source = new List<Node> (node.LastChild.Clone ().UntieChildren ());
                    }
                }
            }

            // returning source
            return source;
        }

        /// <summary>
        ///     retrieves the value of [source], [rel-source], [src] or [rel-src] child node, forcing one single return value, somehow.
        ///     returns null if no source exists. used in among other things [set].
        /// </summary>
        /// <param name="node">node where [source], [rel-source], [rel-src] or [src] is expected to be a child</param>
        /// <param name="context">application context</param>
        public static object SourceSingle (Node node, ApplicationContext context)
        {
            return SourceSingle (node, node.LastChild, context);
        }

        /// <summary>
        ///     retrieves the value of [source], [rel-source], [src] or [rel-src] child node, forcing one single return value, somehow.
        ///     returns null if no source exists. used in among other things [set].
        /// </summary>
        /// <param name="node">node where [source], [rel-source], [rel-src] or [src] is expected to be a child</param>
        /// <param name="dataSource">node which will be used as data source node for expresions</param>
        /// <param name="context">application context</param>
        public static object SourceSingle (Node node, Node dataSource, ApplicationContext context)
        {
            object source = null;
            if (node.LastChild != null &&
                (node.LastChild.Name == "source" || node.LastChild.Name == "src" ||
                 node.LastChild.Name == "rel-source" || node.LastChild.Name == "rel-source")) {
                // we have a [source] or [src] parameter here, figuring out what it points to, or contains
                if (node.LastChild.Value != null) {
                    // this might be an expression, or a constant, converting value to single object, somehow
                    source = Single<object> (node.LastChild, dataSource, context);
                    
                    // making sure we support "escaped expressions"
                    // else if source is a node, we make sure we clone it, in case source and destination overlaps
                    if (source is string && (source as string).StartsWith ("\\"))
                        source = (source as string).Substring (1);
                    else if (source is Node)
                        source = (source as Node).Clone ();
                } else {
                    // there are no values in [src] node, trying to create source out of [src]'s children
                    if (node.LastChild.Count == 1) {
                        // source is a constant node, making sure we clone it, in case source and destination overlaps
                        source = node.LastChild.FirstChild.Clone ();
                    } else {
                        // more than one source, making sure we convert it into one single value, meaning a 'string'
                        source = Utilities.Convert<string> (node.LastChild.Children, context);
                    }
                }
            }

            // returning source
            return source;
        }

        /// <summary>
        ///     retrieves the value of [source], [rel-source], [src] or [rel-src] child node. used in among other things [append]
        /// </summary>
        /// <param name="node">node where [source] or [src] is expected to be a child</param>
        /// <param name="context">application context</param>
        public static List<Node> SourceNodes (Node node, ApplicationContext context)
        {
            return SourceNodes (node, node.LastChild, context);
        }

        /// <summary>
        ///     retrieves the value of [source], [rel-source], [rel-src], or [src] child node.
        ///     used in among other things [append]
        /// </summary>
        /// <param name="node">node where [source] or [src] is expected to be a child</param>
        /// <param name="dataSource">node used as dataSource for expressions</param>
        /// <param name="context">application context</param>
        public static List<Node> SourceNodes (Node node, Node dataSource, ApplicationContext context)
        {
            // return value
            var sourceNodes = new List<Node> ();

            // checking if any source exists
            if (node.LastChild == null ||
                (node.LastChild.Name != "source" &&
                 node.LastChild.Name != "src" &&
                 node.LastChild.Name != "rel-source" &&
                 node.LastChild.Name != "rel-src"))
                return null; // no source was given

            // checking to see if we're given an expression
            if (IsExpression (node.LastChild.Value)) {
                // [source] or [src] is an expression somehow
                foreach (var idx in Iterate (node.LastChild, dataSource, context)) {
                    if (idx.TypeOfMatch != Match.MatchType.node && !(idx.Value is Node)) {
                        // [source] is an expression leading to something that's not a node, this
                        // will trigger conversion from string to node, adding a "root node" during
                        // conversion. we make sure we remove this node, when creating our source
                        sourceNodes.AddRange (Utilities.Convert<Node> (idx.Value, context).Children.Select (idxInner => idxInner.Clone ()));
                    } else {
                        // [source] is an expression, leading to something that's already a node somehow
                        var nodeValue = idx.Value as Node;
                        if (nodeValue != null)
                            sourceNodes.Add (nodeValue.Clone ());
                    }
                }
            } else {
                var nodeValue = node.LastChild.Value as Node;
                if (nodeValue != null) {
                    // value of source is a node, adding this node
                    sourceNodes.Add (nodeValue.Clone ());
                } else if (node.LastChild.Value is string) {
                    // source is not an expression, but has a string value. this will trigger a conversion
                    // from string, to node, creating a "root node" during conversion. we are discarding this 
                    // "root" node, and only adding children of that automatically generated root node
                    sourceNodes.AddRange (Utilities.Convert<Node> (node.LastChild.Value, context).Children.Select (idx => idx.Clone ()));
                } else if (node.LastChild.Value == null) {
                    // source has no value, neither static string values, nor expressions
                    // adding all children of source node, if any
                    sourceNodes.AddRange (node.LastChild.Children.Select (idx => idx.Clone ()));
                } else {
                    // source is not an expression, but has a non-string value. making sure we create a node
                    // out of that value, returning that node back to caller
                    sourceNodes.Add (new Node (string.Empty, node.LastChild.Value));
                }
            }

            // returning node list back to caller
            return sourceNodes.Count > 0 ? sourceNodes : null;
        }

        /*
         * helper method to recursively format node's value
         */
        private static string FormatNodeRecursively (
            Node node,
            Node dataSource,
            ApplicationContext context)
        {
            var isFormatted = IsFormatted (node);
            var isExpression = IsExpression (node.Value);

            if (isExpression && isFormatted) {
                // node is recursively formatted, and also an expression
                // formating node first, then evaluating expression
                // PS, we cannot return null here, in case expression yields null
                return Single (FormatNode (node, dataSource, context), dataSource, context, "");
            }
            if (isFormatted) {
                // node is formatted recursively, but not an expression
                return FormatNode (node, dataSource, context);
            }
            return isExpression ?
                Single (node.Get<string> (context), dataSource, context, "") :
                node.Get (context, string.Empty);
        }

        // TODO: try to refactor, too complex
        /*
         * common implementation for Single<T> methods. requires a delegate responsible for returning
         * the IEnumerable that the method iterates over
         */
        private static T SingleImplementation<T> (
            SingleDelegate<T> functor,
            ApplicationContext context,
            T defaultValue)
        {
            object singleRetVal = null;
            string multipleRetVal = null;
            var firstRun = true;
            foreach (var idx in functor ()) {
                // hack, to make sure we never convert object to string, unless necessary
                if (firstRun) {
                    // first iteration of foreach loop
                    singleRetVal = idx;
                    firstRun = false;
                } else {
                    // second, third, or fourth, etc, iteration of foreach
                    // this means we will have to convert the iterated objects into string, concatenate the objects,
                    // before converting to type T afterwards
                    if (multipleRetVal == null) {
                        // second iteration of foreach
                        multipleRetVal = Utilities.Convert<string> (singleRetVal, context);
                    }
                    if (idx is Node || (singleRetVal is Node)) {
                        // current iteration contains a node, making sure we format our string nicely, such that
                        // the end result becomes valid hyperlisp, before trying to convert to type T afterwards
                        multipleRetVal += "\r\n";
                        singleRetVal = null;
                    }
                    multipleRetVal += Utilities.Convert<string> (idx, context);
                }
            }

            // if there was not multiple iterations above, we use our "singleRetVal" object, which never was
            // converted into a string, to make sure we don't convert unless necessary, and keep reference objects
            // stay just that
            return Utilities.Convert (multipleRetVal ?? singleRetVal, context, defaultValue);
        }

        /*
         * used internally when somehow requesting children nodes being iterated
         */
        private static IEnumerable<T> IterateChildren<T> (
            Node node,
            ApplicationContext context)
        {
            if (typeof (T) == typeof (Node)) {
                // node's value is null, caller requests nodes, 
                // iterating through children of node, yielding results back to caller
                foreach (var idx in node.Children) {
                    yield return Utilities.Convert<T> (idx, context);
                }
            }
            else {
                // node's value is null, caller requests anything but node, iterating children, yielding
                // values of children, converted to type back to caller
                foreach (var idx in node.Children) {
                    yield return idx.Get<T> (context);
                }
            }
        }

        // used to retrieve items in Single<T> methods
        private delegate IEnumerable<T> SingleDelegate<out T> ();
    }
}