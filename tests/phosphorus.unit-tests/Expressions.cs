/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Linq;
using NUnit.Framework;
using phosphorus.core;
using phosphorus.expressions;

namespace phosphorus.unittests
{
    /// <summary>
    ///     expressions unit tests, tests all sorts of different expressions
    ///     and verify they work as expected
    /// </summary>
    [TestFixture]
    public class Expressions : TestBase
    {
        public Expressions ()
            : base ("phosphorus.types", "phosphorus.hyperlisp", "phosphorus.lambda") { }

        /// <summary>
        ///     verifies that expressions are defined as such correctly
        /// </summary>
        /// <returns><c>true</c> if this instance is expression; otherwise, <c>false</c>.</returns>
        [Test]
        public void IsExpression ()
        {
            var notExp = XUtil.IsExpression ("mumbo jumbo");
            var isExp = XUtil.IsExpression ("@/x?value");
            Assert.AreEqual (false, notExp);
            Assert.AreEqual (true, isExp);
        }

        /// <summary>
        ///     verifies a simple 'value' expression works correctly
        /// </summary>
        [Test]
        public void ValueExpression ()
        {
            var node = new Node ("root", "success");
            var match = Expression.Create ("@?value").Evaluate (node, Context);
            Assert.AreEqual (1, match.Count);
            Assert.AreEqual ("success", match [0].Value);
        }

        /// <summary>
        ///     verifies a simple 'name' expression works correctly
        /// </summary>
        [Test]
        public void NameExpression ()
        {
            var node = new Node ("success");
            var match = Expression.Create ("@?name").Evaluate (node, Context);
            Assert.AreEqual (match.Count, 1);
            Assert.AreEqual ("success", match [0].Value);
        }

        /// <summary>
        ///     verifies a simple 'count' expression works correctly
        /// </summary>
        [Test]
        public void CountExpression ()
        {
            var node = new Node ("root");
            var match = Expression.Create ("@?count").Evaluate (node, Context);
            Assert.AreEqual (1, match.Count);
            Assert.AreEqual (Match.MatchType.count, match.TypeOfMatch);
        }

        /// <summary>
        ///     verifies a simple 'path' expression works correctly
        /// </summary>
        [Test]
        public void PathExpression ()
        {
            var node = new Node ("root");
            var match = Expression.Create ("@?path").Evaluate (node, Context);
            Assert.AreEqual (1, match.Count);
            Assert.AreEqual (new Node.Dna (""), match [0].Value);
        }

        /// <summary>
        ///     verifies a simple 'node' expression works correctly
        /// </summary>
        [Test]
        public void NodeExpression ()
        {
            var node = new Node ("root");
            var match = Expression.Create ("@?node").Evaluate (node, Context);
            Assert.AreEqual (1, match.Count);
            Assert.AreEqual (node, match [0].Value);
        }

        /// <summary>
        ///     verifies IsFormatted from XUtil works corectly
        /// </summary>
        /// <returns><c>true</c> if this instance is formatted; otherwise, <c>false</c>.</returns>
        [Test]
        public void IsFormatted ()
        {
            var node = new Node ("root", "{0}{1}")
                .Add ("", "su")
                .Add ("x", "error")
                .Add ("", "ccess");
            Assert.AreEqual (true, XUtil.IsFormatted (node));

            node = new Node ("root", "{0}{1}")
                .Add ("x")
                .Add ("y", "error")
                .Add ("z");
            Assert.AreEqual (false, XUtil.IsFormatted (node));
        }

        /// <summary>
        ///     verifies formatting a node using XUtil works correctly
        /// </summary>
        [Test]
        public void Format1 ()
        {
            var node = new Node ("root", "{0}{1}")
                .Add ("", "su")
                .Add ("x", "error")
                .Add ("", "ccess");
            var value = XUtil.FormatNode (node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies formatting a node with an explicit data source node using XUtil works correctly
        /// </summary>
        [Test]
        public void Format2 ()
        {
            var node = new Node ("root", "{0}cc{1}")
                .Add ("", "@/*/_first?value")
                .Add ("", "@/*/_second?value")
                .Add ("_source").LastChild
                .Add ("_first", "su")
                .Add ("x", "error")
                .Add ("_second", "ess").Root;

            // notice that data source node and formatting nodes are different here ...
            var value = XUtil.FormatNode (node, node [2], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single01 ()
        {
            var node = new Node ("root", "success");
            var value = XUtil.Single<string> (node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single02 ()
        {
            var node = new Node ("root")
                .Add ("", "su")
                .Add ("", "cc")
                .Add ("", "ess");
            var value = XUtil.Single<string> ("@/*?value", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single03 ()
        {
            var node = new Node ("root", "@/*?value")
                .Add ("", "su")
                .Add ("", "cc")
                .Add ("", "ess");
            var value = XUtil.Single<string> (node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single04 ()
        {
            var node = new Node ("root", "{0}")
                .Add ("", "success");
            var value = XUtil.Single<string> (node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single05 ()
        {
            var node = new Node ("root", "{0}")
                .Add ("", "@/0?name").LastChild
                .Add ("success").Root;
            var value = XUtil.Single<string> (node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single06 ()
        {
            var node = new Node ("root", "{0}")
                .Add ("", "@/0?name").LastChild
                .Add ("success").Root;
            var value = XUtil.Single<string> (node, node [0], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single07 ()
        {
            var node = new Node ("root", "success")
                .Add ("", "error");

            // making sure first node is used for finding single value
            var value = XUtil.Single<string> (node, node [0], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single08 ()
        {
            var node = new Node ("root")
                .Add ("1")
                .Add ("2")
                .Add ("3");
            var value = XUtil.Single<int> ("@/*?name", node, Context);
            Assert.AreEqual (123, value);
        }

        /// <summary>
        ///     verifies Single from XUtil works correctly
        /// </summary>
        [Test]
        public void Single09 ()
        {
            var node = new Node ("root")
                .Add ("", 1)
                .Add ("", 2)
                .Add ("", 3);
            var value = XUtil.Single<int> ("@/*?value", node, Context);
            Assert.AreEqual (123, value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when iterating over an expression, of type value,
        ///     requesting return type of 'string', using the expressionOrConstant overload of Iterate
        /// </summary>
        [Test]
        public void Iterate01 ()
        {
            var node = new Node ("root")
                .Add ("", "su")
                .Add ("", "cc")
                .Add ("", "ess");
            var value = XUtil.Iterate<string> ("@/*?value", node, Context).Aggregate<string, string> (null, (current, idx) => current + idx);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when iterating over an expression, of type value,
        ///     requesting return type of 'string', using the Node overload of Iterate
        /// </summary>
        [Test]
        public void Iterate02 ()
        {
            var node = new Node ("root", "@/*?value")
                .Add ("", "su")
                .Add ("", "cc")
                .Add ("", "ess");
            var value = XUtil.Iterate<string> (node, Context).Aggregate<string, string> (null, (current, idx) => current + idx);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting a 'string', iterating over a constant, who's
        ///     value is already a string
        /// </summary>
        [Test]
        public void Iterate03 ()
        {
            var node = new Node ("root", "success")
                .Add ("", "error");
            var value = XUtil.Iterate<string> (node, node [0], Context).Aggregate<string, string> (null, (current, idx) => current + idx);

            // making sure first node is used for evaluating iteration
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting a 'string', iterating over an expression, that
        ///     is formatted, where the formatting parameter is an expression in itself, using the overload of Iterate,
        ///     taking the optional dataSource for formatters
        /// </summary>
        [Test]
        public void Iterate04 ()
        {
            var node = new Node ("root", "@{0}?value")
                .Add ("", "@/0?name").LastChild
                .Add ("/0", "success").Root;
            var value = XUtil.Iterate<string> (node, node [0], Context).Aggregate<string, string> (null, (current, idx) => current + idx);

            // making sure second node is used as data source
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting 'string', iterating over a formatted expression,
        ///     where one of the formatting values is an expression in itself, not using the dataSource overload of Iterate,
        ///     but the default version, where formatting nodes and dataSource nodes are the same values
        /// </summary>
        [Test]
        public void Iterate05 ()
        {
            var node = new Node ("root", "@{0}?value")
                .Add ("", "@/0?name").LastChild
                .Add ("/0/0", "success").Root;
            var value = XUtil.Iterate<string> (node, Context).Aggregate<string, string> (null, (current, idx) => current + idx);

            // making sure first node is used as data source
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting 'string', iterating over an expression,
        ///     where the expression's result leads to a value, which holds objects of integer types
        /// </summary>
        [Test]
        public void Iterate06 ()
        {
            var node = new Node ("root")
                .Add ("", 1)
                .Add ("", 2)
                .Add ("", 3);
            var value = XUtil.Iterate<string> ("@/*?value", node, Context).Aggregate<string, string> (null, (current, idx) => current + idx);
            Assert.AreEqual ("123", value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when using the MatchEntity version, iterating over an expression,
        ///     leading to a bunch of values, which are of type "int"
        /// </summary>
        [Test]
        public void Iterate07 ()
        {
            var node = new Node ("root")
                .Add ("", 1)
                .Add ("", 2)
                .Add ("", 3);
            var value = XUtil.Iterate ("@/*?value", node, Context).Sum (idx => Utilities.Convert<int> (idx.Value, Context));
            Assert.AreEqual (6, value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting 'int', iterating over a node, who's
        ///     children node's values are integer values
        /// </summary>
        [Test]
        public void Iterate08 ()
        {
            var node = new Node ("root")
                .Add ("", 1)
                .Add ("", 2)
                .Add ("", 3);
            var value = XUtil.Iterate<int> (node, Context).Sum ();
            Assert.AreEqual (6, value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting 'Node', iterating over a node, who's children
        ///     values are integer values
        /// </summary>
        [Test]
        public void Iterate09 ()
        {
            var node = new Node ("root")
                .Add ("", 1)
                .Add ("", 2)
                .Add ("", 3);
            var value = XUtil.Iterate<Node> (node, Context).Sum (idx => idx.Get<int> (Context));
            Assert.AreEqual (6, value);
        }

        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when iterating over a formatted constant value
        /// </summary>
        [Test]
        public void Iterate10 ()
        {
            var node = new Node ("root", "succ{0}")
                .Add (string.Empty, "ess");
            var value = XUtil.Iterate<string> (node, Context).Aggregate ("", (current, idx) => current + idx);
            Assert.AreEqual ("success", value);
        }

        /// \todo the next one has pretty weird logic, and I am not even sure if it is correct ...?
        /// <summary>
        ///     verifies Iterate from XUtil works correctly, when requesting 'node', and iterating over a constant which is formatted,
        ///     and the value of that formatted expression should be converted into a Node, where it's value is an integer
        /// </summary>
        [Test]
        public void Iterate11 ()
        {
            var node = new Node ("root", "succ{0}")
                .Add (string.Empty, "ess:int:5");
            var value = "";

            // string should be converted into a Node
            foreach (var idx in XUtil.Iterate<Node> (node, Context)) {
                value += Utilities.Convert<string> (idx [0], Context);
                Assert.AreEqual (5, idx [0].Value);
            }
            Assert.AreEqual ("success:int:5", value);
        }

        /// <summary>
        ///     uses XUtil.Iterate to iterate the children of a node who's value is a node, verifying
        ///     Iterate works as expected
        /// </summary>
        [Test]
        public void Iterate12 ()
        {
            var node = CreateNode (@"_foo:node:@""_root
  foo1:bar1
  foo2:bar2""");
            var result = XUtil.Iterate<Node> (node [0], Context, true)
                .Aggregate (string.Empty, (current, idx) => current + (idx.Name + ":" + idx.Value + "-"));
            Assert.AreEqual ("foo1:bar1-foo2:bar2-", result);
        }

        /// <summary>
        ///     uses XUtil.Iterate to iterate the children of a node who's value is a string, verifying
        ///     Iterate works as expected
        /// </summary>
        [Test]
        public void Iterate13 ()
        {
            var node = CreateNode (@"_foo:@""foo1:bar1
foo2:bar2""");
            var result = XUtil.Iterate<Node> (node [0], Context, true)
                .Aggregate (string.Empty, (current, idx) => current + (idx.Name + ":" + idx.Value + "-"));
            Assert.AreEqual ("foo1:bar1-foo2:bar2-", result);
        }

        /// <summary>
        ///     uses XUtil.Iterate to iterate the children of a node who's value is an expression, 
        ///     leading to a string, verifying Iterate works as expected
        /// </summary>
        [Test]
        public void Iterate14 ()
        {
            var node = CreateNode (@"_foo:@""foo1:bar1
foo2:bar2""
_bar:@/-?value");
            var result = XUtil.Iterate<Node> (node [1], Context, true)
                .Aggregate (string.Empty, (current, idx) => current + (idx.Name + ":" + idx.Value + "-"));
            Assert.AreEqual ("foo1:bar1-foo2:bar2-", result);
        }

        /// <summary>
        ///     uses XUtil.Iterate to iterate the children of a node who's value is an expression, 
        ///     leading to a value. who's value is a node, verifying Iterate works as expected
        /// </summary>
        [Test]
        public void Iterate15 ()
        {
            var node = CreateNode (@"_foo:node:@""_exe
  foo1:bar1
  foo2:bar2""
_bar:@/-?value");
            var result = XUtil.Iterate<Node> (node [1], Context, true)
                .Aggregate (string.Empty, (current, idx) => current + (idx.Name + ":" + idx.Value + "-"));
            Assert.AreEqual ("foo1:bar1-foo2:bar2-", result);
        }

        /// <summary>
        ///     uses XUtil.Iterate to iterate the children of a node who's value is a reference expression, 
        ///     leading to a value, that's an expression, leading to a value, who's value is a node, 
        ///     verifying Iterate works as expected
        /// </summary>
        [Test]
        public void Iterate16 ()
        {
            var node = CreateNode (@"_foo:node:@""_exe
  foo1:bar1
  foo2:bar2""
_foo2:@/-?value
_bar:@@/-?value");
            var result = XUtil.Iterate<Node> (node [2], Context, true)
                .Aggregate (string.Empty, (current, idx) => current + (idx.Name + ":" + idx.Value + "-"));
            Assert.AreEqual ("foo1:bar1-foo2:bar2-", result);
        }
        
        /// <summary>
        ///     uses XUtil.Iterate to iterate the children of a node who's value is an expression, 
        ///     being a 'node' expression, verifying Iterate works as expected
        /// </summary>
        [Test]
        public void Iterate17 ()
        {
            var node = CreateNode (@"_foo
  foo1:bar1
  foo2:bar2
_bar:@/-?node");
            var result = XUtil.Iterate<Node> (node [1], Context, true)
                .Aggregate (string.Empty, (current, idx) => current + (idx.Name + ":" + idx.Value + "-"));
            Assert.AreEqual ("foo1:bar1-foo2:bar2-", result);
        }

        /// <summary>
        ///     verifies root expressions works correctly
        /// </summary>
        [Test]
        public void RootExpression ()
        {
            var node = new Node ("success")
                .Add ("");
            var value = XUtil.Single<string> ("@/..?name", node [0], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies children retrieval expressions works correctly
        /// </summary>
        [Test]
        public void ChildrenExpression ()
        {
            var node = new Node ("")
                .Add ("su")
                .Add ("ccess");
            var value = XUtil.Single<string> ("@/*?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies descendants retrieval expressions works correctly
        /// </summary>
        [Test]
        public void DescendantsExpression ()
        {
            var node = new Node ("")
                .Add ("su").LastChild
                .Add ("cc").LastChild
                .Add ("es").Root.FirstChild
                .Add ("s");
            var value = XUtil.Single<string> ("@/**?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies retrieve ancestor expressions works correctly
        /// </summary>
        [Test]
        public void AncestorExpression ()
        {
            var node = new Node ("")
                .Add ("_start", "success").LastChild
                .Add (string.Empty).LastChild
                .Add (string.Empty).Root;
            var value = XUtil.Single<string> ("@/.._start?value", node [0] [0] [0], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies retrieve sibling expressions works correctly
        /// </summary>
        [Test]
        public void SiblingExpressions ()
        {
            var node = new Node ("")
                .Add ("success")
                .Add ("error");
            var value = XUtil.Single<string> ("@/-?name", node [1], Context);
            Assert.AreEqual ("success", value);

            node = new Node ("")
                .Add ("error")
                .Add ("success");
            value = XUtil.Single<string> ("@/+?name", node [0], Context);
            Assert.AreEqual ("success", value);

            node = new Node ("")
                .Add ("error")
                .Add ("error")
                .Add ("success");
            value = XUtil.Single<string> ("@/+2?name", node [0], Context);
            Assert.AreEqual ("success", value);

            node = new Node ("")
                .Add ("success")
                .Add ("error")
                .Add ("error");
            value = XUtil.Single<string> ("@/-2?name", node [2], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies named expressions works correctly
        /// </summary>
        [Test]
        public void NameEqualsExpression ()
        {
            var node = new Node ("root")
                .Add ("success")
                .Add ("error");
            var value = XUtil.Single<string> ("@/*/success?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies valued expressions works correctly
        /// </summary>
        [Test]
        public void ValueEqualsExpression ()
        {
            var node = new Node ("root")
                .Add ("success", "query");
            var value = XUtil.Single<string> ("@/*/=query?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies valued expressions works correctly with types
        /// </summary>
        [Test]
        public void ValueTypeExpression1 ()
        {
            var node = new Node ("root")
                .Add ("success", 5);
            var value = XUtil.Single<string> ("@/*/=:int:5?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies valued expressions works correctly with types
        /// </summary>
        [Test]
        public void ValueTypeExpression2 ()
        {
            var node = new Node ("root")
                .Add ("error", "5");
            var value = XUtil.Single<string> ("@/*/=:int:5?name", node, Context);
            Assert.IsNull (value);
        }

        /// <summary>
        ///     verifies valued expressions works correctly with types
        /// </summary>
        [Test]
        public void ValueTypeExpression3 ()
        {
            var node = new Node ("root")
                .Add ("error", 5);
            var value = XUtil.Single<string> ("@/*/=5?name", node, Context);
            Assert.IsNull (value);
        }

        /// <summary>
        ///     verifies numbered child expressions works correctly
        /// </summary>
        [Test]
        public void NumberedExpression ()
        {
            var node = new Node ("root")
                .Add ("error")
                .Add ("success")
                .Add ("error");
            var value = XUtil.Single<string> ("@/1?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies range expressions works correctly
        /// </summary>
        [Test]
        public void RangeExpression ()
        {
            var node = new Node ("root")
                .Add ("error")
                .Add ("su")
                .Add ("cc")
                .Add ("e")
                .Add ("ss")
                .Add ("error");
            var value = XUtil.Single<string> ("@/*/[1,5]?name", node, Context);
            Assert.AreEqual ("success", value);

            node = new Node ("root")
                .Add ("error")
                .Add ("su")
                .Add ("cc")
                .Add ("e")
                .Add ("ss");
            value = XUtil.Single<string> ("@/*/[1,]?name", node, Context);
            Assert.AreEqual ("success", value);

            node = new Node ("root")
                .Add ("su")
                .Add ("cc")
                .Add ("e")
                .Add ("ss")
                .Add ("error");
            value = XUtil.Single<string> ("@/*/[,4]?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies retrieve reference node expressions works correctly
        /// </summary>
        [Test]
        public void ReferenceExpression ()
        {
            var node = new Node ("root")
                .Add ("_1")
                .Add ("_2", new Node ("_value", "success"));
            var value = XUtil.Single<string> ("@/1/#?value", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies retrieve parent expressions works correctly
        /// </summary>
        [Test]
        public void ParentExpression ()
        {
            var node = new Node ("success")
                .Add ("error")
                .Add ("error");
            var value = XUtil.Single<string> ("@/.?name", node [1], Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies named regex expressions works correctly
        /// </summary>
        [Test]
        public void NamedRegexExpression ()
        {
            var node = new Node ("root")
                .Add ("error")
                .Add ("success");
            var value = XUtil.Single<string> (@"@/*/""/s/""?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies valued regex expressions works correctly
        /// </summary>
        [Test]
        public void ValuedRegexExpression ()
        {
            var node = new Node ("root")
                .Add ("error")
                .Add ("success", "val");
            var value = XUtil.Single<string> (@"@/*/""=/val/""?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies modulo expressions works correctly
        /// </summary>
        [Test]
        public void ModuloExpression ()
        {
            var node = new Node ("root")
                .Add ("succ")
                .Add ("error")
                .Add ("ess");
            var value = XUtil.Single<string> ("@/*/%2?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies left shift expressions works correctly
        /// </summary>
        [Test]
        public void LeftShiftExpression ()
        {
            var node = new Node ("succ")
                .Add ("error")
                .Add ("ess")
                .Add ("error");
            var value = XUtil.Single<string> ("@/*/%2/<?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies right shift expressions works correctly
        /// </summary>
        [Test]
        public void RightShiftExpression ()
        {
            var node = new Node ("root")
                .Add ("success")
                .Add ("error");
            var value = XUtil.Single<string> ("@/>?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies logical or expressions works correctly
        /// </summary>
        [Test]
        public void OrExpression ()
        {
            var node = new Node ("root")
                .Add ("succ")
                .Add ("ess");
            var value = XUtil.Single<string> ("@/0/|/1?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies logical and expressions works correctly
        /// </summary>
        [Test]
        public void AndExpression ()
        {
            var node = new Node ("root")
                .Add ("value1", "error")
                .Add ("value2", "success");
            var value = XUtil.Single<string> ("@/*/&/*/value2?value", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies logical xor expressions works correctly
        /// </summary>
        [Test]
        public void XorExpression ()
        {
            var node = new Node ("root")
                .Add ("succ")
                .Add ("ess")
                .Add ("error");
            var value = XUtil.Single<string> ("@/*/((/succ/|/error/)^(/ess/|/error/))?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies logical not expressions works correctly
        /// </summary>
        [Test]
        public void NotExpression ()
        {
            // verifying simple not works
            var node = new Node ("root")
                .Add ("succ")
                .Add ("ess")
                .Add ("error");
            var value = XUtil.Single<string> ("@/*/!/2?name", node, Context);
            Assert.AreEqual ("success", value);

            // verifying grouped not works
            node = new Node ("root")
                .Add ("succ")
                .Add ("ess")
                .Add ("error");
            value = XUtil.Single<string> ("@/*/(!/error/)?name", node, Context);
            Assert.AreEqual ("success", value);

            // verifying logical is using last "root"
            node = new Node ("root")
                .Add ("succ")
                .Add ("ess")
                .Add ("error");
            value = XUtil.Single<string> ("@/*/!/error?name", node, Context);
            Assert.AreEqual ("successerror", value); // this one is not supposed to return "success"
        }

        /// <summary>
        ///     verifies expressions handles precedence correctly
        /// </summary>
        [Test]
        public void PrecedenceExpression ()
        {
            var node = new Node ("root")
                .Add ("_1", "error")
                .Add ("_2", "error")
                .Add ("_3", "success");
            var value = XUtil.Single<string> ("@/*/(/_1/|/_2/|/_3/&/_3/)?value", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies expressions handles orders of logical components correctly
        /// </summary>
        [Test]
        public void OrderedExpression ()
        {
            var node = new Node ("root")
                .Add ("ess")
                .Add ("succ")
                .Add ("error");
            var value = XUtil.Single<string> ("@/1/|/0?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies it is possible to create a multiline expression
        /// </summary>
        [Test]
        public void MultilineExpression ()
        {
            var node = new Node ("root")
                .Add ("error")
                .Add ("success", "x\r\ny")
                .Add ("error");
            var value = XUtil.Single<string> ("@/*/@\"=x\r\ny\"?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies creating expressions referencing other expressions works correctly
        /// </summary>
        [Test]
        public void ReferencedExpression1 ()
        {
            var node = new Node ("root")
                .Add ("error", "@/+?name")
                .Add ("success")
                .Add ("error");
            var value = XUtil.Single<string> ("@@/0?value", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies creating expressions referencing other expressions recursively works correctly
        /// </summary>
        [Test]
        public void ReferencedExpression2 ()
        {
            var node = new Node ("root")
                .Add ("error", "@@/+2?value")
                .Add ("success")
                .Add ("exp", "@/-?name");
            var value = XUtil.Single<string> ("@@/0?value", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies creating expressions referencing other expressions and constants
        ///     intermingled with each other works correctly
        /// </summary>
        [Test]
        public void ReferencedExpression3 ()
        {
            var node = ExecuteLambda (@"_data
  _1:su
  cc:@?name
  _3:ess
set:@/..?value
  source:@@/../*/_data/*?value");
            Assert.AreEqual ("success", node.Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result from string to integer works
        /// </summary>
        [Test]
        public void ConvertExpression01 ()
        {
            var node = ExecuteLambda (@"_data:567
set:@/..?value
  source:@/../*/_data?value.int");
            Assert.AreEqual (567, node.Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result from integer to string works
        /// </summary>
        [Test]
        public void ConvertExpression02 ()
        {
            var node = ExecuteLambda (@"_data:int:567
set:@/..?value
  source:@/../*/_data?value.string");
            Assert.AreEqual ("567", node.Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result to and from byte[] works
        /// </summary>
        [Test]
        public void ConvertExpression03 ()
        {
            var node = ExecuteLambda (@"_data:abcde
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.string
_byte
_string");
            Assert.AreEqual (System.Text.Encoding.UTF8.GetBytes ("abcde"), node [3].Value);
            Assert.AreEqual ("abcde", node [4].Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result from integer to string works
        /// </summary>
        [Test]
        public void ConvertExpression04 ()
        {
            var node = ExecuteLambda (@"_data:int:123
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.int
_byte
_int");
            Assert.AreEqual (123, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from guid to string works
        /// </summary>
        [Test]
        public void ConvertExpression05 ()
        {
            var node = ExecuteLambda (@"_data:guid:b91c9121-0a17-4b26-a09d-d5980eb532db
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.guid
_byte
_guid");
            Assert.AreEqual (Guid.Parse ("b91c9121-0a17-4b26-a09d-d5980eb532db"), node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from long to string works
        /// </summary>
        [Test]
        public void ConvertExpression06 ()
        {
            var node = ExecuteLambda (@"_data:long:345543543
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.long
_byte
_long");
            Assert.AreEqual ((long)345543543, node [4].Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression07 ()
        {
            var node = ExecuteLambda (@"_data:ulong:345543543
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.ulong
_byte
_ulong");
            Assert.AreEqual ((ulong)345543543, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression08 ()
        {
            var node = ExecuteLambda (@"_data:int:12345
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.int
_byte
_int");
            Assert.AreEqual (12345, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression09 ()
        {
            var node = ExecuteLambda (@"_data:uint:12345
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.uint
_byte
_uint");
            Assert.AreEqual (12345, node [4].Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression10 ()
        {
            var node = ExecuteLambda (@"_data:short:145
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.short
_byte
_short");
            Assert.AreEqual (145, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression11 ()
        {
            var node = ExecuteLambda (@"_data:float:145.45
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.float
_byte
_short");
            Assert.AreEqual (145.45F, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression12 ()
        {
            var node = ExecuteLambda (@"_data:double:145.45
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.double
_byte
_short");
            Assert.AreEqual (145.45D, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression13 ()
        {
            var node = ExecuteLambda (@"_data:decimal:145.45
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.decimal
_byte
_decimal");
            Assert.AreEqual (145.45D, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression14 ()
        {
            var node = ExecuteLambda (@"_data:bool:true
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.bool
_byte
_bool");
            Assert.AreEqual (true, node [4].Value);
        }

        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression15 ()
        {
            var node = ExecuteLambda (@"_data:byte:128
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.byte
_bytes
_byte");
            Assert.AreEqual (128, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression16 ()
        {
            var node = ExecuteLambda (@"_data:sbyte:125
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.sbyte
_bytes
_sbyte");
            Assert.AreEqual (125, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression17 ()
        {
            var node = ExecuteLambda (@"_data:char:x
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.char
_bytes
_sbyte");
            Assert.AreEqual ('x', node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression18 ()
        {
            var node = ExecuteLambda (@"_data:date:""2015-01-25T23:59:57""
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.date
_bytes
_date");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression19 ()
        {
            var node = ExecuteLambda (@"_data:time:""99.23:59:59.9990000""
set:@/../3?value
  source:@/../*/_data?value.blob
set:@/../4?value
  source:@/../3?value.time
_bytes
_time");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression20 ()
        {
            var node = ExecuteLambda (@"_data:time:""99.23:59:59.9990000""
set:@/../3?value
  source:@/../*/_data?value.string
set:@/../4?value
  source:@/../3?value.time
_bytes
_time");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression21 ()
        {
            var node = ExecuteLambda (@"_data:date:""2015-11-23T22:47:55.987""
set:@/../3?value
  source:@/../*/_data?value.string
set:@/../4?value
  source:@/../3?value.date
_bytes
_res");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression22 ()
        {
            var node = ExecuteLambda (@"_data:date:""2015-11-23T22:47:55""
set:@/../3?value
  source:@/../*/_data?value.string
set:@/../4?value
  source:@/../3?value.date
_bytes
_res");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression23 ()
        {
            var node = ExecuteLambda (@"_data:date:""2015-11-23""
set:@/../3?value
  source:@/../*/_data?value.string
set:@/../4?value
  source:@/../3?value.date
_bytes
_res");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }
        
        /// <summary>
        ///     verifies creating expressions converting the result from ulong to string works
        /// </summary>
        [Test]
        public void ConvertExpression24 ()
        {
            var node = ExecuteLambda (@"_data:decimal:2344.43
set:@/../3?value
  source:@/../*/_data?value.string
set:@/../4?value
  source:@/../3?value.decimal
_bytes
_res");
            Assert.AreEqual (node [0].Value, node [4].Value);
        }

        /// <summary>
        ///     converts a Node to its string representation
        /// </summary>
        [Test]
        public void Convert1 ()
        {
            var node = new Node ("root")
                .Add ("foo", 5);
            var value = Utilities.Convert<string> (node, Context);
            Assert.AreEqual ("root\r\n  foo:int:5", value);
        }

        /// <summary>
        ///     converts a DateTime to its Hyperlisp string representation
        /// </summary>
        [Test]
        public void Convert2 ()
        {
            var date = new DateTime (2015, 01, 22, 23, 59, 59);
            var value = Utilities.Convert<string> (date, Context);
            Assert.AreEqual ("2015-01-22T23:59:59", value);
        }

        /// <summary>
        ///     converts a Node without "phosphorus.types" loaded into the ApplicationContext
        /// </summary>
        [Test]
        public void Convert3 ()
        {
            Loader.Instance.UnloadAssembly ("phosphorus.types");
            Context = Loader.Instance.CreateApplicationContext ();
            try {
                var node = new Node ("root")
                    .Add ("foo", 5);
                var value = Utilities.Convert<string> (node, Context);
                Assert.AreEqual ("Name=root, Count=1", value);
            } finally {
                Loader.Instance.LoadAssembly ("phosphorus.types");
                Context = Loader.Instance.CreateApplicationContext ();
            }
        }

        /// <summary>
        ///     verifies escaped iterators works
        /// </summary>
        [Test]
        public void EscapedIterators ()
        {
            var node = new Node ("_data")
                .Add ("*").LastChild
                .Add ("..").LastChild
                .Add (".").LastChild
                .Add ("/").LastChild
                .Add ("\\").LastChild
                .Add ("success").Root;
            var value = XUtil.Single<string> (@"@/*/\*/*/\../*/\./*/""\\/""/*/\\/*?name", node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies escaped iterators works
        /// </summary>
        [Test]
        public void ConvertBytesToString ()
        {
            var node = new Node ("@?value.string", System.Text.Encoding.UTF8.GetBytes ("success"));
            string value = XUtil.Single<string> (node.Name, node, Context);
            Assert.AreEqual ("success", value);
        }

        /// <summary>
        ///     verifies expressions can span multiple lines, with muliple white spaces
        /// </summary>
        [Test]
        public void MultiLineExpression ()
        {
            var node = ExecuteLambda (@"_mammal
  sea
    dolphins
      smart
    salmon
    killer-whales
      smart
  land
    ape
      smart
    dogs
    humans
      smart
    donkey
_result
append:@/-/?node
  source:@""@/..
    /*
      /_mammal
        /*
           (
              /**/smart/.
              !
              /land/*/humans
           )
?node""");
            Assert.AreEqual ("dolphins", node [1] [0].Name);
            Assert.AreEqual ("killer-whales", node [1] [1].Name);
            Assert.AreEqual ("ape", node [1] [2].Name);
        }
    }
}