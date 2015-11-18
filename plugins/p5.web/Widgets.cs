/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using p5.ajax.widgets;
using p5.core;
using p5.exp;
using Void = p5.ajax.widgets.Void;

namespace p5.web.ui.widgets
{
    /// <summary>
    ///     Class for creating web widgets.
    /// </summary>
    public static class Widgets
    {
        /// <summary>
        ///     Creates an Ajax Container Web Widget.
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "p5.web.widgets.container")]
        private static void p5_web_controls_container (ApplicationContext context, ActiveEventArgs e)
        {
            CreateWidget<Container> (context, e.Args, "div");
        }

        /// <summary>
        ///     Creates an Ajax Literal Web Widget.
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "p5.web.widgets.literal")]
        private static void p5_web_controls_literal (ApplicationContext context, ActiveEventArgs e)
        {
            CreateWidget<Literal> (context, e.Args, "p");
        }

        /// <summary>
        ///     Creates an Ajax Void Web Widget.
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "p5.web.widgets.void")]
        private static void p5_web_controls_void (ApplicationContext context, ActiveEventArgs e)
        {
            CreateWidget<Void> (context, e.Args, "input");
        }

        /*
         * creates a widget from the given node
         */
        private static void CreateWidget<T> (
            ApplicationContext context, 
            Node args, 
            string elementType) where T : Widget, new ()
        {
            // creating widget as persistent control
            var parent = args.GetChildValue<Container> ("__parent", context);
            var position = args.GetChildValue ("position", context, -1);

            // getting [oninit], if any
            var onInitialLoad = CreateLoadingEvents (context, args);
            EventHandler handler = null;
            if (onInitialLoad != null) {
                handler = delegate (object sender, EventArgs e) {
                    onInitialLoad.Insert (0, new Node ("_event", ((Control)sender).ID));
                    context.Raise ("eval", onInitialLoad);
                };
            }

            // creating control as persistent control
            var widget = parent.CreatePersistentControl<T> (
                args.Get<string> (context),
                position,
                handler);

            // setting ElementType (html element) of Widget
            widget.ElementType = elementType;

            // making sure main widget ID is passed into decoration process
            DecorateWidget (context, widget, args);

            // making sure we return Widget to caller
            args.Value = widget;
        }

        /*
         * creates the [oninit] event for widget, if we should
         */
        private static Node CreateLoadingEvents (ApplicationContext context, Node node)
        {
            // checking to see if we've got an "initialload" Active Event for widget, and if so, handle it
            var onInitialLoad = node.Find ("oninit");
            if (onInitialLoad == null)
                return null;

            // returning p5.lambda node
            return onInitialLoad.Clone ();
        }

        /*
         * decorates widget with common properties
         */
        private static Widget DecorateWidget (ApplicationContext context, Widget widget, Node args)
        {
            // looping through all children nodes of Widget's node to decorate Widget
            foreach (var idxArg in args.Children) {

                switch (idxArg.Name) {
                    case "visible":
                        widget.Visible = idxArg.Get<bool> (context);
                        break;
                    case "invisible-element":
                        widget.InvisibleElement = idxArg.Get<string> (context);
                        break;
                    case "element":
                        widget.ElementType = idxArg.Get<string> (context);
                        break;
                    case "has-id":
                        widget.NoIdAttribute = !idxArg.Get<bool> (context);
                        break;
                    case "render-type":
                        widget.RenderType = (Widget.RenderingType) Enum.Parse (typeof (Widget.RenderingType), idxArg.Get<string> (context));
                        break;
                    case "widgets":
                        CreateChildWidgets (context, widget, idxArg);
                        break;
                    case "events":
                        CreateWidgetLambdaEvents (context, widget, idxArg);
                        break;
                    case "__parent":
                    case "position":
                    case "parent":
                    case "has-name":
                        // skipping these buggers, since they're not supposed to be handled here
                        break;
                    default:

                        // this might be an event, it might be a node we should ignore (starting with "_") or it might be any arbitrary attribute
                        // we should render. HandleDefaultProperty will figure out
                        HandleDefaultProperty (context, widget, idxArg);
                        break;
                }
            }

            // ensures "name" property is created, if necessary
            EnsureNameProperty (widget, args, context);
            
            // ensures "name" property is created, if necessary
            EnsureValueProperty (widget, args, context);

            return widget;
        }

        /*
         * ensuring the "value" property is the same as the "ID" of the widget, but only for "radio" input elements,
         * unless a value property is explicitly given
         */
        private static void EnsureValueProperty (Widget widget, Node node, ApplicationContext context)
        {
            if (widget.HasAttribute ("value"))
                return; // caller already explicitly added value attribute
            
            // making sure "input" type "radio" widgets have a value corresponding to 
            // their ID, unless value is explicitly given
            if (widget.ElementType == "input" && widget ["type"] == "radio") {
                widget ["value"] = widget.ID;
            }
        }
            
        /*
         * ensuring the "name" property is the same as the "ID" of the widget, unless a name property is explicitly given,
         * or element type doesn't necessarily require a "name" to function correctly, or [has-name] equals false
         */
        private static void EnsureNameProperty (Widget widget, Node node, ApplicationContext context)
        {
            if (widget.HasAttribute ("name"))
                return; // caller already explicitly added name attribute

            if (!node.GetChildValue ("has-name", context, true)) {
                return; // caller explicitly told us he didn't want no name
            }

            // making sure "input", "select" and "textarea" widgets have a name corresponding to 
            // their ID, unless name is explicitly given
            var addName = false;
            switch (widget.ElementType) {
                case "input":
                    if (widget.ElementType != "button")
                        addName = true;
                    break;
                case "textarea":
                case "select":
                    addName = true;
                    break;
            }
            if (addName)
                widget ["name"] = widget.ID;
        }

        /*
         * creates children widgets of widget
         */
        private static void CreateChildWidgets (ApplicationContext context, Widget widget, Node children)
        {
            foreach (var idxChild in children.Children) {

                idxChild.Insert (0, new Node ("__parent", widget));
                context.Raise ("p5.web.widgets." + idxChild.Name, idxChild);
            }
        }

        /*
         * Creates lambda events for Widget
         */
        private static void CreateWidgetLambdaEvents (ApplicationContext context, Widget widget, Node events)
        {
            // Looping through all events for widget
            foreach (var idxEvt in events.Children.ToList ()) {

                // Letting p5.webapp do the actual creation
                var eventNode = new Node (idxEvt.Name, widget);
                eventNode.AddRange (idxEvt.Children);
                context.Raise ("_p5.web.add-widget-lambda-event", eventNode);
            }
        }

        /*
         * handles all default properties of Widget
         */
        private static void HandleDefaultProperty (ApplicationContext context, Widget widget, Node node)
        {
            if (node.Name.StartsWith ("on")) {
                CreateEventHandler (context, widget, node);
            } else if (!node.Name.StartsWith ("_")) {
                widget [node.Name] = node.Get<string> (context);
            }
        }

        /*
         * Creates an event handler on the given widget for the given node. If the value of the node is set, the
         * event will be assumed to be a JavaScript event, and simply sent back to client as JavaScript. If it 
         * does not contain a value, the event will be handled as a server-side p5.lambda event, assuming children 
         * widgets are lambda code to evaluate
         */
        private static void CreateEventHandler (ApplicationContext context, Widget widget, Node node)
        {
            if (node.Value != null) {

                // Javascript code to be executed
                widget [node.Name] = node.Get<string> (context);
            } else {

                // Raising the Active Event that actually creates our Ajax event handler for our p5.lambda object
                var eventNode = new Node (node.Name, widget);
                eventNode.Add ("_event", widget.ID);
                eventNode.AddRange (node.Children);
                context.Raise ("_p5.web.add-widget-ajax-event", eventNode);
            }
        }
    }
}
