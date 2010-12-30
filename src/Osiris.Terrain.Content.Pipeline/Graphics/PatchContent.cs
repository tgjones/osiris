using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Osiris.Terrain.Content.Pipeline.Graphics
{
	public class PatchContent
	{
		public VertexBufferContent VertexBuffer { get; set; }
		public LevelContent[] Levels { get; set; }

		public BoundingBox BoundingBox { get; set; }
		public Vector3 Center { get; set; }
		public Vector2 Offset { get; set; }
	}
}