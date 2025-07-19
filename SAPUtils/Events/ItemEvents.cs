using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Represents a class that handles events related to items.
    /// Provides event management functionalities for item-related actions.
    /// </summary>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class ItemEvents {

        /// <summary>
        /// Represents the delegate for handling the "After" item events in SAP Business One.
        /// This delegate is invoked for actions that occur after a specific item event has been processed.
        /// </summary>
        /// <param name="formUid">
        /// The unique identifier of the form where the item event occurred.
        /// </param>
        /// <param name="pVal">
        /// The <see cref="SAPbouiCOM.ItemEvent"/> object containing detailed information about the item event.
        /// </param>
        /// <seealso cref="SAPUtils.Events.ItemEvents"/>
        public delegate void ItemAfterHandler(string formUid, ref ItemEvent pVal);

        /// <summary>
        /// Delegate representing the event handler for the "Before" phase of item events in SAP Business One.
        /// </summary>
        /// <param name="formUid">
        /// The unique identifier of the form for which the event is being raised.
        /// </param>
        /// <param name="pVal">
        /// A reference to an <c>ItemEvent</c> object containing details about the event being triggered.
        /// </param>
        /// <param name="bubbleEvent">
        /// A boolean value indicating whether the event processing should continue. Set to <c>false</c> to stop further processing of the event.
        /// </param>
        /// <seealso cref="SAPbouiCOM.ItemEvent"/>
        /// <seealso cref="SAPUtils.Events.ItemEvents.ItemAfterHandler"/>
        public delegate void ItemBeforeHandler(string formUid, ref ItemEvent pVal, out bool bubbleEvent);

        /// <summary>
        /// Event raised before an action is performed on an Item in the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event allows customization or validation logic to be executed prior to an Item event execution.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form in which the Item action is being triggered.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing event details.</param>
        /// <param name="bubbleEvent">
        /// A boolean flag that determines whether the event should continue to propagate.
        /// Set to `false` to stop the event propagation.
        /// </param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.ItemEvents"/>
        public static event ItemBeforeHandler ItemBefore;

        /// <summary>
        /// Event triggered after an item event occurs in a form within the SAP Business One application framework.
        /// </summary>
        /// <remarks>
        /// This event provides an opportunity to handle post-processing logic after an item interaction event completes in a form.
        /// It can be used to update UI elements, validate data, or perform other operations based on the completed action.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form where the item event occurred.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object that contains details about the event.</param>
        /// <seealso cref="SAPbouiCOM.ItemEvent"/>
        /// <seealso cref="SAPUtils.Events.ItemEvents.ItemBeforeHandler"/>
        public static event ItemAfterHandler ItemAfter;

        /// <summary>
        /// Handles event logic for specific application events.
        /// </summary>
        /// <param name="formUid">The unique identifier of the form associated with the event.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing event-specific details.</param>
        /// <param name="bubbleEvent">
        /// A boolean parameter used to determine whether the event should continue bubbling or be stopped.
        /// Set to true to allow further processing, or false to stop the event.
        /// </param>
        /// <seealso cref="SAPbouiCOM.ItemEvent"/>
        internal static void Handle(string formUid, ref ItemEvent pVal, out bool bubbleEvent) {
            bubbleEvent = true;
            if (pVal.BeforeAction) {
                ItemBefore?.Invoke(formUid, ref pVal, out bubbleEvent);
            }
            else {
                ItemAfter?.Invoke(formUid, ref pVal);
            }
        }
    }
}