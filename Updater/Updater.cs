using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace Xannden.Update
{
	public class Updater
	{
		private readonly string url;
		private VersionInfo newest;
		private readonly Version appVersion;

		public Updater(string updateInfoUrl, Assembly assembly)
		{
			this.url = updateInfoUrl;
			this.appVersion = assembly.GetName().Version;
		}

		public bool NeedUpdate()
		{
			if (Debugger.IsAttached)
			{
				return false;
			}

			this.CleanUp();

			List<VersionInfo> versions = ParseUpdateInfo(this.url);

			if (versions != null)
			{
				versions.Sort();

				if (versions[versions.Count - 1].Version > this.appVersion)
				{
					this.newest = versions[versions.Count - 1];
					return true;
				}
			}

			return false;
		}

		public void RunUpdate()
		{
			string folder = GetFolder();

			try
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadFile(this.newest.Link, folder + "Update");
				}

				string exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

				if (this.newest.Link.EndsWith(".zip"))
				{
					using (ZipArchive archive = new ZipArchive(File.OpenRead(folder + "Update"), ZipArchiveMode.Read))
					{
						archive.Extract();
					}
				}
				else
				{
					File.Move(exeName, exeName + ".Old");
					File.Move(folder + "Update", exeName);
				}

				Process.Start(exeName);
			}
			finally
			{
				Directory.Delete(folder, true);
			}
		}

		private static string GetFolder()
		{
			string folder = "_temp";

			while (Directory.Exists(folder))
			{
				folder += "_";
			}

			Directory.CreateDirectory(folder);

			return folder + "\\";
		}

		private static List<VersionInfo> ParseUpdateInfo(string updateInfoUrl)
		{
			List<VersionInfo> info = new List<VersionInfo>();

			HttpWebRequest request = WebRequest.CreateHttp(updateInfoUrl);

			request.UserAgent = "XanndenUpdateBot";

			try
			{
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				byte[] data = ReadAll(response.GetResponseStream());

				Utf8JsonReader reader = new Utf8JsonReader(data);

				while (reader.Read())
				{
					switch (reader.TokenType)
					{
						case JsonTokenType.StartObject:
							info.Add(VersionInfo.Parse(reader));
							break;
					}
				}
			}
			catch (Exception)
			{
				return null;
			}

			return info;
		}

		private static byte[] ReadAll(Stream stream)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		private void CleanUp()
		{
			void CleanDir(string path)
			{
				foreach (string dir in Directory.EnumerateDirectories(path))
				{
					CleanDir(dir);
				}

				foreach (string file in Directory.EnumerateFiles(path, "*.Old"))
				{
					File.Delete(file);
				}
			}

			CleanDir(".\\");
		}
	}
}