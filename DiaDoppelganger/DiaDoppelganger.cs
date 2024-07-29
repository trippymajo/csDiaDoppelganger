using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

using BamlReader;

internal class DiaDoppelganger
{
  [STAThread]
  static void Main()
  {
    // Path to the DLL containing the WPF window XAML
    string strDllPath = @"C:\Users\Admin\Desktop\pseudo_seam\bin\Debug\net8.0-windows\PseudoSteam.dll";
    string strXamlPath = @"C:\Users\Admin\Desktop\MainWindow.xaml";
    string strOutputPath = @"C:\Users\Admin\Desktop\Baml.txt";

    BamlReader.BamlShaman bamlS = new BamlReader.BamlShaman();
    var streamXaml = bamlS.ReadDll(strDllPath, BamlShaman.SaveMode.Xaml);

    streamXaml.Seek(0, SeekOrigin.Begin);

    using (XmlReader xmlReader = XmlReader.Create(streamXaml))
    {
      Window dynamicDialog = (Window)XamlReader.Load(xmlReader);
      // Start the WPF application
      Application app = new Application();
      app.Run(dynamicDialog);
    }
  }
}
