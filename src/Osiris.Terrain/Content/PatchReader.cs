using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Terrain.Graphics;

namespace Osiris.Terrain.Content
{
	public class PatchReader : ContentTypeReader<Patch>
	{
		protected override Patch Read(ContentReader input, Patch existingInstance)
		{
			VertexBuffer vertexBuffer = input.ReadObject<VertexBuffer>();

			int levelCount = input.ReadInt32();
			Level[] levels = new Level[levelCount];
			for (int i = 0; i < levelCount; ++i)
				levels[i] = input.ReadObject<Level>();

			BoundingBox boundingBox = input.ReadObject<BoundingBox>();
			Vector3 center = input.ReadVector3();
			Vector2 offset = input.ReadVector2();

			return new Patch(vertexBuffer, levels, boundingBox, center, offset);
		}
	}
}