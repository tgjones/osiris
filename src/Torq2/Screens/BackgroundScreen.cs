using System;
using Osiris.Content;
using Microsoft.Xna.Framework.Graphics;
using Osiris.ScreenManager;
using Microsoft.Xna.Framework;

namespace Torq2.Screens
{
	/// <summary>
	/// The background screen sits behind all the other menu screens.
	/// It draws a background image that remains fixed in place regardless
	/// of whatever transitions the screens on top of it may be doing.
	/// </summary>
	class BackgroundScreen : GameScreen
	{
		#region Fields

		Texture2D backgroundTexture;

		#endregion

		#region Initialization


		/// <summary>
		/// Constructor.
		/// </summary>
		public BackgroundScreen()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}


		/// <summary>
		/// Loads graphics content for this screen. The background texture is quite
		/// big, so we use our own local ContentManager to load it. This allows us
		/// to unload before going from the menus into the game itself, wheras if we
		/// used the shared ContentManager provided by the Game class, the content
		/// would remain loaded forever.
		/// </summary>
		public override void LoadContent()
		{
			backgroundTexture = AssetLoader.LoadAsset<Texture2D>("background", ScreenManager.Game);
		}

		#endregion

		#region Update and Draw

		/// <summary>
		/// Updates the background screen. Unlike most screens, this should not
		/// transition off even if it has been covered by another screen: it is
		/// supposed to be covered, after all! This overload forces the
		/// coveredByOtherScreen parameter to false in order to stop the base
		/// Update method wanting to transition off.
		/// </summary>
		public override void Update(GameTime gameTime, bool otherScreenHasFocus,
																									 bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, false);
		}


		/// <summary>
		/// Draws the background screen.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
			byte fade = TransitionAlpha;

			spriteBatch.Begin(SpriteBlendMode.None);

			spriteBatch.Draw(backgroundTexture, fullscreen,
											 new Color(fade, fade, fade));

			spriteBatch.End();
		}


		#endregion
	}
}
