using System;
using System.Windows.Forms;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Gtk.Application.Quit();
        a.RetVal = true;
    }

    protected void OnButtonAboutClicked(object sender, EventArgs e)
    {
        MessageBox.Show("Taiko Difficulty Analyzer v1.1\n" +
                        "Created by dewero\n\n" +
                        "Check for new releases at https://github.com/NotMichaelChen/TDA\n" +
                        "Discuss program results at https://osu.ppy.sh/forum/t/485330",
                        "About"
                       );
    }
}
