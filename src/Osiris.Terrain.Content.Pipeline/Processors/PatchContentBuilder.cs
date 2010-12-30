using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Processors
{
	public class PatchContentBuilder
	{
		private readonly int _patchSize;
		private readonly int _patchOffsetX;
		private readonly int _patchOffsetY;
		private readonly HeightMapContent _heightMap;
		private readonly int _numLevels;
		private readonly int _detailTextureTiling;
		private readonly int _horizontalScale;

		public PatchContentBuilder(int patchSize, int patchOffsetX, int patchOffsetY, HeightMapContent heightMap, int numLevels, int detailTextureTiling, int horizontalScale)
		{
			_patchSize = patchSize;
			_patchOffsetX = patchOffsetX;
			_patchOffsetY = patchOffsetY;
			_heightMap = heightMap;
			_numLevels = numLevels;
			_detailTextureTiling = detailTextureTiling;
			_horizontalScale = horizontalScale;
		}

		public PatchContent Build()
		{
			#region Local vertex buffer

			// create local vertex buffer for patch
			int numVertices = _patchSize * _patchSize;
			VertexBufferContent localVertexBuffer = new VertexBufferContent(numVertices);

			// fill vertex buffer
			VertexPositionNormalTexture2[] vertices = new VertexPositionNormalTexture2[numVertices];

			int nStartX = _patchOffsetX * (_patchSize - 1);
			int nStartY = _patchOffsetY * (_patchSize - 1);
			int nEndX = nStartX + _patchSize;
			int nEndY = nStartY + _patchSize;

			float fMinZ = float.MaxValue, fMaxZ = float.MinValue;
			int index = 0;
			for (int y = nStartY; y < nEndY; y++)
			{
				for (int x = nStartX; x < nEndX; x++)
				{
					// write local data
					float fZ = _heightMap[x, y];

					if (fZ < fMinZ) fMinZ = fZ;
					if (fZ > fMaxZ) fMaxZ = fZ;

					Vector2 texCoords1 = new Vector2(x / (float) (_heightMap.Width - 1), y / (float) (_heightMap.Height - 1));
					Vector2 texCoords2 = texCoords1 * _detailTextureTiling;

					vertices[index++] = new VertexPositionNormalTexture2(
						new Vector3(x * _horizontalScale, fZ, y * _horizontalScale),
						new Vector3(0, 1, 0),
						texCoords1,
						texCoords2);
				}
			}

			localVertexBuffer.Write(0, VertexBufferContent.SizeOf(typeof(VertexPositionNormalTexture2)), vertices);
			localVertexBuffer.VertexDeclaration = new VertexDeclarationContent();
			foreach (VertexElement vertexElement in VertexPositionNormalTexture2.VertexDeclaration.GetVertexElements())
				localVertexBuffer.VertexDeclaration.VertexElements.Add(vertexElement);

			#endregion

			LevelContent[] levels = new LevelContent[_numLevels];
			for (int i = 0; i < _numLevels; i++)
			{
				LevelContentBuilder levelContentBuilder = new LevelContentBuilder(_heightMap, _patchSize, _numLevels, i, nStartX,
					nEndX, nStartY, nEndY);
				levels[i] = levelContentBuilder.Build();
			}

			#region Bounding box, centre, and offset

			BoundingBox boundingBox = new BoundingBox(
				new Vector3(nStartX * _horizontalScale, fMinZ, nStartY * _horizontalScale),
				new Vector3(nEndX * _horizontalScale, fMaxZ, nEndY * _horizontalScale));

			float fAverageZ = (fMinZ + fMaxZ) / 2.0f;

			Vector3 center = new Vector3(
				(nStartX + ((_patchSize - 1) / 2.0f)) * _horizontalScale,
				fAverageZ,
				(nStartY + ((_patchSize - 1) / 2.0f)) * _horizontalScale);

			Vector2 offset = new Vector2(
				(_patchOffsetX * (_patchSize - 1)) * _horizontalScale,
				(_patchOffsetY * (_patchSize - 1)) * _horizontalScale);

			#endregion

			return new PatchContent
			{
				VertexBuffer = localVertexBuffer,
				Levels = levels,
				BoundingBox  =boundingBox,
				Center = center,
				Offset = offset
			};
		}
	}
}