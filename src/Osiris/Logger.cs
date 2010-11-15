using System;
using Osiris.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Osiris
{
	public class Logger : Microsoft.Xna.Framework.DrawableGameComponent, ILoggerService
	{
		private SpriteBatch _spriteBatch;
		private SpriteFont _spriteFont;
		private List<string> _messages;

		public Logger(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(ILoggerService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);

			this.UpdateOrder = 1;
			this.DrawOrder = 100;

			_messages = new List<string>();
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(ILoggerService));
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(this.GraphicsDevice);
			_spriteFont = AssetLoader.LoadAsset<SpriteFont>(@"LoggerFont", this.Game);
		}

		public override void Update(GameTime gameTime)
		{
			_messages.Clear();
		}

		public void WriteLine(string message)
		{
			_messages.Add(message);
		}

		public override void Draw(GameTime gameTime)
		{
			this.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

			int y = 60;
			_spriteBatch.Begin();
			foreach (string message in _messages)
			{
				_spriteBatch.DrawString(_spriteFont, message, new Vector2(33, y), Color.Black);
				_spriteBatch.DrawString(_spriteFont, message, new Vector2(32, y), Color.White);
				y += 33;
			}
			_spriteBatch.End();
		}
	}
}
