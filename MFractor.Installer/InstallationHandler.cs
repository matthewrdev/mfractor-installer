using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Dialogs;
using System.Reflection;
using Gtk;

namespace MFractor.Installer
{
	public class InstallationHandler : CommandHandler
	{

		protected override void Run ()
		{
			RegisterAddinUrl ();
		}

		public void RegisterAddinUrl()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies ();

			var addinSetupAssebmly = assemblies.FirstOrDefault (a => a.FullName.Contains ("MonoDevelop.Core"));
			if (addinSetupAssebmly == null) {
				Console.WriteLine ("Addin setup assebmly wasn't resolved");
				return;
			}

			var runtime = addinSetupAssebmly.GetType ("MonoDevelop.Core.Runtime");
			if (runtime == null) {
				Console.WriteLine ("runtime wasn't resolved");
				return;
			}

			var addinService = runtime.GetField ("setupService", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			if (addinService == null) {
				Console.WriteLine ("addinService wasn't resolved");
				return;
			}

			var type = addinService.GetType ();
			var repo = type.GetProperty ("Repositories", BindingFlags.Instance | BindingFlags.Public).GetValue(addinService);

			// TODO: 

			const string primaryAddinUrl = "http://addins.mfractor.com";
			string[] addinUrls = new [] { "http://addins.mfractor.com/main.mrep", "http://addins.mfractor.com/", "http://addins.mfractor.com", "http://addins.mfractor.com/root.mrep" } ;

			var repoMehods = repo.GetType().GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			var containsRepoMethod = repoMehods.FirstOrDefault (m => m.Name == "ContainsRepository");

			bool hasUrl = false;
			foreach (var url in addinUrls) {
				hasUrl |= (bool)containsRepoMethod.Invoke (repo, new [] { url });
			}

			if (!hasUrl) {
				var registerRepoMethod = repoMehods.FirstOrDefault (m => m.Name == "RegisterRepository");
				registerRepoMethod.Invoke (repo, new [] { null,  primaryAddinUrl });


				var d = new InstallSuccessDialog (IdeApp.Workbench.RootWindow);
				d.Show ();
			}

			// Attempt to install mfractor.

		}
	}
}

