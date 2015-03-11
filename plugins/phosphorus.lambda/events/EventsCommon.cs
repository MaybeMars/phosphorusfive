/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using System.Linq;
using phosphorus.core;
using phosphorus.expressions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace phosphorus.lambda.events
{
    /// <summary>
    ///     Common helper methods for dynamically created Active Events.
    /// 
    ///     Contains helper methods for creating and manipulating dynamically created Active Events.
    /// </summary>
    public static class EventsCommon
    {
        // contains our list of dynamically created Active Events
        private static readonly Dictionary<string, Node> Events = new Dictionary<string, Node> ();
        // used to remember the overrides across Application Context instances
        private static readonly Dictionary<string, List<string>> Overrides = new Dictionary<string, List<string>> ();
        // used to create lock when creating, deleting and consuming events
        private static readonly object Lock;
        // necessary to make sure we have a global "lock" object
        static EventsCommon () { Lock = new object (); }

        /// <summary>
        ///     Retrieves one or more dynamically created Active Events.
        /// 
        ///     Will return all [lambda.xxx] objects for the specified dynamically created Active Events.
        /// 
        ///     Example;
        /// 
        ///     <pre>pf.meta.event.get:foo</pre>
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "pf.meta.event.get")]
        private static void pf_meta_event_get (ApplicationContext context, ActiveEventArgs e)
        {
            // syntax checking
            if (e.Args.Value == null)
                return; // nothing to do here

            // looping through all events caller wish to retrieve
            foreach (var idxEventName in XUtil.Iterate<string> (e.Args, context)) {
                Node appendNode = null;
                if (Events.ContainsKey (idxEventName)) {
                    foreach (Node idxLambda in Events [idxEventName].Children) {
                        if (appendNode == null) {
                            appendNode = e.Args.Add (idxEventName).LastChild;
                        }
                        appendNode.Add (idxLambda.Clone ());
                    }
                }
            }
        }

        /// <summary>
        ///     Lists all dynamically created Active Events.
        /// 
        ///     Returns the names of all dynamically created Active Events, created through the [event] keyword.
        ///     Optionally, pass in a filter as the value of the main node.
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "pf.meta.event.list")]
        private static void pf_meta_event_list (ApplicationContext context, ActiveEventArgs e)
        {
            // retrieving filter, if any
            var filter = new List<string> (XUtil.Iterate<string> (XUtil.TryFormat<object> (e.Args, context, null), e.Args, context));
            if (e.Args.Value != null && filter.Count == 0)
                return; // possibly a filter expression, leading into oblivion

            // looping through each Active Event from core
            foreach (var idx in Events.Keys) {
                // checking to see if we have any filter
                if (filter.Count == 0) {
                    // no filter(s) given, slurping up everything
                    e.Args.Add (new Node ("dynamic", idx));
                } else {
                    // we have filter(s), checking to see if Active Event name matches at least one of our filters
                    if (filter.Any (idxFilter => idx.IndexOf (idxFilter, StringComparison.Ordinal) != -1)) {
                        e.Args.Add (new Node ("dynamic", idx));
                    }
                }
            }
        }

        /*
         * Creates a new, or appends to an existing, Active Event the given [lambda.xxx] objects,
         * to be executed when event is raised.
         */
        internal static void CreateEvent (string name, IEnumerable<Node> lambdas)
        {
            // acquiring lock since we're consuming object shared amongst more than one thread (_events)
            lock (Lock) {
                // making sure we have a key for Active Event name
                if (!Events.ContainsKey (name))
                    Events [name] = new Node ();

                // looping through each "lambda.xxx" node inside of event creation node, appending these
                // into our event node
                foreach (var idxLambda in lambdas) {
                    Events [name].Add (idxLambda.Clone ());
                }
            }
        }

        /*
         * removes the given dynamically created Active Event(s)
         */
        internal static void RemoveEvent (string name)
        {
            // acquiring lock since we're consuming object shared amongst more than one thread (_events)
            lock (Lock) {
                // removing event, if it exists
                if (Events.ContainsKey (name))
                    Events.Remove (name);
            }
        }

        /*
         * Removesthe specified override.
         */
        internal static void RemoveOverride (ApplicationContext context, string baseEvent, string superEvent)
        {
            // acquiring lock since we're consuming object shared amongst more than one thread (_override)
            lock (Lock) {
                // removing override, if it exists
                if (Overrides.ContainsKey (baseEvent)) {
                    // removing override from internal list of overrides
                    Overrides [baseEvent].Remove (superEvent);
                    if (Overrides [baseEvent].Count == 0)
                        Overrides.Remove (baseEvent);

                    // removing override from core, to make sure current context has override removed
                    context.RemoveOverride (baseEvent, superEvent);
                }
            }
        }

        /*
         * creates a new override.
         */
        internal static void CreateOverride (ApplicationContext context, string baseEvent, string superEvent)
        {
            // acquiring lock since we're consuming object shared amongst more than one thread (_overrides)
            lock (Lock) {
                // checking to see if dictionary of overrides contains base event, and if not, we create it
                if (!Overrides.ContainsKey (baseEvent)) {
                    // this event has not been overriddet yet
                    Overrides [baseEvent] = new List<string> ();
                }

                // creating a mapping from "base" to "super"
                Overrides [baseEvent].Add (superEvent);

                // making sure core actually creates our override for current context
                context.Override (baseEvent, superEvent);
            }
        }

        /*
         * Responsible for re-mapping our overrides when Application Context is initialized.
         */
        [ActiveEvent (Name = "pf.core.initialize-application-context")]
        private static void pf_core_initialize_application_context (ApplicationContext context, ActiveEventArgs e)
        {
            // acquiring lock since we're consuming object shared amongst more than one thread (_overrides)
            lock (Lock) {
                // looping through each base events first
                foreach (var idxBase in Overrides.Keys) {
                    // looping through each super events for base instance
                    foreach (var idxSuper in Overrides [idxBase]) {
                        // creating override from base to super
                        context.Override (idxBase, idxSuper);
                    }
                }
            }
        }

        /*
         * responsible for executing all dynamically created Active Events or lambda objects
         */
        [ActiveEvent (Name = "")]
        private static void _pf_core_null_active_event (ApplicationContext context, ActiveEventArgs e)
        {
            // checking if there's an event with given name in dynamically created events
            // to avoid creating a lock on every single event invocation in system, we create a "double check"
            // here, first checking for existance of key, then to create lock, for then to re-check again, which
            // should significantly improve performance of event invocations in system
            if (Events.ContainsKey (e.Name)) {
                // keep a reference to all lambda objects in current event, such that we can later delete them
                var lambdas = new List<Node> ();

                // acquiring lock to make sure we're thread safe,
                // this lock must be released before event is invoked, and is only here since we're consuming
                // an object shared among different threads (_events)
                lock (Lock) {
                    // then re-checking after lock is acquired, to make sure event is still around
                    // note, we could acquire lock before checking first time, but that would impose
                    // a significant overhead on all Active Event invocations, since "" (null Active Events)
                    // are invoked for every single Active Event raised in system
                    if (Events.ContainsKey (e.Name)) {
                        // looping through all [lambda.xxx] objects in current event, concatenating these into
                        // event invocation statement, storing a reference to each lambda object,
                        // before we release lock, and execute event invocation node
                        var idxLambdaParent = Events [e.Name];
                        foreach (var idxLambda in idxLambdaParent.Children) {
                            // appending lambda nodes into current Active Event node, and storing lambda such that we can
                            // later remove it from current node
                            var tmp = idxLambda.Clone ();
                            e.Args.Add (tmp);
                            lambdas.Add (tmp);
                        }
                    }
                }

                // invoking each [lambda.xxx] object from event
                foreach (var idxLambda in lambdas) {
                    context.Raise (idxLambda.Name, idxLambda);
                }

                // cleaning up after ourselves, deleting only the lambda objects that came
                // from our dynamically created event
                foreach (var idxLambda in lambdas) {
                    idxLambda.UnTie ();
                }
            }
        }
    }
}
