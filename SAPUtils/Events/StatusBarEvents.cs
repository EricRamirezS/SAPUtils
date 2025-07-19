using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides event handlers for SAP StatusBar messages based on different message types.
    /// Triggers events when messages are received in the StatusBar of a SAP UI application.
    /// </summary>
    /// <remarks>
    /// This class captures and categorizes StatusBar events based on SAP's `BoStatusBarMessageType`
    /// and invokes the corresponding event handler.
    /// </remarks>
    /// <seealso cref="SAPbouiCOM.BoStatusBarMessageType"/>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public static class StatusBarEvents {
        /// Represents a handler that manages status bar messages in the application.
        /// This handler dispatches messages to the appropriate event based on the
        /// `BoStatusBarMessageType`.
        /// See Also:
        /// <see cref="SAPUtils.Events.StatusBarEvents"/>
        public delegate void StatusBarMessageHandler(string text);

        /// <summary>
        /// An event handler triggered when a message of type `smt_None` is raised in the Status Bar.
        /// </summary>
        /// <remarks>
        /// This event is a part of the `StatusBarEvents` class and allows users to handle
        /// events categorized as "None" type messages in the context of SAP Business One.
        /// </remarks>
        /// <seealso cref="SAPUtils.Events.StatusBarEvents" />
        public static event StatusBarMessageHandler None;

        /// <summary>
        /// Event triggered when a warning message is displayed in the status bar.
        /// </summary>
        /// <remarks>
        /// The <c>Warning</c> event corresponds to warning-type messages displayed in the application's status bar,
        /// allowing developers to handle these messages programmatically.
        /// </remarks>
        /// <param name="text">The warning message text displayed in the status bar.</param>
        /// <seealso cref="SAPUtils.Events.StatusBarEvents.None"/>
        /// <seealso cref="SAPUtils.Events.StatusBarEvents.Error"/>
        /// <seealso cref="SAPUtils.Events.StatusBarEvents.Success"/>
        public static event StatusBarMessageHandler Warning;

        /// <summary>
        /// Represents an event triggered for error messages in the SAP status bar.
        /// </summary>
        /// <remarks>
        /// This event is invoked when an error message type (BoStatusBarMessageType.smt_Error) is displayed
        /// in the SAP Business One status bar.
        /// </remarks>
        /// <param name="text">The error message text to be displayed.</param>
        /// <seealso cref="SAPbouiCOM.BoStatusBarMessageType" />
        /// <seealso cref="SAPUtils.Events.StatusBarEvents" />
        public static event StatusBarMessageHandler Error;

        /// <summary>
        /// Event invoked when a success message is pushed to the status bar.
        /// </summary>
        /// <param name="text">The text message displayed on the status bar when the event is triggered.</param>
        /// <remarks>
        /// This event allows custom handling of success messages that are displayed
        /// in the status bar of the SAP Business One application.
        /// </remarks>
        /// <see cref="SAPbouiCOM.BoStatusBarMessageType"/>
        public static event StatusBarMessageHandler Success;

        /// <summary>
        /// Handles the specified event based on the given text and message type and raises the appropriate status bar message event.
        /// </summary>
        /// <param name="text">The message text to be displayed on the status bar.</param>
        /// <param name="messageType">The type of the status bar message, indicating the message severity or status.</param>
        /// <seealso cref="SAPbouiCOM.BoStatusBarMessageType"/>
        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        internal static void Handle(string text, BoStatusBarMessageType messageType) {
            switch (messageType) {
                case BoStatusBarMessageType.smt_None:
                    None?.Invoke(text);
                    break;
                case BoStatusBarMessageType.smt_Warning:
                    Warning?.Invoke(text);
                    break;
                case BoStatusBarMessageType.smt_Error:
                    Error?.Invoke(text);
                    break;
                case BoStatusBarMessageType.smt_Success:
                    Success?.Invoke(text);
                    break;
            }
        }
    }
}