using System;
using Gtk;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.Init();
        MainWindow win = new MainWindow();
        win.Show();
        Application.Run();
    }
}
