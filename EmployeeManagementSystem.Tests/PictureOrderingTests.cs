using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EmployeeManagementSystem.Views;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// A file cannot take part in a database transaction, so EmployeeView orders the
    /// two instead: write the database first, copy the photo second.
    ///
    /// The invariant that makes the ordering safe is that working out *where* a photo
    /// will go must not touch the disk. When the copy happened first - which it used
    /// to - an update that changed no rows had already overwritten the old photo, and
    /// it was gone with nothing written to the database in exchange.
    ///
    /// These tests need no database.
    /// </summary>
    public class PictureOrderingTests
    {
        /// <summary>Runs on an STA thread, because the view is a WinForms control.</summary>
        private static void OnStaThread(Action action)
        {
            Exception failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Timed out.");

            if (failure != null)
            {
                throw new Xunit.Sdk.XunitException(failure.ToString());
            }
        }

        private static object Invoke(EmployeeView view, string method, params object[] args)
        {
            MethodInfo m = typeof(EmployeeView).GetMethod(
                method, BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.True(m != null, method + " no longer exists - this test needs updating.");
            return m.Invoke(view, args);
        }

        /// <summary>A folder this process is definitely allowed to write to.</summary>
        private static string NewTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "pic-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void SetImported(EmployeeView view, string path)
        {
            typeof(EmployeeView)
                .GetField("_importedPicturePath", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(view, path);
        }

        [Fact]
        public void Working_out_the_photo_path_does_not_touch_the_disk()
        {
            OnStaThread(() =>
            {
                string imported = Path.Combine(Path.GetTempPath(), "src-" + Guid.NewGuid().ToString("N") + ".jpg");
                File.WriteAllText(imported, "a photo");

                string dir = NewTempDir();
                string target = Path.Combine(dir, "ORDER-01.jpg");

                try
                {
                    using (var view = new EmployeeView())
                    {
                        view.PictureDirectory = dir;
                        SetImported(view, imported);

                        string planned = (string)Invoke(view, "PlannedPicturePath", "ORDER-01", null);

                        // It must say where the file WILL go...
                        Assert.Equal(Path.Combine("Directory", "ORDER-01.jpg"), planned);

                        // ...without having put anything there. This is what lets the
                        // database go first and still leave the old photo intact if the
                        // write turns out to change nothing.
                        Assert.False(File.Exists(target),
                            "PlannedPicturePath copied the file; the ordering guarantee is broken.");
                    }
                }
                finally
                {
                    try { File.Delete(imported); } catch (IOException) { }
                    try { Directory.Delete(dir, true); } catch (IOException) { }
                }
            });
        }

        [Fact]
        public void With_nothing_imported_the_existing_path_is_returned_unchanged()
        {
            OnStaThread(() =>
            {
                using (var view = new EmployeeView())
                {
                    SetImported(view, null);

                    Assert.Equal(@"Directory\EMID-01.jpg",
                        Invoke(view, "PlannedPicturePath", "EMID-01", @"Directory\EMID-01.jpg"));

                    // Adding a new employee passes null: nobody inherits a stray photo.
                    Assert.Null(Invoke(view, "PlannedPicturePath", "NEW-01", null));
                }
            });
        }

        [Fact]
        public void Committing_the_photo_writes_it_where_the_plan_said()
        {
            OnStaThread(() =>
            {
                string imported = Path.Combine(Path.GetTempPath(), "src-" + Guid.NewGuid().ToString("N") + ".jpg");
                File.WriteAllText(imported, "the new photo");

                string dir = NewTempDir();
                string target = Path.Combine(dir, "ORDER-02.jpg");

                try
                {
                    using (var view = new EmployeeView())
                    {
                        view.PictureDirectory = dir;
                        SetImported(view, imported);

                        string planned = (string)Invoke(view, "PlannedPicturePath", "ORDER-02", null);
                        bool stored = (bool)Invoke(view, "TryCommitPicture", "ORDER-02");

                        Assert.True(stored, "TryCommitPicture reported failure.");

                        Assert.True(File.Exists(target), "CommitPicture did not write the file.");
                        Assert.Equal("the new photo", File.ReadAllText(target));
                        Assert.Equal(Path.Combine("Directory", "ORDER-02.jpg"), planned);
                    }
                }
                finally
                {
                    try { File.Delete(imported); } catch (IOException) { }
                    try { Directory.Delete(dir, true); } catch (IOException) { }
                }
            });
        }

        [Fact]
        public void Committing_with_nothing_imported_leaves_the_existing_photo_alone()
        {
            OnStaThread(() =>
            {
                string dir = NewTempDir();
                string target = Path.Combine(dir, "ORDER-03.jpg");
                File.WriteAllText(target, "the original photo");

                try
                {
                    using (var view = new EmployeeView())
                    {
                        view.PictureDirectory = dir;
                        SetImported(view, null);

                        // Nothing imported is success: there was nothing to store.
                        Assert.True((bool)Invoke(view, "TryCommitPicture", "ORDER-03"));

                        Assert.Equal("the original photo", File.ReadAllText(target));
                    }
                }
                finally
                {
                    try { Directory.Delete(dir, true); } catch (IOException) { }
                }
            });
        }
    }
}
