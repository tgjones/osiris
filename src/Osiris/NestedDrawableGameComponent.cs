using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;

namespace Osiris
{
	public abstract class NestedDrawableGameComponent
	{
		private Game _game;

		public Game Game
		{
			get { return _game; }
		}

		public GraphicsDevice GraphicsDevice
		{
			get { return _game.GraphicsDevice; }
		}

		public NestedDrawableGameComponent(Game game)
		{
			_game = game;
		}

		protected T GetService<T>()
		{
			return (T) this.Game.Services.GetService(typeof(T));
		}

		public virtual void Initialize()
		{
			LoadContent();
		}

		protected virtual void LoadContent()
		{

		}

		public virtual void Draw(ExtendedEffect effect)
		{

		}
	}
}
