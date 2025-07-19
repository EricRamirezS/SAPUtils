using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// The <c>RightClickEvents</c> class handles the subscription and processing of SAP Business One right-click events.
    /// This class exposes delegates and events for both pre-event and post-event handling of right-click actions in the application.
    /// </summary>
    /// <remarks>
    /// Right-click events allow the customization of context menus within SAP Business One.
    /// The class ensures that both "before action" and "after action" events can be handled and bubbled accordingly.
    /// </remarks>
    /// <seealso cref="T:SAPUtils.SapAddon"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class RightClickEvents {

        /// <summary>
        /// Represents the delegate for handling "Right Click After" events in the SAP Business One application.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the <c>ContextMenuInfo</c> object containing details of the right-click event.
        /// </param>
        /// <remarks>
        /// This delegate is triggered after the right-click event is completed within the application.
        /// </remarks>
        /// <seealso cref="T:SAPbouiCOM.ContextMenuInfo" />
        public delegate void RightClickAfterHandler(ref ContextMenuInfo eventInfo);

        /// <summary>
        /// Represents the delegate for handling the RightClickBefore event.
        /// This event is triggered before a right-click context menu is shown.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the <see cref="SAPbouiCOM.ContextMenuInfo"/> containing information
        /// about the right-click context menu.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean value indicating whether the event should be passed to the application for processing.
        /// Set to `false` to prevent the default processing.
        /// </param>
        /// <seealso cref="SAPbouiCOM.ContextMenuInfo"/>
        public delegate void RightClickBeforeHandler(ref ContextMenuInfo eventInfo, out bool bubbleEvent);

        /// <summary>
        /// Represents the event triggered before a right-click action is executed in the context menu within the SAP Business One application.
        /// </summary>
        /// <param name="eventInfo">
        /// A reference to the <see cref="SAPbouiCOM.ContextMenuInfo" /> object containing information about the right-click event.
        /// This includes the item details, form context, and additional metadata related to the triggered right-click action.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean output parameter that determines whether to continue processing the event in SAP Business One.
        /// Set to <c>false</c> to cancel further execution of the event. Defaults to <c>true</c>.
        /// </param>
        /// <remarks>
        /// This event is part of the <see cref="SAPUtils.Events.RightClickEvents" /> class and is invoked before the display of the context menu.
        /// It allows custom logic or validation to be inserted before the standard right-click action is handled.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.ContextMenuInfo" />
        /// <seealso cref="SAPUtils.Events.RightClickEvents" />
        public static event RightClickBeforeHandler RightClickBefore;

        /// <summary>
        /// Event triggered after the right-click context menu is finalized in the SAP Business One application.
        /// </summary>
        /// <remarks>
        /// This event is part of the <see cref="RightClickEvents" /> class and is invoked
        /// when the right-click menu action has been completed.
        /// </remarks>
        /// <param name="eventInfo">
        /// A reference to <see cref="ContextMenuInfo" />, providing details about the context menu event.
        /// </param>
        /// <seealso cref="ContextMenuInfo" />
        /// <seealso cref="RightClickEvents" />
        public static event RightClickAfterHandler RightClickAfter;

        /// <summary>
        /// Handles a specific SAP Business One event. This method determines whether the event is fired before or after the action
        /// and raises the corresponding event handlers.
        /// </summary>
        /// <param name="eventInfo">The context menu event information, encapsulating details about the specific event.</param>
        /// <param name="bubbleEvent">
        /// A boolean parameter determining the continuation of the event flow.
        /// Set it to <c>false</c> to stop the default SAP Business One behavior for the event.
        /// </param>
        /// <seealso cref="SAPbouiCOM.ContextMenuInfo"/>
        internal static void Handle(ref ContextMenuInfo eventInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (eventInfo.BeforeAction) {
                RightClickBefore?.Invoke(ref eventInfo, out bubbleEvent);
            }
            else {
                RightClickAfter?.Invoke(ref eventInfo);
            }
        }
    }
}