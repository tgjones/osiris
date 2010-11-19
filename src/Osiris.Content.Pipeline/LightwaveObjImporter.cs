using Microsoft.Xna.Framework.Content.Pipeline;

namespace Osiris.Content.Pipeline
{
	[ContentImporter(".obj", DisplayName = "OBJ Model - Osiris Framework", CacheImportedData = true, DefaultProcessor = "ModelProcessor")]
	public class LightwaveObjImporter : MeshellatorImporter
	{
		protected override bool SwapWindingOrder
		{
			get { return true; }
		}
	}
}