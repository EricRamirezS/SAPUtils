using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides event handling for the ServerInvokeCompleted event in the SAP Business One environment.
    /// </summary>
    /// <remarks>
    /// Allows subscribers to handle events when a server invocation process completes in SAP Business One.
    /// </remarks>
    /// <see cref="SAPUtils.SapAddon" />
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class ServerInvokeCompletedEvents {
        /// <summary>
        /// Represents the delegate for handling the `ServerInvokeCompletedBefore` event.
        /// </summary>
        /// <param name="b1IEventArgs">A reference to an instance of <see cref="B1iEvent"/>
        /// containing the event-specific data.</param>
        /// <param name="bubbleEvent">An out parameter indicating whether the event should
        /// continue to propagate (`true`) or be canceled (`false`).</param>
        /// <remarks>
        /// This delegate can be used to hook into the `ServerInvokeCompletedBefore` event
        /// to implement custom logic before the main event processing occurs.
        /// </remarks>
        /// <seealso cref="SAPUtils.Events.ServerInvokeCompletedEvents" />
        public delegate void ServerInvokeCompletedBeforeHandler(ref B1iEvent b1IEventArgs, out bool bubbleEvent);

        /// <summary>
        /// Represents an event triggered before the server invocation is completed.
        /// </summary>
        /// <remarks>
        /// This event allows intervention before the server completes the invocation, providing
        /// access to the event data and controlling whether the event should bubble further.
        /// </remarks>
        /// <param name="b1IEventArgs">
        /// The event arguments associated with the server invocation, passed by reference.
        /// The arguments contain detailed information about the invocation’s context and state.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean output parameter that determines whether the event should bubble further
        /// in the event pipeline. Setting this to <c>false</c> prevents the event from propagating further.
        /// </param>
        /// <seealso cref="SAPUtils.Events.RightClickEvents" />
        /// <seealso cref="SAPUtils.Events.UDOEvents" />
        /// <seealso cref="SAPUtils.Events.StatusBarEvents" />
        public static event ServerInvokeCompletedBeforeHandler ServerInvokeCompletedBefore;

        /// <summary>
        /// Handles specific events triggered in the SAP Business One application.
        /// Each subclass implements handling logic for a corresponding event type.
        /// </summary>
        /// <param name="b1IEventArgs">
        /// The structure containing information about the event being handled, including event type, action, and triggering item.
        /// </param>
        /// <param name="bubbleEvent">
        /// A flag indicating whether the event should propagate further. Set to False to stop event propagation.
        /// </param>
        /// <seealso cref="SAPbouiCOM.ItemEvent" />
        /// <seealso cref="SAPUtils.SapAddon" />
        internal static void Handle(ref B1iEvent b1IEventArgs, out bool bubbleEvent) {
            bubbleEvent = true;
            ServerInvokeCompletedBefore?.Invoke(ref b1IEventArgs, out bubbleEvent);
        }
    }
}