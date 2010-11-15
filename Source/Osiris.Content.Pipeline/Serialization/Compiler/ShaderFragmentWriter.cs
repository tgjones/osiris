using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Osiris.Content.Pipeline.Graphics.Shaders;
using Osiris.Graphics.Shaders;

namespace Osiris.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class ShaderFragmentWriter : ContentTypeWriter<ShaderFragmentContent>
	{
		protected override void Write(ContentWriter output, ShaderFragmentContent value)
		{
			output.Write(value.Name);
			output.Write((int) value.Class);
			WriteShaderParameters(output, value.Parameters);

			output.Write(value.Textures.Length);
			foreach (ShaderTextureContent texture in value.Textures)
			{
				output.Write(texture.Name);
				output.Write(texture.MipFilter);
				output.Write(texture.MinFilter);
				output.Write(texture.MagFilter);
				output.Write(texture.AddressU);
				output.Write(texture.AddressV);
				output.Write(texture.Description);
			}

			WriteShaderParameters(output, value.VertexInputs);
			WriteShaderParameters(output, value.Interpolators);
			output.Write(value.VertexProgram.Trim());
			output.Write(value.PixelProgram.Trim());
		}

		private static void WriteShaderParameters(ContentWriter output, ShaderParameterContent[] parameters)
		{
			output.Write(parameters.Length);
			foreach (ShaderParameterContent parameter in parameters)
			{
				output.Write(parameter.DataType);
				output.Write(parameter.Name);
				output.Write(parameter.Semantic);
				output.Write(parameter.Description);
			}
		}

		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			// Never used.
			return typeof(object).AssemblyQualifiedName;
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			// Never used.
			return typeof(object).AssemblyQualifiedName;
		}
	}
}