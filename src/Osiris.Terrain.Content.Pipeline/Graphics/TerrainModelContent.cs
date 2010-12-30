using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Graphics
{
	public class TerrainModelContent : ContentItem
	{
		public int NumPatchesX { get; set; }
		public int NumPatchesY { get; set; }
		public PatchContent[,] Patches { get; set; }
		public HeightMapContent HeightMap { get; set; }

		public float Tau { get; set; }

		public DualTextureMaterialContent Material { get; set; }
	}
}