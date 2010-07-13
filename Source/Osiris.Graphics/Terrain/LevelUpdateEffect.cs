using System;
using Osiris.Graphics.Effects;
using Osiris.Graphics.SceneGraph;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.Terrain
{
	public class LevelUpdateEffect : ShaderEffect
	{
		private Int2 m_tToroidalOrigin;

		private ScreenSpaceQuadRenderer _elevationTextureRenderer, _normalMapTextureRenderer, _colourMapTextureRenderer;
		private Texture2D m_pElevationTexture, m_pNormalMapTexture, m_pColourMapTexture;

		#region Update elevation texture methods

		private void UpdateElevationTexture()
		{
			// TODO: currently we just update the whole texture. we need to do this toroidally

			// render to texture here
			_elevationTextureRenderer.Effect.SetValue("GridSpacing", (float) m_nGridSpacing);
			_elevationTextureRenderer.Effect.SetValue("WorldPosMin", (Vector2) m_tPositionMin);
			_elevationTextureRenderer.Effect.SetValue("ToroidalOrigin", m_tToroidalOrigin);
			if (m_pNextCoarserLevel != null)
			{
				_elevationTextureRenderer.Effect.ChangeTechnique("UpdateElevationRandom");
				_elevationTextureRenderer.Effect.SetValue("CoarserLevelElevationTexture", m_pNextCoarserLevel.ElevationTexture);
				_elevationTextureRenderer.Effect.SetValue("CoarserLevelTextureOffset", m_pNextCoarserLevel.InteriorTrim.CoarserGridPosMin);
			}
			else
			{
				_elevationTextureRenderer.Effect.ChangeTechnique("UpdateElevationRandomCoarsest");
			}

			m_pElevationTexture = _elevationTextureRenderer.RenderToTexture(
				0, Settings.ElevationTextureSize,
				0, Settings.ElevationTextureSize);
		}

		#endregion

		#region Update normal map texture methods

		private void UpdateNormalMapTexture()
		{
			// render to texture here
			_normalMapTextureRenderer.Effect.SetValue("ElevationTexture", m_pElevationTexture);
			if (m_pNextCoarserLevel != null)
			{
				_normalMapTextureRenderer.Effect.ChangeTechnique("ComputeNormals");
				_normalMapTextureRenderer.Effect.SetValue("CoarserNormalMapTexture", m_pNextCoarserLevel.NormalMapTexture);
				_normalMapTextureRenderer.Effect.SetValue("CoarserLevelTextureOffset", m_pNextCoarserLevel.InteriorTrim.CoarserGridPosMin);
			}
			else
			{
				_normalMapTextureRenderer.Effect.ChangeTechnique("ComputeNormalsCoarsest");
			}
			const float ZMax = 1.0f;
			_normalMapTextureRenderer.Effect.SetValue("NormalScaleFactor", 0.5f / (ZMax * m_nGridSpacing));

			m_pNormalMapTexture = _normalMapTextureRenderer.RenderToTexture(
				0, Settings.NormalMapTextureSize,
				0, Settings.NormalMapTextureSize);
		}

		private void UpdateColourMapTexture()
		{
			// render to texture here
			_colourMapTextureRenderer.Effect.SetValue("ElevationTexture", m_pElevationTexture);

			m_pColourMapTexture = _colourMapTextureRenderer.RenderToTexture(
				0, Settings.ColourMapTextureSize,
				0, Settings.ColourMapTextureSize);
		}

		#endregion

		public override void DrawPreProcess(Spatial spatial)
		{
			base.DrawPreProcess(spatial);

			Parent.Geometry.Effect.ElevationTexture = m_pElevationTexture;
			Parent.Geometry.Effect.NormalMapTexture = m_pNormalMapTexture;
			//Parent.Geometry.Effect.ColourMapTexture = m_pColourMapTexture;
		}
	}
}
