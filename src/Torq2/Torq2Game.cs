using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Osiris.Graphics;
using Osiris;
using Osiris.ScreenManager;
using Torq2.Screens;
using Microsoft.Xna.Framework.GamerServices;

namespace Torq2
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Torq2Game : OsirisGame
	{
		public Torq2Game()
			: base(false)
		{
			ScreenManager screenManager = new ScreenManager(this);

			Components.Add(screenManager);
			Components.Add(new MessageDisplayComponent(this));
			Components.Add(new GamerServicesComponent(this));

			// Activate the first screens.
			screenManager.AddScreen(new BackgroundScreen());
			screenManager.AddScreen(new MainMenuScreen());	
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			KeyboardState pKeyboardState = Keyboard.GetState();
			if (pKeyboardState.IsKeyDown(Keys.F11))
				ScreenshotManager.TakeScreenshot(GraphicsDevice);
		}
	}
}
