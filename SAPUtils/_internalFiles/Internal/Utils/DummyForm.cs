using System.Drawing;
using System.Windows.Forms;

namespace SAPUtils.Internal.Utils
{
    internal partial class DummyForm : Form
    {
        public DummyForm()
        {
            TopMost = true;
            TopLevel = true;
            Icon = new Icon(HitchIconClass.HitchIcon);
            Opacity = 0;
            AllowTransparency = true;
            ShowIcon = true;
            WindowState = FormWindowState.Maximized;
            Focus();
        }
    }
}