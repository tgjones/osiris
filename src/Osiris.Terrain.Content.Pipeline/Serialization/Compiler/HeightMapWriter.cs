using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class HeightMapWriter : ContentTypeWriter<HeightMapContent>
	{
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "Osiris.Terrain.Content.HeightMapReader, Osiris.Terrain";
		}

		protected override void Write(ContentWriter output, HeightMapContent value)
		{
			output.Write(value.Width);
			output.Write(value.Height);
			for (int y = 0; y < value.Height; ++y)
				for (int x = 0; x < value.Width; ++x)
					output.Write(value[x, y]);
			output.Write(value.HorizontalScale);
		}
	}
}