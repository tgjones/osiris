using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Content;
using Osiris.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Graphics.Lights;
using Osiris.Graphics.Shadows;

namespace Osiris
{
	public abstract class DrawableGameComponent : Microsoft.Xna.Framework.DrawableGameComponent
	{
		private bool _nestedComponent;
		private string _effectAssetName;
		private bool _castsShadow, _receivesShadow;
		private Matrix _textureScaleAndOffsetMatrix;

		protected Matrix _worldMatrix;
		protected ExtendedEffect _effect;

		public bool IsAlwaysVisible = false;

		private VertexDeclaration _screenSpaceQuadVertexDeclaration;

		public abstract BoundingBox BoundingBox
		{
			get;
		}

		public Matrix WorldMatrix
		{
			set { _worldMatrix = value; }
		}

		public Matrix TextureScaleAndOffsetMatrix
		{
			get { return _textureScaleAndOffsetMatrix; }
		}

		public DrawableGameComponent(Game game, string effectAssetName, bool castsShadow, bool receivesShadow, bool nestedComponent)
			: base(game)
		{
			_nestedComponent = nestedComponent;
			_effectAssetName = effectAssetName;
			_castsShadow = castsShadow;
			_receivesShadow = receivesShadow;

			_worldMatrix = Matrix.Identity;

			this.UpdateOrder = 1000;
		}

		protected T GetService<T>()
		{
			return (T) this.Game.Services.GetService(typeof(T));
		}

		protected override void LoadContent()
		{
			if (!_nestedComponent)
			{
				_effect = new ExtendedEffect(AssetLoader.LoadAsset<Effect>(_effectAssetName, this.Game).Clone(this.GraphicsDevice));

				ILightService light = (ILightService)this.Game.Services.GetService(typeof(ILightService));
				if (light != null)
				{
					_effect.SetValue("LightDiffuse", light.Diffuse);
					_effect.SetValue("LightAmbient", light.Ambient);
				}

				if (_receivesShadow)
				{
					IShadowMapService shadowMap = (IShadowMapService) this.Game.Services.GetService(typeof(IShadowMapService));
					if (shadowMap != null)
					{
						_effect.SetValue("ShadowMapSize", shadowMap.ShadowMapSize);
						_effect.SetValue("ShadowMapSizeInverse", 1.0f / (float) shadowMap.ShadowMapSize);

						float offset = 0.5f + (0.5f / (float) shadowMap.ShadowMapSize);
						_textureScaleAndOffsetMatrix = new Matrix(
							0.5f, 0.0f, 0.0f, 0.0f,
							0.0f, -0.5f, 0.0f, 0.0f,
							0.0f, 0.0f, 1.0f, 0.0f,
							offset, offset, 0.0f, 1.0f);
					}
				}
			}

			_screenSpaceQuadVertexDeclaration = new VertexDeclaration(GraphicsDevice, ShortVector2.VertexElements);

			base.LoadContent();
		}

		public void DrawShadowMap(Matrix lightProjectionMatrix, GameTime gameTime)
		{
			if (_castsShadow && !_nestedComponent)
			{
				ILightService light = (ILightService) this.Game.Services.GetService(typeof(ILightService));
				_effect.SetValue("LightWorldViewProjection", _worldMatrix * light.ViewMatrix * lightProjectionMatrix);
				_effect.ChangeTechnique("ShadowMap");
				DrawComponent(gameTime, true);
			}
		}

		public sealed override void Update(GameTime gameTime)
		{
			if (!_nestedComponent)
			{
				ILightService light = (ILightService) this.Game.Services.GetService(typeof(ILightService));
				if (light != null)
					_effect.SetValue("LightDirection", light.Direction);
			}

			UpdateComponent(gameTime);

			// update frustum visibility
			if (!IsAlwaysVisible)
			{
				ICameraService camera = this.GetService<ICameraService>();
				if (this is Osiris.Terrain.GeoMipMapping.Patch)
					((Osiris.Terrain.GeoMipMapping.Patch) this).Visible = (camera.BoundingFrustum.Contains(this.BoundingBox) != ContainmentType.Disjoint);
				else
					this.Visible = (camera.BoundingFrustum.Contains(this.BoundingBox) != ContainmentType.Disjoint);
				/*BoundingBox boundingBox = this.BoundingBox; ContainmentType containmentType;
				camera.BoundingFrustum.Contains(ref boundingBox, out containmentType);
				this.Visible = containmentType != ContainmentType.Disjoint;*/
			}

			base.Update(gameTime);
		}

		public sealed override void Draw(GameTime gameTime)
		{
			if (!_nestedComponent)
			{
				ICameraService camera = GetService<ICameraService>();
				_effect.SetValue("WorldViewProjection", _worldMatrix * camera.ViewProjectionMatrix);

				IShadowMapService shadowMap = GetService<IShadowMapService>();
				if (_receivesShadow && shadowMap != null)
				{
					ILightService light = GetService<ILightService>();
					_effect.SetValue("ShadowMapProjector", _worldMatrix * light.ViewMatrix * light.ProjectionMatrix * _textureScaleAndOffsetMatrix);

					// render geometry with shadow
					_effect.SetValue("ShadowMap", shadowMap.ShadowMapTextures[0]);
					_effect.ChangeTechnique("ShadowedScene");
					DrawComponent(gameTime, false);
				}
				else
				{
					_effect.ChangeTechnique("NormalScene");
					DrawComponent(gameTime, false);
				}
			}
			else
			{
				DrawComponent(gameTime, false);
			}

			base.Draw(gameTime);
		}

		protected virtual void UpdateComponent(GameTime gameTime) { }
		protected abstract void DrawComponent(GameTime gameTime, bool shadowPass);

		public Texture2D RenderToTexture(RenderTarget2D renderTarget, ExtendedEffect effect,
			short minVertexX, short maxVertexX, short minVertexY, short maxVertexY)
		{
			GraphicsDevice.SetRenderTarget(0, renderTarget);

			GraphicsDevice.VertexDeclaration = _screenSpaceQuadVertexDeclaration;

			effect.Begin();
			foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
			{
				effectPass.Begin();

				RenderScreenSpaceQuad(
					minVertexX, maxVertexX,
					minVertexY, maxVertexY);

				effectPass.End();
			}
			effect.End();

			GraphicsDevice.SetRenderTarget(0, null);

			return renderTarget.GetTexture();
		}

		/// <summary>
		/// This is an exercise in mapping between three different coordinate systems.
		/// Let's say the elevation texture size is 7.
		/// 
		/// The first coordinate system is the one we define our quad in:
		/// (0, 0)               (7, 0)
		/// ---------------------------
		/// |                         |
		/// |                         |
		/// ...                     ...
		/// ---------------------------
		/// (0, 7)               (7, 7)
		/// 
		/// In the vertex shader, this will be mapped to the following
		/// normalized screen-space coordinates:
		/// 
		/// (-1, 1)              (1, 1)
		/// ---------------------------
		/// |                         |
		/// |                         |
		/// ...                     ...
		/// ---------------------------
		/// (-1, -1)            (1, -1)
		///
		/// Finally, D3D will transform normalized screen-space coordinates
		/// to the following rasterized screen coordinates:
		/// 
		/// ---------------------------
		/// |(0, 0)             (6, 0)|
		/// |                         |
		/// ...                     ...
		/// |(0, 6)             (6, 6)|
		/// ---------------------------
		/// </summary>
		private void RenderScreenSpaceQuad(short minVertexX, short maxVertexX, short minVertexY, short maxVertexY)
		{
			ShortVector2[] vertices = new ShortVector2[4];

			// top left
			vertices[0] = new ShortVector2(minVertexX, minVertexY);

			// bottom left
			vertices[1] = new ShortVector2(minVertexX, maxVertexY);

			// top right
			vertices[2] = new ShortVector2(maxVertexX, minVertexY);

			// bottom right
			vertices[3] = new ShortVector2(maxVertexX, maxVertexY);

			GraphicsDevice.DrawUserPrimitives<ShortVector2>(
				PrimitiveType.TriangleStrip,
				vertices, 0, 2);
		}
	}
}
