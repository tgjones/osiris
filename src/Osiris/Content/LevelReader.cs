using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Terrain;

namespace Osiris.Content
{
	public class LevelReader : ContentTypeReader<Level>
	{
		protected override Level Read(ContentReader input, Level existingInstance)
		{
			int indexCollectionCount = input.ReadInt32();
			IndexBuffer[] indexBuffers = new IndexBuffer[indexCollectionCount];
			for (int i = 0; i < indexCollectionCount; ++i)
				indexBuffers[i] = input.ReadObject<IndexBuffer>();

			float maximumDelta = input.ReadSingle();

			return new Level(indexBuffers, maximumDelta);
		}
	}
}