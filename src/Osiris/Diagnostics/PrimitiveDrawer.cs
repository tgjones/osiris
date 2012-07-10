using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics
{
	internal class PrimitiveDrawer : IDisposable
	{
		private readonly GraphicsDevice _graphicsDevice;
		private readonly BasicEffect _effect;

		public PrimitiveDrawer(GraphicsDevice graphicsDevice)
		{
			_graphicsDevice = graphicsDevice;
			_effect = new BasicEffect(graphicsDevice)
			{
				LightingEnabled = false
			};
		}

		public void Draw<T>(Matrix world, Matrix view, Matrix projection,
			Color color, Texture2D texture,
			PrimitiveType primitiveType,
			T[] vertices, short[] indices,
			bool enableDepthBuffer)
			where T : struct, IVertexType
		{
			BeginDraw(world, view, projection, color, texture, enableDepthBuffer);

			int primitiveCount = CalculatePrimitiveCount(primitiveType, indices);

			_graphicsDevice.DrawUserIndexedPrimitives(
				primitiveType,
				vertices, 0, vertices.Length,
				indices, 0, primitiveCount);

			EndDraw();
		}

		public void Draw<T>(Matrix world, Matrix view, Matrix projection, Color color, Texture2D texture,
			PrimitiveType primitiveType,
			T[] vertices, bool enableDepthBuffer)
			where T : struct, IVertexType
		{
			BeginDraw(world, view, projection, color, texture, enableDepthBuffer);

			int primitiveCount = CalculatePrimitiveCount(primitiveType, vertices);

			_graphicsDevice.SetVertexBuffer(null);
			_graphicsDevice.DrawUserPrimitives(
				primitiveType,
				vertices, 0, primitiveCount);

			EndDraw();
		}

		private void BeginDraw(Matrix world, Matrix view, Matrix projection,
			Color color, Texture2D texture, bool enableDepthBuffer)
		{
			_effect.World = world;

			_graphicsDevice.RasterizerState = RasterizerState.CullNone;
			_graphicsDevice.BlendState = BlendState.AlphaBlend;
			if (!enableDepthBuffer)
				_graphicsDevice.DepthStencilState = DepthStencilState.None;

			SetEffectParameters(view, projection, color, texture);

			_effect.CurrentTechnique.Passes[0].Apply();
		}

		private void EndDraw()
		{
			_graphicsDevice.DepthStencilState = DepthStencilState.Default;
			_graphicsDevice.BlendState = BlendState.Opaque;
		}

		private static int CalculatePrimitiveCount<T>(PrimitiveType primitiveType, T[] indices)
		{
			switch (primitiveType)
			{
				case PrimitiveType.TriangleList:
					return indices.Length / 3;
				case PrimitiveType.LineList:
					return indices.Length / 2;
				default:
					throw new ArgumentOutOfRangeException("primitiveType");
			}
		}

		private void SetEffectParameters(Matrix view, Matrix projection, Color color, Texture2D texture)
		{
			_effect.View = view;
			_effect.Projection = projection;
			_effect.DiffuseColor = color.ToVector3();
			_effect.Alpha = color.A / 255.0f;
			_effect.VertexColorEnabled = (texture == null);
			_effect.TextureEnabled = (texture != null);
			_effect.Texture = texture;
		}

		public void Dispose()
		{
			_effect.Dispose();
		}
	}
}