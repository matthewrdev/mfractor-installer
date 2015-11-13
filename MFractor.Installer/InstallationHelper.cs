using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace MFractor.Installer
{
	public class InstallationHelper
	{
		public InstallationHelper ()
		{
		}

		public static string DirectoryForAssembly(Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}

		public static Object GetSetupServiceInstance()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies ();

			var addinSetupAssebmly = assemblies.FirstOrDefault (a => a.FullName.Contains ("MonoDevelop.Core"));
			if (addinSetupAssebmly == null) {
				Console.WriteLine ("Addin setup assebmly wasn't resolved");
				return null;
			}

			var runtime = addinSetupAssebmly.GetType ("MonoDevelop.Core.Runtime");
			if (runtime == null) {
				Console.WriteLine ("runtime wasn't resolved");
				return null;
			}

			var addinService = runtime.GetField ("setupService", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			if (addinService == null) {
				Console.WriteLine ("addinService wasn't resolved");
				return null;
			}

			return addinService;
		}

		public static bool ContainsRepository(Object addinService, string[] addinUrls)
		{
			var type = addinService.GetType ();
			var repo = type.GetProperty ("Repositories", BindingFlags.Instance | BindingFlags.Public).GetValue(addinService);
			var repoMehods = repo.GetType().GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			var containsRepoMethod = repoMehods.FirstOrDefault (m => m.Name == "ContainsRepository");

			bool hasUrl = false;
			foreach (var url in addinUrls) {
				hasUrl |= (bool)containsRepoMethod.Invoke (repo, new [] { url });
			}

			return hasUrl;
		}

		public static void RegisterRepository(Object addinService, string url)
		{
			var type = addinService.GetType ();
			var repo = type.GetProperty ("Repositories", BindingFlags.Instance | BindingFlags.Public).GetValue(addinService);
			var repoMehods = repo.GetType().GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			var registerRepoMethod = repoMehods.FirstOrDefault (m => m.Name == "RegisterRepository");
			registerRepoMethod.Invoke (repo, new [] { null,  url });
		}

		public static void InstallAddin (object addinService, InstallMonitor monitor, string addinFilePath)
		{
			var type = addinService.GetType ();
			var repoMehods = type.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			System.Collections.Generic.IEnumerable<MethodInfo> installMethods = repoMehods.Where (m => m.Name == "Install");

			MethodInfo method = null;

			foreach (var m in installMethods) {
				var param = m.GetParameters ();
				foreach (var p in param) {
					if (p.Name == "files") {
						method = m;
						break;
					}
				}
				if (method != null) {
					break;
				}
			}

			method.Invoke (addinService, new [] { monitor, (object)(new string [] { addinFilePath })});
			
		}
	}
}

