using System;
using Gtk;
using System.Diagnostics;

namespace MFractor.Installer
{
	public class InstallSuccessDialog : Window
	{
		Label hiThere;
		Label installSuccess;


		Label productInformation;

		Image image;
		Button getStartedButton;

		string email;

		public InstallSuccessDialog (Window parent)
			: base("Welcome to MFractor")
		{
			const int windowWidth = 600;
			const int windowHeight = 480;

			this.WindowPosition = WindowPosition.CenterAlways;
			this.SetSizeRequest (windowWidth, windowHeight);
			this.Resizable = false;
			var vBox = new VBox ();
			vBox.SetSizeRequest (windowWidth, windowHeight);

			image = new Image (System.Reflection.Assembly.GetExecutingAssembly(), "MFractor.Installer.Assets.logo_medium.png");
			image.SetSizeRequest (250, 297);
			vBox.Add (image);

			installSuccess = new Label ("MFractor For Xamarin Studio has been installed successfully!");
			installSuccess.Layout.Alignment = Pango.Alignment.Center;
			productInformation = new Label ("MFractor is a suite of productivity tools for Xamarin Studio to supercharge Android app development. \nFeatures:\n - Resource static analysis and auto-magic issue fixing.\n - Type-system driven auto-completion for most xml resources.\n - Enhanced navigation for Android Resources with go-to declaration, tooltips and search bar integration.\n - Resource refactoring.\n - And much, much more!");
			productInformation.Layout.Alignment = Pango.Alignment.Center;
			productInformation.Wrap = true;

			getStartedButton = new Button ();
			getStartedButton.Label = "Let's get started";
			getStartedButton.Clicked += (object sender, EventArgs e) => {
				Process.Start("http://www.mfractor.com/");
				this.Hide();
			};

			vBox.Add (hiThere);
			vBox.Add (installSuccess);
			vBox.Add (productInformation);
			vBox.Add (getStartedButton);

			Add (vBox);
			ShowAll ();
		}
	}
}

