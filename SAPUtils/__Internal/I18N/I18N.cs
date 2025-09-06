using System.Globalization;
using SAPbobsCOM;

namespace SAPUtils.__Internal.I18N {
    internal static class L10N {
        public static CultureInfo GetCulture(BoSuppLangs lang) {
            switch (lang) {
                case BoSuppLangs.ln_Spanish:
                case BoSuppLangs.ln_Spanish_Ar:
                case BoSuppLangs.ln_Spanish_Pa:
                case BoSuppLangs.ln_Spanish_La:
                    return new CultureInfo("es");
                case BoSuppLangs.ln_Portuguese_Br:
                    return new CultureInfo("pt-BR");
                case BoSuppLangs.ln_Portuguese:
                    return new CultureInfo("pt-PT");
                case BoSuppLangs.ln_French:
                    return new CultureInfo("fr");
                case BoSuppLangs.ln_German:
                    return new CultureInfo("de");
                case BoSuppLangs.ln_Italian:
                    return new CultureInfo("it");
                case BoSuppLangs.ln_Russian:
                    return new CultureInfo("ru");
                case BoSuppLangs.ln_English:
                case BoSuppLangs.ln_English_Gb:
                case BoSuppLangs.ln_English_Sg:
                case BoSuppLangs.ln_English_Cy:
                    return new CultureInfo("en");
                case BoSuppLangs.ln_Null:
                case BoSuppLangs.ln_Hebrew:
                case BoSuppLangs.ln_Polish:
                case BoSuppLangs.ln_Serbian:
                case BoSuppLangs.ln_Danish:
                case BoSuppLangs.ln_Norwegian:
                case BoSuppLangs.ln_Hungarian:
                case BoSuppLangs.ln_Chinese:
                case BoSuppLangs.ln_Dutch:
                case BoSuppLangs.ln_Finnish:
                case BoSuppLangs.ln_Greek:
                case BoSuppLangs.ln_Swedish:
                case BoSuppLangs.ln_Czech_Cz:
                case BoSuppLangs.ln_Slovak_Sk:
                case BoSuppLangs.ln_Korean_Kr:
                case BoSuppLangs.ln_Japanese_Jp:
                case BoSuppLangs.ln_Turkish_Tr:
                case BoSuppLangs.ln_Arabic:
                case BoSuppLangs.ln_Ukrainian:
                case BoSuppLangs.ln_TrdtnlChinese_Hk:
                default:
                    return new CultureInfo("en"); // fallback
            }
        }
    }
}