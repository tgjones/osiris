using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Osiris.Content.Pipeline.Graphics;
using Osiris.Content.Pipeline.Graphics.Shaders;

namespace Osiris.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Shader Catalog - Osiris Framework")]
	public class ShaderCatalogProcessor : ContentProcessor<ShaderCatalogContent, CompiledShaderCatalogContent>
	{
		public override CompiledShaderCatalogContent Process(ShaderCatalogContent input, ContentProcessorContext context)
		{
			CompiledShaderCatalogContent output = new CompiledShaderCatalogContent();

			// For each shader fragment combo, we want to load the fragments and compile the shader.
			foreach (ShaderContent shaderFragmentCombo in input)
			{
				// Load fragments.
				List<ShaderFragmentContent> shaderFragments = LoadShaderFragments(context, shaderFragmentCombo.ShaderFragments);

				// Compile fragments.
				CompiledShaderContent compiledShaderFragmentCombo = ShaderGenerator.CreateFromFragments(shaderFragments, shaderFragmentCombo.VertexElements, context);
				output.Add(compiledShaderFragmentCombo);
			}

			return output;
		}

		private List<ShaderFragmentContent> LoadShaderFragments(ContentProcessorContext context, string[] fragmentAssetNames)
		{
			List<ShaderFragmentContent> result = new List<ShaderFragmentContent>();
			foreach (string fragmentAssetName in fragmentAssetNames)
			{
				ExternalReference<ShaderFragmentContent> sourceFragment = new ExternalReference<ShaderFragmentContent>(fragmentAssetName);
				ShaderFragmentContent shaderFragment = context.BuildAndLoadAsset<ShaderFragmentContent, ShaderFragmentContent>(sourceFragment, null);
				result.Add(shaderFragment);
			}
			return result;
		}
	}
}