/*using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Graphics.Lights;

namespace Osiris.Graphics.Shadows
{
	public class ParallelSplitShadowMap : ShadowMap
	{
		private int _numSplits;
		private float[] _splitDistances;
		private float _splitSchemeLambda;

		private Texture2D[] _shadowMapTextures;

		private float SplitSchemaLambda
		{
			get { return _splitSchemeLambda; }
			set { _splitSchemeLambda = MathHelper.Clamp(value, 0, 1); }
		}

		private int NumSplits
		{
			get { return _numSplits; }
			set
			{
				_numSplits = value;
				_splitDistances = new float[_numSplits + 1];
				_shadowMapTextures = new Texture2D[_numSplits];
			}
		}

		public override Texture2D[] ShadowMapTextures
		{
			get { return _shadowMapTextures; }
		}

		public ParallelSplitShadowMap(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(IShadowMapService), this);
			this.UpdateOrder = 3000;
			this.NumSplits = 4;
			this.SplitSchemaLambda = 0.5f;
		}

		private ICameraService GetCamera()
		{
			return (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
		}

		public override void Update(GameTime gameTime)
		{
			// position the camera far plane as near as possible
			// to minimize the amount of empty space
			float dynamicProjectionFar = AdjustCameraPlanes();

			// calculate the distances of split planes
			// according to the selected split scheme
			CalculateSplitDistances(dynamicProjectionFar);

			// enable rendering to shadow map texture
			DepthStencilBuffer oldDepthStencilBuffer = this.GraphicsDevice.DepthStencilBuffer;
			this.GraphicsDevice.DepthStencilBuffer = _shadowMapDepthStencilBuffer;
			this.GraphicsDevice.SetRenderTarget(0, _shadowMapRenderTarget);

			// for each split...
			for (int i = 0; i < _numSplits; i++)
			{
				this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

				// get near and far planes for current frustum split
				float near = _splitDistances[i];
				float far = _splitDistances[i + 1];

				// calculate corner points of frustum split.
				// to avoid edge problems
				// TODO: scale the frustum so that it's at least a few pixels larger
				Vector3[] frustumCorners = CalculateFrustumCorners(near, far);

				// calculate view and projection matrices for light
				Matrix lightProjectionMatrix = CalculateLightForFrustum(frustumCorners);

				// loop through all other drawable game components, getting them to draw to the shadow map
				foreach (GameComponent gameComponent in this.Game.Components)
				{
					if (gameComponent is Osiris.DrawableGameComponent)
					{
						Osiris.DrawableGameComponent drawableGameComponent = (Osiris.DrawableGameComponent) gameComponent;
						drawableGameComponent.DrawShadowMap(lightProjectionMatrix, gameTime);
					}
				}

				// extract shadow map texture from render target
				this.GraphicsDevice.ResolveRenderTarget(0);
				_shadowMapTextures[i] = _shadowMapRenderTarget.GetTexture();
				//_shadowMapTextures[i].Save("ShadowMap.dds", ImageFileFormat.Dds);
			}

			// reset render target to back buffer
			this.GraphicsDevice.SetRenderTarget(0, null);
			this.GraphicsDevice.DepthStencilBuffer = oldDepthStencilBuffer;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			// render sprites with textures
			_spriteBatch.Begin();
			for (int i = 0, length = _shadowMapTextures.Length; i < length; i++)
			{
				Rectangle rectangle = new Rectangle(20 + (i * (128 + 20)), 400, 128, 128);
				_spriteBatch.Draw(_shadowMapTextures[i], rectangle, Color.White);
			}
			_spriteBatch.End();

			base.Draw(gameTime);
		}

		#region Helper methods

		/// <summary>
		/// Calculates the view and projection matrix for a light. The projection
		/// matrix is "zoomed in" on the given frustum split.
		/// </summary>
		/// <param name="pCorners"></param>
		private Matrix CalculateLightForFrustum(Vector3[] corners)
		{
			// create standard view and projection matrices for light
			ILightService light = (ILightService) this.Game.Services.GetService(typeof(ILightService));
			Matrix lightViewProjectionMatrix = light.ViewProjectionMatrix;

			// find min and max values of current frustum split in light's post-projection space
			// (where coordinate range is from -1 to 1)
			float maxX = float.MinValue;
			float maxY = float.MinValue;
			float minX = float.MaxValue;
			float minY = float.MaxValue;
			float maxZ = 0;

			// for each corner point
			for(int i = 0, length = corners.Length; i < length; i++)
			{
				// transform point
				Vector4 transformedCorner = Vector4.Transform(corners[i], lightViewProjectionMatrix);

				// project x and y
				transformedCorner.X /= transformedCorner.W;
				transformedCorner.Y /= transformedCorner.W;

				// find min and max values
				if (transformedCorner.X > maxX) maxX = transformedCorner.X;
				if (transformedCorner.Y > maxY) maxY = transformedCorner.Y;
				if (transformedCorner.Y < minY) minY = transformedCorner.Y;
				if (transformedCorner.X < minX) minX = transformedCorner.X;

				// find largest z distance
				if (transformedCorner.Z > maxZ) maxZ = transformedCorner.Z;
			}

			// set values to valid range (post-projection)
			maxX = MathHelper.Clamp(maxX, -1.0f, 1.0f);
			maxY = MathHelper.Clamp(maxY, -1.0f, 1.0f);
			minX = MathHelper.Clamp(minX, -1.0f, 1.0f);
			minY = MathHelper.Clamp(minY, -1.0f, 1.0f);

			// adjust the far plane of the light to be at the farthest
			// point of the frustum split. some bias may be necessary.
			float dynamicLightFar = maxZ + light.ProjectionNear + 1.5f;
		  
			// re-calculate light's matrices with the new far plane
			Matrix lightProjectionMatrix = light.CreateProjectionMatrix(light.ProjectionNear, dynamicLightFar);

			// next we build a special matrix for cropping the lights view
			// to only contain points of the current frustum split

			float scaleX = 2.0f / (maxX - minX);
			float scaleY = 2.0f / (maxY - minY);

			float offsetX = -0.5f * (maxX + minX) * scaleX;
			float offsetY = -0.5f * (maxY + minY) * scaleY;

			Matrix cropView = Matrix.CreateTranslation(offsetX, offsetY, 0)
				* Matrix.CreateScale(scaleX, scaleY, 1);

			// multiply the projection matrix with it
			lightProjectionMatrix *= cropView;

			// finally modify projection matrix for linearized depth
			lightProjectionMatrix.M33 /= dynamicLightFar;
			lightProjectionMatrix.M43 /= dynamicLightFar;

			return lightProjectionMatrix;
		}

		/// <summary>
		/// Calculates the frustum split distances, or in other
		/// words, the near and far planes for each split.
		/// </summary>
		private void CalculateSplitDistances(float dynamicCameraFar)
		{
			ICameraService camera = GetCamera();
 
			// Practical split scheme:
			//
			// CLi = n*(f/n)^(i/numsplits)
			// CUi = n + (f-n)*(i/numsplits)
			// Ci = CLi*(lambda) + CUi*(1-lambda)
			//
			// lambda scales between logarithmic and uniform

			for (int i = 1; i < _numSplits; i++)
			{
				float iDivM = i / (float) _numSplits;
				float log = camera.ProjectionNear * MathsHelper.Pow((dynamicCameraFar / camera.ProjectionNear), iDivM);
				float uniform = camera.ProjectionNear + (dynamicCameraFar - camera.ProjectionNear) * iDivM;
				_splitDistances[i] = MathHelper.Lerp(log, uniform, _splitSchemeLambda);				
			}

			// make sure border values are right
			_splitDistances[0] = camera.ProjectionNear;
			_splitDistances[_numSplits] = dynamicCameraFar;
		}

		/// <summary>
		/// Computes corner points of a frustum
		/// </summary>
		/// <returns></returns>
		private Vector3[] CalculateFrustumCorners(float near, float far)
		{
			ICameraService camera = GetCamera();
			Matrix projectionMatrix = camera.CreateProjectionMatrix(near, far);
			BoundingFrustum boundingFrustum = new BoundingFrustum(camera.ViewMatrix * projectionMatrix);
			Vector3[] corners = boundingFrustum.GetCorners();
			return corners;
		}

		/// <summary>
		/// Adjusts the camera planes to contain the visible scene
		/// as tightly as possible. This implementation is not very
		/// accurate.
		/// </summary>
		/// <returns>
		/// Dynamic value to use as the "far" distance for camera projection
		/// </returns>
		private float AdjustCameraPlanes()
		{
			// get view matrix
			ICameraService camera = GetCamera();
			Matrix viewMatrix = camera.ViewMatrix;

			// transform corners of scene's AABB by view matrix, to find the most distant point in view-space
			Vector3[] transformedCorners = new Vector3[BoundingBox.CornerCount];
			ISceneService scene = (ISceneService) this.Game.Services.GetService(typeof(ISceneService));
			Vector3.Transform(scene.BoundingBox.GetCorners(), ref viewMatrix, transformedCorners);
			float minZ = float.MaxValue;
			foreach (Vector3 transformedCorner in transformedCorners)
			{
				// find largest Z
				if (transformedCorner.Z < minZ)
					minZ = transformedCorner.Z;
			}

			// return dynamic "projection far" value
			return -minZ + camera.ProjectionNear;
		}

		#endregion
	}
}
*/