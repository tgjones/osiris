using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexPositionNormalTexture2 : IVertexType
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 TextureCoordinate1;
		public Vector2 TextureCoordinate2;

		public static readonly VertexDeclaration VertexDeclaration;
		public VertexPositionNormalTexture2(Vector3 position, Vector3 normal, Vector2 textureCoordinate1, Vector2 textureCoordinate2)
		{
			Position = position;
			Normal = normal;
			TextureCoordinate1 = textureCoordinate1;
			TextureCoordinate2 = textureCoordinate2;
		}

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{{Position:{0} Normal:{1} TextureCoordinate1:{2} TextureCoordinate2:{3}}}", new object[] { this.Position, this.Normal, this.TextureCoordinate1, this.TextureCoordinate2 });
		}

		static VertexPositionNormalTexture2()
		{
			VertexElement[] elements = new [] {
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
				new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) ,
				new VertexElement(32, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1) 
			};
			VertexDeclaration declaration = new VertexDeclaration(elements);
			declaration.Name = "VertexPositionNormalTexture2.VertexDeclaration";
			VertexDeclaration = declaration;
		}
	}
}