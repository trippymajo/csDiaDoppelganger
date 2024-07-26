using Mono.Cecil;
using System;
using System.Collections;
using System.IO;
using System.Resources;

namespace BamlReader
{
  public class BamlShaman
  {
    private string m_strOutPath { get; set; }
    private SaveMode m_enumSaveMode { get; set; }

    /// <summary>
    /// Available mods for saving .BAML content inside .DLL as...
    /// </summary>
    public enum SaveMode 
    { 
      NoSave = -1,
      Baml = 0, 
      Xaml = 1 
    };

    private void SaveXaml(Stream stream, string strResName)
    {
      //TODO: Make a Save Baml like Xaml in file
    }

    /// <summary>
    /// Saves .BAML as file near Read .DLL
    /// </summary>
    /// <param name="streamBaml">Stream to .BAML file inside of .resources of the .DLL</param>
    /// <param name="strResName">Current .BAML resource name</param>
    private void SaveBaml(Stream streamBaml, string strResName)
    {
      //TODO: Check if m_strOutPath is not null
      m_strOutPath += '\\';
      // Need to replace slashes with _ in order to create an ordinal file name
      m_strOutPath += strResName.Replace('/', '_'); 

      using (FileStream fileStream = new FileStream(m_strOutPath, FileMode.Create, FileAccess.Write))
        streamBaml.CopyTo(fileStream);

      Console.WriteLine($"BAML content from {strResName} extracted and saved to {m_strOutPath}");
    }

    /// <summary>
    /// Points a Stream to the .BAML file inside .resources
    /// </summary>
    /// <param name="streamRes">Stream to .resources from a .DLL file</param>
    /// <exception cref="Exception">If stream to .BAML is null, exception</exception>
    private void ExtractBamlFromResources(Stream streamRes)
    {
      using (ResourceReader resReader = new ResourceReader(streamRes))
      {
        foreach (DictionaryEntry ent in resReader)
        {
          //Check if entry is .baml resource
          string strResName = ent.Key.ToString();
          if (!strResName.EndsWith(".baml"))
            continue;


          //Yeah, there are actually 2 types of streams could be found
          var streamBaml = ent.Value as Stream ?? new MemoryStream((byte[])ent.Value);

          if (streamBaml == null)
            throw new Exception("Baml Stream is null!");

          if (m_enumSaveMode == SaveMode.Baml)
            SaveBaml(streamBaml, strResName);
          else if (m_enumSaveMode == SaveMode.Xaml)
            SaveXaml(streamBaml, strResName);


        }
      }
    }

    /// <summary>
    /// Reads .DLL file and tries to extract baml
    /// </summary>
    /// <param name="strDllPath">Path to .DLL file to Read</param>
    /// <param name="enumSaveMode">Save mode for the result</param>
    public void ReadDll(string strDllPath, SaveMode enumSaveMode)
    {
      m_enumSaveMode = enumSaveMode;
      m_strOutPath = Path.GetDirectoryName(strDllPath);

      // Load the assembly using Mono.Cecil
      var assemblyDefinition = AssemblyDefinition.ReadAssembly(strDllPath);

      foreach (var res in assemblyDefinition.MainModule.Resources)
      {
        if (res is EmbeddedResource embeddedResource)
        {
          // Get the resource stream
          using (Stream resStream = embeddedResource.GetResourceStream())
          {
            ExtractBamlFromResources(resStream);
          }
        }
      }
    }

    //static string ConvertBamlToXaml(string bamlFilePath)
    //{
    //  using (FileStream bamlStream = new FileStream(bamlFilePath, FileMode.Open, FileAccess.Read))
    //  {
    //    // Read BAML
    //    var bamlReader = new Baml2006Reader(bamlStream);

    //    // Create XAML writer settings
    //    var writerSettings = new XamlObjectWriterSettings
    //    {
    //      RootObjectInstance = null
    //    };
    //    var xamlWriter = new XamlObjectWriter(bamlReader.SchemaContext, writerSettings);

    //    // Read BAML and write XAML
    //    while (bamlReader.Read())
    //    {
    //      xamlWriter.WriteNode(bamlReader);
    //    }

    //    // Convert the XAML object to a string
    //    var xamlObject = xamlWriter.Result;
    //    return XamlWriter.Save(xamlObject);
    //  }
    //}
  }
}
