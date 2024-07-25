using System;
using System.Windows;

using BamlReader;

internal class DiaDoppelganger
{
  [STAThread]
  static void Main()
  {
    // Path to the DLL containing the WPF window XAML
    string dllPath = @"C:\Users\Admin\Desktop\pseudo_seam\bin\Debug\net8.0-windows\PseudoSteam.dll";
    string xamlPath = @"C:\Users\Admin\Desktop\MainWindow.xaml";
    string outputPath = @"C:\Users\Admin\Desktop\Baml.txt";

    BamlReader.BamlShaman bamlS = new BamlReader.BamlShaman();
    bamlS.ReadDll(dllPath, outputPath);
    // Start the WPF application
    Application app = new Application();
    app.Run();
  }
}
