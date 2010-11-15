using System;
using Microsoft.Xna.Framework.Content;

namespace Osiris.Graphics.Shaders
{
	public class ShaderFragmentReader : ContentTypeReader<ShaderFragment>
	{
		protected override ShaderFragment Read(ContentReader input, ShaderFragment existingInstance)
		{
			ShaderFragment fragment = new ShaderFragment();

			fragment.Name = input.ReadString();
			fragment.Class = (ShaderFragmentClass) input.ReadInt32();
			fragment.Parameters = ReadShaderParameters(input);

			fragment.Textures = new ShaderTexture[input.ReadInt32()];
			for (int i = 0; i < fragment.Textures.Length; i++)
			{
				fragment.Textures[i] = new ShaderTexture();
				fragment.Textures[i].Name = input.ReadString();
				fragment.Textures[i].MipFilter = input.ReadString();
				fragment.Textures[i].MinFilter = input.ReadString();
				fragment.Textures[i].MagFilter = input.ReadString();
				fragment.Textures[i].AddressU = input.ReadString();
				fragment.Textures[i].AddressV = input.ReadString();
				fragment.Textures[i].Description = input.ReadString();
			}

			fragment.VertexInputs = ReadShaderParameters(input);
			fragment.Interpolators = ReadShaderParameters(input);
			fragment.VertexProgram = input.ReadString();
			fragment.PixelProgram = input.ReadString();

			return fragment;
		}

		private static ShaderParameter[] ReadShaderParameters(ContentReader input)
		{
			ShaderParameter[] parameters = new ShaderParameter[input.ReadInt32()];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ShaderParameter();
				parameters[i].DataType = input.ReadString();
				parameters[i].Name = input.ReadString();
				parameters[i].Semantic = input.ReadString();
				parameters[i].Description = input.ReadString();
			}
			return parameters;
		}
	}
}
