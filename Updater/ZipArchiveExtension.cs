using System.IO;
using System.IO.Compression;

namespace Xannden.Update
{
	public static class ZipArchiveExtension
	{
		public static void Extract(this ZipArchive archive)
		{
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (entry.FullName.EndsWith("/"))
				{
					Directory.CreateDirectory(entry.FullName);
				}
				else
				{
					if (File.Exists(entry.FullName))
					{
						File.Move(entry.FullName, entry.FullName + ".Old");
					}

					entry.ExtractToFile(entry.FullName, true);
				}
			}
		}
	}
}