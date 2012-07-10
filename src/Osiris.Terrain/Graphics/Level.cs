using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.Graphics
{
	/// <summary>
	/// Summary description for Level.
	/// </summary>
	public class Level
	{
		#region Fields

		private readonly IndexBuffer[] _indexBuffers;
		private readonly float _maximumDelta;
		
		#endregion

		#region Properties

		public int NeighboursCode { get; set; }

		public float MinimumDSq { get; private set; }

		#endregion

		#region Constructors

		#region Instance constructor

		internal Level(IndexBuffer[] indexBuffers, float maximumDelta)
		{
			_indexBuffers = indexBuffers;
			_maximumDelta = maximumDelta;
		}

		#endregion

		#endregion

		#region Methods

		public void Initialize(float tau, ICameraService camera, GraphicsDevice graphicsDevice)
		{
			RecalculateMinimumD(tau, camera, graphicsDevice);
		}

		private void RecalculateMinimumD(float tau, ICameraService camera, GraphicsDevice graphicsDevice)
		{
			// precalculate C
			float fA = camera.ProjectionNear / Math.Abs(camera.ProjectionTop); // 2
			float fT = 2 * tau / (float)graphicsDevice.Viewport.Height; // 0.01333
			float fC = fA / fT; // 150

			// we now have maximum delta
			float fMinimumD = _maximumDelta * fC;
			MinimumDSq = fMinimumD * fMinimumD;
		}

		public void Draw(int numVertices)
		{
			IndexBuffer indexBuffer = _indexBuffers[NeighboursCode];
			indexBuffer.GraphicsDevice.Indices = indexBuffer;

			int primitiveCount = indexBuffer.IndexCount - 2;
			indexBuffer.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
				0,               // base vertex
				0,               // min vertex index
				numVertices,     // total num vertices - note that is NOT just vertices that are indexed, but all vertices
				0,               // start index
				primitiveCount); // primitive count
		}


		#endregion
	}
}
