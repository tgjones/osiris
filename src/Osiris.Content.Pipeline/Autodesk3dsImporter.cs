using Microsoft.Xna.Framework.Content.Pipeline;

namespace Osiris.Content.Pipeline
{
	[ContentImporter(".3ds", DisplayName = "3DS Model - Osiris Framework", CacheImportedData = true, DefaultProcessor = "ModelProcessor")]
	public class Autodesk3dsImporter : MeshellatorImporter
	{
		
	}
}