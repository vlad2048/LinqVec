using Jot;

namespace UILib.Utils;

static class TrackerSetup
{
    public static void Init(Tracker tracker)
    {
        // tell the tracker how to track Form objects (this goes in a startup class)
        tracker.Configure<Form>()
            // include the screen resolution in the id
            .Id(e => e.Name, SystemInformation.VirtualScreen.Size)
            .Properties(e => new
            {
                e.Left,
                e.Top,
                e.Width,
                e.Height,
                e.WindowState
            })
            .PersistOn(nameof(Form.Move), nameof(Form.Resize), nameof(Form.FormClosing))
            // do not track form size and location when minimized/maximized
            .WhenPersistingProperty(
                (f, p) =>
                    p.Cancel =
                        f.WindowState != FormWindowState.Normal &&
                        (
                            p.Property == nameof(Form.Height) ||
                            p.Property == nameof(Form.Width) ||
                            p.Property == nameof(Form.Top) ||
                            p.Property == nameof(Form.Left)
                        )
            )
            // a form should not be persisted after it is closed since properties will be empty
            .StopTrackingOn(nameof(Form.FormClosing));
    }
}