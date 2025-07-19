using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides event handling for ProgressBar-related events in the SAP B1 application.
    /// This class supports subscribing to and managing the lifecycle of ProgressBar events
    /// triggered by the SAP client.
    /// </summary>
    /// <remarks>
    /// Handles both "Before" and "After" ProgressBar events, allowing developers to implement
    /// custom logic during the ProgressBar's operation states.
    /// </remarks>
    /// <seealso cref="SAPbouiCOM.ProgressBarEvent"/>
    /// <seealso cref="SAPUtils.SapAddon"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class ProgressBarEvents {

        /// <summary>
        /// Represents a delegate for handling ProgressBar After Events.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the <c>ProgressBarEvent</c> instance that contains the event data.
        /// </param>
        /// <remarks>
        /// This delegate is used to define methods that handle events triggered after a progress bar action is completed.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.ProgressBarEvent"/>
        public delegate void ProgressBarAfterHandler(ref ProgressBarEvent eventInfo);

        /// <summary>
        /// Delegate to handle the 'Before' phase of a ProgressBar event in SAP Business One.
        /// This delegate is invoked during the progress bar event processing, before the action is executed.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the instance of <see cref="ProgressBarEvent"/> containing
        /// information about the progress bar event, such as its current state.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean output parameter that determines whether the event should continue bubbling up to other subscribers.
        /// If set to false, the event will not propagate further.
        /// </param>
        /// <remarks>
        /// This delegate is used to handle operations that need to be validated, modified,
        /// or canceled before the main progress bar event occurs.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.BoStatusBarMessageType"/>
        /// <seealso cref="SAPUtils.Events.ProgressBarEvents"/>
        public delegate void ProgressBarBeforeHandler(ref ProgressBarEvent eventInfo, out bool bubbleEvent);

        /// <summary>
        /// Represents the event that occurs before a progress bar action is performed in the SAP Business One UI API.
        /// Subscribers can use this event to execute custom logic before the progress bar action is finalized.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the <c cref="SAPbouiCOM.ProgressBarEvent"/> object that contains information about the progress bar event being triggered, such as the action being performed and other contextual details.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean value that determines whether the event should continue to be processed by subsequent handlers.
        /// Set this to <c>false</c> to prevent further propagation of the event.
        /// </param>
        /// <remarks>
        /// This event is triggered prior to the execution of a progress bar-related action. Use this event if you need to cancel, modify, or validate actions before they occur.
        /// </remarks>
        /// <see cref="SAPbouiCOM.ProgressBarEvent"/>
        public static event ProgressBarBeforeHandler ProgressBarBefore;

        /// <summary>
        /// Represents an event that occurs after a progress bar action is completed.
        /// </summary>
        /// <remarks>
        /// The <c>ProgressBarAfter</c> event is triggered when the progress bar finishes an operation.
        /// It allows subscribing to operations that are performed after the progress bar activity is completed.
        /// </remarks>
        /// <param name="eventInfo">
        /// A <see cref="ProgressBarEvent"/> object that contains information about the progress bar event.
        /// </param>
        /// <seealso cref="SAPbouiCOM.BoStatusBarMessageType" />
        /// <seealso cref="ProgressBarEvents.ProgressBarBefore" />
        public static event ProgressBarAfterHandler ProgressBarAfter;

        /// <summary>
        /// Handles the ProgressBarEvent triggered by the application. This method distinguishes between
        /// events that occur before or after the action and invokes the appropriate event handlers.
        /// </summary>
        /// <param name="eventInfo">Reference to the ProgressBarEvent providing information about the event.</param>
        /// <param name="bubbleEvent">Indicates whether subsequent handlers will be triggered. Can be modified by the handler.</param>
        /// <remarks>
        /// This method is internally invoked by the application and is not intended for direct usage.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.ProgressBarEvent"/>
        internal static void Handle(ref ProgressBarEvent eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (eventInfo.BeforeAction) {
                ProgressBarBefore?.Invoke(ref eventInfo, out bubbleEvent);
            }
            else {
                ProgressBarAfter?.Invoke(ref eventInfo);
            }
        }
    }
}