using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Osiris.Graphics.Shaders;

namespace Osiris.Content.Pipeline
{
	[ContentImporter(".xml", CacheImportedData = true, DisplayName = "Shader Fragment - Osiris Framework", DefaultProcessor = "ShaderFragmentProcessor")]
	public class ShaderFragmentImporter : ContentImporter<ShaderFragment>
	{
		public override ShaderFragment Import(string filename, ContentImporterContext context)
		{
			XmlImporter xmlImporter = new XmlImporter();
			ShaderFragment fragment = (ShaderFragment) xmlImporter.Import(filename, context);
			fragment.Identity = new ContentIdentity(filename, "ShaderFragmentImporter");
			return fragment;
		}
	}
}
