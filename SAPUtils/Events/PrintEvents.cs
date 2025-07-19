using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides handlers and events related to print operations in the SAP Business One application.
    /// </summary>
    /// <remarks>
    /// The <c>PrintEvents</c> class allows subscription to events triggered before or after print actions in the application.
    /// It facilitates handling print-related operations and modifying or responding to them as needed.
    /// </remarks>
    /// <seealso cref="SAPUtils.SapAddon"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class PrintEvents {

        /// Represents the delegate for handling After events related to printing operations.
        /// This delegate is invoked after a print operation is performed.
        /// <param name="eventInfo">
        /// A reference to the <c>PrintEventInfo</c> instance containing information about the completed print event.
        /// </param>
        /// See Also:
        /// <seealso cref="SAPUtils.Events.PrintEvents"/>
        public delegate void PrintAfterHandler(ref PrintEventInfo eventInfo);

        /// <summary>
        /// Defines a delegate for handling events triggered before a print operation in SAP.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the event information related to the print operation.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean output parameter that determines whether the event should continue to bubble up to other handlers.
        /// Set to 'false' to prevent further processing of the event.
        /// </param>
        /// <remarks>
        /// This delegate is used to register event handlers for operations that occur before a print action is executed.
        /// </remarks>
        /// <seealso cref="SAPUtils.Events.PrintEvents" />
        public delegate void PrintBeforeHandler(ref PrintEventInfo eventInfo, out bool bubbleEvent);

        /// <summary>
        /// Event triggered before a print action is processed in the application.
        /// </summary>
        /// <remarks>
        /// The event allows handling or altering the behavior of the print action before it occurs.
        /// It provides an opportunity to make modifications or cancel the action based on the provided parameters.
        /// </remarks>
        /// <param name="eventInfo">
        /// Reference to <c cref="PrintEventInfo"/>, providing detailed information about the print event.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean value indicating whether the event should continue to bubble to the next listener.
        /// Setting this to <c>false</c> will prevent further propagation of the event.
        /// </param>
        /// <seealso cref="PrintEventInfo"/>
        public static event PrintBeforeHandler PrintBefore;

        /// <summary>
        /// Represents an event that is triggered after a print operation completes.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the <c cref="PrintEventInfo"/> object containing information about the print event.
        /// The <c>PrintEventInfo</c> provides relevant data regarding the print operation.
        /// </param>
        /// <remarks>
        /// The <c>PrintAfter</c> event is part of the print operation workflow and is invoked after the printing action completes.
        /// It allows for any post-print logic or handling to be executed.
        /// </remarks>
        /// <seealso cref="PrintEvents"/>
        public static event PrintAfterHandler PrintAfter;

        /// <summary>
        /// Handles a specific event and performs relevant processing based on the event type or related data.
        /// </summary>
        /// <param name="eventInfo">The information about the event being processed.</param>
        /// <param name="bubbleEvent">
        /// A boolean indicating whether the event should continue to bubble to other listeners.
        /// Set to <c>false</c> to stop further propagation.
        /// </param>
        /// <seealso cref="SAPbouiCOM.ContextMenuInfo"/>
        /// <seealso cref="SAPbouiCOM.ReportDataInfo"/>
        /// <seealso cref="SAPbouiCOM.ProgressBarEvent"/>
        /// <seealso cref="SAPbouiCOM.MenuEvent"/>
        /// <seealso cref="SAPbouiCOM.LayoutKeyInfo"/>
        /// <seealso cref="SAPbouiCOM.ItemEvent"/>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPbouiCOM.PrintEventInfo"/>
        internal static void Handle(ref PrintEventInfo eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (eventInfo.BeforeAction) {
                PrintBefore?.Invoke(ref eventInfo, out bubbleEvent);
            }
            else {
                PrintAfter?.Invoke(ref eventInfo);
            }
        }
    }
}