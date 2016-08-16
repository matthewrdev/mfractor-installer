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
using System.Diagnostics;

namespace MFractor.Installer
{
	public class InstallationHandler : CommandHandler
	{
		const string hasRanFile = ".has_ran";
		const string primaryAddinUrl = "http://addins.mfractor.com";
		const string installationUrl = "http://addins.mfractor.com/releases/latest/MFractor.mpack";

		protected override void Run ()
		{
			var version = IdeApp.Version;

			bool urlRegistered = CheckIfUrlRegistered ();

			if (version.Major == 6) {
				if (!urlRegistered) {
					InstallUrl ();
				}

				if (!CheckIfInstalled ()) {
					InstallAddin (primaryAddinUrl);
				}
			} else {
				if (!urlRegistered) {
					// Unsupported version.
					MessageDialog message = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok, "MFractor is only supported in versions 6.0 and higher for " + BrandingService.ApplicationName);
					message.Show ();

					InstallUrl();
				}
			}
		}

		readonly string[] addinUrls = new [] { "http://addins.mfractor.com/main.mrep", "http://addins.mfractor.com/", "http://addins.mfractor.com", "http://addins.mfractor.com/root.mrep" } ;


		public bool CheckIfUrlRegistered ()
		{
			var repos = Runtime.AddinSetupService.Repositories.GetRepositories ();

			foreach (var r in repos) {
				if (r.Url.Contains ("http://addins.mfractor.com")) {
					return true;
				}
			}

			return false;
		}

		public void InstallUrl ()
		{
			Runtime.AddinSetupService.Repositories.RegisterRepository (null, "http://addins.mfractor.com/");
		}

		bool CheckIfInstalled ()
		{
			return AddinManager.Registry.GetAddin ("MFractor") != null;
		}

		public static string DirectoryForAssembly (Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uri = new UriBuilder (codeBase);
			string path = Uri.UnescapeDataString (uri.Path);
			return Path.GetDirectoryName (path);
		}

		public void InstallAddin(string url)
		{
			ProgressDialog d = new ProgressDialog (IdeApp.Workbench.RootWindow, false, true);

			d.Show ();
			d.Message = "Installing MFractor";
			Task.Run (() => {
				
				Runtime.RunInMainThread (() => { d.BeginTask ("Downloading MFractor For Xamarin Studio..."); });

				string downloadUrl = url + @"/root.mrep";
				string downloadFolder = Path.Combine (DirectoryForAssembly (Assembly.GetExecutingAssembly ()), ".temp");
				string mrepFilePath = Path.Combine (downloadFolder, "root.mrep");

				if (Directory.Exists (downloadFolder)) {
					Directory.Delete (downloadFolder, true);
				}

				Directory.CreateDirectory (downloadFolder);

				Runtime.RunInMainThread (() => { d.WriteText ("Locating MFractor addin package...\n"); });

				var webClient = new WebClient ();

				webClient.DownloadFile (downloadUrl, mrepFilePath);

				string addinDownloadUrl = "";
				string addinFilePath = "";

				try {
					var xdoc = XDocument.Load (mrepFilePath);
					string addinFile = xdoc.Root.Element ("Addin").Element ("Url").Value;

					addinDownloadUrl = url + "/" + addinFile;
					addinFilePath = Path.Combine (downloadFolder, addinFile);

					var fi = new FileInfo (addinFilePath);
					if (!Directory.Exists (fi.DirectoryName)) {
						Directory.CreateDirectory (fi.DirectoryName);
					}
				} catch {

				}

				if (String.IsNullOrEmpty (addinDownloadUrl)) {
					Runtime.RunInMainThread (() => { d.EndTask (); });
					Runtime.RunInMainThread (() => { d.Message = "Installation failed. Please try again through the addin manager\n"; });
					return;
				}

				Runtime.RunInMainThread (() => { d.WriteText ("Downloading " + addinDownloadUrl + "...\n"); });

				webClient = new WebClient ();
				webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
					Runtime.RunInMainThread (() => {
						d.Progress = (double)e.ProgressPercentage / 100.0; ;
					});
				};

				webClient.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => {
					if (e.Cancelled) {
						Runtime.RunInMainThread (() => {
							d.Message = "The download was cancelled.";
						});

						return;
					}

					if (e.Error != null) {
						Runtime.RunInMainThread (() => {
							d.Message = "An error occurred while trying to download. Please file a bug report at https://github.com/matthewrdev/mfractor-installer";
							d.WriteText ("Error:\n" + e.Error.ToString ());
						});

						return;
					}

					Runtime.RunInMainThread (() => { d.EndTask (); });

					Runtime.RunInMainThread (() => {
						d.BeginTask ("Installing MFractor For Xamarin Studio");

						var monitor = new InstallMonitor (d);

						string dataPath = "";
						if (Platform.IsWindows) {
							dataPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "mfractor");
						} else {
							dataPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "mfractor");
						}

						bool requiresDialog = File.Exists (Path.Combine (dataPath, ".first_run"));

						Task.Run (() => {
							Runtime.AddinSetupService.Install (monitor, addinFilePath);

							Runtime.RunInMainThread (() => {
								d.Hide ();
							});
						});
					});
				};

				webClient.DownloadFileAsync (new Uri (addinDownloadUrl), addinFilePath);
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
			Runtime.RunInMainThread (() => {
				_dialog.Progress = progress;
			});
			RunPendingEvents ();
		}

		public void Log (string msg)
		{
			Runtime.RunInMainThread (() => {
				_dialog.WriteText (msg);
			});
		}

		public void ReportWarning (string message)
		{
			Runtime.RunInMainThread (() => {
				_dialog.WriteText (message + "\n");
			});
			warnings.Add (message);
		}

		public void ReportError (string message, Exception exception)
		{
			Runtime.RunInMainThread (() => {
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

