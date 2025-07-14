using System.Threading;
using System.Windows.Forms;
using SAPUtils.Internal.Utils;

namespace SAPUtils.Utils
{
    public static class TopMostDialog
    {
        public static DialogResult ShowDialog(CommonDialog dialog)
        {
            DialogResult dialogResult = DialogResult.None;
            Thread t = new Thread(() =>
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

                using (Form f = new DummyForm())
                {
                    f.Show();
                    dialogResult = dialog.ShowDialog();
                }
            });

            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();
            return dialogResult;
        }
    }
}