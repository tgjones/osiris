using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Osiris.Graphics.Shaders
{
	public class ShaderFragment : ContentItem
	{
		public ShaderFragmentClass Class;

		[ContentSerializer(Optional = true)]
		public ShaderParameter[] Parameters;

		[ContentSerializer(Optional = true)]
		public ShaderTexture[] Textures;

		[ContentSerializer(Optional = true)]
		public ShaderParameter[] VertexInputs;

		[ContentSerializer(Optional = true)]
		public ShaderParameter[] Interpolators;

		[ContentSerializer(Optional = true)]
		public string VertexProgram;

		[ContentSerializer(Optional = true)]
		public string PixelProgram;

		public ShaderFragment()
		{
			Parameters = new ShaderParameter[0];
			Textures = new ShaderTexture[0];
			VertexInputs = new ShaderParameter[0];
			Interpolators = new ShaderParameter[0];
			VertexProgram = string.Empty;
			PixelProgram = string.Empty;
		}
	}

	public class ShaderParameter
	{
		public string DataType;
		public string Name;

		[ContentSerializer(Optional = true)]
		public string Semantic;

		[ContentSerializer(Optional = true)]
		public string Description;

		public ShaderParameter()
		{
			Semantic = string.Empty;
			Description = string.Empty;
		}
	}

	public class ShaderTexture
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