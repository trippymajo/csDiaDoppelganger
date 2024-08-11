using System;
using System.Collections;
using System.IO;
using System.Resources;
using System.Collections.Generic;

using Mono.Cecil;

namespace BamlReader
{
  public class BamlShaman
  {
    private string _outPath { get; set; }
    private SaveMode _saveMode { get; set; }
    public List<(Stream xamlStream, string resName)> XamlStreamList;

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
    /// Default constructor
    /// </summary>
    public BamlShaman()
    {
      _outPath = string.Empty;
      _saveMode = SaveMode.NoSave;
      XamlStreamList = new List<(Stream xamlStream, string resName)> ();
    }

    /// <summary>
    /// Saves a stream to resource in XAML interpritation of BAML format
    /// into member variable
    /// </summary>
    /// <param name="streamBaml">Original stream to .BAML</param>
    private void StoreXamlStream(Stream streamBaml, string resName)
    {
      var xamlStream = new MemoryStream();

      using (StreamWriter sr = new StreamWriter(xamlStream, leaveOpen: true))
      {
        streamBaml.Seek(0, SeekOrigin.Begin);
        var bamlReader = new BamlReader(streamBaml);
        sr.WriteLine(bamlReader);
        sr.Flush();
      }

      xamlStream.Seek(0, SeekOrigin.Begin);
      //Creating a tuple in order to push in to the list of tuples.
      (Stream xamlStream, string resName) tuple = (xamlStream, resName);
      XamlStreamList.Add(tuple);
    }

    /// <summary>
    /// Saves BAML file as Xaml near original .DLL file
    /// </summary>
    /// <param name="tuple">Tuple with Stream and resource name</param>
    private void SaveXamlFile((Stream xamlStream, string resName) tuple)
    {
      string OutPath = _outPath;
      string resName = tuple.resName;
      OutPath += '\\';
      resName = Path.ChangeExtension(resName, "xaml");
      // Need to replace slashes with - in order to create an ordinal file name 
      OutPath += resName.Replace('/', '-');
      StreamWriter sw = new StreamWriter(OutPath);

      StreamReader sr = new StreamReader(tuple.xamlStream, leaveOpen: true);
      tuple.xamlStream.Seek(0, SeekOrigin.Begin);

      sw.WriteLine(sr.ReadToEnd());
      sw.Close();
      sr.Close();
    }

    /// <summary>
    /// Saves .BAML as file near Read .DLL
    /// </summary>
    /// <param name="streamBaml">Stream to .BAML file inside of .resources of the .DLL</param>
    /// <param name="resName">Current .BAML resource name</param>
    private void SaveBamlFile(Stream streamBaml, string resName)
    {
      //TODO: Check if _outPath is not null
      string OutPath = _outPath;
      OutPath += '\\';
      // Need to replace slashes with _ in order to create an ordinal file name
      OutPath += resName.Replace('/', '_');

      streamBaml.Seek(0, SeekOrigin.Begin);
      using (FileStream fileStream = new FileStream(OutPath, FileMode.Create, FileAccess.Write))
        streamBaml.CopyTo(fileStream);

      Console.WriteLine($"BAML content from {resName} extracted and saved to {_outPath}");
    }

    /// <summary>
    /// Points a Stream to the .BAML file inside .resources
    /// </summary>
    /// <param name="streamRes">Stream to .resources from a .DLL file</param>
    /// <exception cref="Exception">If stream to .BAML is null, exception</exception>
    private void ExtractBamlFromResources(Stream streamRes)
    {
      using (ResourceReader RR = new ResourceReader(streamRes))
      {
        foreach (DictionaryEntry ent in RR)
        {
          //Check if entry is .baml resource
          string resName = ent.Key.ToString();
          if (!resName.EndsWith(".baml"))
            continue;

          //Yeah, there are actually 2 types of streams could be found
          var streamBaml = ent.Value as Stream ?? new MemoryStream((byte[])ent.Value);

          if (streamBaml == null)
            throw new Exception("Baml Stream is null!");

          // TODO: Here You will end storing last stream, so next .baml entry won't be read.
          // So make sure to store streams as a list for user to choose which dialog to load.
          // Or store as list entries of the res reader which ends with .baml And wait for user input.
          StoreXamlStream(streamBaml, resName);

          if (_saveMode == SaveMode.Baml)
            SaveBamlFile(streamBaml, resName);
        }

        if (_saveMode == SaveMode.Xaml)
        {
          foreach(var xamlTuple in XamlStreamList)
          {
            SaveXamlFile(xamlTuple);
          }
        }
      }
    }

    /// <summary>
    /// Reads .DLL file and tries to extract XAML streams of the BAMLs
    /// </summary>
    /// <param name="strDllPath">Full path to .DLL file</param>
    /// <param name="enumSaveMode">Save Mode for the extracted files</param>
    /// <returns>List of tuples of XAML streams and names</returns>
    public List<(Stream xamlStream, string resName)> ReadDll(string strDllPath, SaveMode enumSaveMode = SaveMode.NoSave)
    {
      _saveMode = enumSaveMode;
      _outPath = Path.GetDirectoryName(strDllPath);

      // Load the assembly using Mono.Cecil
      var assemblyDefinition = AssemblyDefinition.ReadAssembly(strDllPath);

      foreach (var res in assemblyDefinition.MainModule.Resources)
      {
        if (res is EmbeddedResource embeddedResource)
        {
          using (Stream resStream = embeddedResource.GetResourceStream())
            ExtractBamlFromResources(resStream);

          if (XamlStreamList.Count > 0)
            return XamlStreamList;
        }
      }
      return null;
    }
  }
}
