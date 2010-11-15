using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace Osiris.Content.Pipeline
{
	[ContentImporter(".raw", DisplayName = "RAW Texture - Osiris Framework", DefaultProcessor = "TextureProcessor")]
	public class RawImporter : ContentImporter<TextureContent>
	{
		#region Methods

		public override TextureContent Import(string filename, ContentImporterContext context)
		{
			// load raw data
			FileStream reader = File.OpenRead(filename);

			int width = 257;
			int height = 257;

			byte[] bytes = new byte[reader.Length];
			reader.Read(bytes, 0, bytes.Length);
			reader.Close();

			// import into standard XNA bitmap container
			PixelBitmapContent<Color> bitmapContent = new PixelBitmapContent<Color>(width, height);
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					bitmapContent.SetPixel(x, y, new Color(bytes[(y * width) + x], 0, 0));

			// create and return one-mipmap-level
			Texture2DContent content = new Texture2DContent
			{
				Identity = new ContentIdentity(filename)
			};
			content.Mipmaps.Add(bitmapContent);
			return content;
		}

		#endregion
	}
}