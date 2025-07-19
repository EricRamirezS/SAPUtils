using System;
using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

namespace SAPUtils.Events {


    /// <summary>
    /// Provides centralized event handling for application-level events within the SAP Business One SDK.
    /// This static class defines events that can be subscribed to in order to respond to SAP Business One
    /// application-level triggers, including shutdown, company changes, and other application-level modifications.
    /// </summary>
    /// <remarks>
    /// The events exposed by this class correspond to high-level application events provided by the
    /// SAP Business One application.
    /// </remarks>
    /// <see cref="SAPUtils.SapAddon" />
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class AppEvents {


        /// <summary>
        /// Occurs when the SAP Business One application initiates a shutdown process.
        /// </summary>
        /// <remarks>
        /// Subscribing to this event can allow listeners to perform custom cleanup operations
        /// or save state before the application is terminated. It is triggered by the
        /// <c>BoAppEventTypes.aet_ShutDown</c> event type in the SAP Business One SDK.
        /// </remarks>
        /// <see cref="SAPUtils.SapAddon"/>
        /// <see cref="SAPUtils.Events.AppEvents.Handle(SAPbouiCOM.BoAppEventTypes)"/>
        public static event Action OnShutdown;


        /// <summary>
        /// An event that triggers when the company is changed in the SAP Business One application.
        /// </summary>
        /// <remarks>
        /// This event can be subscribed to in order to handle any logic or process that needs to occur
        /// when the current company in SAP Business One changes.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon" />
        public static event Action OnCompanyChanged;


        /// <summary>
        /// Raised when the application detects a change in the language settings within SAP Business One.
        /// </summary>
        /// <remarks>
        /// This event allows handling of changes in the application's language,
        /// which might be triggered when the user updates the language configuration.
        /// Subscribers to this event should account for potential UI or data-related updates
        /// necessary after a language change in the SAP Business One application.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon"/>
        public static event Action OnLanguageChanged;


        /// <summary>
        /// Occurs when the font settings in SAP Business One are changed, triggering the need for UI updates
        /// or font-dependent adjustments in the application.
        /// </summary>
        /// <remarks>
        /// This event is typically fired when the application's font configuration is modified, allowing
        /// subscribers to respond appropriately by adjusting their UI elements or other font-sensitive operations.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon" />
        public static event Action OnFontChanged;


        /// <summary>
        /// Event triggered when the SAP Business One server is terminated.
        /// </summary>
        /// <remarks>
        /// This event allows subscribers to execute custom logic during the termination of the SAP Business One server.
        /// It is part of the high-level application event handling provided by the <c>AppEvents</c> class.
        /// </remarks>
        /// <see cref="SAPUtils.Events.AppEvents" />
        /// <see cref="SAPUtils.SapAddon" />
        public static event Action OnServerTermination;


        /// <summary>
        /// Handles SAP application-level events and triggers the corresponding
        /// event actions based on the provided SAP Business One application event type.
        /// </summary>
        /// <param name="eventType">
        /// The type of the application event being handled. This is an enumeration of type
        /// <see cref="SAPbouiCOM.BoAppEventTypes"/> and represents SAP Business One application-specific events
        /// such as shutdown, company change, language change, etc.
        /// </param>
        /// <remarks>
        /// This method maps specific SAP Business One application events to corresponding actions by invoking
        /// the respective event delegates.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.BoAppEventTypes"/>
        /// <seealso cref="AppEvents.OnShutdown"/>
        /// <seealso cref="AppEvents.OnCompanyChanged"/>
        /// <seealso cref="AppEvents.OnLanguageChanged"/>
        /// <seealso cref="AppEvents.OnFontChanged"/>
        /// <seealso cref="AppEvents.OnServerTermination"/>
        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        internal static void Handle(BoAppEventTypes eventType) {
            switch (eventType) {
                case BoAppEventTypes.aet_ShutDown:
                    OnShutdown?.Invoke();
                    break;
                case BoAppEventTypes.aet_CompanyChanged:
                    OnCompanyChanged?.Invoke();
                    break;
                case BoAppEventTypes.aet_LanguageChanged:
                    OnLanguageChanged?.Invoke();
                    break;
                case BoAppEventTypes.aet_FontChanged:
                    OnFontChanged?.Invoke();
                    break;
                case BoAppEventTypes.aet_ServerTerminition:
                    OnServerTermination?.Invoke();
                    break;
            }
        }
    }
}