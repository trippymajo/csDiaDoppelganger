using Mono.Cecil;
using System;
using System.Collections;
using System.IO;
using System.Resources;

namespace BamlReader
{
  public class BamlShaman
  {
    private string m_strOutPath;

    private void SaveBaml(Stream streamBaml, string strResName)
    {
      //TODO: Check if m_strOutPath is not null
      //Save the raw BAML data
      using (FileStream fileStream = new FileStream(m_strOutPath, FileMode.Create, FileAccess.Write))
        streamBaml.CopyTo(fileStream);

      Console.WriteLine($"BAML content from {strResName} extracted and saved to {m_strOutPath}");
    }

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
          if (ent.Value is Stream)
          {
            Stream streamBaml = (Stream)ent.Value;
            SaveBaml(streamBaml, strResName);
          }
          else if (ent.Value is byte[])
          {
            MemoryStream bamlStream = new MemoryStream((byte[])ent.Value);
            SaveBaml(bamlStream, strResName);
          }
        }
      }
    }

    public void ReadDll(string strDllPath, string strOutPath)
    {
      m_strOutPath = strOutPath;
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
