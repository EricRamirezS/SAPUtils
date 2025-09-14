using System.Drawing;
using System.Windows.Forms;

namespace SAPUtils.__Internal.Utils {
    internal class DummyForm : Form {
        public DummyForm(Icon icon = null) {
            TopMost = true;
            TopLevel = true;
            if (icon != null) Icon = icon;
            Opacity = 0;
            AllowTransparency = true;
            ShowIcon = true;
            WindowState = FormWindowState.Maximized;
            Focus();
        }
    }
}