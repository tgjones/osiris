using System;
using Microsoft.Xna.Framework;

namespace Osiris.Terrain
{
	public abstract class Terrain : Osiris.DrawableGameComponent, ITerrainService
	{
		public Terrain(Game game, string effectAssetName)
			: base(game, effectAssetName, false, true, false)
		{
			game.Services.AddService(typeof(ITerrainService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(ITerrainService));
		}

		public virtual void GetHeightAndNormalAtPoints(Vector2[] p, out float[] height, out Vector3[] normal)
		{
			throw new NotImplementedException();
		}
	}
}
