using System;
using Osiris.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics
{
	public class FrameRateCounter : Microsoft.Xna.Framework.DrawableGameComponent
	{
		private SpriteBatch _spriteBatch;
		private SpriteFont _spriteFont;
		private int _frameRate = 0;
		private int _frameCounter = 0;
		private TimeSpan _elapsedTime = TimeSpan.Zero;

		public FrameRateCounter(Game game)
			: base(game)
		{
			this.DrawOrder = 100;
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(this.GraphicsDevice);
			_spriteFont = AssetLoader.LoadAsset<SpriteFont>(@"Graphics\FrameRateFont", this.Game);
		}

		public override void Update(GameTime gameTime)
		{
			_elapsedTime += gameTime.ElapsedGameTime;

			if (_elapsedTime > TimeSpan.FromSeconds(1))
			{
				_elapsedTime -= TimeSpan.FromSeconds(1);
				_frameRate = _frameCounter;
				_frameCounter = 0;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			_frameCounter++;

			string fps = string.Format("FPS: {0}", _frameRate);

			this.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

			_spriteBatch.Begin();
			_spriteBatch.DrawString(_spriteFont, fps, new Vector2(33, 33), Color.Black);
			_spriteBatch.DrawString(_spriteFont, fps, new Vector2(32, 32), Color.White);
			_spriteBatch.End();
		}
	}
}
