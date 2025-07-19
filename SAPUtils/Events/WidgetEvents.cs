using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Represents the event-handling logic for widget-related events in the SAP Business One application.
    /// </summary>
    /// <remarks>
    /// This class is responsible for handling widget-related events and provides a mechanism
    /// to subscribe and process these events through the <see cref="WidgetBefore"/> delegate.
    /// </remarks>
    /// <seealso cref="SAPUtils.SapAddon"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class WidgetEvents {
        /// <summary>
        /// Represents a method signature for handling "WidgetBefore" events in the WidgetEvents system.
        /// </summary>
        /// <param name="pWidgetData">
        /// A <see cref="WidgetData"/> object that carries information about the widget event.
        /// This parameter is passed by reference and can be modified within the handler.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean flag (out parameter) that determines whether the event should be propagated further.
        /// Set to true to allow further propagation or false to stop propagation.
        /// </param>
        /// <remarks>
        /// This delegate is used to define the structure for event handler methods that handle the "WidgetBefore" event.
        /// Subscribe to the event using this delegate to perform custom logic before a widget event is processed.
        /// </remarks>
        /// <see cref="WidgetEvents" />
        public delegate void WidgetBeforeHandler(ref WidgetData pWidgetData, out bool bubbleEvent);

        /// <summary>
        /// An event that is triggered before a widget-related operation occurs in the system.
        /// </summary>
        /// <remarks>
        /// The <c>WidgetBefore</c> event allows subscribers to intercept and perform custom logic
        /// before a particular widget operation. The event handler can set the `bubbleEvent` parameter
        /// to control whether the event should propagate further in the system.
        /// </remarks>
        /// <param name="pWidgetData">The widget data associated with the event.</param>
        /// <param name="bubbleEvent">
        /// A value indicating whether the event should propagate to other subscribers.
        /// Set to <c>false</c> to stop further propagation.
        /// </param>
        /// <see cref="SAPUtils.Events.UDOEvents"/>
        /// <see cref="SAPUtils.Events.StatusBarEvents"/>
        public static event WidgetBeforeHandler WidgetBefore;

        /// <summary>
        /// Handles the specific event for the subscribed SAP Business One Application event handler.
        /// This method executes the logic when the associated event is triggered.
        /// </summary>
        /// <param name="pWidgetData">Reference to the <see cref="WidgetData"/> object containing the details of the widget data being processed.</param>
        /// <param name="bubbleEvent">Output parameter that determines whether the event should continue bubbling to other subscribed handlers (true) or stop bubbling (false).</param>
        /// <seealso cref="SAPbouiCOM.WidgetData"/>
        /// <seealso cref="SAPUtils.SapAddon"/>
        internal static void Handle(ref WidgetData pWidgetData, out bool bubbleEvent) {
            bubbleEvent = true;
            WidgetBefore?.Invoke(ref pWidgetData, out bubbleEvent);
        }
    }
}