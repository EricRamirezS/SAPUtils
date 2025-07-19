using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides event handling mechanisms for form data interactions in SAP Business One.
    /// This class exposes events for actions such as adding, updating, deleting, and loading form data.
    /// </summary>
    /// <remarks>
    /// The class contains delegates and events to allow users to hook into the lifecycle of
    /// form data operations (before and after actions).
    /// </remarks>
    /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
    /// <seealso cref="SAPbouiCOM.BoEventTypes"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class FormDataEvents {

        /// <summary>
        /// Delegate used to handle the "after" event related to form data operations.
        /// This event is triggered after operations such as adding, updating, deleting, or loading form data.
        /// </summary>
        /// <param name="boInfo">
        /// The <see cref="SAPbouiCOM.BusinessObjectInfo"/> object containing information about the business object event.
        /// </param>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPUtils.Events.FormDataEvents"/>
        public delegate void FormDataAfterHandler(BusinessObjectInfo boInfo);

        /// <summary>
        /// Delegate that represents a handler for the Form Data "Before" events in the SAP Business One user interface.
        /// This delegate is used to define methods that handle events occurring before specific actions on Business Objects.
        /// </summary>
        /// <param name="boInfo">The <see cref="SAPbouiCOM.BusinessObjectInfo"/> object containing information about the Business Object event.</param>
        /// <param name="bubbleEvent">A boolean value indicating whether the event should continue (true) or be stopped (false).</param>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        public delegate void FormDataBeforeHandler(BusinessObjectInfo boInfo, out bool bubbleEvent);

        /// <summary>
        /// Represents an event triggered before a "Form Data Add" action occurs within the system.
        /// </summary>
        /// <remarks>
        /// This event allows subscribers to perform custom logic or validation before a new form data record is added.
        /// </remarks>
        /// <param name="boInfo">
        /// Provides information about the business object associated with the event.
        /// </param>
        /// <param name="bubbleEvent">
        /// Allows control over whether the event should continue to bubble to other handlers. Set to <c>false</c> to interrupt further handling of the event.
        /// </param>
        /// <seealso cref="FormDataEvents" />
        public static event FormDataBeforeHandler FormDataAddBefore;

        /// <summary>
        /// Represents the event that is triggered before a form's data is updated in the SAP B1 application.
        /// </summary>
        /// <remarks>
        /// This event allows subscribers to handle or modify the update process before it occurs.
        /// The <paramref name="boInfo"/> parameter provides information about the business object associated with the event.
        /// The <paramref name="bubbleEvent"/> parameter determines whether subsequent event handlers and the default behavior are executed.
        /// </remarks>
        /// <param name="boInfo">An instance of <see cref="SAPbouiCOM.BusinessObjectInfo"/> containing details about the business object involved in the update operation.</param>
        /// <param name="bubbleEvent">A boolean value that determines if the event should continue to bubble. Setting this to <c>false</c> cancels the default handling of the event.</param>
        /// <example>
        /// To subscribe to this event, attach your custom handler to <c>FormDataEvents.FormDataUpdateBefore</c>.
        /// Ensure your delegate follows the <c>FormDataBeforeHandler</c> signature.
        /// </example>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPbouiCOM.BoEventTypes"/>
        public static event FormDataBeforeHandler FormDataUpdateBefore;

        /// <summary>
        /// This event is triggered before a form data deletion action occurs in the application.
        /// Allows subscribers to handle or modify the deletion process before it completes.
        /// </summary>
        /// <remarks>
        /// The event provides a `BusinessObjectInfo` object containing information about the
        /// operation being performed and a boolean `bubbleEvent` that determines if the event
        /// should continue propagating or be halted.
        /// </remarks>
        /// <param name="boInfo">An object containing metadata about the business object involved in the deletion action.</param>
        /// <param name="bubbleEvent">A boolean output parameter to indicate whether the event should be propagated further. Set to `false` to stop the propagation; otherwise, set to `true`.</param>
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataAddBefore" />
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataUpdateBefore" />
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataDeleteAfter" />
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataLoadBefore"/>
        public static event FormDataBeforeHandler FormDataDeleteBefore;

        /// <summary>
        /// Represents the event triggered before the form data is loaded in the relevant SAP Business One form context.
        /// This event allows for custom logic or operations to be executed prior to the actual load of the form data.
        /// </summary>
        /// <param name="boInfo">An instance of <see cref="SAPbouiCOM.BusinessObjectInfo"/> containing metadata about the form and business object involved in the event.</param>
        /// <param name="bubbleEvent">
        /// A boolean value indicating if the event should continue to propagate (`true`) or be stopped (`false`).
        /// Setting this to `false` will prevent the default loading of the form data.
        /// </param>
        /// <remarks>
        /// Subscribing to this event enables developers to add custom validation or handle logic before the data loading process is performed.
        /// Care should be taken to avoid modifying critical state elements, as it may impact the subsequent operations.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPUtils.Events.FormDataEvents"/>
        public static event FormDataBeforeHandler FormDataLoadBefore;

        /// <summary>
        /// Represents an event that occurs after a form data add action has been completed in the SAP Business One application.
        /// </summary>
        /// <remarks>
        /// This event is triggered after the form data has been successfully added and processed. It allows subscribers to execute custom logic
        /// in response to the completion of the data addition operation.
        /// </remarks>
        /// <param name="boInfo">
        /// An instance of <see cref="SAPbouiCOM.BusinessObjectInfo"/> containing metadata related to the form data event,
        /// such as the type of business object and action performed.
        /// </param>
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataUpdateAfter"/>
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataDeleteAfter"/>
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataLoadAfter"/>
        public static event FormDataAfterHandler FormDataAddAfter;

        /// <summary>
        /// Event triggered after a form data update operation is completed in the system.
        /// </summary>
        /// <remarks>
        /// This event is part of the <see cref="SAPUtils.Events.FormDataEvents"/> class and is triggered after data within a form has been updated successfully.
        /// </remarks>
        /// <param name="boInfo">
        /// Represents the <see cref="BusinessObjectInfo"/> object containing details about the business object involved in the update operation.
        /// </param>
        /// <seealso cref="SAPUtils.Events.FormDataEvents"/>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        public static event FormDataAfterHandler FormDataUpdateAfter;

        /// <summary>
        /// Represents the event that is triggered after a form data deletion operation has occurred in the SAP Business One UI.
        /// </summary>
        /// <remarks>
        /// This event is part of the <c cref="SAPUtils.Events.FormDataEvents"/> class and is triggered after the deletion
        /// of data associated with business objects in the SAP Business One client.
        /// </remarks>
        /// <param name="boInfo">
        /// A <c cref="SAPbouiCOM.BusinessObjectInfo"/> instance that contains detailed information about the business object involved
        /// in the event, such as the type, keys, and action performed.
        /// </param>
        /// <seealso cref="SAPUtils.Events.FormDataEvents.FormDataDeleteBefore"/>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        public static event FormDataAfterHandler FormDataDeleteAfter;

        /// <summary>
        /// Occurs after the form data is loaded in the SAP Business One application.
        /// This event is triggered once the data loading process is successfully completed,
        /// allowing post-processing or custom handling related to the loaded business object.
        /// </summary>
        /// <param name="boInfo">The <see cref="SAPbouiCOM.BusinessObjectInfo"/> instance providing information
        /// about the loaded business object, including its type, keys, and additional metadata.
        /// </param>
        /// <remarks>
        /// This event is part of the SAP B1 Application Event Framework, specifically responding
        /// to the `et_FORM_DATA_LOAD` event type for after-action scenarios.
        /// Implement this event handler to perform operations that must occur after a business object
        /// has been successfully loaded, such as updating a UI element or logging relevant data.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPbouiCOM.BoEventTypes"/>
        public static event FormDataAfterHandler FormDataLoadAfter;

        /// <summary>
        /// Handles the Form Data events triggered by the SAP Business One application.
        /// Invokes the appropriate event delegates based on the event type and whether it is a before or after action.
        /// </summary>
        /// <param name="businessObjectInfo">An instance of <see cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// containing information about the current business object and the associated event.</param>
        /// <param name="bubbleEvent">A boolean output parameter that determines whether the event should continue propagating (true)
        /// or be stopped (false).</param>
        /// <seealso cref="SAPbouiCOM.BusinessObjectInfo"/>
        /// <seealso cref="SAPbouiCOM.BoEventTypes"/>
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        internal static void Handle(ref BusinessObjectInfo businessObjectInfo, out bool bubbleEvent) {
            bubbleEvent = true;
            if (businessObjectInfo.BeforeAction) {
                switch (businessObjectInfo.EventType) {
                    case BoEventTypes.et_FORM_DATA_ADD:
                        FormDataAddBefore?.Invoke(businessObjectInfo, out bubbleEvent);
                        break;
                    case BoEventTypes.et_FORM_DATA_UPDATE:
                        FormDataUpdateBefore?.Invoke(businessObjectInfo, out bubbleEvent);
                        break;
                    case BoEventTypes.et_FORM_DATA_DELETE:
                        FormDataDeleteBefore?.Invoke(businessObjectInfo, out bubbleEvent);
                        break;
                    case BoEventTypes.et_FORM_DATA_LOAD:
                        FormDataLoadBefore?.Invoke(businessObjectInfo, out bubbleEvent);
                        break;
                }
            }
            else {
                switch (businessObjectInfo.EventType) {
                    case BoEventTypes.et_FORM_DATA_ADD:
                        FormDataAddAfter?.Invoke(businessObjectInfo);
                        break;
                    case BoEventTypes.et_FORM_DATA_UPDATE:
                        FormDataUpdateAfter?.Invoke(businessObjectInfo);
                        break;
                    case BoEventTypes.et_FORM_DATA_DELETE:
                        FormDataDeleteAfter?.Invoke(businessObjectInfo);
                        break;
                    case BoEventTypes.et_FORM_DATA_LOAD:
                        FormDataLoadAfter?.Invoke(businessObjectInfo);
                        break;
                }
            }
        }
    }
}