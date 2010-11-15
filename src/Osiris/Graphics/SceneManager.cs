using System;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics
{
	public class SceneManager : GameComponent, ISceneService
	{
		private BoundingBox _boundingBox;

		public BoundingBox BoundingBox
		{
			get { return _boundingBox; }
		}

		public SceneManager(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(ISceneService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);
			this.UpdateOrder = 2000;
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(ISceneService));
		}

		public override void Update(GameTime gameTime)
		{
			// create bounding box from points
			bool first = true;
			foreach (Microsoft.Xna.Framework.GameComponent gameComponent in this.Game.Components)
			{
				if (gameComponent is Osiris.DrawableGameComponent)
				{
					DrawableGameComponent drawableGameComponent = (DrawableGameComponent) gameComponent;
					if (!drawableGameComponent.IsAlwaysVisible)
					{
						if (first)
						{
							_boundingBox = drawableGameComponent.BoundingBox;
							first = false;
						}
						else
						{
							_boundingBox = BoundingBox.CreateMerged(_boundingBox, drawableGameComponent.BoundingBox);
						}
					}
				}
			}

			base.Update(gameTime);
		}
	}
}
