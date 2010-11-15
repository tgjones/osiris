using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Osiris.Content;

namespace Osiris.Terrain
{
	/// <summary>
	/// Summary description for HeightMap.
	/// </summary>
	public class FileHeightMap : HeightMap
	{
		#region Fields

		private const float SCALING = 0.2f;

		#endregion

		#region Properties

		

		#endregion

		#region Constructor

		public FileHeightMap(Game game, string heightMapAssetName, char heightMapChannel, bool applyBoxFilter)
			: base(game, applyBoxFilter)
		{
			game.Services.AddService(typeof(IHeightMapService), this);

			// load bitmap and read heights
			Texture2D texture = AssetLoader.LoadAsset<Texture2D>(heightMapAssetName, game);
			m_nWidth = texture.Width;
			m_nHeight = texture.Height;

			Color[] data = new Color[m_nWidth * m_nHeight];
			texture.GetData<Color>(data);
			texture.Dispose();

			float fMaximumHeight = float.MinValue;
			m_pHeights = new float[m_nWidth, m_nHeight];
			for (int y = 0; y < m_nHeight; y++)
			{
				for (int x = 0; x < m_nWidth; x++)
				{
					byte yHeight = GetColourChannel(data[(y * m_nWidth) + x], heightMapChannel);
					float fHeight = (yHeight * SCALING);
					m_pHeights[x, y] = fHeight;

					if (fHeight > fMaximumHeight)
						m_fMaximumHeight = fHeight;
				}
			}
		}

		#endregion

		#region Methods

		private static byte GetColourChannel(Color colour, char cChannel)
		{
			switch (cChannel)
			{
				case 'R':
					return colour.R;
				case 'G':
					return colour.G;
				case 'B':
					return colour.B;
				case 'A':
					return colour.A;
				default:
					throw new Exception("Unrecognised channel:" + cChannel);
			}
		}

		#endregion
	}
}
