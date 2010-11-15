using System;
using Microsoft.Xna.Framework.Content;
using Osiris.Graphics.Shaders;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Osiris.Content
{
	public class ShaderCatalogReader : ContentTypeReader<ShaderCatalog>
	{
		protected override ShaderCatalog Read(ContentReader input, ShaderCatalog existingInstance)
		{
			ShaderCatalog result = new ShaderCatalog();

			int count = input.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				// Load effect.
				Effect effect = input.ReadObject<Effect>();

				// Load renderer constants and associate them with their runtime EffectParameter counterparts.
				Dictionary<string, string> rendererConstantsInput = input.ReadObject<Dictionary<string, string>>();
				Dictionary<string, EffectParameter> rendererConstants = new Dictionary<string, EffectParameter>();
				foreach (KeyValuePair<string, string> rendererConstantInput in rendererConstantsInput)
				{
					EffectParameter effectParameter = effect.Parameters.GetParameterBySemantic(rendererConstantInput.Key);
					rendererConstants.Add(rendererConstantInput.Key, effectParameter);
				}

				// Load compiled fragments.
				CompiledShaderFragment[] compiledFragments = ReadCompiledFragments(input, effect);

				// Load vertex elements, which are used for looking up the right shader.
				VertexDeclaration vertexDeclaration = input.ReadObject<VertexDeclaration>();

				// Create shader object.
				Shader shader = new Shader(effect, rendererConstants, compiledFragments, vertexDeclaration.GetVertexElements());
				result.Add(shader);
			}

			return result;
		}

		private CompiledShaderFragment[] ReadCompiledFragments(ContentReader input, Effect effect)
		{
			CompiledShaderFragment[] result = new CompiledShaderFragment[input.ReadInt32()];
			for (int i = 0; i < result.Length; i++)
			{
				List<string> effectParametersInput = input.ReadObject<List<string>>();
				List<EffectParameter> effectParameters = new List<EffectParameter>();
				foreach (string effectParameterInput in effectParametersInput)
					effectParameters.Add(effect.Parameters[effectParameterInput]);

				string mangledNamePrefix = input.ReadString();
				string name = input.ReadString();

				result[i] = new CompiledShaderFragment(null, effectParameters, mangledNamePrefix, name);
			}
			return result;
		}
	}
}
