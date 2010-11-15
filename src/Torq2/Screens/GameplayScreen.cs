using System;
using Osiris.ScreenManager;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Threading;
using Torq2.GameComponents;
using System.Collections.Generic;

namespace Torq2.Screens
{
	/// <summary>
	/// This screen implements the actual game logic. It is just a
	/// placeholder to get the idea across: you'll probably want to
	/// put some more interesting gameplay in here!
	/// </summary>
	public class GameplayScreen : GameScreen
	{
		#region Fields

		NetworkSession networkSession;
		private Race _race;
		private List<GameComponent> _raceComponents;
		private bool wonScreenAlreadyShowing;

		#endregion

		#region Properties


		/// <summary>
		/// The logic for deciding whether the game is paused depends on whether
		/// this is a networked or single player game. If we are in a network session,
		/// we should go on updating the game even when the user tabs away from us or
		/// brings up the pause menu, because even though the local player is not
		/// responding to input, other remote players may not be paused. In single
		/// player modes, however, we want everything to pause if the game loses focus.
		/// </summary>
		new bool IsActive
		{
			get
			{
				if (networkSession == null)
				{
					// Pause behavior for single player games.
					return base.IsActive;
				}
				else
				{
					// Pause behavior for networked games.
					return !IsExiting;
				}
			}
		}


		#endregion

		#region Initialization


		/// <summary>
		/// Constructor.
		/// </summary>
		public GameplayScreen(NetworkSession networkSession)
		{
			this.networkSession = networkSession;

			TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}


		/// <summary>
		/// Load graphics content for the game.
		/// </summary>
		public override void LoadContent()
		{
			int counter = 0;

			_raceComponents = new List<GameComponent>();

			_raceComponents.Add(new Osiris.Audio.AudioManager(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Graphics.Shadows.SimpleShadowMap(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Graphics.SceneManager(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Graphics.Lights.SimpleSunlight(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Logger(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Input.InputManager(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Sky.Atmosphere(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Maths.Noise(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			//Osiris.Graphics.Cameras.TrackerCamera camera = new Osiris.Graphics.Cameras.TrackerCamera(ScreenManager.Game);
			Osiris.Graphics.Cameras.Camera camera = new Osiris.Graphics.Cameras.GodCamera(ScreenManager.Game, new Vector3(5, 140, -250));
			_raceComponents.Add(camera);
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			Osiris.Terrain.GeoClipMapping.GeoClipMappedTerrain terrain = new Osiris.Terrain.GeoClipMapping.GeoClipMappedTerrain(ScreenManager.Game);
			terrain.Viewer = camera;
			_raceComponents.Add(terrain);
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			GameComponents.Vehicle vehicle = new GameComponents.Vehicle(ScreenManager.Game, new Vector3(0, 140, -200));
			_raceComponents.Add(vehicle);
			//camera.Target = vehicle;
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_raceComponents.Add(new Osiris.Graphics.FrameRateCounter(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			//this.Components.Add(new Osiris.Gui.ConsoleMenu(this));
			_raceComponents.Add(new Osiris.Sky.SkyDome(ScreenManager.Game));
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			_race = new Torq2.GameComponents.Race(ScreenManager.Game, vehicle);
			_raceComponents.Add(_race);
			ScreenManager.Game.Components.Add(_raceComponents[counter++]);

			/*if (content == null)
				content = new ContentManager(ScreenManager.Game.Services, "Content");*/

			//gameFont = content.Load<SpriteFont>("gamefont");

			// A real game would probably have more content than this sample, so
			// it would take longer to load. We simulate that by delaying for a
			// while, giving you a chance to admire the beautiful loading screen.
			//Thread.Sleep(1000);
		}


		/// <summary>
		/// Unload graphics content used by the game.
		/// </summary>
		public override void UnloadContent()
		{
			//content.Unload();
		}


		#endregion

		#region Update and Draw


		/// <summary>
		/// Updates the state of the game.
		/// </summary>
		public override void Update(GameTime gameTime, bool otherScreenHasFocus,
																									 bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			
		}

		private void ExitRace(object sender, EventArgs e)
		{
			foreach (GameComponent gameComponent in _raceComponents)
			{
				gameComponent.Enabled = false;
				ScreenManager.Game.Components.Remove(gameComponent);
			}
		}


		/// <summary>
		/// Lets the game respond to player input. Unlike the Update method,
		/// this will only be called when the gameplay screen is active.
		/// </summary>
		public override void HandleInput(InputState input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			if (input.PauseGame)
			{
				// If they pressed pause, bring up the pause menu screen.
				PauseMenuScreen screen = new PauseMenuScreen(networkSession);
				screen.ExitRace += ExitRace;
				ScreenManager.AddScreen(screen);
			}

			if (_race.Won && !wonScreenAlreadyShowing)
			{
				WonMenuScreen screen = new WonMenuScreen(networkSession);
				ScreenManager.AddScreen(screen);
				ExitRace(this, EventArgs.Empty);

				wonScreenAlreadyShowing = true;
			}
		}


		/// <summary>
		/// Draws the gameplay screen.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			// This game has a blue background. Why? Because!
			//ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
			//																 Color.CornflowerBlue, 0, 0);

			/*// Our player and enemy are both actually just text strings.
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

			if (networkSession != null)
			{
				string message = "Players: " + networkSession.AllGamers.Count;
				Vector2 messagePosition = new Vector2(100, 480);
				spriteBatch.DrawString(gameFont, message, messagePosition, Color.White);
			}

			spriteBatch.End();*/

			// If the game is transitioning on or off, fade it out to black.
			if (TransitionPosition > 0)
				ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
		}


		#endregion
	}
}
