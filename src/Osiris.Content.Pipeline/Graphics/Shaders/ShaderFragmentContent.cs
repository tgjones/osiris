using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Osiris.Graphics.Shaders;

namespace Osiris.Content.Pipeline.Graphics.Shaders
{
	public class ShaderFragmentContent : ContentItem
	{
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
		public string Functions;

		[ContentSerializer(Optional = true)]
		public string VertexProgram;

		[ContentSerializer(Optional = true)]
		public string PixelProgram;

		/// <summary>
		/// Used by ShaderGenerator to combine fragments.
		/// </summary>
		[ContentSerializer(Optional = true)]
		internal string UniqueName;

		public ShaderFragmentContent()
		{
			Parameters = new ShaderParameterContent[0];
			Textures = new ShaderTextureContent[0];
			VertexInputs = new ShaderParameterContent[0];
			Interpolators = new ShaderParameterContent[0];
			Functions = string.Empty;
			VertexProgram = string.Empty;
			PixelProgram = string.Empty;
		}

		internal string GetShaderProgram(ShaderType shaderType)
		{
			switch (shaderType)
			{
				case ShaderType.Vertex :
					return VertexProgram;
				case ShaderType.Pixel :
					return PixelProgram;
				default :
					throw new ArgumentOutOfRangeException("shaderType");
			}
		}
	}

	public class ShaderParameterContent
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

	public class ShaderTextureContent
	{
		public string Name;
		public string SamplerName;
		public string SamplerDataType;
		public string MipFilter;
		public string MinFilter;
		public string MagFilter;
		public string AddressU;
		public string AddressV;

		[ContentSerializer(Optional = true)]
		public string Description;

		public ShaderTextureContent()
		{
			Description = string.Empty;
		}
	}

	/// <summary>
	/// Used by ShaderGenerator.
	/// </summary>
	internal enum ShaderType
	{
		Vertex,
		Pixel
	}
}