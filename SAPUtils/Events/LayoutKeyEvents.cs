using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides handling for layout key events in the SAP Business One application.
    /// This class allows users to subscribe to events triggered before or after
    /// a layout key event occurs.
    /// </summary>
    /// <remarks>
    /// The layout key events can be used to implement custom logic based on user
    /// interactions with layout keys in the application interface.
    /// </remarks>
    /// <seealso cref="SAPUtils.SapAddon" />
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class LayoutKeyEvents {

        /// <summary>
        /// Represents the delegate for the event that occurs after the layout key event is processed.
        /// </summary>
        /// <param name="eventInfo">An instance of <c cref="LayoutKeyInfo"/> containing the event data.</param>
        /// <remarks>
        /// This delegate is triggered after the layout key event has been processed. Handlers for this event
        /// can process the event data further if needed.
        /// </remarks>
        /// <see cref="LayoutKeyEvents"/>
        public delegate void LayoutKeyAfterHandler(ref LayoutKeyInfo eventInfo);

        /// <summary>
        /// Represents a delegate for handling the `LayoutKeyBefore` event, which is triggered before a layout key action is performed.
        /// </summary>
        /// <param name="eventInfo">A reference to the `LayoutKeyInfo` instance that contains details about the event.</param>
        /// <param name="bubbleEvent">
        /// An output parameter indicating whether the event should continue propagating (`true`) or not (`false`).
        /// </param>
        /// <remarks>
        /// This event is specifically designed to provide a way to intercept and possibly prevent layout key actions
        /// before they are executed. It is commonly used for validation or precondition checks.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.LayoutKeyInfo"/>
        public delegate void LayoutKeyBeforeHandler(ref LayoutKeyInfo eventInfo, out bool bubbleEvent);

        /// <summary>
        /// An event triggered before a layout key operation occurs in the SAP Business One application.
        /// This event allows subscribers to perform actions or validations and control whether the event
        /// should continue or be canceled.
        /// </summary>
        /// <remarks>
        /// The event provides the opportunity to inspect and manipulate data related to the layout key action
        /// before it is executed.
        /// </remarks>
        /// <param name="eventInfo">The <see cref="LayoutKeyInfo"/> instance containing the event-related data.</param>
        /// <param name="bubbleEvent">
        /// A boolean value indicating whether the event should continue bubbling. If set to <c>false</c>, the
        /// operation will be canceled. Default is <c>true</c>.
        /// </param>
        /// <seealso cref="LayoutKeyInfo"/>
        public static event LayoutKeyBeforeHandler LayoutKeyBefore;

        /// <summary>
        /// Event triggered after a layout key action is performed within the SAP Business One UI.
        /// </summary>
        /// <remarks>
        /// This event is part of the <see cref="SAPUtils.Events.LayoutKeyEvents"/> class and is executed after a layout key action,
        /// allowing for any necessary post-processing or event handling in the context of a layout key interaction.
        /// </remarks>
        /// <param name="eventInfo">
        /// An instance of <see cref="LayoutKeyInfo"/> that contains detailed information about the layout key event, such as the form and item affected.
        /// </param>
        /// <see cref="SAPUtils.Events.LayoutKeyEvents"/>
        public static event LayoutKeyAfterHandler LayoutKeyAfter;

        /// <summary>
        /// Handles the specified event by processing the provided event information and determining
        /// whether the event should bubble further to the application.
        /// </summary>
        /// <param name="eventInfo">Information about the event being handled, such as details specific to the event type.</param>
        /// <param name="bubbleEvent">A boolean value indicating whether the event should continue to bubble.
        /// Set this value to false to stop the event from propagating further.</param>
        /// <seealso cref="SAPbouiCOM.ContextMenuInfo"/>
        /// <seealso cref="SAPbouiCOM.ItemEvent"/>
        /// <seealso cref="SAPbouiCOM.MenuEvent"/>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPbouiCOM.PrintEventInfo"/>
        /// <seealso cref="SAPbouiCOM.ProgressBarEvent"/>
        /// <seealso cref="SAPbouiCOM.ReportDataInfo"/>
        /// <seealso cref="SAPbouiCOM.LayoutKeyInfo"/>
        internal static void Handle(ref LayoutKeyInfo eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (eventInfo.BeforeAction) {
                LayoutKeyBefore?.Invoke(ref eventInfo, out bubbleEvent);
            }
            else {
                LayoutKeyAfter?.Invoke(ref eventInfo);
            }
        }
    }
}