using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides static methods and events to handle menu interactions in the SAP Add-On application.
    /// </summary>
    /// <remarks>
    /// This class defines delegates and events for handling both the "before" and "after" states
    /// of menu events raised by the application.
    /// </remarks>
    /// <seealso cref="SAPUtils.SapAddon"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class MenuEvents {

        /// <summary>
        /// Delegate for handling events that occur after a menu action is processed.
        /// </summary>
        /// <param name="eventInfo">Reference to the <c>MenuEvent</c> object containing details about the menu action.</param>
        /// <remarks>
        /// This is invoked for post-action handling of menu events. Ensures appropriate customization
        /// or additional processing can occur after SAP Business One has completed its menu action.
        /// </remarks>
        /// <seealso cref="SAPUtils.Events.MenuEvents" />
        public delegate void MenuAfterHandler(ref MenuEvent eventInfo);

        /// <summary>
        /// Represents a delegate for handling the MenuBefore event in the SAP UI framework.
        /// This event occurs before a menu action is processed, providing the ability to
        /// control the continuation of the event.
        /// </summary>
        /// <param name="eventInfo">A reference to the <see cref="SAPbouiCOM.MenuEvent"/> object that contains information about the menu action.</param>
        /// <param name="bubbleEvent">
        /// A boolean passed by reference that determines whether the event should
        /// continue to be processed (true) or be stopped (false).
        /// </param>
        /// <remarks>
        /// This delegate is part of the <see cref="SAPUtils.Events.MenuEvents"/> class.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.MenuEvent"/>
        public delegate void MenuBeforeHandler(ref MenuEvent eventInfo, out bool bubbleEvent);

        /// <summary>
        /// Occurs before a menu action is processed in the SAP Business One client application.
        /// </summary>
        /// <remarks>
        /// This event allows subscribers to handle or suppress a menu action before it is executed.
        /// </remarks>
        /// <param name="pVal">
        /// Contains information about the menu event, such as the menu item ID and whether the action is before or after the event.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean parameter that determines whether the event should continue to propagate.
        /// Setting this to <c>false</c> will stop further processing of the event.
        /// </param>
        /// <see cref="SAPbouiCOM.MenuEvent" />
        /// <see cref="SAPUtils.Events.MenuEvents.MenuAfter" />
        public static event MenuBeforeHandler MenuBefore;

        /// <summary>
        /// Represents the event triggered after a menu action has been executed in the SAP Business One application.
        /// </summary>
        /// <remarks>
        /// This delegate is used to handle post-execution logic for menu actions.
        /// The event is raised after the menu operation has been successfully completed.
        /// </remarks>
        /// <param name="pVal">The event information, including the details of the executed menu action.</param>
        /// <seealso cref="SAPUtils.Events.MenuEvents"/>
        public static event MenuAfterHandler MenuAfter;

        /// <summary>
        /// Handles the specified event data for processing in a specific event class.
        /// </summary>
        /// <param name="pVal">
        /// The event information or data passed by the SAP application.
        /// This can vary by the type of event being handled, such as MenuEvent, ContextMenuInfo, or others.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean output parameter that determines whether the event should continue to propagate to other handlers.
        /// Set this to false to stop further propagation if the event is fully handled.
        /// </param>
        /// <seealso cref="SAPbouiCOM.MenuEvent" />
        /// <seealso cref="SAPbouiCOM.ContextMenuInfo" />
        /// <seealso cref="SAPbouiCOM.ProgressBarEvent" />
        /// <seealso cref="SAPUtils.SapAddon" />
        internal static void Handle(ref MenuEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;
            if (pVal.BeforeAction) {
                MenuBefore?.Invoke(ref pVal, out bubbleEvent);
            }
            else {
                MenuAfter?.Invoke(ref pVal);
            }
        }
    }
}