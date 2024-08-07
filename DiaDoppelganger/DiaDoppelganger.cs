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

    BamlShaman bamlS = new BamlShaman();
    var xamlList = bamlS.ReadDll(strDllPath, BamlShaman.SaveMode.Xaml);

    Console.WriteLine("Which window to show?\n");
    for (int i = 0; i < xamlList.Count; ++i)
    {
      Console.WriteLine($"#{i+1} Dialog Name: {xamlList[i].resName}");
    }

    int numberInput = -1;
    while (numberInput < 0 || numberInput > xamlList.Count)
      numberInput = Convert.ToInt32(Console.ReadLine());

    var xamlStream = xamlList[numberInput - 1].xamlStream;
    xamlStream.Seek(0, SeekOrigin.Begin);

    using (XmlReader xmlReader = XmlReader.Create(xamlStream))
    {
      Window dynamicDialog = (Window)XamlReader.Load(xmlReader);
      // Start the WPF application
      Application app = new Application();
      app.Run(dynamicDialog);
    }
  }
}
