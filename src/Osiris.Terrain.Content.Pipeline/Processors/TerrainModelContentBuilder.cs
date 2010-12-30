using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Processors
{
	public class TerrainModelContentBuilder
	{
		private readonly int _patchSize;
		private readonly float _tau;
		private readonly HeightMapContent _heightMap;
		private readonly DualTextureMaterialContent _material;
		private readonly int _detailTextureTiling;
		private readonly int _horizontalScale;

		public TerrainModelContentBuilder(int patchSize, float tau, HeightMapContent heightMap, DualTextureMaterialContent material, int detailTextureTiling, int horizontalScale)
		{
			_patchSize = patchSize;
			_tau = tau;
			_heightMap = heightMap;
			_material = material;
			_detailTextureTiling = detailTextureTiling;
			_horizontalScale = horizontalScale;
		}

		public TerrainModelContent Build(ContentProcessorContext context)
		{
			// TODO: i think numlevels is log2, or something like that

			// calculate number of levels, based on patch size
			int nCurrent = (_patchSize - 1) * 2;
			int numLevels = 0;
			while (nCurrent != 1)
			{
				nCurrent /= 2;
				numLevels++;
			}

			int numPatchesX = (_heightMap.Width - 1) / (_patchSize - 1);
			int numPatchesY = (_heightMap.Height - 1) / (_patchSize - 1);

			// create patches
			PatchContent[,] patches = new PatchContent[numPatchesX, numPatchesY];
			for (int y = 0; y < numPatchesY; y++)
				for (int x = 0; x < numPatchesX; x++)
				{
					PatchContentBuilder patchContentBuilder = new PatchContentBuilder(_patchSize, x, y, _heightMap, numLevels, _detailTextureTiling, _horizontalScale);
					patches[x, y] = patchContentBuilder.Build();
				}

			return new TerrainModelContent
			{
				NumPatchesX = numPatchesX,
				NumPatchesY = numPatchesY,
				Patches = patches,
				HeightMap = _heightMap,
				Tau = _tau,
				Material = _material
			};
		}
	}
}