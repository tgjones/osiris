using System;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;
using System.Reflection;
using Osiris.Graphics.Shaders;
using System.Collections.Generic;

namespace Osiris.Content.Pipeline.Serialization.Compiler
{
	[ContentTypeWriter]
	public class ShaderFragmentWriter : ContentTypeWriter<ShaderFragment>
	{
		protected override void Write(ContentWriter output, ShaderFragment value)
		{
			output.Write(value.Name);
			output.Write((int) value.Class);
			WriteShaderParameters(output, value.Parameters);

			output.Write(value.Textures.Length);
			foreach (ShaderTexture texture in value.Textures)
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

		private static void WriteShaderParameters(ContentWriter output, ShaderParameter[] parameters)
		{
			output.Write(parameters.Length);
			foreach (ShaderParameter parameter in parameters)
			{
				output.Write(parameter.DataType);
				output.Write(parameter.Name);
				output.Write(parameter.Semantic);
				output.Write(parameter.Description);
			}
		}

		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(ShaderFragment).AssemblyQualifiedName;
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(ShaderFragmentReader).AssemblyQualifiedName;
		}
	}
}