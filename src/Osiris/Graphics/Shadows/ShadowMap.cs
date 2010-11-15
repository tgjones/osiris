using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Shadows
{
	public abstract class ShadowMap : Microsoft.Xna.Framework.DrawableGameComponent, IShadowMapService
	{
		protected SpriteBatch _spriteBatch;
		protected RenderTarget2D _shadowMapRenderTarget;
		protected DepthStencilBuffer _shadowMapDepthStencilBuffer;

		public abstract Texture2D[] ShadowMapTextures
		{
			get;
		}

		public int ShadowMapSize
		{
			get { return 2048; }
		}

		public ShadowMap(Game game)
			: base(game)
		{

		}

		protected override void LoadContent()
		{
			_shadowMapRenderTarget = new RenderTarget2D(this.GraphicsDevice, this.ShadowMapSize, this.ShadowMapSize, 1, SurfaceFormat.Single);
			_shadowMapDepthStencilBuffer = new DepthStencilBuffer(this.GraphicsDevice, this.ShadowMapSize, this.ShadowMapSize, DepthFormat.Depth24Stencil8);
			_spriteBatch = new SpriteBatch(this.GraphicsDevice);
			base.LoadContent();
		}
	}
}
