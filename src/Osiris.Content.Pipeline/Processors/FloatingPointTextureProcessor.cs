using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;
using TOutput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;

namespace Osiris.Content.Pipeline.Processors
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "Texture (No mipmaps, fp) - Osiris Framework")]
    public class FloatingPointTextureProcessor : ContentProcessor<TInput, TOutput>
    {
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            input.ConvertBitmapType(typeof(PixelBitmapContent<float>));
            return input;
        }
    }
}