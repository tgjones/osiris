using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class PatchWriter : ContentTypeWriter<PatchContent>
	{
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "Osiris.Terrain.Content.PatchReader, Osiris.Terrain";
		}

		protected override void Write(ContentWriter output, PatchContent value)
		{
			output.WriteObject(value.VertexBuffer);
			output.Write(value.Levels.Length);
			foreach (LevelContent level in value.Levels)
				output.WriteObject(level);
			output.WriteObject(value.BoundingBox);
			output.Write(value.Center);
			output.Write(value.Offset);
		}
	}
}