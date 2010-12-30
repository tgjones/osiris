using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Graphics
{
	//[ContentSerializerRuntimeType("RoastedAmoeba.Xna.Graphics.Terrain.Level, RoastedAmoeba.Xna")]
	public class LevelContent : ContentItem
	{
		public IndexCollection[] Indices { get; set; }
		public float MaximumDelta { get; set; }
	}
}