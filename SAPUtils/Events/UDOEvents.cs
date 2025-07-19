using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;

#pragma warning disable CS1574, CS1584, CS1581, CS1580, CS1572 // XML comment has a param tag, but there is no parameter by that name

namespace SAPUtils.Events {
    /// <summary>
    /// Provides event handling for User-Defined Objects (UDO) in SAP Business One.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the handling of UDO-related events fired by SAP Business One applications.
    /// </remarks>
    /// <seealso cref="SAPUtils.SapAddon" />
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class UDOEvents {
        /// <summary>
        /// Delegate representing the handler for the UDO (User-Defined Object) before event.
        /// This delegate is invoked before the processing of a UDO event, allowing custom logic
        /// to manage event handling, including canceling the event by adjusting the <paramref name="bubbleEvent"/> parameter.
        /// </summary>
        /// <param name="udoEventArgs">A reference to the <see cref="UDOEvent"/> object containing information about the triggered UDO event.</param>
        /// <param name="bubbleEvent">An output parameter to determine whether the event should continue to bubble (true) or be stopped (false).</param>
        /// <remarks>
        /// Use this delegate in scenarios where pre-processing or validation of UDO events is required.
        /// </remarks>
        /// <seealso cref="SAPUtils.Events.UDOEvents.UdoBefore"/>
        public delegate void UdoBeforeHandler(ref UDOEvent udoEventArgs, out bool bubbleEvent);

        /// <summary>
        /// An event that is triggered before a User Defined Object (UDO) operation occurs.
        /// </summary>
        /// <remarks>
        /// This event provides an opportunity to intercept and possibly modify or cancel a UDO operation before it is fully processed.
        /// </remarks>
        /// <param name="udoEventArgs">
        /// A reference to the <see cref="UDOEvent"/> object containing details about the UDO operation.
        /// </param>
        /// <param name="bubbleEvent">
        /// An output parameter which determines whether the event should continue to bubble up.
        /// Set to <c>false</c> to stop further event propagation, or <c>true</c> to allow it.
        /// </param>
        /// <seealso cref="SAPUtils.Events.WidgetEvents.WidgetBeforeHandler"/>
        /// <seealso cref="SAPUtils.Events.UDOEvents"/>
        public static event UdoBeforeHandler UdoBefore;

        /// <summary>
        /// Handles a specified event by processing its arguments and controlling the event flow.
        /// </summary>
        /// <param name="udoEventArgs">A reference to the <see cref="UDOEvent"/> object containing event data.</param>
        /// <param name="bubbleEvent">An output parameter that determines whether the event should continue to bubble up.</param>
        /// <seealso cref="SAPbouiCOM.UDOEvent"/>
        internal static void Handle(ref UDOEvent udoEventArgs, out bool bubbleEvent) {
            bubbleEvent = true;
            UdoBefore?.Invoke(ref udoEventArgs, out bubbleEvent);
        }
    }
}