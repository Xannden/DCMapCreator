using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using CompendiumMapCreator.Format;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for App.xaml.
	/// </summary>
	public partial class App : Application
	{
		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			if (!Debugger.IsAttached)
			{
				File.AppendAllText("errorLog.txt", e.Exception.ToString());

				MessageBox.Show("An unexpected error has occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				e.Handled = true;
			}
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (File.Exists("updater.exe") && this.NeedsUpdate())
			{
				MessageBoxResult result = MessageBox.Show("There is an update available.\n\nDo you want to update?", "DDO Compendium Map Creator", MessageBoxButton.YesNo, MessageBoxImage.Information);

				if (result == MessageBoxResult.Yes)
				{
					Process.Start("updater.exe", "CompendiumMapCreator.exe DCMapCreator");

					this.Shutdown();
				}
			}

			if (File.Exists("errorLog.txt"))
			{
				File.Delete("errorLog.txt");
			}

			if (e.Args.Length >= 3 && string.Equals(e.Args[0], "find", StringComparison.OrdinalIgnoreCase))
			{
				List<string> result = new List<string>();

				string dir = e.Args[1];

				List<IconType> types = this.GetIcons(e.Args[2].Split(','));

				List<string> files = this.GetFiles(dir, true);

				for (int i = 0; i < files.Count; i++)
				{
					try
					{
						Project project = Project.LoadFile(files[i]);

						if (types != null)
						{
							List<IconType> pTypes = project.Elements.Select(ele => ele.Type).Distinct().ToList();

							if (pTypes.Intersect(types).Any())
							{
								result.Add(files[i]);
							}
						}
					}
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
					catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
					{
					}
				}

				MessageBox.Show(result.Aggregate("Found these files\n\n", (a, s) => a + "\n\n" + s), "DDO Compendium Map Creator", MessageBoxButton.OK, MessageBoxImage.Information);

				this.Shutdown();
			}

			string inputDir = null;
			string outputDir = null;
			bool recursive = false;
			bool addLegend = true;
			string[] include = null;

			for (int i = 0; i < e.Args.Length; i++)
			{
				if ((e.Args[i] == "-e" || e.Args[i] == "-export") && e.Args.Length > (i + 1))
				{
					inputDir = e.Args[i + 1];
				}

				if ((e.Args[i] == "-o" || e.Args[i] == "-output") && e.Args.Length > (i + 1))
				{
					outputDir = e.Args[i + 1];

					if (!outputDir.EndsWith("/") && !outputDir.EndsWith("\\"))
					{
						outputDir += "\\";
					}
				}

				if (e.Args[i] == "-r" || e.Args[i] == "-recursive")
				{
					recursive = true;
				}

				if (e.Args[i] == "-nl" || e.Args[i] == "-noLegend")
				{
					addLegend = false;
				}

				if ((e.Args[i] == "-i" || e.Args[i] == "-includes") && e.Args.Length > (i + 1))
				{
					include = e.Args[i + 1].Split(',');
				}
			}

			if (string.IsNullOrEmpty(outputDir))
			{
				outputDir = inputDir;

				if (outputDir?.EndsWith("/") == false && !outputDir.EndsWith("\\"))
				{
					outputDir += "\\";
				}
			}

			if (!string.IsNullOrEmpty(inputDir))
			{
				List<string> files = this.GetFiles(inputDir, recursive);
				List<IconType> types = this.GetIcons(include);

				for (int i = 0; i < files.Count; i++)
				{
					try
					{
						Project project = Project.LoadFile(files[i]);

						if (types != null)
						{
							List<IconType> pTypes = project.Elements.Select(ele => ele.Type).Distinct().ToList();

							if (!pTypes.Intersect(types).Any())
							{
								continue;
							}
						}

						project.Export(addLegend, outputDir + Path.GetFileNameWithoutExtension(files[i]) + ".png");
					}
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
					catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
					{
					}
				}

				this.Shutdown();
			}

			this.StartupUri = new Uri("View/MainWindow.xaml", UriKind.RelativeOrAbsolute);
		}

		private List<string> GetFiles(string path, bool recursive)
		{
			static void GetFilesRecursive(string dir, List<string> list)
			{
				string[] dirs = Directory.GetDirectories(dir);

				for (int i = 0; i < dirs.Length; i++)
				{
					GetFilesRecursive(dirs[i], list);
				}

				list.AddRange(Directory.GetFiles(dir));
			}

			if (recursive)
			{
				List<string> files = new List<string>();

				GetFilesRecursive(path, files);

				return files;
			}
			else
			{
				return Directory.GetFiles(path).ToList();
			}
		}

		private List<IconType> GetIcons(string[] includes)
		{
			if (includes == null)
			{
				return null;
			}

			List<IconType> types = new List<IconType>();

			for (int i = 0; i < includes.Length; i++)
			{
				IconType? type = includes[i].FromName();

				if (type != null)
				{
					types.Add(type.Value);
				}
			}

			return types;
		}

		private bool NeedsUpdate()
		{
			HttpWebRequest request = WebRequest.CreateHttp("https://api.github.com/repos/Xannden/DCMapCreator/releases/latest");

			request.UserAgent = "Xannden";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Temp));

			Temp temp = (Temp)serializer.ReadObject(response.GetResponseStream());

			return temp.Tag_name != "V1.8.1";
		}

		[DataContract]
		private class Temp
		{
#pragma warning disable CS0649
			[DataMember]
			internal string Tag_name { get; set; } = null;

#pragma warning restore CS0649
		}
	}
}