using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Dialogs;
using System.Reflection;
using Gtk;
using Mono.Addins;
using MonoDevelop.Core;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Threading;
using System.Xml.Linq;

namespace MFractor.Installer
{
	public class InstallationHandler : CommandHandler
	{
		const string hasRanFile = ".has_ran";
		const string primaryAddinUrl = "http://addins.mfractor.com";
		const string roslynAddinUrl = "http://addins.mfractor.com/roslyn";
		const string installationUrl = "http://addins.mfractor.com/releases/latest/MFractor.mpack";

		protected override void Run ()
		{
			var version = IdeApp.Version;

			bool urlRegistered = CheckIfUrlRegistered ();
			bool isInstalled = CheckIfInstalled ();

			if (version.Major == 6) {

				// Check the installed url, update to 

				//InstallRolsynUrl ();
				//InstallRoslynAddin ();
			} else if (version.Major == 5 && version.Minor >= 10)
			{
				if (!urlRegistered) 
				{
					InstallUrl ();
				}

				if (!isInstalled)
				{
					InstallAddin (primaryAddinUrl);
				}
			} else {
				if (urlRegistered == false) {
					// Unsupported version.
					MessageDialog message = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok, "MFractor is only supported in versions 5.10 and higher for " + BrandingService.ApplicationName);
					message.Show ();

					InstallUrl();
				}
			}
		}

		readonly string[] addinUrls = new [] { "http://addins.mfractor.com/main.mrep", "http://addins.mfractor.com/", "http://addins.mfractor.com", "http://addins.mfractor.com/root.mrep" } ;

		bool CheckIfUrlRegistered ()
		{
			var addinService = InstallationHelper.GetSetupServiceInstance ();

			return InstallationHelper.ContainsRepository (addinService, addinUrls);
		}

		bool CheckIfInstalled ()
		{
			var addinService = InstallationHelper.GetSetupServiceInstance ();

			return AddinManager.Registry.GetAddin ("MFractor") != null;
		}

		public void InstallUrl()
		{
			var addinService = InstallationHelper.GetSetupServiceInstance ();

			var hasUrl = InstallationHelper.ContainsRepository (addinService, addinUrls);

			if (!hasUrl) {

				InstallationHelper.RegisterRepository (addinService, primaryAddinUrl);
			}


			// Attempt to install mfractor.
		}


		public void InstallAddin(string url)
		{
			ProgressDialog d = new ProgressDialog (IdeApp.Workbench.RootWindow, false, true);

			var setupService = InstallationHelper.GetSetupServiceInstance ();

			d.Show ();
			d.Message = "Installing MFractor";

			Task.Run (() => {


				DispatchService.GuiDispatch( () => { d.BeginTask ("Downloading addin..."); });

				string downloadUrl = url + @"/root.mrep";
				string downloadFolder = Path.Combine(InstallationHelper.DirectoryForAssembly(Assembly.GetExecutingAssembly()), ".temp");
				string mrepFilePath = Path.Combine (downloadFolder, "root.mrep");

				if (Directory.Exists(downloadFolder)) {
					Directory.Delete(downloadFolder, true);
				}

				Directory.CreateDirectory (downloadFolder);

				DispatchService.GuiDispatch( () => {d.WriteText("Locating MFractor addin package...\n");});

				var webClient = new WebClient();

				webClient.DownloadFile(downloadUrl, mrepFilePath);

				string addinDownloadUrl = "";
				string addinFilePath = "";

				try {
					var xdoc = XDocument.Load(mrepFilePath);
					string addinFile = xdoc.Root.Element("Addin").Element("Url").Value;

					addinDownloadUrl = url + "/" + addinFile;
					addinFilePath = Path.Combine(downloadFolder, addinFile);

					var fi = new FileInfo(addinFilePath);
					if (!Directory.Exists(fi.DirectoryName)) {
						Directory.CreateDirectory(fi.DirectoryName);
					}
				} catch {
					
				}

				if (String.IsNullOrEmpty(addinDownloadUrl)) {
					DispatchService.GuiDispatch( () => { d.EndTask(); });
					DispatchService.GuiDispatch( () => { d.Message = "Installation failed. Please try again through the addin manager\n"; });
					return;
				}

				DispatchService.GuiDispatch( () => {d.WriteText("Downloading " + addinDownloadUrl + "...\n");});

				webClient = new WebClient();
				webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
					DispatchService.GuiDispatch( () => { d.WriteText( "  " + e.TotalBytesToReceive + " bytes remaining...\n");});
				};
				webClient.DownloadFile(addinDownloadUrl, addinFilePath);

				DispatchService.GuiDispatch( () => { d.EndTask(); });

				DispatchService.GuiDispatch( () => {
					d.BeginTask("Installing addin...");


					var monitor = new InstallMonitor (d);

					InstallationHelper.InstallAddin(setupService, monitor, addinFilePath);
					d.Hide();
				});

			});
		}
	}

	public class InstallMonitor: IProgressStatus, IDisposable
	{
		StringCollection errors = new StringCollection ();
		StringCollection warnings = new StringCollection ();
		bool canceled;
		bool done;

		ProgressDialog _dialog;

		public InstallMonitor (ProgressDialog dialog)
		{
			_dialog = dialog;
		}

		public InstallMonitor ()
		{
		}

		public void SetMessage (string msg)
		{
			if (_dialog != null)
			{
				_dialog.Message = GLib.Markup.EscapeText (msg);
			}
			
			RunPendingEvents ();
		}

		public void SetProgress (double progress)
		{
			DispatchService.GuiDispatch (() => {
				_dialog.Progress = progress;
			});
			RunPendingEvents ();
		}

		public void Log (string msg)
		{
			DispatchService.GuiDispatch (() => {
				_dialog.WriteText (msg + "\n");
			});
		}

		public void ReportWarning (string message)
		{
			DispatchService.GuiDispatch (() => {
				_dialog.WriteText (message + "\n");
			});
			warnings.Add (message);
		}

		public void ReportError (string message, Exception exception)
		{
			DispatchService.GuiDispatch (() => {
				_dialog.WriteText (message + "\n");
				_dialog.WriteText (exception.Message + "\n");
			});
			errors.Add (message);
		}

		public bool IsCanceled {
			get { return canceled; }
		}

		public StringCollection Errors {
			get { return errors; }
		}

		public StringCollection Warnings {
			get { return warnings; }
		}

		public void Cancel ()
		{
			canceled = true;
		}

		public int LogLevel {
			get { return 1; }
		}

		public void Dispose ()
		{
			done = true;
		}

		public void WaitForCompleted ()
		{
			while (!done) {
				RunPendingEvents ();
				Thread.Sleep (50);
			}
		}

		public bool Success {
			get { return errors.Count == 0; }
		}

		void RunPendingEvents ()
		{
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
	}
}

