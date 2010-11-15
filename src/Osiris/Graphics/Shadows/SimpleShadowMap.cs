using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Graphics.Lights;

namespace Osiris.Graphics.Shadows
{
	public class SimpleShadowMap : ShadowMap
	{
		private Texture2D _shadowMapTexture;

		public override Texture2D[] ShadowMapTextures
		{
			get { return new Texture2D[] { _shadowMapTexture }; }
		}

		public SimpleShadowMap(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(IShadowMapService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);
			this.UpdateOrder = 3000;
			this.DrawOrder = 3000;
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(IShadowMapService));
		}

		private ICameraService GetCamera()
		{
			return (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
		}

		public override void Update(GameTime gameTime)
		{
			// enable rendering to shadow map texture
			DepthStencilBuffer oldDepthStencilBuffer = this.GraphicsDevice.DepthStencilBuffer;
			this.GraphicsDevice.DepthStencilBuffer = _shadowMapDepthStencilBuffer;
			this.GraphicsDevice.SetRenderTarget(0, _shadowMapRenderTarget);

			this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

			// calculate view and projection matrices for light
			ILightService light = (ILightService) this.Game.Services.GetService(typeof(ILightService));
			Matrix lightProjectionMatrix = light.ProjectionMatrix;

			// loop through all other drawable game components, getting them to draw to the shadow map
			foreach (GameComponent gameComponent in this.Game.Components)
			{
				if (gameComponent is Osiris.DrawableGameComponent)
				{
					Osiris.DrawableGameComponent drawableGameComponent = (Osiris.DrawableGameComponent) gameComponent;
					drawableGameComponent.DrawShadowMap(lightProjectionMatrix, gameTime);
				}
			}

			// reset render target to back buffer
			this.GraphicsDevice.SetRenderTarget(0, null);
			this.GraphicsDevice.DepthStencilBuffer = oldDepthStencilBuffer;

			// extract shadow map texture from render target
			_shadowMapTexture = _shadowMapRenderTarget.GetTexture();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			/*if (_shadowMapTexture != null)
			{
				// render sprites with textures
				_spriteBatch.Begin();

				const int size = 256;
				Rectangle rectangle = new Rectangle(20, this.Game.Window.ClientBounds.Height - size - 20, size, size);
				_spriteBatch.Draw(_shadowMapTexture, rectangle, Color.White);

				_spriteBatch.End();
			}*/

			base.Draw(gameTime);
		}
	}
}