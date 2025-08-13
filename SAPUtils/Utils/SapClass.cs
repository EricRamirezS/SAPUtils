using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;
using Company = SAPbobsCOM.Company;

namespace SAPUtils.Utils {

    /// <summary>
    /// Provides global static access to commonly used SAP objects such as 
    /// <see cref="Company"/>, <see cref="Application"/>, and <see cref="Logger"/>.
    /// <para>
    /// Recommended usage:
    /// <code>
    /// using static SAPUtils.Utils.SapClass;
    /// 
    /// // Then access members directly:
    /// log.Info("Connected to SAP");
    /// </code>
    /// </para>
    /// </summary>
    /// <remarks>
    /// This class is intended to simplify access to the SAP Business One COM objects
    /// and the application logger throughout the entire project.
    /// </remarks>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class SapClass {
        /// <summary>
        /// Gets the current <see cref="SAPbobsCOM.Company"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static Company Company => SapAddon.Instance().Company;

        /// <summary>
        /// Gets the current <see cref="SAPbobsCOM.Company"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static Company company => Company;

        /// <summary>
        /// Gets the current <see cref="SAPbouiCOM.Application"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static Application Application => SapAddon.Instance().Application;

        /// <summary>
        /// Gets the current <see cref="SAPbouiCOM.Application"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static Application App => Application;

        /// <summary>
        /// Gets the current <see cref="SAPbouiCOM.Application"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static Application app => Application;

        /// <summary>
        /// Gets the current <see cref="SAPbouiCOM.Application"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static Application application => Application;

        /// <summary>
        /// Gets the current <see cref="ILogger"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static ILogger Logger => SapAddon.Instance().Logger;

        /// <summary>
        /// Gets the current <see cref="ILogger"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static ILogger Log => Logger;

        /// <summary>
        /// Gets the current <see cref="ILogger"/> instance from <see cref="SapAddon"/>.
        /// </summary>
        public static ILogger log => Logger;
    }
}