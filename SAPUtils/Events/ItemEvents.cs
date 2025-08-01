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
        /// Event raised before a form is loaded in the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event allows for customization or validation logic to be executed prior to the form loading process.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form that is being loaded.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing the details of the event.</param>
        /// <param name="bubbleEvent">
        /// A boolean flag that determines whether the event should continue to propagate.
        /// Set to `false` to stop the event propagation.
        /// </param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.ItemEvents"/>
        public static event ItemBeforeHandler FormLoadBefore;

        /// <summary>
        /// Event raised before the "Item Pressed" action occurs in the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event enables custom logic or validations to be applied before executing an action triggered by pressing an item.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form where the item press event is triggered.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing the details of the item press event.</param>
        /// <param name="bubbleEvent">
        /// A boolean flag indicating whether the event should continue to propagate.
        /// Set to `false` to cancel the propagation of the event.
        /// </param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.ItemEvents"/>
        public static event ItemBeforeHandler ItemPressedBefore;

        /// <summary>
        /// Event raised before a combo box selection is changed in the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event allows for customization or validation logic to be executed prior to a combo box selection change.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form on which the combo box selection is being changed.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing details of the combo box selection event.</param>
        /// <param name="bubbleEvent">
        /// A boolean flag that determines whether the event should continue to propagate.
        /// Set to `false` to stop the event propagation.
        /// </param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.ItemEvents"/>
        public static event ItemBeforeHandler ComboSelectBefore;

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
        /// Event triggered after a form is loaded in the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event is used to perform custom actions or initializations after the form loading process has been completed.
        /// It provides an opportunity to interact with form elements after they are fully initialized.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form that has been loaded.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing event details.</param>
        /// <param name="bubbleEvent">
        /// A boolean flag that determines whether subsequent event handlers should execute.
        /// Set to `false` to prevent further propagation of the event.
        /// </param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.FormEvents"/>
        public static event ItemAfterHandler FormLoadAfter;

        /// <summary>
        /// Event raised after an ItemPress action is performed within the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event allows for post-processing logic to be implemented following the completion of an ItemPress action.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form where the ItemPress action occurred.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing details of the event.</param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.ItemEvents"/>
        public static event ItemAfterHandler ItemPressedAfter;

        /// <summary>
        /// Event raised after a ComboBox selection is made in the SAP Business One application interface.
        /// </summary>
        /// <remarks>
        /// This event allows post-action handling or additional processing to occur after a ComboBox selection is completed.
        /// </remarks>
        /// <param name="formUid">The unique identifier of the form where the ComboBox selection occurred.</param>
        /// <param name="pVal">A reference to the <see cref="SAPbouiCOM.ItemEvent"/> object containing event details.</param>
        /// <see cref="SAPbouiCOM.ItemEvent"/>
        /// <see cref="SAPUtils.Events.ItemEvents"/>
        public static event ItemAfterHandler ComboSelectAfter;

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
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (pVal.EventType) {
                    case BoEventTypes.et_FORM_LOAD:
                        FormLoadBefore?.Invoke(formUid, ref pVal, out bubbleEvent);
                        break;
                    case BoEventTypes.et_ITEM_PRESSED:
                        ItemPressedBefore?.Invoke(formUid, ref pVal, out bubbleEvent);
                        break;
                    case BoEventTypes.et_COMBO_SELECT:
                        ComboSelectBefore?.Invoke(formUid, ref pVal, out bubbleEvent);
                        break;
                    default:
                        ItemBefore?.Invoke(formUid, ref pVal, out bubbleEvent);
                        break;
                }
            }
            else {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (pVal.EventType) {
                    case BoEventTypes.et_FORM_LOAD:
                        FormLoadAfter?.Invoke(formUid, ref pVal);
                        break;
                    case BoEventTypes.et_ITEM_PRESSED:
                        ItemPressedAfter?.Invoke(formUid, ref pVal);
                        break;
                    case BoEventTypes.et_COMBO_SELECT:
                        ComboSelectAfter?.Invoke(formUid, ref pVal);
                        break;
                    default:
                        ItemAfter?.Invoke(formUid, ref pVal);
                        break;
                }
            }
        }
    }
}