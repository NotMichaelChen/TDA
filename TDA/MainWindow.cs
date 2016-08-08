using System;
using System.Windows.Forms;
using Gtk;

using PerformanceProcessor;

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

    protected void OnButtonGetFileClicked(object sender, EventArgs e)
    {
        OpenFileDialog fileselector = new OpenFileDialog();

        //Initial Directory is directory of exe
        fileselector.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        fileselector.Filter = ".osu files (*.osu)|*.osu|All files (*.*)|*.*";

        if(fileselector.ShowDialog() == DialogResult.OK)
        {
            entryFilePath.Text = fileselector.FileName;
        }
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

    protected void OnButtonCalculateClicked(object sender, EventArgs e)
    {
        try
        {
            string filepath = entryFilePath.Text;
            string amount100 = entryHundreds.Text;
            string amountmiss = entryMisses.Text;
            string maxcombo = entryMaxCombo.Text;

            Modifiers usedmods = Modifiers.None;
            if(checkbuttonEasy.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.Easy);
            if(checkbuttonNoFail.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.NoFail);
            if(checkbuttonHalfTime.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.HalfTime);
            if(checkbuttonHardRock.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.HardRock);
            if(checkbuttonDoubleTime.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.DoubleTime);
            if(checkbuttonHidden.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.Hidden);
            if(checkbuttonFlashlight.Active)
                usedmods = (Modifiers)((int)usedmods | (int)Modifiers.Flashlight);
        }
        catch(Exception ex)
        {
            textviewOutput.Buffer.Text = ex.Message;
        }
    }
}
