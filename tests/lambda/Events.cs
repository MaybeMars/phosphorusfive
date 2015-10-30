/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System.Linq;
using NUnit.Framework;
using p5.core;

namespace p5.unittests.lambda
{
    [TestFixture]
    public class Events : TestBase
    {
        public Events ()
            : base ("p5.hyperlisp", "p5.lambda", "p5.types") { }

        /// <summary>
        ///     creates a simple event, and invokes it, to verify events works as they should
        /// </summary>
        [Test]
        public void Events01 ()
        {
            var node = ExecuteLambda (@"event:test.foo1
  lambda
    set:@/././*/_out/?value
      source:success
test.foo1
  _out");
            Assert.AreEqual ("success", node [1] [0].Value);
        }

        /// <summary>
        ///     creates a simple event, and invokes it with an expression as root argument, consuming this expression
        ///     from inside the event as a referenced expression, to verify events can take parameters
        ///     as values when invoked
        /// </summary>
        [Test]
        public void Events02 ()
        {
            var node = ExecuteLambda (@"event:test.foo2
  lambda
    set:@@/././?value
      source:success
test.foo2:@/+/?value
_out");
            Assert.AreEqual ("success", node [2].Value);
        }

        /// <summary>
        ///     creates an event with multiple [lambda] objects, making sure events can take multiple lambda statements,
        ///     and execute them in order declared
        /// </summary>
        [Test]
        public void Events03 ()
        {
            var node = ExecuteLambda (@"remove-event:test.foo3
event:test.foo3
  lambda
    set:@/././*/_out/?value
      source:{0}{1}
        :succ
        :@/././././*/_out/?value
  lambda
    set:@/././*/_out/?value
      source:{0}{1}
        :@/././././*/_out/?value
        :ess
test.foo3
  _out");
            Assert.AreEqual ("success", node [2] [0].Value);
        }

        /// <summary>
        ///     creates an event with a [lambda.copy] statement as child, making sure lambda objects are
        ///     invoket correctly from events
        /// </summary>
        [Test]
        public void Events04 ()
        {
            var node = ExecuteLambda (@"event:test.foo4
  lambda.copy
    set:@/././*/_out/?value
      source:error
test.foo4
  _out:success");
            Assert.AreEqual ("success", node [1] [0].Value);
        }

        /// <summary>
        ///     creates an event in one Application Context, to invoke it in a different, making sure
        ///     events works the way they should
        /// </summary>
        [Test]
        public void Events05 ()
        {
            ExecuteLambda (@"event:test.foo5
  lambda
    set:@/././*/_out/?value
      source:success");

            // creating new Application Context
            Context = Loader.Instance.CreateApplicationContext ();
            var node = ExecuteLambda (@"test.foo5
  _out");
            Assert.AreEqual ("success", node [0] [0].Value);
        }

        /// <summary>
        ///     creates an event twice, to make sure both invocations are invoked, in order of creation
        /// </summary>
        [Test]
        public void Events06 ()
        {
            var node = ExecuteLambda (@"event:test.foo6
  lambda
    set:@/././*/_out/?value
      source:succ
event:test.foo6
  lambda
    set:@/././*/_out/?value
      source:{0}{1}
        :@/././././*/_out/?value
        :ess
test.foo6
  _out");
            Assert.AreEqual ("success", node [2] [0].Value);
        }

        /// <summary>
        ///     creates an event using a formatting expression, making sure events works as they should
        /// </summary>
        [Test]
        public void Events07 ()
        {
            ExecuteLambda (@"event:test.f{0}
  :oo7
  lambda
    set:@/././*/_out/?value
      source:success");
            var node = ExecuteLambda (@"test.foo7
  _out");
            Assert.AreEqual ("success", node [0] [0].Value);
        }

        /// <summary>
        ///     creates an event, for then to delete it, making sure deletion is successful
        /// </summary>
        [Test]
        public void Events08 ()
        {
            var node = ExecuteLambda (@"event:test.foo8
  lambda
    set:@/././*/_out/?value
      source:error
event-remove:test.foo8
test.foo8
  _out:success");
            Assert.AreEqual ("success", node [2] [0].Value);
        }

        /// <summary>
        ///     creates an event, for then to delete it, using a formatting expression,
        ///     making sure deletion is successful
        /// </summary>
        [Test]
        public void Events09 ()
        {
            var node = ExecuteLambda (@"event:test.foo9
  lambda
    set:@/././*/_out/?value
      source:error
event-remove:test.{0}
  :foo9
test.foo9
  _out:success");
            Assert.AreEqual ("success", node [2] [0].Value);
        }

        /// <summary>
        ///     creates two events, for then to delete them both, using expressions,
        ///     making sure deletion is successful
        /// </summary>
        [Test]
        public void Events10 ()
        {
            var node = ExecuteLambda (@"event:test.foo10
  lambda
    set:@/././*/_out/?value
      source:error
event:test.foo11
  lambda
    set:@/././*/_out/?value
      source:error
event-remove:@/../*/(/test.foo10|test.foo11)?name
test.foo10
  _out:success
test.foo11
  _out:success");
            Assert.AreEqual ("success", node [3] [0].Value);
            Assert.AreEqual ("success", node [4] [0].Value);
        }

        /// <summary>
        ///     creates two events with one [event] statement
        /// </summary>
        [Test]
        public void Events11 ()
        {
            var node = ExecuteLambda (@"_evts
  test.foo12
  test.foo13
event:@/-/*/?name
  lambda
    set:@/././*/_out/?value
      source:success
test.foo12
  _out
test.foo13
  _out");
            Assert.AreEqual ("success", node [2] [0].Value);
            Assert.AreEqual ("success", node [3] [0].Value);
        }

        [ActiveEvent (Name = "test.hardcoded")]
        private static void test_hardcoded (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Value += "succ";
        }

        /// <summary>
        ///     creates an Active Event that already exists as a C# Active Event, verifying both are called,
        ///     and that "dynamically created" event is invoked last
        /// </summary>
        [Test]
        public void Events12 ()
        {
            var node = ExecuteLambda (@"event:test.hardcoded
  lambda
    set:@/././?value
      source:{0}ess
        :@/./././.?value
test.hardcoded");
            Assert.AreEqual ("success", node [1].Value);
        }

        /// <summary>
        ///     creates an event that contains "persistent data" in a mutable data node, making sure
        ///     dynamically created Active Events can contain "mutable data segments"
        /// </summary>
        [Test]
        public void Events13 ()
        {
            var node = ExecuteLambda (@"event:test.foo14
  lambda
    set:@/././?value
      source:@/./+/#/?name
    _foo:node:
    set:@/-/#/?name
      source:success
test.foo14
test.foo14");
            Assert.AreEqual ("", node [1].Value);
            Assert.AreEqual ("success", node [2].Value);
        }

        [ActiveEvent (Name = "test.static.event-1")]
        [ActiveEvent (Name = "test.static.event-2")]
        private static void test_static_event_1 (ApplicationContext context, ActiveEventArgs e)
        { }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having no filter, and Active Event
        ///     is supposed to return two statically created Active Events
        /// </summary>
        [Test]
        public void Events14 ()
        {
            var node = ExecuteLambda (@"list-events");
            Assert.IsTrue (node [0].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
            Assert.IsTrue (node [0].Children.SingleOrDefault (idx => "test.static.event-2".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a string filter, and Active Event
        ///     is supposed to return two statically created Active Events
        /// </summary>
        [Test]
        public void Events15 ()
        {
            var node = ExecuteLambda (@"list-events:test.static.event-");
            Assert.AreEqual (2, node [0].Count);
            Assert.IsTrue (node [0].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
            Assert.IsTrue (node [0].Children.SingleOrDefault (idx => "test.static.event-2".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a filter being a 'value' expression, and Active Event
        ///     is supposed to return two statically created Active Events
        /// </summary>
        [Test]
        public void Events16 ()
        {
            var node = ExecuteLambda (@"_filter:test.static.event-
list-events:@/-?value");
            Assert.AreEqual (2, node [1].Count);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-2".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a filter being a 'value' expression,
        ///     and value is a list of reference node, which is supposed to be converted into string, and Active Event
        ///     is supposed to return two statically created Active Events
        /// </summary>
        [Test]
        public void Events17 ()
        {
            var node = ExecuteLambda (@"_filter:node:""test.static.event-""
list-events:@/-?value");
            Assert.AreEqual (2, node [1].Count);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-2".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a filter, which is a reference expression,
        ///     leading another expression, leading to multiple nodes, where each node's value is an exact match, and Active Event
        ///     is supposed to return two statically created Active Events
        /// </summary>
        [Test]
        public void Events18 ()
        {
            var node = ExecuteLambda (@"_filter:@/*?value
  :test.static.event-1
  :test.static.event-2
list-events:@@/-?value");
            Assert.AreEqual (2, node [1].Count);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-2".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a filter, which leads to nothing, 
        ///     and Active Event is supposed to return nothing
        /// </summary>
        [Test]
        public void Events19 ()
        {
            var node = ExecuteLambda (@"list-events:@/-?value");
            Assert.AreEqual (0, node [0].Count);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a filter, which is a reference expression,
        ///     leading another expression, leading to a single node, where that node's value is an exact match, and Active Event
        ///     is supposed to return two statically created Active Events
        /// </summary>
        [Test]
        public void Events20 ()
        {
            var node = ExecuteLambda (@"_filter:@/*?value
  :test.static.event-
list-events:@@/-?value");
            Assert.AreEqual (2, node [1].Count);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-2".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly when having a filter, which is a reference expression,
        ///     leading another expression, leading to a single node, where that node's value is an exact match, and Active Event
        ///     is supposed to return one statically created Active Events
        /// </summary>
        [Test]
        public void Events21 ()
        {
            var node = ExecuteLambda (@"_filter:@/*?value
  :test.static.event-1
list-events:@@/-?value");
            Assert.AreEqual (1, node [1].Count);
            Assert.IsTrue (node [1].Children.SingleOrDefault (idx => "test.static.event-1".Equals (idx.Value)) != null);
        }

        /// <summary>
        ///     verifies that [p5.meta.list-event] works correctly and returns static events as [static] node names
        /// </summary>
        [Test]
        public void Events22 ()
        {
            var node = ExecuteLambda (@"list-events:test.static.event-1");
            Assert.AreEqual (1, node [0].Count);
            Assert.IsTrue (node [0].Children.SingleOrDefault (idx => "static".Equals (idx.Name)) != null);
        }

        /// <summary>
        ///     creates an event that returns a new node by using [append], verifying events work as they should
        /// </summary>
        [Test]
        public void Events23 ()
        {
            var node = ExecuteLambda (@"event:test.foo15
  lambda
    append:@/./.?node
      source
        _result:success
test.foo15");
            Assert.AreEqual (1, node [1].Count);
            Assert.AreEqual ("_result", node [1] [0].Name);
            Assert.AreEqual ("success", node [1] [0].Value);
        }
    }
}