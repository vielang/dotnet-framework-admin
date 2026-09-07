using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EmployeeManagementSystem.Forms
{
    /// <summary>
    /// Base class for this application's windows.
    ///
    /// Every form here uses <see cref="FormBorderStyle.None"/> to get the custom purple
    /// chrome. That also removes the title bar - and the title bar is what Windows uses
    /// to move a window. Without the code below, none of these windows could be moved
    /// at all: they open centred and stay there, even if they cover something the user
    /// needs to see.
    ///
    /// Two techniques are needed, because they cover different areas.
    ///
    /// The first is one message. Windows asks a window "what is under this point?" by
    /// sending WM_NCHITTEST; the answer decides what a drag there does. Letting the
    /// base class answer first and then upgrading "client area" to "title bar" makes
    /// the bare form background draggable - see WndProc below.
    ///
    /// That alone is not enough. Child controls are separate windows and answer their
    /// own WM_NCHITTEST, so wherever a control sits the form is never asked. MainForm's
    /// three panels leave only a one-pixel strip of background, which nobody can hit.
    /// MakeDraggable covers that case by handing the drag back to Windows from the
    /// control itself.
    /// </summary>
    public class AppForm : Form
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCLIENT = 1;    // "the point is over the client area"
        private const int HTCAPTION = 2;   // "the point is over the title bar"

        /// <summary>Stops the control that was clicked from swallowing the rest of the drag.</summary>
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public AppForm()
        {
            // Dragging by the background would otherwise let a double-click maximise
            // the window, which a fixed-size borderless layout is not designed for.
            MaximizeBox = false;
        }

        /// <summary>Lets the window be dragged by its background. Set false to switch off.</summary>
        public bool DragByBackground { get; set; }

        /// <summary>
        /// Closes the window when Escape is pressed.
        ///
        /// Normally a form gets this from <see cref="Form.CancelButton"/>, but that
        /// property needs an <see cref="IButtonControl"/> and the purple X in the corner
        /// of these forms is a Label with a Click handler, not a Button. A Label cannot
        /// take focus either, so that X is unreachable from the keyboard - this at least
        /// gives the window an Escape route.
        /// </summary>
        public bool EscapeClosesForm { get; set; }

        /// <summary>
        /// Runs before the key reaches any control, which is what makes it a *command*
        /// key rather than ordinary input. Returning true means "handled, stop here".
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (EscapeClosesForm && keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (DragByBackground
                && m.Msg == WM_NCHITTEST
                && m.Result == (IntPtr)HTCLIENT)
            {
                m.Result = (IntPtr)HTCAPTION;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Enabled here rather than in the constructor so the Visual Studio designer,
            // which hosts the form as a child control, is never affected.
            if (!DesignMode)
            {
                DragByBackground = true;
            }
        }

        /// <summary>
        /// Lets the window be dragged by <paramref name="root"/> and its passive
        /// children - panels, labels and picture boxes.
        ///
        /// <see cref="WndProc"/> alone is not enough. It only answers for bare form
        /// background, and every control is a window of its own that answers its own
        /// WM_NCHITTEST. On MainForm the three panels leave only a one-pixel strip of
        /// background, so the window could not be moved at all.
        ///
        /// The trick is to hand the drag back to Windows: release the mouse capture the
        /// clicked control has just taken, then tell the *form* that a press happened on
        /// its title bar. Windows then runs its own move loop, which is why this feels
        /// native - snapping, Aero shake and multi-monitor all keep working.
        /// </summary>
        /// <param name="except">
        /// Controls that must keep their own click behaviour. The purple X is a Label,
        /// so it looks passive but is really a button.
        /// </param>
        protected void MakeDraggable(Control root, params Control[] except)
        {
            if (root == null)
            {
                return;
            }

            var excluded = new System.Collections.Generic.HashSet<Control>(
                except ?? new Control[0]);

            Attach(root, excluded);
        }

        private void Attach(Control control, System.Collections.Generic.HashSet<Control> excluded)
        {
            if (excluded.Contains(control))
            {
                return;     // and do not walk into it either
            }

            // Only controls that do nothing on a click. Buttons, text boxes and the grid
            // are left alone so they keep working.
            if (control is Panel || control is Label || control is PictureBox)
            {
                control.MouseDown -= BeginDrag;     // idempotent: safe to call twice
                control.MouseDown += BeginDrag;
            }

            foreach (Control child in control.Controls)
            {
                Attach(child, excluded);
            }
        }

        private void BeginDrag(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }
    }
}
