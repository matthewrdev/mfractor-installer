using System;
using Gtk;

namespace MFractor.Installer
{
	public class InstallSuccessDialog : Window
	{
		Label hiThere;
		Label installSuccess;


		Label pleaseOpen;

		Image image;
		Button getStartedButton;

		string email;

		public InstallSuccessDialog (Window parent)
			: base("Welcome to MFractor")
		{
			const int windowWidth = 600;
			const int windowHeight = 420;

			this.WindowPosition = WindowPosition.CenterAlways;
			this.SetSizeRequest (windowWidth, windowHeight);
			this.Resizable = false;
			var vBox = new VBox ();
			vBox.SetSizeRequest (windowWidth, windowHeight);

			image = new Image (System.Reflection.Assembly.GetExecutingAssembly(), "MFractor.Installer.Assets.logo_medium.png");
			image.SetSizeRequest (250, 297);
			vBox.Add (image);

			hiThere = new Label ("Hi there!");
			hiThere.Layout.Alignment = Pango.Alignment.Center;
			installSuccess = new Label ("The url to install and update MFractor has been added to the Addin Manager.");
			installSuccess.Layout.Alignment = Pango.Alignment.Center;
			pleaseOpen = new Label ("Please open the Addin Gallery and install MFractor to get started.");
			pleaseOpen.Layout.Alignment = Pango.Alignment.Center;

			getStartedButton = new Button ();
			getStartedButton.Label = "Ok!";
			getStartedButton.Clicked += (object sender, EventArgs e) => {
				this.Hide();
			};

			vBox.Add (image);
			vBox.Add (hiThere);
			vBox.Add (installSuccess);
			vBox.Add (pleaseOpen);
			vBox.Add (getStartedButton);

			Add (vBox);
			ShowAll ();
		}
	}
}

