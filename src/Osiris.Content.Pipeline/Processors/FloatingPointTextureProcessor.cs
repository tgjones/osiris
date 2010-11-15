using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Osiris.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Texture (No mipmaps, fp) - Osiris Framework")]
	public class FloatingPointTextureProcessor : ContentProcessor<Texture2DContent, Texture2DContent>
    {
		public override Texture2DContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            input.ConvertBitmapType(typeof(PixelBitmapContent<float>));
            return input;
        }
    }
}