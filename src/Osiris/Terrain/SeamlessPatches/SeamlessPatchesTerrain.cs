using System;
using Microsoft.Xna.Framework;
using Osiris.Graphics.Cameras;

namespace Osiris.Terrain.SeamlessPatches
{
	public class SeamlessPatchesTerrain : Microsoft.Xna.Framework.DrawableGameComponent
	{
		private Game _game;
		private Patch _root;

		public SeamlessPatchesTerrain(Game game) : base(game)
		{
			_game = game;
			_root = new Patch(new BoundingSquare(IntVector2.Zero, new IntVector2(64, -64)));
		}

		protected override void LoadContent()
		{
			base.LoadContent();
			Patch.LoadContent(_game);
		}

		public override void Update(GameTime gameTime)
		{
			ICameraService camera = (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
			_root.Update(camera.Position);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			ICameraService camera = (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
			Patch.Effect.SetValue("WorldViewProjection", Matrix.Transpose(camera.ViewProjectionMatrix));

			_root.Draw();

			base.Draw(gameTime);
		}
	}
}
