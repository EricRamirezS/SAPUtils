using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Handles events related to report data in the SAP Business One application.
    /// </summary>
    /// <remarks>
    /// This class provides event handlers for managing report data before and after specific actions occur
    /// within the system. It exposes two key events: <see cref="ReportDataBefore"/> and <see cref="ReportDataAfter"/>.
    /// </remarks>
    /// <seealso cref="SAPbouiCOM.ReportDataInfo"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class ReportDataEvents {

        /// <summary>
        /// Represents the delegate for handling the "After Action" event triggered during report data processing.
        /// </summary>
        /// <param name="eventInfo">Reference to the <c>ReportDataInfo</c> object containing event-related data.</param>
        /// <remarks>
        /// This event is invoked when the action associated with the report data operation has been completed.
        /// </remarks>
        /// <seealso cref="ReportDataEvents" />
        public delegate void ReportDataAfterHandler(ref ReportDataInfo eventInfo);

        /// <summary>
        /// Delegate for handling the Report Data 'Before' event.
        /// </summary>
        /// <param name="eventInfo">Reference to the <see cref="ReportDataInfo"/> object containing the event information.</param>
        /// <param name="bubbleEvent">
        /// A boolean value used to control whether the event should continue to propagate.
        /// Set to false to stop propagation of the event.
        /// </param>
        /// <remarks>
        /// This delegate is used in conjunction with the <see cref="ReportDataEvents.ReportDataBefore"/> event.
        /// </remarks>
        /// <seealso cref="ReportDataEvents" />
        public delegate void ReportDataBeforeHandler(ref ReportDataInfo eventInfo, out bool bubbleEvent);

        /// <summary>
        /// Represents the event delegate <c>ReportDataBefore</c>, which is triggered before a report data-related action occurs.
        /// </summary>
        /// <remarks>
        /// This event allows for the modification or examination of the <c>ReportDataInfo</c> object before an action is performed.
        /// Subscribing to this event ensures that custom logic can be executed prior to the completion of the action.
        /// </remarks>
        /// <param name="eventInfo">The event data encapsulated in a <c>ReportDataInfo</c> instance.</param>
        /// <param name="bubbleEvent">
        /// A boolean value indicating whether the event should continue to propagate.
        /// Set this parameter to <c>false</c> to stop further processing of the event.
        /// </param>
        /// <seealso cref="ReportDataInfo"/>
        public static event ReportDataBeforeHandler ReportDataBefore;

        /// <summary>
        /// Represents an event that is triggered after a report data action is completed.
        /// </summary>
        /// <remarks>
        /// This event is part of the <see cref="SAPUtils.Events.ReportDataEvents" /> class and is invoked after the report data action concludes.
        /// It allows subscribers to handle any post-processing logic after the main action on the report data has been executed.
        /// </remarks>
        /// <param name="eventInfo">A reference to the <see cref="ReportDataInfo" /> object containing event-specific data.</param>
        /// <seealso cref="SAPUtils.Events.ReportDataEvents" />
        /// <seealso cref="SAPUtils.Events.ReportDataEvents.ReportDataBefore" />
        public static event ReportDataAfterHandler ReportDataAfter;

        /// <summary>
        /// Handles specific application-level events for SAP Business One.
        /// </summary>
        /// <param name="eventInfo">
        /// Specifies the type of application event being handled.
        /// The <see cref="SAPbouiCOM.BoAppEventTypes"/> enumeration provides the possible event types.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean value used to control whether the event should continue to propagate.
        /// Set to false to stop propagation of the event.
        /// </param>
        /// <seealso cref="SAPbouiCOM.BoAppEventTypes"/>
        internal static void Handle(ref ReportDataInfo eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (eventInfo.BeforeAction) {
                ReportDataBefore?.Invoke(ref eventInfo, out bubbleEvent);
            }
            else {
                ReportDataAfter?.Invoke(ref eventInfo);
            }
        }
    }
}