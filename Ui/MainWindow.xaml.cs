using System.IO;
using System.Windows;

using BamlReader;

namespace Ui
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    
    public MainWindow()
    {
      InitializeComponent();
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new Microsoft.Win32.OpenFileDialog();
      //dialog.FileName = "Document";
      dialog.DefaultExt = ".dll";
      dialog.Filter = "Dynamic-Link Library (.dll)|*.dll" + "|All Files|*.*";

      bool? bRet = dialog.ShowDialog();

      if (bRet == true)
      {
        string strFullFilePath = dialog.FileName;

        var BamlShaman = new BamlShaman();
        var BamlStream = BamlShaman._outPath;
      }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {

    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}