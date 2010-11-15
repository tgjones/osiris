using System;
using Osiris.ScreenManager.Screens;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework;
using Torq2.Net;

namespace Torq2.Screens
{
	/// <summary>
	/// The pause menu comes up over the top of the game,
	/// giving the player options to resume or quit.
	/// </summary>
	class WonMenuScreen : MenuScreen
	{
		#region Fields

		NetworkSession networkSession;
		public event EventHandler<EventArgs> ExitRace;

		#endregion

		#region Initialization


		/// <summary>
		/// Constructor.
		/// </summary>
		public WonMenuScreen(NetworkSession networkSession)
			: base("You have won")
		{
			this.networkSession = networkSession;

			// Flag that there is no need for the game to transition
			// off when the pause menu is on top of it.
			IsPopup = true;

			if (networkSession == null)
			{
				// If this is a single player game, add the Quit menu entry.
				MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");
				quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
				MenuEntries.Add(quitGameMenuEntry);
			}
			else
			{
				// If we are hosting a network game, add the Return to Lobby menu entry.
				if (networkSession.IsHost)
				{
					MenuEntry lobbyMenuEntry = new MenuEntry("Return to Lobby");
					lobbyMenuEntry.Selected += ReturnToLobbyMenuEntrySelected;
					MenuEntries.Add(lobbyMenuEntry);
				}

				// Add the End/Leave Session menu entry.
				string leaveEntryText = networkSession.IsHost ? "End Session" :
																												"Leave Session";

				MenuEntry leaveSessionMenuEntry = new MenuEntry(leaveEntryText);
				leaveSessionMenuEntry.Selected += LeaveSessionMenuEntrySelected;
				MenuEntries.Add(leaveSessionMenuEntry);
			}
		}


		#endregion

		#region Handle Input


		/// <summary>
		/// Event handler for when the Quit Game menu entry is selected.
		/// </summary>
		void QuitGameMenuEntrySelected(object sender, EventArgs e)
		{
			if (ExitRace != null)
				ExitRace(sender, e);

			LoadingScreen.Load(ScreenManager, false, new BackgroundScreen(),
																							 new MainMenuScreen());
		}


		/// <summary>
		/// Event handler for when the Return to Lobby menu entry is selected.
		/// </summary>
		void ReturnToLobbyMenuEntrySelected(object sender, EventArgs e)
		{
			if (ExitRace != null)
				ExitRace(sender, e);

			if (networkSession.SessionState == NetworkSessionState.Playing)
			{
				networkSession.EndGame();
			}
		}


		/// <summary>
		/// Event handler for when the End/Leave Session menu entry is selected.
		/// </summary>
		void LeaveSessionMenuEntrySelected(object sender, EventArgs e)
		{
			if (ExitRace != null)
				ExitRace(sender, e);

			NetworkSessionComponent.LeaveSession(ScreenManager);
		}


		#endregion

		#region Draw


		/// <summary>
		/// Draws the pause menu screen. This darkens down the gameplay screen
		/// that is underneath us, and then chains to the base MenuScreen.Draw.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

			base.Draw(gameTime);
		}


		#endregion
	}
}
