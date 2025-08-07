using SAPbouiCOM;
using Company = SAPbobsCOM.Company;

namespace SAPUtils.Utils {
    public class SapClass {
        public static Company Company => SapAddon.Instance().Company;
        public static Application Application => SapAddon.Instance().Application;
        public static SAPbouiCOM.Framework.Application MainApplication => SapAddon.Instance().MainApplication;
        public static ILogger Log => SapAddon.Instance().Logger;
    }
}