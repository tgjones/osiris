using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics
{
	public static class ExtensionMethods
	{
		public static int GetIndexQuantity(this IndexBuffer indexBuffer)
		{
			int elementSizeInBytes = (indexBuffer.IndexElementSize == IndexElementSize.SixteenBits) ? 2 : 4;
			return indexBuffer.SizeInBytes / elementSizeInBytes;
		}
	}
}
