using System;
using Osiris.ScreenManager.Screens;
using Microsoft.Xna.Framework.Net;

namespace Torq2.Screens
{
	/// <summary>
	/// The main menu screen is the first thing displayed when the game starts up.
	/// </summary>
	public class MainMenuScreen : MenuScreen
	{
		#region Initialization


		/// <summary>
		/// Constructor fills in the menu contents.
		/// </summary>
		public MainMenuScreen()
			: base("Main Menu")
		{
			// Create our menu entries.
			MenuEntry singlePlayerMenuEntry = new MenuEntry("Single Player");
			MenuEntry liveMenuEntry = new MenuEntry("LIVE");
			MenuEntry systemLinkMenuEntry = new MenuEntry("System Link");
			MenuEntry exitMenuEntry = new MenuEntry("Exit");

			// Hook up menu event handlers.
			singlePlayerMenuEntry.Selected += SinglePlayerMenuEntrySelected;
			liveMenuEntry.Selected += LiveMenuEntrySelected;
			systemLinkMenuEntry.Selected += SystemLinkMenuEntrySelected;
			exitMenuEntry.Selected += OnCancel;

			// Add entries to the menu.
			MenuEntries.Add(singlePlayerMenuEntry);
			MenuEntries.Add(liveMenuEntry);
			MenuEntries.Add(systemLinkMenuEntry);
			MenuEntries.Add(exitMenuEntry);
		}


		#endregion

		#region Handle Input


		/// <summary>
		/// Event handler for when the Single Player menu entry is selected.
		/// </summary>
		void SinglePlayerMenuEntrySelected(object sender, EventArgs e)
		{
			LoadingScreen.Load(ScreenManager, true, new GameplayScreen(null));
		}


		/// <summary>
		/// Event handler for when the Live menu entry is selected.
		/// </summary>
		void LiveMenuEntrySelected(object sender, EventArgs e)
		{
			CreateOrFindSession(NetworkSessionType.PlayerMatch);
		}


		/// <summary>
		/// Event handler for when the System Link menu entry is selected.
		/// </summary>
		void SystemLinkMenuEntrySelected(object sender, EventArgs e)
		{
			CreateOrFindSession(NetworkSessionType.SystemLink);
		}


		/// <summary>
		/// Helper method shared by the Live and System Link menu event handlers.
		/// </summary>
		void CreateOrFindSession(NetworkSessionType sessionType)
		{
			/*// First, we need to make sure a suitable gamer profile is signed in.
			ProfileSignInScreen profileSignIn = new ProfileSignInScreen(sessionType);

			// Hook up an event so once the ProfileSignInScreen is happy,
			// it will activate the CreateOrFindSessionScreen.
			profileSignIn.ProfileSignedIn += delegate
			{
				ScreenManager.AddScreen(new CreateOrFindSessionScreen(sessionType));
			};

			// Activate the ProfileSignInScreen.
			ScreenManager.AddScreen(profileSignIn);*/
		}


		/// <summary>
		/// When the user cancels the main menu, ask if they want to exit the sample.
		/// </summary>
		protected override void OnCancel()
		{
			MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen("Are you sure you want to exit Torq2?");

			confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

			ScreenManager.AddScreen(confirmExitMessageBox);
		}


		/// <summary>
		/// Event handler for when the user selects ok on the "are you sure
		/// you want to exit" message box.
		/// </summary>
		void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
		{
			ScreenManager.Game.Exit();
		}


		#endregion
	}
}
