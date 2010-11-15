using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Osiris.Graphics.Shaders;

namespace Osiris.Content.Pipeline.Graphics
{
	public class ShaderFragmentContent : MarshalByRefObject
	{
		public string Name;

		public ShaderFragmentClass Class;

		[ContentSerializer(Optional = true)]
		public ShaderParameterContent[] Parameters;

		[ContentSerializer(Optional = true)]
		public ShaderTextureContent[] Textures;

		[ContentSerializer(Optional = true)]
		public ShaderParameterContent[] VertexInputs;

		[ContentSerializer(Optional = true)]
		public ShaderParameterContent[] Interpolators;

		[ContentSerializer(Optional = true)]
		public string VertexProgram;

		[ContentSerializer(Optional = true)]
		public string PixelProgram;

		public ShaderFragmentContent()
		{
			Parameters = new ShaderParameterContent[0];
			Textures = new ShaderTextureContent[0];
			VertexInputs = new ShaderParameterContent[0];
			Interpolators = new ShaderParameterContent[0];
			VertexProgram = string.Empty;
			PixelProgram = string.Empty;
		}
	}

	public class ShaderParameterContent : MarshalByRefObject
	{
		public string DataType;
		public string Name;

		[ContentSerializer(Optional = true)]
		public string Semantic;

		[ContentSerializer(Optional = true)]
		public string Description;

		public ShaderParameterContent()
		{
			Semantic = string.Empty;
			Description = string.Empty;
		}
	}

	public class ShaderTextureContent : MarshalByRefObject
	{
		public string Name;
		public string MipFilter;
		public string MinFilter;
		public string MagFilter;
		public string AddressU;
		public string AddressV;

		[ContentSerializer(Optional = true)]
		public string Description;
	}
}