using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin (
	"MFractor.Installer", 
	Namespace = "MFractor.Installer",
	Version = "2.0.0"
)]

[assembly:AddinName ("MFractor.Installer")]
[assembly:AddinCategory ("IDE Extensions")]
[assembly:AddinDescription ("Installs MFractor into Xamarin Studio; the essential productivity tool for Xamarin Studio. Featuring Xaml code analysis, Xaml -> C# code generation, Android resource IntelliSense and much, much more.")]
[assembly:AddinUrl("http://www.mfractor.com")]
[assembly:AddinAuthor ("Matthew Robbins")]
