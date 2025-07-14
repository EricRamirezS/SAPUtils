using System.Reflection;
using SAPbouiCOM.Framework;

namespace SAPUtils.Forms
{
    public abstract partial class UserForm
    {
        internal string FormResource
        {
            get
            {
                PropertyInfo propertyInfo =
                    typeof(FormBase).GetProperty("FormResource", BindingFlags.NonPublic | BindingFlags.Instance);
                string formResource = "";
                if (propertyInfo != null)
                {
                    formResource = (string)propertyInfo.GetValue(this);
                }

                return formResource;
            }
        }

        internal bool InitializedSetter
        {
            set
            {
                PropertyInfo propertyInfo =
                    typeof(FormBase).GetProperty("Initialized", BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this, value);
                }
            }
        }
    }
}