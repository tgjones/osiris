using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics
{
	public static class ScreenshotManager
	{
		public static void TakeScreenshot(GraphicsDevice device)
		{
			if (!Directory.Exists("Screenshots"))
			{
				Directory.CreateDirectory("Screenshots");
			}

			// get next index
			string sFileName;
			int i = 1;
			do
			{
				sFileName = @"Screenshots\Screen" + i.ToString().PadLeft(4, '0') + ".jpg";
				i++;
			}
			while (File.Exists(sFileName));

			using (ResolveTexture2D destinationTexture = new ResolveTexture2D(
				device,
				device.Viewport.Width,
				device.Viewport.Height,
				1,
				SurfaceFormat.Color))
			{
				device.ResolveBackBuffer(destinationTexture);
				destinationTexture.Save(sFileName, ImageFileFormat.Jpg);
			}
		}
	}
}
