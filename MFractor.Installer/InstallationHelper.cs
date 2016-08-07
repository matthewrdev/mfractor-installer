using System;
using System.Linq;
using System.Reflection;
using System.IO;
using Mono.Addins;
using MonoDevelop.Core;

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
	}
}

