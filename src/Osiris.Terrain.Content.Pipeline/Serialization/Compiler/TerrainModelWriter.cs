using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class TerrainModelWriter : ContentTypeWriter<TerrainModelContent>
	{
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "Osiris.Terrain.Content.TerrainModelReader, Osiris.Terrain";
		}

		protected override void Write(ContentWriter output, TerrainModelContent value)
		{
			output.Write(value.NumPatchesX);
			output.Write(value.NumPatchesY);
			for (int y = 0; y < value.NumPatchesY; ++y)
				for (int x = 0; x < value.NumPatchesX; ++x)
					output.WriteObject(value.Patches[x, y]);
			output.WriteObject(value.HeightMap);
			output.Write(value.Tau);
			output.WriteObject(value.Material);
		}
	}
}