using Microsoft.Xna.Framework.Content.Pipeline;

namespace Osiris.Content.Pipeline
{
	[ContentImporter(".nff", DisplayName = "NFF Model - Osiris Framework", CacheImportedData = true, DefaultProcessor = "ModelProcessor")]
	public class NffImporter : MeshellatorImporter
	{
		
	}
}