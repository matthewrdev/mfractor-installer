using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin (
	"MFractor.Installer", 
	Namespace = "MFractor.Installer",
	Version = "1.5.0"
)]

[assembly:AddinName ("MFractor.Installer")]
[assembly:AddinCategory ("IDE Extensions")]
[assembly:AddinDescription ("Installs MFractor into Xamarin Studio; a Xamarin.Android power tooling kit featuring a resource static analyser, type-system driven resource auto-completion, enhanced resource browsing and a suite of code generation tools (XML and CSharp).")]
[assembly:AddinUrl("http://www.mfractor.com")]
[assembly:AddinAuthor ("Matthew Robbins")]
