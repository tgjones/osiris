using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.GeoMipMapping
{
	/// <summary>
	/// Summary description for LightMap.
	/// </summary>
	public class LightMap
	{
		#region Fields

		//private const float SCALING = 2.0f;

		private int m_nWidth, m_nHeight;
		private bool[,] m_pShadows;
		private float[,] m_pShadowIntensities;

		private Texture2D m_pTexture;

		#endregion

		#region Properties

		public Texture2D Texture
		{
			get {return m_pTexture;}
		}

		#endregion

		#region Constructor

		public LightMap(IHeightMapService pHeightMap, ITerrainNormalsService pNormalMap, float fAngle, Vector3 tLightDirection, float fShadowedIntensity, GraphicsDevice graphicsDevice)
		{
			m_nWidth = pHeightMap.Width;
			m_nHeight = pHeightMap.Height;

			/*m_pShadows = new bool[m_nWidth, m_nHeight];
			m_pShadowIntensities = new float[m_nWidth, m_nHeight];
			CalculateShadows(pHeightMap, fAngle, fShadowedIntensity);

			BlurShadows();*/

			CopyToBitmap(pNormalMap, graphicsDevice);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Calculates shadows. There is a limitation that the direction of the sun
		/// must be parallel to the x-axis.
		/// 
		/// This uses the Strider technique described at
		/// http://www.gamedev.net/reference/articles/article2125.asp
		/// 
		/// Addition to method: calculate intensity of shadow based on angle from hilltop
		/// to terrain point.
		/// </summary>
		/// <param name="pHeightMap"></param>
		/// <param name="fAngle">Angle of the sun, measured from the ground plane</param>
		private void CalculateShadows(FileHeightMap pHeightMap, float fAngle, float fShadowedIntensity)
		{
			// set default shadow intensities
			for (int y = 0; y < m_nHeight; y++)
				for (int x = 0; x < m_nWidth; x++)
					m_pShadowIntensities[x, y] = 1.0f;

			// calculate change in z for each unit of x
			float fDeltaZ = MathsHelper.Tan(fAngle);

			// loop away from the sun
			for (int x = m_nWidth - 1; x > 0; x--)
			{
				for (int y = 0; y < m_nHeight - 1; y++)
				{
					float fWorkingHeight = GetHeight(pHeightMap, x, y);
					int nCurrentX = x;

					// skip if already shadowed
					if (!m_pShadows[x, y])
					{
						// cast the line
						while (nCurrentX > 0)
						{
							// decrement x to get to the next point
							nCurrentX--;

							// get distance from previous unshadowed point
							float fDistance = x - nCurrentX;

							// get height of ray at this point
							float fRayHeight = fWorkingHeight - (fDistance * fDeltaZ);

							// test if the ray is higher than the terrain at this point. if it is,
							// point is in shadow. otherwise, we've found our next unshadowed point
							float fTerrainHeight = GetHeight(pHeightMap, nCurrentX, y);
							if (fRayHeight > fTerrainHeight)
							{
								// calculate angle from this point on terrain to hilltop
								// we know angle will always be greater than sun angle
								float fHillAngle = MathsHelper.Atan2(fWorkingHeight - fTerrainHeight, fDistance);

								// calculate ratio
								float fShadowIntensity = MathsHelper.ComputeWeight(fHillAngle, fAngle, MathHelper.PiOver2);
								fShadowIntensity = 1.0f - fShadowIntensity;

								// set shadow intensity
								m_pShadowIntensities[nCurrentX, y] = fShadowIntensity;
								m_pShadows[nCurrentX, y] = true;
							}
							else
							{
								break;
							}
						}
					}
				}
			}
		}

		private float GetHeight(FileHeightMap pHeightMap, int nX, int nY)
		{
			return pHeightMap[nX, nY];
			//return pHeightMap[nX / (float) (m_nWidth), nY / (float) (m_nHeight)];
		}

		private void BlurShadows()
		{
			for (int y = 1; y < m_nHeight - 1; y++)
			{
				for (int x = 1; x < m_nWidth; x++)
				{
					// we only blur points in shadow
					if (m_pShadows[x, y])
					{
						m_pShadowIntensities[x, y] = (m_pShadowIntensities[x - 1, y + 1]
							+ m_pShadowIntensities[x, y + 1] + m_pShadowIntensities[x - 1, y]
							+ m_pShadowIntensities[x, y]     + m_pShadowIntensities[x - 1, y - 1]
							+ m_pShadowIntensities[x, y - 1]) / 6.0f;
					}
				}
			}
		}

		private void CopyToBitmap(ITerrainNormalsService normalMap, GraphicsDevice graphicsDevice)
		{
			// load into bitmap
			m_pTexture = new Texture2D(graphicsDevice, m_nWidth, m_nHeight, 1, TextureUsage.None, SurfaceFormat.Color);

			Color[] pixels = new Color[m_nWidth * m_nHeight];
			for (int x = 0; x < m_nWidth; x++)
			{
				for (int y = 0; y < m_nHeight; y++)
				{
					// light colour
					//float fShadowIntensity = m_pShadowIntensities[x, y];
					//byte yShadowIntensity = (byte) (fShadowIntensity * 255);
					byte yShadowIntensity = 255;

					Vector3 tNormal = normalMap[x, y];
					Color tColour = new Color(
						(byte) ((tNormal.X + 1) * 127),
						(byte) ((tNormal.Y + 1) * 127),
						(byte) ((tNormal.Z + 1) * 127),
						yShadowIntensity);
					pixels[(y * m_nWidth) + x] = tColour;
				}
			}

			m_pTexture.SetData<Color>(pixels);
		}

		#endregion
	}
}