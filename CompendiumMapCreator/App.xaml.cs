using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show("An unexpected error has occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

			e.Handled = true;
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (File.Exists("updater.exe") && this.NeedsUpdate())
			{
				Process.Start("updater.exe", "CompendiumMapCreator.exe DCMapCreator");

				this.Shutdown();
			}
		}

		private bool NeedsUpdate()
		{
			HttpWebRequest request = WebRequest.CreateHttp("https://api.github.com/repos/Xannden/DCMapCreator/releases/latest");

			request.UserAgent = "Xannden";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Temp));

			Temp temp = (Temp)serializer.ReadObject(response.GetResponseStream());

			return temp.tag_name != "V1.2";
		}

		[DataContract]
		internal class Temp
		{
			[DataMember]
			internal string tag_name;
		}
	}
}