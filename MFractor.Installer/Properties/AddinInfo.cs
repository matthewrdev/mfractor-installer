using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin (
	"MFractor.Installer", 
	Namespace = "MFractor.Installer",
	Version = "2.1.0"
)]

[assembly:AddinName ("MFractor.Installer")]
[assembly:AddinCategory ("IDE extensions")]
[assembly:AddinDescription ("Installs MFractor into Xamarin Studio.\n\nAs the essential productivity tool for Xamarin Studio, MFractor adds:\n\nXamarin.Forms Xaml code analysis.\nXamarin.Forms Xaml to C# code generation.\nStreamlined Xamarin.Forms navigation.\nIntelliSense for Xamarin.Android Xml Resources.\nStatic analysis for Android Resources\nStreamlined Android resource navigation.\nC# code analysis for mobile apps.")]
[assembly:AddinUrl("http://www.mfractor.com")]
[assembly:AddinAuthor ("Matthew Robbins")]
