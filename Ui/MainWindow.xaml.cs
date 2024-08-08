using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Markup;

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


    /// <summary>
    /// A recursive function for creating a proper TreeView
    /// </summary>
    /// <param name="rootItem">Root item of the Tree (it is a .DLL name)</param>
    /// <param name="splitResName">Splitted resource name</param>
    /// <param name="xamlStream">Xaml stream to resource</param>
    private void CreateTreeView(TreeViewItem rootItem, string[] splitResName, Stream xamlStream)
    {
      string curResSegment = splitResName[0];

      // Be sure that the Item of the TreeView is exists or not
      var existingItem = rootItem.Items.OfType<TreeViewItem>().FirstOrDefault(i => i.Header.ToString() == curResSegment);

      if (existingItem == null) // Adding new item to TreeView
      {
        existingItem = new TreeViewItem
        {
          Header = curResSegment,
          Tag = splitResName.Length == 1 ? xamlStream : null
        };
        rootItem.Items.Add(existingItem);
      }

      if (splitResName.Length > 1) // Recursion start
      {
        CreateTreeView(existingItem, splitResName.Skip(1).ToArray(), xamlStream);
      }
    }

    private void TreeViewItem_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      this.WindowState = WindowState.Minimized;
      if (TreeViewControl.SelectedItem != null)
      {
        var selectedItem = (TreeViewItem)TreeViewControl.SelectedItem;
        var xamlStream = selectedItem.Tag as Stream;

        if (xamlStream == null)
          return;

        using (XmlReader xmlReader = XmlReader.Create(xamlStream))
        {
          // W.I.P. Here Need to rewrite xaml so that Window tag would be UserControl
          RecreatedDialog.Content = (Window)XamlReader.Load(xmlReader);
        }
      }
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
      TreeViewControl.Items.Clear();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new Microsoft.Win32.OpenFileDialog();
      //dialog.FileName = "Document";
      dialog.Multiselect = false;
      dialog.DefaultExt = ".dll";
      dialog.Filter = "Dynamic-Link Library (.dll)|*.dll" + "|All Files|*.*";

      bool? bRet = dialog.ShowDialog();

      if (bRet == true)
      {
        string fullFilePath = dialog.FileName;
        string fileName = Path.GetFileName(fullFilePath);

        var bamlShaman = new BamlShaman();
        var xamlSteamList = bamlShaman.ReadDll(fullFilePath);

        TreeViewControl.Items.Clear();
        var rootItem = new TreeViewItem { Header = fileName, Tag = null };
        foreach (var xamlListItem in xamlSteamList)
        {
          string[] splitResName = xamlListItem.resName.Split('/');
          CreateTreeView(rootItem, splitResName, xamlListItem.xamlStream);
        }
        TreeViewControl.Items.Add(rootItem);
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