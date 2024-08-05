using Mono.Cecil;
using System;
using System.Collections;
using System.IO;
using System.Resources;

namespace BamlReader
{
  public class BamlShaman : IDisposable
  {
    private MemoryStream m_mstXaml { get; set; }
    private string m_strOutPath { get; set; }
    private SaveMode m_enumSaveMode { get; set; }

    /// <summary>
    /// Closes the member stream
    /// </summary>
    public void Dispose()
    {
      m_mstXaml?.Dispose();
    }

    /// <summary>
    /// Available mods for saving .BAML content inside .DLL as...
    /// </summary>
    public enum SaveMode
    {
      NoSave = -1,
      Baml = 0,
      Xaml = 1
    };

    /// <summary>
    /// Saves a stream to resource in XAML interpritation of BAML format
    /// into member variable
    /// </summary>
    /// <param name="streamBaml">Original stream to .BAML</param>
    private void StoreXamlStream(Stream streamBaml)
    {
      m_mstXaml = new MemoryStream();

      using (StreamWriter sr = new StreamWriter(m_mstXaml, leaveOpen: true))
      {
        streamBaml.Seek(0, SeekOrigin.Begin);
        var bamlReader = new BamlReader(streamBaml);
        sr.WriteLine(bamlReader);
        sr.Flush();
      }

      m_mstXaml.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Saves BAML file as Xaml near original .DLL file
    /// </summary>
    /// <param name="strResName">Current .BAML resource name</param>
    private void SaveXamlFile(string strResName)
    {
      string strOutPath = m_strOutPath;
      strOutPath += '\\';
      strResName = Path.ChangeExtension(strResName, "xaml");
      // Need to replace slashes with _ in order to create an ordinal file name 
      strOutPath += strResName.Replace('/', '_');
      StreamWriter sw = new StreamWriter(strOutPath);

      StreamReader sr = new StreamReader(m_mstXaml, leaveOpen: true);
      m_mstXaml.Seek(0, SeekOrigin.Begin);

      sw.WriteLine(sr.ReadToEnd());
      sw.Close();
      sr.Close();
    }

    /// <summary>
    /// Saves .BAML as file near Read .DLL
    /// </summary>
    /// <param name="streamBaml">Stream to .BAML file inside of .resources of the .DLL</param>
    /// <param name="strResName">Current .BAML resource name</param>
    private void SaveBamlFile(Stream streamBaml, string strResName)
    {
      //TODO: Check if m_strOutPath is not null
      string strOutPath = m_strOutPath;
      strOutPath += '\\';
      // Need to replace slashes with _ in order to create an ordinal file name
      strOutPath += strResName.Replace('/', '_');

      streamBaml.Seek(0, SeekOrigin.Begin);
      using (FileStream fileStream = new FileStream(strOutPath, FileMode.Create, FileAccess.Write))
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

          // TODO: Here You will end storing last stream, so next .baml entry won't be read.
          // So make sure to store streams as a list for user to choose which dialog to load.
          // Or store as list entries of the res reader which ends with .baml And wait for user input.
          StoreXamlStream(streamBaml);

          if (m_enumSaveMode == SaveMode.Baml)
            SaveBamlFile(streamBaml, strResName);
          else if (m_enumSaveMode == SaveMode.Xaml)
            SaveXamlFile(strResName);
        }
      }
    }

    /// <summary>
    /// Reads .DLL file and tries to extract baml
    /// </summary>
    /// <param name="strDllPath">Path to .DLL file to Read</param>
    /// <param name="enumSaveMode">Save mode for the result</param>
    public Stream ReadDll(string strDllPath, SaveMode enumSaveMode)
    {
      m_enumSaveMode = enumSaveMode;
      m_strOutPath = Path.GetDirectoryName(strDllPath);

      // Load the assembly using Mono.Cecil
      var assemblyDefinition = AssemblyDefinition.ReadAssembly(strDllPath);

      foreach (var res in assemblyDefinition.MainModule.Resources)
      {
        if (res is EmbeddedResource embeddedResource)
        {
          using (Stream resStream = embeddedResource.GetResourceStream())
            ExtractBamlFromResources(resStream);

          if (m_mstXaml != null)
            return m_mstXaml;
        }
      }
      return null;
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
