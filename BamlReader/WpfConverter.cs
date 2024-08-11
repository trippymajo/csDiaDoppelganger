using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WpfConverter
{
  public static class WpfConverter
  {
    private static Stack<Dictionary<string, string>> windowProps = new Stack<Dictionary<string, string>>();

    /// <summary>
    /// Will change all the UserControl tags to Window in order to
    /// use it as a control in WPF app
    /// </summary>
    /// <param name="xamlStream">[OUT] stream to read and change</param>
    public static Stream UserControl2Window(Stream xamlStream)
    {
      if (xamlStream == null)
        return null;

      XDocument xdoc;
      using (var reader = new StreamReader(xamlStream))
      {
        xdoc = XDocument.Load(reader);
      }

      XElement rootElement = xdoc.Root;

      // Change the root element to Window
      var rootNamespace = rootElement.Name.Namespace;
      rootElement.Name = rootNamespace + "Window";

      // Window.Resources handling
      var resourcesElement = rootElement.Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}UserControl.Resources");
      if (resourcesElement != null)
        resourcesElement.Name = "{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Window.Resources";

      // Add previously stored Window attributes
      foreach (var prop in windowProps.Peek())
      {
        rootElement.SetAttributeValue(prop.Key, prop.Value);
      }

      // Clear the stored properties after using them
      windowProps.Pop();

      return XDoc2Stream(xdoc);
    }

    /// <summary>
    /// Converts XDocument to Stream
    /// </summary>
    /// <param name="xdoc">XDocument to convert</param>
    /// <returns>New MemoryStream to XDocument</returns>
    private static Stream XDoc2Stream(XDocument xdoc)
    {
      var memoryStream = new MemoryStream();
      using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
      {
        xdoc.Save(writer);
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
      }
      return memoryStream;
    }

    /// <summary>
    /// Is attribute is a window specific?
    /// </summary>
    /// <param name="attributeName">Attribute of the root element</param>
    /// <returns>
    /// True - Windows specific
    /// <br/>
    /// False - Non window specific
    /// </returns>
    private static bool IsWindowSpecificAttribute(string attributeName)
    {
      return attributeName == "Title" ||
             attributeName == "Height" ||
             attributeName == "Width" ||
             attributeName == "MaxHeight" ||
             attributeName == "MaxWidth" ||
             attributeName == "WindowStartupLocation" ||
             attributeName == "WindowStyle";
    }

    /// <summary>
    /// Will change all the Window tags to UserControl in order to
    /// use it as a control in WPF app
    /// </summary>
    /// <param name="xamlStream">[OUT] stream to read and change</param>
    public static Stream Window2UserControl(Stream xamlStream)
    {
      windowProps.Clear();

      if (xamlStream == null)
        return null;

      XDocument xdoc;
      using (var reader = new StreamReader(xamlStream))
      {
        xdoc = XDocument.Load(reader);
      }
      
      XElement rootElement = xdoc.Root;

      // Change root tag
      // TODO: Root tags may be different from Window. So you need
      // to store it anyways.
      var rootNamespace = rootElement.Name.Namespace;
      rootElement.Name = rootNamespace + "UserControl";

      // Window.Resources handling
      var resourcesElement = rootElement.Element("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Window.Resources");
      if (resourcesElement != null)
        resourcesElement.Name = "{http://schemas.microsoft.com/winfx/2006/xaml/presentation}UserControl.Resources";

      var attsToRemove = new List<XAttribute>();

      // Read attributes and save them in order to recreate original Xaml when saving
      foreach (var att in rootElement.Attributes())
      {
        if (IsWindowSpecificAttribute(att.Name.LocalName))
        {
          Dictionary <string, string> curWindowProps = new Dictionary<string, string>();
          curWindowProps[att.Name.LocalName] = att.Value;
          windowProps.Push(curWindowProps);

          attsToRemove.Add(att);
        }
      }

      foreach (var att in attsToRemove)
      {
        att.Remove();
      }

      return XDoc2Stream(xdoc);
    }
  }
}
