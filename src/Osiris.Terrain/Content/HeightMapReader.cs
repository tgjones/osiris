using Microsoft.Xna.Framework.Content;
using Osiris.Terrain.Graphics;

namespace Osiris.Terrain.Content
{
	public class HeightMapReader : ContentTypeReader<HeightMap>
	{
		protected override HeightMap Read(ContentReader input, HeightMap existingInstance)
		{
			int width = input.ReadInt32();
			int height = input.ReadInt32();

			// Load patches.
			float[,] values = new float[width, height];
			for (int y = 0; y < height; ++y)
				for (int x = 0; x < width; ++x)
					values[x, y] = input.ReadSingle();

			int horizontalScale = input.ReadInt32();

			return new HeightMap(width, height, values, horizontalScale);
		}
	}
}