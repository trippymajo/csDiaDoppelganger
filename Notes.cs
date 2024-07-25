using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Resources;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

public class WpfWindowLoader
{
	 summary
	 Read from DLL and create a window directly from what been read.
	 summary
	 param name=dllPathPath to C# WPF .dll fileparam
	public static void LoadAndShowWindow(string dllPath)
	{
		var assembly = Assembly.LoadFrom(dllPath);  Load assembly

		foreach (Type type in assembly.GetTypes())
		{
			if (typeof(Window).IsAssignableFrom(type))
			{
				 Constructing URI. Dunno, that what i found on S.O. The problem here is baml to xaml actually
				string resourceName = ${assembly.GetName().Name};componentview{type.Name}.xaml;
				Uri resourceUri = new Uri(resourceName, UriKind.Relative);
				try
				{
					StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);
					if (resourceInfo != null)
					{
						using (Stream stream = Application.GetResourceStream(resourceUri).Stream)
						{
							if (stream != null)
							{
								Window window = (Window)XamlReader.Load(stream);
								window.Show();
								return;
							}
							else
								Console.WriteLine($Resource stream {resourceName} is null.);
						}
					}
					else
						Console.WriteLine($Resource {resourceName} not found.);
				}
				catch (Exception ex)
				{
					MessageBox.Show($Error reading xaml-o-bamlo {ex.Message}, Error, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
	}

	 summary
	 Read Xaml and show window based on xaml. Simply.
	 summary
	 param name=xamlPath Path to XAML file to readparam
	public static void ReadAndShowWindow(string xamlPath)
	{
		Window window;
		try
		{
			using (FileStream fs = new FileStream(xamlPath, FileMode.Open, FileAccess.Read))
			{
				window = (Window)XamlReader.Load(fs);
			}
			window.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show($Error reading XAML file {ex.Message}, Error, MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	 summary
	 Read baml file from DLL and write it as Xaml file
	 summary
	 param name=dllPathPath to C# WPF .dll fileparam
	public static void ReadAndWriteXaml(string dllPath)
	{
		var assembly = Assembly.LoadFrom(dllPath);

		foreach (Type type in assembly.GetTypes())
		{
			if (typeof(Window).IsAssignableFrom(type))
			{
				string resourceName = ${assembly.GetName().Name};componentview{type.Name}.baml; TUT CHTOTO ELIA!!!!!!!!!!!!
				Uri resourceUri = new Uri(resourceName, UriKind.Relative);
				try
				{
					StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);
					if (resourceInfo != null)
					{
						using (Stream stream = resourceInfo.Stream)
						{
							if (stream != null)
							{
								string outputDirectory = @CUsersAdminDesktop;
								string outputFilePath = Path.Combine(outputDirectory, ${type.Name}.xaml);

								byte[] bamlContent;
								using (MemoryStream ms = new MemoryStream())
								{
									stream.CopyTo(ms);
									bamlContent = ms.ToArray();
								}

								File.WriteAllBytes(outputFilePath, bamlContent);

								Console.WriteLine($XAML for {type.Name} exported to {outputFilePath});
							}
							else
								Console.WriteLine($Resource {resourceName} not found.);
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($Error parsing XAML {ex.Message}, Error, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
	}
}

class Program
{
	[STAThread]
	static void Main()
	{
		 Path to the DLL containing the WPF window XAML
		string dllPath = @CUsersAdminDesktoppseudo_seambinDebugnet8.0-windowsPseudoSteam.dll;
		string xamlPath = @CUsersAdminDesktopMainWindow.xaml;

		 Load and show the window from the DLL
		WpfWindowLoader.LoadAndShowWindow(dllPath);
		WpfWindowLoader.ReadAndShowWindow(xamlPath);
		WpfWindowLoader.ReadAndWriteXaml(dllPath);

		 Start the WPF application
		Application app = new Application();
		app.Run();
	}
}