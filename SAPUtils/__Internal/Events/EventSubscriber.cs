// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable InconsistentNaming

using SAPUtils.Events;

#pragma warning disable CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.__Internal.Events {
    /// <summary>
    /// Provides functionality to subscribe to various SAP application events.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the subscription logic for multiple event types exposed by the SAPbouiCOM.Application object.
    /// </remarks>
    /// <see cref="SAPUtils.SapAddon"/>
    internal static class EventSubscriber {
        /// <summary>
        /// Subscribes the application to various SAP Business One-specific events.
        /// This method attaches event handlers for numerous event types, enabling custom handling and processing of these events.
        /// </summary>
        /// <remarks>
        /// The method ensures that the singleton instance of <see cref="SapAddon"/> is used to access the application object
        /// and hook into the event system. It includes event subscriptions for form data, menu clicks, layout key interactions,
        /// progress bar updates, and more.
        /// </remarks>
        /// <seealso cref="SAPUtils.SapAddon" />
        /// <seealso cref="SAPbouiCOM.Application" />
        internal static void Subscribe() {
            SapAddon.__application.FormDataEvent += FormDataEvents.Handle;
            SapAddon.__application.AppEvent += AppEvents.Handle;
            SapAddon.__application.ItemEvent += ItemEvents.Handle;
            SapAddon.__application.LayoutKeyEvent += LayoutKeyEvents.Handle;
            SapAddon.__application.MenuEvent += MenuEvents.Handle;
            SapAddon.__application.PrintEvent += PrintEvents.Handle;
            SapAddon.__application.ProgressBarEvent += ProgressBarEvents.Handle;
            SapAddon.__application.ReportDataEvent += ReportDataEvents.Handle;
            SapAddon.__application.RightClickEvent += RightClickEvents.Handle;
            SapAddon.__application.ServerInvokeCompletedEvent += ServerInvokeCompletedEvents.Handle;
            SapAddon.__application.StatusBarEvent += StatusBarEvents.Handle;
            SapAddon.__application.UDOEvent += UDOEvents.Handle;
            SapAddon.__application.WidgetEvent += WidgetEvents.Handle;
        }
    }

}