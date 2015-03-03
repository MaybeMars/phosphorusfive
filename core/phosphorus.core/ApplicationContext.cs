/*
 * phosphorus five, copyright 2014 - Mother Earth, Jannah, Gaia
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable PossibleNullReferenceException

namespace phosphorus.core
{
    /// <summary>
    ///     the application context allows your to register instance event listeners and raise Active Events. for an executable
    ///     program, you'd probably only have one application context, while for a website project, and similar types of
    ///     projects,
    ///     you'd probably create an application context for each thread that access your website, by creating your application
    ///     context in the beginning of your page life cycle. to create your application context, use the
    ///     <see cref="phosphorus.core.Loader.CreateApplicationContext" /> method. make sure all Active Event assemblies are
    ///     loaded before
    ///     you create your application context
    /// </summary>
    public class ApplicationContext
    {
        private readonly Dictionary<string, List<string>> _overrides;
        private readonly Dictionary<string, List<Tuple<MethodInfo, object>>> _registeredActiveEvents;
        private readonly Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> _typesInstanceActiveEvents;

        internal ApplicationContext (
            Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> instanceEvents,
            Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> staticEvents)
        {
            _registeredActiveEvents = new Dictionary<string, List<Tuple<MethodInfo, object>>> ();
            _typesInstanceActiveEvents = instanceEvents;
            _overrides = new Dictionary<string, List<string>> ();
            InitializeApplicationContext (staticEvents);
        }

        /// <summary>
        ///     returns all Active Events registered within the system
        /// </summary>
        /// <value>The active events.</value>
        public IEnumerable<string> ActiveEvents
        {
            get { return _registeredActiveEvents.Select (idx => idx.Key); }
        }

        /// <summary>
        ///     returns all overrides in system
        /// </summary>
        /// <value>The active events.</value>
        public IEnumerable<Tuple<string, List<string>>> Overrides
        {
            get { return _overrides.Select (idx => new Tuple<string, List<string>> (idx.Key, idx.Value)); }
        }

        /// <summary>
        ///     registers an instance Active Event listening object
        /// </summary>
        /// <param name="instance">object to register for Active Event handling</param>
        public void RegisterListeningObject (object instance)
        {
            if (instance == null)
                throw new ArgumentNullException ("instance");

            // recursively iterating the Type of the object given, until we reach an object where we know there won't exist
            // any Active Events
            var type = instance.GetType ();
            while (!type.FullName.StartsWith ("System.")) {
                // checking to see if this type is registered in our list of types that has Active Events
                if (_typesInstanceActiveEvents.ContainsKey (type)) {
                    // retrieving the list of ActiveEvent/MethodInfo  for the currently iterated Type
                    var listAvMethodInfo = _typesInstanceActiveEvents [type];
                    foreach (var idxTupleAvMethodInfo in listAvMethodInfo) {
                        // checking to see if our registered active events list contains the key for the 
                        // currently iterated Active Event, and if not, we create a list with the given Active Event name
                        if (!_registeredActiveEvents.ContainsKey (idxTupleAvMethodInfo.Item1.Name))
                            _registeredActiveEvents [idxTupleAvMethodInfo.Item1.Name] = new List<Tuple<MethodInfo, object>> ();

                        // adding a MethodInfo/InstanceObject tuple to our list of registered Active Events with 
                        // the Active Event name as Key
                        _registeredActiveEvents [idxTupleAvMethodInfo.Item1.Name].Add (
                            new Tuple<MethodInfo, object> (idxTupleAvMethodInfo.Item2, instance));

                        // checking to see if this is an override, and if so, we register it as such
                        if (idxTupleAvMethodInfo.Item1.Overrides != null)
                            Override (idxTupleAvMethodInfo.Item1.Overrides, idxTupleAvMethodInfo.Item1.Name);
                    }
                }

                // continue iteration over Types until we reach a type we know does not have Active Events
                type = type.BaseType;
            }
        }

        /// <summary>
        ///     unregisters an instance listening object
        /// </summary>
        /// <param name="instance">object to unregister</param>
        public void UnregisterListeningObject (object instance)
        {
            if (instance == null)
                throw new ArgumentNullException ("instance");

            // iterating over the Type until we find a type we know for a fact won't contains Active Events
            var type = instance.GetType ();
            while (!type.FullName.StartsWith ("System.")) {
                // checking to see if our list of instance Active Events contains the currently iterated Type
                if (_typesInstanceActiveEvents.ContainsKey (type)) {
                    // retrieving the list of ActiveEvent/MethodInfo for the currently iterated type
                    var list = _typesInstanceActiveEvents [type];
                    foreach (var idxTupleAvMethodInfo in list) {
                        // checking to see if our registered Active Events contains the Name of the currently iterated Active Event
                        // and if not, we continue iteration. This allows us to "unregister" an object that's not registered
                        if (!_registeredActiveEvents.ContainsKey (idxTupleAvMethodInfo.Item1.Name))
                            continue;

                        // retrieving our list of MethodInfo/ObjectInstance for the currently iterated Active Event
                        var methods = _registeredActiveEvents [idxTupleAvMethodInfo.Item1.Name];

                        // removing all methods from our list of MethodInfo/ObjectInstance that matches the instance object
                        // we were given as input, that the client code wanted to remove or "unregister"
                        methods.RemoveAll (idxTupleToRemove => idxTupleToRemove.Item2 == instance);

                        // checking to see if this was the last Active Event with the given Active Event Name, and if so,
                        // we entirely remove the entire mapping from our list of Active Events to clean up after ourselves
                        if (methods.Count == 0)
                            _registeredActiveEvents.Remove (idxTupleAvMethodInfo.Item1.Name);

                        // removing any overrides, if they exist
                        if (idxTupleAvMethodInfo.Item1.Overrides != null &&
                            _overrides.ContainsKey (idxTupleAvMethodInfo.Item1.Overrides))
                            RemoveOverride (idxTupleAvMethodInfo.Item1.Overrides, idxTupleAvMethodInfo.Item1.Name);
                    }
                }

                // finding base type, to continue iteration over its Type until we find a Type we're sure of does not 
                // contain Active Event declarations
                type = type.BaseType;
            }
        }

        /// <summary>
        ///     override the specified baseActiveEvent with the given overriddenActiveEvent.
        /// </summary>
        /// <param name="baseActiveEvent">active event to override</param>
        /// <param name="newActiveEvent">which active event we should override the base with</param>
        public void Override (string baseActiveEvent, string newActiveEvent)
        {
            // checking to see if this is our first override of the given base Active Event, and if so, we create our list of 
            // overridden Active Events for the given key of the base Active Event
            if (!_overrides.ContainsKey (baseActiveEvent))
                _overrides [baseActiveEvent] = new List<string> ();

            // this might produce multiple results for the same override, hence we must check during invocation
            // that we only raise the same override once!
            // However, since we're using the _overrides as a "stack", when removing overrides, this is the correct
            // semantic way to do this
            _overrides [baseActiveEvent].Add (newActiveEvent);
        }

        /// <summary>
        ///     removes an overriden Active Event
        /// </summary>
        /// <param name="baseActiveEvent">name of the Active Event the override overrides</param>
        /// <param name="newActiveEvent">name of the new Active Event to override the baseActiveEvent</param>
        public void RemoveOverride (string baseActiveEvent, string newActiveEvent)
        {
            if (_overrides.ContainsKey (baseActiveEvent)) {
                // removing first occurency of newActiveEvent, which might not necessarily remove ALL entries with same name
                _overrides [baseActiveEvent].Remove (newActiveEvent);

                // checking to see if this was the last override, and if so, remove the dictionary item all together to clean up
                if (_overrides [baseActiveEvent].Count == 0)
                    _overrides.Remove (baseActiveEvent);
            }
        }

        /// <summary>
        ///     raises the specified Active Event with the given arguments. Will traverse the "inheritance chain", or
        ///     the overridden Active Events, to check if the Active Event client code wish to raise is overridden to
        ///     another Active Event
        /// </summary>
        /// <param name="name">name of Active Event to raise</param>
        /// <param name="args">arguments to pass into the Active Event</param>
        public Node Raise (string name, Node args = null)
        {
            if (args == null)
                args = new Node ();
            var e = new ActiveEventArgs (name, args);
            RaiseImplementation (e);
            return e.Args;
        }

        /// <summary>
        ///     raises the specified Active Event directly with the given arguments. will not traverse the "inheritance chain", or
        ///     the overridden Active Events
        /// </summary>
        /// <param name="name">name of Active Event to raise</param>
        /// <param name="args">arguments to pass into the Active Event</param>
        public void RaiseDirectly (string name, Node args = null)
        {
            if (args == null)
                args = new Node ();
            var e = new ActiveEventArgs (name, args);
            RaiseDirectly (e);
        }

        /// <summary>
        ///     raises the specified Active Event with the given arguments, directly, without trying to figure
        ///     out any derived Active Events. Useful for invoking "base Active Events" from Active Events you
        ///     know are overridden
        /// </summary>
        /// <param name="e">Active Event you wish to call base for</param>
        public void CallBase (ActiveEventArgs e)
        {
            if (e.Base != null) {
                RaiseDirectly (e.Base);
            }
        }

        /*
         * initializes app context
         */

        private void InitializeApplicationContext (Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> staticEvents)
        {
            // looping through each Type in static Active Events given
            foreach (var idxType in staticEvents.Keys) {
                // looping through each ActiveEvent/MethodInfo tuple in Type
                foreach (var idxTupleAvMethodInfo in staticEvents [idxType]) {
                    // checking to see if there exist a string Key for the name of our current iterated Active Event
                    // and if not, we create a string key, with a list of MethodInfo/InstanceObject tuples
                    // before we add the current MethodInfo as a handler of that given Active Event name
                    if (!_registeredActiveEvents.ContainsKey (idxTupleAvMethodInfo.Item1.Name))
                        _registeredActiveEvents [idxTupleAvMethodInfo.Item1.Name] = new List<Tuple<MethodInfo, object>> ();

                    // checking to see if this is an override of another Active Event, and if it is, we register it as an override
                    if (idxTupleAvMethodInfo.Item1.Overrides != null) {
                        // this is an override of another Active Event, making sure we get our mappings right here
                        Override (idxTupleAvMethodInfo.Item1.Overrides, idxTupleAvMethodInfo.Item1.Name);
                    }

                    // retrieving the list of MethodInfo/InstanceObject typles for Active Event name,
                    // and adding currently iterated method, with a "null instance object" since it's static
                    var methods = _registeredActiveEvents [idxTupleAvMethodInfo.Item1.Name];
                    methods.Add (new Tuple<MethodInfo, object> (idxTupleAvMethodInfo.Item2, null));
                }
            }

            // raising "initialize" Active Event
            Raise ("pf.core.initialize-application-context");
        }

        /*
         * responsible for recursively figuring out the most derived Active Event to actually invoke
         */

        private void RaiseImplementation (ActiveEventArgs e)
        {
            // checking to see if the Active Event currently raised, is overridden, and if it is
            // we invoke the override, and not the given Active Event, recursively, coming to the 
            // "outer most derived" Active Event
            if (_overrides.ContainsKey (e.Name)) {
                // raising the overridden Active Events, and not the given Active Event,
                foreach (var idxActiveEventName in _overrides [e.Name]) {
                    // creating a "derived" Active Event invocation, storing the base Active Event in our derived ActiveEventArgs
                    var derived = new ActiveEventArgs (idxActiveEventName, e.Args, e);

                    // recursively calling "self", in case the derived Active Event is overridden too
                    RaiseImplementation (derived);
                }
            } else {
                // raising the Active Event given, since there are no overridden Active Events overriding the current Active Event
                RaiseDirectly (e);
            }
        }

        /*
         * actual implementation of raising an Active Event. will raise the Active Event given directly, without
         * trying to figure out any polymorphistically overriden Active Events overriding the given Active Event
         */

        private void RaiseDirectly (ActiveEventArgs e)
        {
            // checking if we have any handlers
            if (_registeredActiveEvents.ContainsKey (e.Name)) {
                // looping through all Active Events handlers for the given Active Event name, and invoking them
                foreach (var idxMethod in _registeredActiveEvents [e.Name]) {
                    idxMethod.Item1.Invoke (idxMethod.Item2, new object[] { this, e });
                }
            }

            // then looping through all "null Active Event handlers" afterwards
            // ORDER COUNTS. Since most native Active Events are dependent upon arguments
            // being specifically ordered somehow, we must wait until after we have raised
            // all "native Active Events", before we raise all "null Active Event handlers".
            // this is because "null event handlers" might possibly append nodes to the current
            // Active Event's "root node", and hence mess up the parameter passing of native Active
            // Events, that also have "null event handlers", where these null event handlers,
            // are handling events, existing also as "native Active Event handlers"
            if (_registeredActiveEvents.ContainsKey (string.Empty)) {
                foreach (var idxMethod in _registeredActiveEvents [string.Empty]) {
                    idxMethod.Item1.Invoke (idxMethod.Item2, new object[] {this, e});
                }
            }
        }
    }
}