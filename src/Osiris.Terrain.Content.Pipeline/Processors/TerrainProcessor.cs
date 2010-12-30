using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Processors
{
	/// <summary>
	/// Custom content processor for creating terrain meshes. Given an
	/// input heightfield texture, this processor uses the MeshBuilder
	/// class to programatically generate terrain geometry.
	/// </summary>
	[ContentProcessor(DisplayName = "Terrain - Osiris Framework")]
	public class TerrainProcessor : ContentProcessor<Texture2DContent, TerrainModelContent>
	{
		#region Fields

		private int _patchSize;

		#endregion

		/*const float terrainScale = 4;
		const float terrainBumpiness = 64;
		const float texCoordScale = 0.1f;
		const string terrainTexture = "rocks.bmp";*/

		[DefaultValue(129), DisplayName("Patch Size"), Description("Patch size must be 2^n + 1. For example, 129.")]
		public int PatchSize
		{
			get { return _patchSize; }
			set
			{
				// TODO: validate patch size.
				_patchSize = value;
			}
		}

		[DefaultValue(2.0f), Description("Error metric for geo-mipmapped terrain. Corresponds to the largest screen-space error that will be tolerated before switching to the next most detailed level.")]
		public float Tau { get; set; }

		[DisplayName("Vertical Scale"), DefaultValue(20.0f), Description("Amount to scale the height of the terrain by.")]
		public float VerticalScale { get; set; }

		[DisplayName("Horizontal Scale"), DefaultValue(5), Description("Amount to scale the width and length of the terrain by.")]
		public int HorizontalScale { get; set; }

		[DisplayName("Lighting Type"), DefaultValue(TerrainLightingType.PreBaked), Description("PreBaked = Lighting is baked into terrain color texture. Static = Lighting is calculated at build-time from the directional light settings for this terrain.")]
		public TerrainLightingType LightingType { get; set; }

		[DisplayName("Light Direction"), DefaultValue("1, -1, 1")]
		public Vector3 LightDirection { get; set; }

		[DisplayName("Color Texture"), Description("Set to the relative path of a colour texture.")]
		public string ColorTexture { get; set; }

		[DisplayName("Detail Texture"), Description("Set to the relative path of a detail texture.")]
		public string DetailTexture { get; set; }

		[DisplayName("Detail Texture Tiling"), DefaultValue(10), Description("The number of times the detail texture should be tiled across the terrain.")]
		public int DetailTextureTiling { get; set; }

		public TerrainProcessor()
		{
			PatchSize = 129;
			Tau = 2.0f;
			VerticalScale = 20.0f;
			HorizontalScale = 5;
			LightingType = TerrainLightingType.PreBaked;
			LightDirection = new Vector3(1, -1, 1);
			DetailTextureTiling = 10;
		}

		/// <summary>
		/// Generates a terrain mesh from an input heightfield texture.
		/// </summary>
		public override TerrainModelContent Process(Texture2DContent input,
											 ContentProcessorContext context)
		{
			Texture2DContent texture = context.Convert<Texture2DContent, Texture2DContent>(input, "FloatingPointTextureProcessor");

			PixelBitmapContent<float> heightfield = (PixelBitmapContent<float>)texture.Mipmaps[0];
			float[,] heights = new float[heightfield.Width, heightfield.Height];
			for (int y = 0; y < heightfield.Height; y++)
				for (int x = 0; x < heightfield.Width; x++)
					heights[x, y] = heightfield.GetPixel(x, y);

			HeightMapContent heightMap = new HeightMapContent(heightfield.Width, heightfield.Height, heights, VerticalScale, HorizontalScale);

			string directory = Path.GetDirectoryName(input.Identity.SourceFilename);
			string texture1 = Path.Combine(directory, ColorTexture);
			string texture2 = Path.Combine(directory, DetailTexture);

			// Create a material, and point it at our terrain texture.
			DualTextureMaterialContent material = new DualTextureMaterialContent
			{
				Texture = context.BuildAsset<TextureContent, TextureContent>(new ExternalReference<TextureContent>(texture1), null),
				Texture2 = context.BuildAsset<TextureContent, TextureContent>(new ExternalReference<TextureContent>(texture2), null),
			};

			TerrainModelContentBuilder terrainModelContentBuilder = new TerrainModelContentBuilder(PatchSize, Tau, heightMap, material, DetailTextureTiling, HorizontalScale);
			return terrainModelContentBuilder.Build(context);
		}
	}
}