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
            SapAddon.Instance().Application.FormDataEvent += FormDataEvents.Handle;
            SapAddon.Instance().Application.AppEvent += AppEvents.Handle;
            SapAddon.Instance().Application.ItemEvent += ItemEvents.Handle;
            SapAddon.Instance().Application.LayoutKeyEvent += LayoutKeyEvents.Handle;
            SapAddon.Instance().Application.MenuEvent += MenuEvents.Handle;
            SapAddon.Instance().Application.PrintEvent += PrintEvents.Handle;
            SapAddon.Instance().Application.ProgressBarEvent += ProgressBarEvents.Handle;
            SapAddon.Instance().Application.ReportDataEvent += ReportDataEvents.Handle;
            SapAddon.Instance().Application.RightClickEvent += RightClickEvents.Handle;
            SapAddon.Instance().Application.ServerInvokeCompletedEvent += ServerInvokeCompletedEvents.Handle;
            SapAddon.Instance().Application.StatusBarEvent += StatusBarEvents.Handle;
            SapAddon.Instance().Application.UDOEvent += UDOEvents.Handle;
            SapAddon.Instance().Application.WidgetEvent += WidgetEvents.Handle;
        }
    }

}