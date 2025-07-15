using System.Threading;
using System.Windows.Forms;
using SAPUtils.__Internal.Utils;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides functionality to show modal dialog windows as top-most dialogs.
    /// </summary>
    public static class TopMostDialog {
        /// Displays a common dialog box as a top-most dialog ensuring it stays above all other windows.
        /// <param name="dialog">The common dialog object to be displayed.</param>
        /// <returns>The result of the dialog box, indicating the user's action.</returns>
        public static DialogResult ShowDialog(CommonDialog dialog) {
            DialogResult dialogResult = DialogResult.None;
            Thread t = new Thread(() =>
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

                using (Form f = new DummyForm()) {
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