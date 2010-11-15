using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Osiris.Content
{
	/// <summary>
	/// Summary description for ResourceLoader.
	/// </summary>
	public static class AssetLoader
	{
		public static T LoadAsset<T>(string assetName, Game game)
		{
			ContentManager contentManager = (ContentManager) game.Services.GetService(typeof(ContentManager));
			T asset = contentManager.Load<T>(assetName);
			return asset;
		}
	}
}