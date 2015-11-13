using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin (
	"MFractor.Installer", 
	Namespace = "MFractor.Installer",
	Version = "1.1.0"
)]

[assembly:AddinName ("MFractor.Installer")]
[assembly:AddinCategory ("IDE Extensions")]
[assembly:AddinDescription ("Registers the URL to install MFractor into Xamarin Studio.")]
[assembly:AddinUrl("https://twitter.com/matthewrdev")]
[assembly:AddinAuthor ("Matthew Robbins")]
