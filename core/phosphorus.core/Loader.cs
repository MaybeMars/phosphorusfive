/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace phosphorus.core
{
    /// <summary>
    ///     Loads up assemblies for handling Active Events.
    /// 
    ///     Class is a natural singleton, use the Instance static member to access its singleton instance.
    /// 
    ///     This class is also responsible for creating your <see cref="phosphorus.core.ApplicationContext" />,
    ///     but make sure you create your application context AFTER you have initialized all assemblies you want to handle
    ///     Active Events for you, since once you've created your application context, you can no longer load more assemblies to handle
    ///     Active Events, without creating a new application context.
    /// 
    ///     Every time you load or unload an assembly, you should re-create your application context, at the very least.
    /// 
    ///     If you are within a "web context", using the phosphorus.application-pool as your driver, then you will have one
    ///     ApplicationContext automatically created for you, for each request towards your site.
    /// </summary>
    public class Loader
    {
        /// <summary>
        ///     gets the instance
        /// </summary>
        /// <value>the singleton instance</value>
        public static readonly Loader Instance = new Loader ();

        private readonly List<Assembly> _assemblies = new List<Assembly> ();
        private readonly Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> _instanceActiveEvents;
        private readonly Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> _staticActiveEvents;

        private Loader ()
        {
            _instanceActiveEvents = new Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> ();
            _staticActiveEvents = new Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> ();
        }

        /// <summary>
        ///     Creates a new ApplicationContext for you.
        /// 
        ///     Creates an application context. There should normally be one application context for every user or request
        ///     in your system. The application context is used for registering instance Active Event handlers, and  raising Active Events.
        /// </summary>
        /// <returns>The newly created context.</returns>
        public ApplicationContext CreateApplicationContext ()
        {
            var context = new ApplicationContext (_instanceActiveEvents, _staticActiveEvents);
            return context;
        }

        /// <summary>
        ///     Loads an assembly for handling Active Events.
        /// 
        ///     If you have an assembly which you wish for to handle Active Events, then you must register your assembly
        ///     through this method, or one of its overloads, before it can handle Active Events.
        /// 
        ///     If assembly is not already loaded into your ApplicationDomain, then it will be so after execution of this method.
        /// </summary>
        /// <param name="assembly">Assembly to register as Active event handler.</param>
        public void LoadAssembly (Assembly assembly)
        {
            // checking to see if assembly is already loaded up, to avoid initializing the same assembly twice
            if (_assemblies.Exists (idx => idx == assembly))
                return;

            // finding the assembly in our current AppDomain, for then to initialize it
            foreach (var idxAsm in AppDomain.CurrentDomain.GetAssemblies ()) {
                if (idxAsm == assembly) {
                    InitializeAssembly (idxAsm);
                    _assemblies.Add (idxAsm);
                }
            }
        }

        /// <summary>
        ///     Loads and registers the assembly, containing the given type, for handling Active Events.
        /// 
        ///     If you have an assembly which you wish for to handle Active Events, then you must register your assembly
        ///     through this method, or one of its overloads, before it can handle Active Events.
        /// 
        ///     If assembly is not already loaded into your ApplicationDomain, then it will be so after execution of this method.
        /// </summary>
        /// <param name="type">type from assembly you wish to load</param>
        public void LoadAssembly (Type type)
        {
            // checking to see if assembly is already loaded up, to avoid initializing the same assembly twice
            var assembly = type.Assembly;
            if (_assemblies.Exists (idx => idx == assembly))
                return;

            // finding the assembly in our current AppDomain, for then to initialize it
            foreach (var idxAsm in AppDomain.CurrentDomain.GetAssemblies ()) {
                if (idxAsm == assembly) {
                    InitializeAssembly (idxAsm);
                    _assemblies.Add (idxAsm);
                }
            }
        }

        /// <summary>
        ///     Loads an assembly for handling Active Events.
        /// 
        ///     If you have an assembly which you wish for to handle Active Events, then you must register your assembly
        ///     through this method, or one of its overloads, before it can handle Active Events.
        /// 
        ///     If assembly is not already loaded into your ApplicationDomain, then it will be so after execution of this method.
        /// 
        ///     This overload, uses the current directory to resolve where your Asembly exists.
        /// </summary>
        /// <param name="name">The name of the assembly you wish to load.</param>
        public void LoadAssembly (string name)
        {
            LoadAssembly (string.Empty, name);
        }

        /// <summary>
        ///     Loads an assembly for handling Active Events
        /// 
        ///     If you have an assembly which you wish for to handle Active Events, then you must register your assembly
        ///     through this method, or one of its overloads, before it can handle Active Events.
        /// 
        ///     If assembly is not already loaded into your ApplicationDomain, then it will be so after execution of this method.
        /// 
        ///     This overload, uses the given directory to resolve where your Asembly exists.
        /// </summary>
        /// <param name="path">Directory where assembly exists.</param>
        /// <param name="name">Name of your assembly.</param>
        public void LoadAssembly (string path, string name)
        {
            // "normalizing" name of assembly
            if (!name.ToLower ().EndsWith (".dll"))
                name += ".dll";

            // checking to see if assembly is already loaded
            if (_assemblies.Exists (idx => String.Equals (idx.ManifestModule.Name, name, StringComparison.Ordinal)))
                return;

            // checking our current AppDomain to see if assembly is already a part of our AppDomain
            foreach (var idxAsm in AppDomain.CurrentDomain.GetAssemblies ()) {
                if (idxAsm.ManifestModule.Name.ToLower () == name.ToLower ()) {
                    InitializeAssembly (idxAsm);
                    _assemblies.Add (idxAsm);
                    return;
                }
            }

            // we must dynamically load assembly and initialize it
            var assembly = Assembly.LoadFile (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, path + name));
            InitializeAssembly (assembly);
            _assemblies.Add (assembly);
        }

        /// <summary>
        ///     Unloads the assembly with the given name.
        /// 
        ///     All ApplicationContext objects created after this method is invoked, will no longer have Active Event handlers
        ///     in the assembly you unload using this method.
        /// </summary>
        /// <param name="name">name of assembly to unload</param>
        public void UnloadAssembly (string name)
        {
            // "normalizing" assembly name
            if (!name.ToLower ().EndsWith (".dll"))
                name += ".dll";

            // finding the assembly in our list of initialized assemblies
            var assembly = _assemblies.Find (idx => idx.ManifestModule.Name.ToLower () == name);

            if (assembly != null) {
                // removing assembly, and making sure all Active Events are "unregistered"
                // please notice that assembly is still in AppDomain, but will no longer handle Active Events
                /// \todo figure out how to "unload" assembly from AppDomain
                _assemblies.Remove (assembly);
                RemoveAssembly (assembly);
            }
        }

        /*
         * removes an assembly such that all Active Events from given assembly will no longer
         * be a part of our list of potential invocation objects for Active Events
         */
        private void RemoveAssembly (Assembly assembly)
        {
            // looping through all types from assembly, to see if they're handling Active Events
            foreach (var idxType in assembly.GetTypes ()) {
                if (_instanceActiveEvents.ContainsKey (idxType))
                    _instanceActiveEvents.Remove (idxType);
                if (_staticActiveEvents.ContainsKey (idxType))
                    _staticActiveEvents.Remove (idxType);
            }
        }

        /*
         * initializes an assembly by looping through all types from it, and see if type has
         * Active Event attributes for one or more of its methods, and if it does, we register
         * type as Active Event sink
         */
        private void InitializeAssembly (Assembly assembly)
        {
            // looping through all types in assembly
            foreach (var idxType in assembly.GetTypes ()) {
                // adding instance Active Events
                var instanceMethods = idxType.GetMethods (
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public);
                AddActiveEventsForType (idxType, instanceMethods, _instanceActiveEvents);

                // adding static Active Events
                var staticMethods = idxType.GetMethods (
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Static |
                    BindingFlags.NonPublic |
                    BindingFlags.Public);
                AddActiveEventsForType (idxType, staticMethods, _staticActiveEvents);
            }
        }

        /*
         * loops through all MethodInfo objects given, and adds them to the associated dictionary with type as key,
         * if they have Active Event attributes declared
         */
        private void AddActiveEventsForType (
            Type type,
            MethodInfo[] methods,
            Dictionary<Type, List<Tuple<ActiveEventAttribute, MethodInfo>>> dictionary)
        {
            // creating a list of Active Events for our type, which we check later if it contains any items, and if it does, we
            // associate it with our type
            var activeEvents = new List<Tuple<ActiveEventAttribute, MethodInfo>> ();

            // looping through all MethodInfo from type we currently are iterating
            foreach (var idxMethod in methods) {
                // checking to see if current MethodInfo has our Active Event attribute, and if it does, we check if it has
                // the right signature before we add it to our list of Active Event sinks
                var atrs = idxMethod.GetCustomAttributes (typeof (ActiveEventAttribute), true) as ActiveEventAttribute[];
                if (atrs != null && atrs.Length > 0) {
                    // checking if Active Event has a valid signature
                    VerifyActiveEventSignature (idxMethod);

                    // adding all Active Event attributes such that they become associate with our MethodInfo, to our list of Active Events
                    activeEvents.AddRange (atrs.Select (idxAtr => new Tuple<ActiveEventAttribute, MethodInfo> (idxAtr, idxMethod)));
                }
            }

            // making sure we only add type as Active Event sinks, if it actually has Active Events declared through ActiveEventAttribute
            if (activeEvents.Count > 0)
                dictionary [type] = activeEvents;
        }

        /*
         * verifies that the signature of our Active Event is correct
         */
        private static void VerifyActiveEventSignature (MethodInfo method)
        {
            var pars = method.GetParameters ();
            if (pars.Length != 2 ||
                pars [0].ParameterType != typeof (ApplicationContext) ||
                pars [1].ParameterType != typeof (ActiveEventArgs))
                throw new ArgumentException (
                    string.Format ("method '{0}.{1}' is not a valid active event, parameters of method is wrong. all Active Events must take an ApplicationContext and an ActiveEventArgs object",
                        // ReSharper disable once PossibleNullReferenceException
                        method.DeclaringType.FullName,
                        method.Name));
        }
    }
}
