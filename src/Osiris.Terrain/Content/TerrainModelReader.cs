using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Terrain.Graphics;

namespace Osiris.Terrain.Content
{
	public class TerrainModelReader : ContentTypeReader<TerrainModel>
	{
		protected override TerrainModel Read(ContentReader input, TerrainModel existingInstance)
		{
			int numPatchesX = input.ReadInt32();
			int numPatchesY = input.ReadInt32();

			// Load patches.
			Patch[,] patches = new Patch[numPatchesX,numPatchesY];
			for (int y = 0; y < numPatchesY; ++y)
				for (int x = 0; x < numPatchesX; ++x)
					patches[x, y] = input.ReadObject<Patch>();

			Func<int, int, Patch> getPatch = (x, y) =>
			{
				if (x < 0 || x > numPatchesX - 1 || y < 0 || y > numPatchesY - 1)
					return null;
				return patches[x, y];
			};

			// Now set patch neighbours.
			for (int y = 0; y < numPatchesY; y++)
				for (int x = 0; x < numPatchesX; x++)
					patches[x, y].SetNeighbours(
						getPatch(x - 1, y),
						getPatch(x + 1, y),
						getPatch(x, y - 1),
						getPatch(x, y + 1));

			HeightMap heightMap = input.ReadObject<HeightMap>();

			float tau = input.ReadSingle();

			Effect effect = input.ReadObject<Effect>();

			return new TerrainModel(numPatchesX, numPatchesY, patches, heightMap, tau, effect);
		}
	}
}