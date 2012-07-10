using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class LevelWriter : ContentTypeWriter<LevelContent>
	{
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "Osiris.Terrain.Content.LevelReader, Osiris.Terrain";
		}

		protected override void Write(ContentWriter output, LevelContent value)
		{
			output.Write(value.Indices.Length);
			foreach (IndexCollection indexCollection in value.Indices)
				output.WriteObject(indexCollection);
			output.Write(value.MaximumDelta);
		}
	}
}