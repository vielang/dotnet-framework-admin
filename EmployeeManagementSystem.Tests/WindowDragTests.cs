using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EmployeeManagementSystem.Forms;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Guards the window-drag wiring on borderless forms.
    ///
    /// This exists because of a real bug that a property check would have missed.
    /// AppForm.WndProc upgrades WM_NCHITTEST from HTCLIENT to HTCAPTION, which makes
    /// bare form background draggable - but MainForm's three panels tile the client
    /// area exactly, so there is no bare background and the form never sees the
    /// message. MainForm could not be moved at all, while DragByBackground happily
    /// reported true.
    ///
    /// So these tests assert the thing that actually moves the window: whether the
    /// control under the cursor has a MouseDown subscriber declared on AppForm. They
    /// also assert the reverse - that controls with their own click behaviour were
    /// left alone.
    ///
    /// Forms are only constructed, never shown, because MakeDraggable runs in the
    /// constructor. That keeps these tests free of the database and of AppServices.
    /// </summary>
    public class WindowDragTests
    {
        [Fact]
        public void MainForm_header_and_menu_can_drag_the_window()
        {
            RunOnStaThread(() =>
            {
                using (var main = new MainForm())
                {
                    Control header = Find(main, "panel1");
                    Control menu = Find(main, "panel2");

                    Assert.True(IsDragHandle(header), "panel1 (the header strip) cannot drag the window.");
                    Assert.True(IsDragHandle(Find(header, "label2")), "The title label in the header cannot drag the window.");
                    Assert.True(IsDragHandle(menu), "panel2 (the left menu) cannot drag the window.");
                }
            });
        }

        [Fact]
        public void MainForm_interactive_controls_keep_their_own_click()
        {
            RunOnStaThread(() =>
            {
                using (var main = new MainForm())
                {
                    // The purple X is a Label, so it looks passive but closes the window.
                    Assert.False(IsDragHandle(Find(Find(main, "panel1"), "exit")),
                        "The close label was turned into a drag handle.");
                    Assert.False(IsDragHandle(Find(main, "dashboard_btn")), "A menu button was turned into a drag handle.");
                    Assert.False(IsDragHandle(Find(main, "dataGridView1")), "The grid was turned into a drag handle.");
                }
            });
        }

        /// <summary>
        /// The reason MakeDraggable has to exist at all. The three panels leave only a
        /// one-pixel strip of bare background, so the WM_NCHITTEST override in AppForm
        /// has nowhere to work on this form. If a later layout change frees up a real
        /// area of background, this test fails and the comment above WndProc needs
        /// revisiting.
        /// </summary>
        [Fact]
        public void MainForm_panels_leave_no_usable_bare_background()
        {
            RunOnStaThread(() =>
            {
                using (var main = new MainForm())
                using (var bare = new Region(main.ClientRectangle))
                {
                    foreach (Control c in main.Controls)
                    {
                        if (c is Panel) bare.Exclude(c.Bounds);
                    }

                    RectangleF[] left = bare.GetRegionScans(new Matrix());

                    // A strip a pixel or two thick is not something anyone can grab.
                    RectangleF[] grabbable = left.Where(r => r.Width > 2 && r.Height > 2).ToArray();

                    Assert.True(grabbable.Length == 0,
                        "There is bare form background at "
                        + string.Join(", ", grabbable.Select(r => r.ToString()).ToArray())
                        + ". WndProc could drag the window there - but MakeDraggable is what "
                        + "moves this window today, and the docs say the panels cover it.");
                }
            });
        }

        [Fact]
        public void LoginForm_background_drags_but_its_links_do_not()
        {
            RunOnStaThread(() =>
            {
                using (var login = new LoginForm())
                {
                    Assert.True(IsDragHandle(Find(login, "panel2")), "The login panel cannot drag the window.");
                    Assert.False(IsDragHandle(Find(login, "label5")), "The \"Sign up\" link was turned into a drag handle.");
                    Assert.False(IsDragHandle(Find(login, "exit")), "The close label was turned into a drag handle.");
                    Assert.False(IsDragHandle(Find(login, "login_username")), "A text box was turned into a drag handle.");
                }
            });
        }

        [Fact]
        public void RegisterForm_background_drags_but_its_link_does_not()
        {
            RunOnStaThread(() =>
            {
                using (var register = new RegisterForm())
                {
                    Assert.True(IsDragHandle(Find(register, "panel2")), "The register panel cannot drag the window.");
                    Assert.False(IsDragHandle(Find(register, "label5")), "The \"Login\" link was turned into a drag handle.");
                }
            });
        }

        /// <summary>
        /// True when this control has a MouseDown subscriber whose method is declared on
        /// AppForm - which is BeginDrag. There is no public way to enumerate a control's
        /// subscribers, so this reads the same EventHandlerList WinForms itself uses.
        /// </summary>
        private static bool IsDragHandle(Control control)
        {
            FieldInfo key = typeof(Control).GetField("EventMouseDown",
                BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo events = typeof(Component).GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.True(key != null, "Control.EventMouseDown is gone - this test needs rewriting.");
            Assert.True(events != null, "Component.Events is gone - this test needs rewriting.");

            var list = events.GetValue(control, null) as EventHandlerList;
            if (list == null) return false;

            var handler = list[key.GetValue(null)] as Delegate;
            if (handler == null) return false;

            return handler.GetInvocationList().Any(d =>
                d.Method.DeclaringType != null
                && typeof(AppForm).IsAssignableFrom(d.Method.DeclaringType));
        }

        /// <summary>
        /// Searches one container only. Control names repeat across the three views, so
        /// a search from the form finds several matches for names like "label2".
        /// </summary>
        private static Control Find(Control host, string name)
        {
            Control[] hits = host.Controls.Find(name, true);
            Assert.True(hits.Length > 0, "No control named " + name + " under " + host.Name + ".");
            return hits[0];
        }

        /// <summary>WinForms needs an STA thread; failures are rethrown on the caller's.</summary>
        private static void RunOnStaThread(Action action)
        {
            Exception fatal = null;

            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { fatal = ex; }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(60)),
                "Timed out - a modal dialog is probably blocking, which is itself the bug.");

            if (fatal != null)
            {
                throw new Xunit.Sdk.XunitException(fatal.ToString());
            }
        }
    }
}
