using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;
using Osiris.Content;
using Osiris.Graphics.Cameras;
using Osiris.Maths;

namespace Osiris.Terrain.GeoClipMapping
{
	public class Level : NestedDrawableGameComponent
	{
		#region Fields

		private readonly int m_nGridSpacing;
		private readonly int m_nHalfWidth;

		private IntVector2 m_tPositionMin;

		private GeoClipMappedTerrain m_pParentTerrain;

		private Block[] m_pBlocks;
		private RingFixups m_pRingFixups;
		private InteriorTrim m_pInteriorTrim;
		private Block[] m_pCentreBlocks;
		private InteriorTrim m_pCentreInteriorTrim;
		private EdgeStitches m_pEdgeStitches;

		private Level m_pNextFinerLevel;
		private bool m_bFinestLevel;
		private Level m_pNextCoarserLevel;

		private bool m_bActive;

		private IntVector2 m_tToroidalOrigin;

		private RenderTarget2D m_pElevationTextureTarget, m_pNormalMapTextureTarget, m_pColourMapTextureTarget;
		private Texture2D m_pElevationTexture, m_pNormalMapTexture, m_pColourMapTexture;
		private ExtendedEffect m_pElevationUpdateEffect, m_pNormalMapUpdateEffect, m_pColourMapUpdateEffect;

		//public static Texture2D _heightMapTexture;
		private static Texture2D _permutationTexture;
		private static Texture2D _gradientTexture;

		private SpriteBatch _randomTextureBatch;
		private Texture2D _randomTexture;

		#endregion

		#region Properties

		public IntVector2 PositionMin
		{
			get { return m_tPositionMin; }
		}

		public IntVector2 PositionMax
		{
			get
			{
				return m_tPositionMin + new IntVector2(Settings.GRID_SIZE_N_MINUS_ONE * m_nGridSpacing);
			}
		}

		private bool Active
		{
			get { return m_bActive; }
		}

		public Texture2D ElevationTexture
		{
			get { return m_pElevationTexture; }
		}

		public Texture2D NormalMapTexture
		{
			get { return m_pNormalMapTexture; }
		}

		private InteriorTrim InteriorTrim
		{
			get { return m_pInteriorTrim; }
		}

		#endregion

		#region Constructor

		public Level(Game game, GeoClipMappedTerrain pParentTerrain, int nGridSpacing)
			: base(game)
		{
			m_pParentTerrain = pParentTerrain;

			m_nGridSpacing = nGridSpacing;
			m_nHalfWidth = (Settings.GRID_SIZE_N_MINUS_ONE / 2) * m_nGridSpacing;

			// comment these offsets!
			m_pBlocks = new Block[12];
			m_pBlocks[0] = CreateBlock(new Vector2(0, 0));
			m_pBlocks[1] = CreateBlock(new Vector2(0, Settings.BLOCK_SIZE_M_MINUS_ONE));
			m_pBlocks[2] = CreateBlock(new Vector2(0, (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2));
			m_pBlocks[3] = CreateBlock(new Vector2(0, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));
			m_pBlocks[4] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M_MINUS_ONE, 0));
			m_pBlocks[5] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M_MINUS_ONE, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));
			m_pBlocks[6] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2, 0));
			m_pBlocks[7] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));
			m_pBlocks[8] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, 0));
			m_pBlocks[9] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, Settings.BLOCK_SIZE_M_MINUS_ONE));
			m_pBlocks[10] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 2));
			m_pBlocks[11] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2, (Settings.BLOCK_SIZE_M_MINUS_ONE * 3) + 2));

			m_pCentreBlocks = new Block[4];
			m_pCentreBlocks[0] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M, Settings.BLOCK_SIZE_M));
			m_pCentreBlocks[1] = CreateBlock(new Vector2(Settings.BLOCK_SIZE_M, (Settings.BLOCK_SIZE_M * 2) - 1));
			m_pCentreBlocks[2] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M * 2) - 1, Settings.BLOCK_SIZE_M));
			m_pCentreBlocks[3] = CreateBlock(new Vector2((Settings.BLOCK_SIZE_M * 2) - 1, (Settings.BLOCK_SIZE_M * 2) - 1));

			m_pRingFixups = new RingFixups(game, m_nGridSpacing);

			m_pInteriorTrim = new InteriorTrim(game, m_nGridSpacing);

			m_pCentreInteriorTrim = new InteriorTrim(game, m_nGridSpacing);

			m_pEdgeStitches = new EdgeStitches(game, m_nGridSpacing);

			m_bActive = true;

			//m_tToroidalOrigin = new IntVector2(1, 1);
			m_tToroidalOrigin = IntVector2.Zero;
		}

		#endregion

		#region Methods

		private Block CreateBlock(Vector2 tGridSpaceOffset)
		{
			return new Block(Game, m_nGridSpacing, tGridSpaceOffset, tGridSpaceOffset * m_nGridSpacing);
		}

		public void Create(Level pNextFinerLevel, Level pNextCoarserLevel, IntVector2 tViewerPosition2D)
		{
			foreach (Block pBlock in m_pBlocks)
			{
				pBlock.Initialize();
			}

			m_pRingFixups.Initialize();

			m_pInteriorTrim.Initialize();

			foreach (Block pBlock in m_pCentreBlocks)
			{
				pBlock.Initialize();
			}

			m_pCentreInteriorTrim.Initialize();

			m_pEdgeStitches.Initialize();

			m_pNextFinerLevel = pNextFinerLevel;
			m_pNextCoarserLevel = pNextCoarserLevel;

			// set initial min position of level
			IntVector2 tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;
			IntVector2 tDeltaPositionTemp1 = tViewerPosGridCoords - Settings.CentralSquareMin;
			IntVector2 tDeltaPositionTemp2 = new IntVector2(Math.Abs(tDeltaPositionTemp1.X), Math.Abs(tDeltaPositionTemp1.Y));
			IntVector2 tDeltaPositionTemp3 = tDeltaPositionTemp1 - (tDeltaPositionTemp2 % 2);
			m_tPositionMin += tDeltaPositionTemp3 * m_nGridSpacing;

			m_pElevationTextureTarget = new RenderTarget2D(GraphicsDevice,
				Settings.ELEVATION_TEXTURE_SIZE,
				Settings.ELEVATION_TEXTURE_SIZE,
				1,
				SurfaceFormat.Vector4,
				RenderTargetUsage.PreserveContents); // TODO: Remove this flag

			m_pNormalMapTextureTarget = new RenderTarget2D(GraphicsDevice,
				Settings.NORMAL_MAP_TEXTURE_SIZE,
				Settings.NORMAL_MAP_TEXTURE_SIZE,
				1,
				SurfaceFormat.Color,
				RenderTargetUsage.PreserveContents); // TODO: Remove this flag*/

			m_pColourMapTextureTarget = new RenderTarget2D(GraphicsDevice,
				Settings.COLOUR_MAP_TEXTURE_SIZE,
				Settings.COLOUR_MAP_TEXTURE_SIZE,
				1,
				SurfaceFormat.Color,
				RenderTargetUsage.PreserveContents); // TODO: Remove this flag*/

			m_pElevationUpdateEffect = new ExtendedEffect(AssetLoader.LoadAsset<Effect>(@"Terrain\GeoClipMapping\UpdateElevation", Game));
			m_pNormalMapUpdateEffect = new ExtendedEffect(AssetLoader.LoadAsset<Effect>(@"Terrain\GeoClipMapping\ComputeNormals", Game));
			m_pColourMapUpdateEffect = new ExtendedEffect(AssetLoader.LoadAsset<Effect>(@"Terrain\GeoClipMapping\UpdateColourMap", Game));

			if (_permutationTexture == null)
			{
				INoiseService noise = GetService<INoiseService>();
				_permutationTexture = noise.GetPermutationTexture();
				_gradientTexture = noise.GetGradientTexture();
			}

			_randomTextureBatch = new SpriteBatch(GraphicsDevice);
			_randomTexture = new Texture2D(GraphicsDevice, Settings.ELEVATION_TEXTURE_SIZE, Settings.ELEVATION_TEXTURE_SIZE, 1, TextureUsage.None, SurfaceFormat.Color);
		}

		public void Create2()
		{
			UpdateElevationTexture();
			UpdateNormalMapTexture();
			//UpdateColourMapTexture();

			m_pParentTerrain.Effect.SetValue("PermutationTexture", _permutationTexture);
			m_pParentTerrain.Effect.SetValue("GradientTexture", _gradientTexture);
		}

		public void Update(Vector3 tViewerPosition, IntVector2 tViewerPosition2D)
		{
			// there is a central square of 2x2 grid units that we use to determine
			// if the level needs to move. if it doesn't need to move, we still might
			// need to update the interior trim position

			// each level only ever moves by TWICE the level's grid spacing

			// transform viewer position from world coords to grid coords
			IntVector2 tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;

			// check if viewer position is still in central square
			bool lUpdate = false;
			if (!(tViewerPosGridCoords >= Settings.CentralSquareMin && tViewerPosGridCoords <= Settings.CentralSquareMax))
			{
				// need to move level, so calculate new minimum position
				IntVector2 tDeltaPositionTemp1 = tViewerPosGridCoords - Settings.CentralSquareMin;
				IntVector2 tDeltaPositionTemp2 = new IntVector2(Math.Abs(tDeltaPositionTemp1.X), Math.Abs(tDeltaPositionTemp1.Y));
				IntVector2 tDeltaPositionTemp3 = tDeltaPositionTemp1 - (tDeltaPositionTemp2 % 2);
				m_tPositionMin += tDeltaPositionTemp3 * m_nGridSpacing;

				// recalculate viewer pos in grid coordinates
				tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;

				lUpdate = true;
			}

			Vector2 tPositionMin = m_tPositionMin;

			foreach (Block pBlock in m_pBlocks)
			{
				pBlock.Update(tPositionMin);
			}

			m_pRingFixups.Update(tPositionMin);

			m_pInteriorTrim.Update(tPositionMin, tViewerPosGridCoords);

			m_pEdgeStitches.Update(tPositionMin);

			// if this is the finest active level, we need to fill the hole with more blocks
			if (m_pNextFinerLevel == null || !m_pNextFinerLevel.Active)
			{
				m_bFinestLevel = true;

				m_pInteriorTrim.ActiveInteriorTrim = InteriorTrim.WhichInteriorTrim.BottomLeft;

				foreach (Block pBlock in m_pCentreBlocks)
				{
					pBlock.Update(tPositionMin);
				}

				m_pCentreInteriorTrim.Update(tPositionMin, tViewerPosGridCoords);
				m_pCentreInteriorTrim.ActiveInteriorTrim = InteriorTrim.WhichInteriorTrim.TopRight;
			}
			else
			{
				m_bFinestLevel = false;
			}

			// checking blocks for frustum visibility
			ICameraService pCamera = (ICameraService) GetService<ICameraService>();
			BoundingFrustum lCameraFrustum = pCamera.BoundingFrustum;
			foreach (Block pBlock in m_pBlocks)
			{
				//pBlock.Visible = true;
				pBlock.Visible = (lCameraFrustum.Contains(pBlock.BoundingBox) != ContainmentType.Disjoint);
			}
			foreach (Block pBlock in m_pCentreBlocks)
			{
				//pBlock.Visible = true;
				pBlock.Visible = (lCameraFrustum.Contains(pBlock.BoundingBox) != ContainmentType.Disjoint);
			}

			if (lUpdate)
			{
				UpdateElevationTexture();
				UpdateNormalMapTexture();
				//UpdateColourMapTexture();
			}
		}

		#region Update elevation texture methods

		private void UpdateElevationTexture()
		{
			// TODO: currently we just update the whole texture. we need to do this toroidally

			// render to texture here
			//m_pElevationUpdateEffect.SetValue("HeightMapTexture", _heightMapTexture);
			//m_pElevationUpdateEffect.SetValue("HeightMapSizeInverse", 1.0f / (float) _heightMapTexture.Width);
			m_pElevationUpdateEffect.SetValue("GridSpacing", (float) m_nGridSpacing);
			m_pElevationUpdateEffect.SetValue("WorldPosMin", (Vector2) m_tPositionMin);
			m_pElevationUpdateEffect.SetValue("ToroidalOrigin", m_tToroidalOrigin);
			m_pElevationUpdateEffect.SetValue("ElevationTextureSize", Settings.ELEVATION_TEXTURE_SIZE);
			m_pElevationUpdateEffect.SetValue("PermutationTexture", _permutationTexture);
			m_pElevationUpdateEffect.SetValue("GradientTexture", _gradientTexture);
			if (m_pNextCoarserLevel != null)
			{
				m_pElevationUpdateEffect.ChangeTechnique("UpdateElevationRandom");
				m_pElevationUpdateEffect.SetValue("CoarserLevelElevationTexture", m_pNextCoarserLevel.ElevationTexture);
				m_pElevationUpdateEffect.SetValue("CoarserLevelTextureOffset", m_pNextCoarserLevel.InteriorTrim.CoarserGridPosMin);
			}
			else
			{
				m_pElevationUpdateEffect.ChangeTechnique("UpdateElevationRandomCoarsest");
			}

			m_pElevationTexture = m_pParentTerrain.RenderToTexture(
				m_pElevationTextureTarget, m_pElevationUpdateEffect,
				0, Settings.ELEVATION_TEXTURE_SIZE,
				0, Settings.ELEVATION_TEXTURE_SIZE);
		}

		#endregion

		#region Update normal map texture methods

		private void UpdateNormalMapTexture()
		{
			// render to texture here
			m_pNormalMapUpdateEffect.SetValue("ElevationTexture", m_pElevationTexture);
			m_pNormalMapUpdateEffect.SetValue("ElevationTextureSizeInverse", Settings.ELEVATION_TEXTURE_SIZE_INVERSE);
			m_pNormalMapUpdateEffect.SetValue("NormalMapTextureSize", Settings.NORMAL_MAP_TEXTURE_SIZE);
			if (m_pNextCoarserLevel != null)
			{
				m_pNormalMapUpdateEffect.ChangeTechnique("ComputeNormals");
				m_pNormalMapUpdateEffect.SetValue("CoarserNormalMapTexture", m_pNextCoarserLevel.NormalMapTexture);
				m_pNormalMapUpdateEffect.SetValue("CoarserLevelTextureOffset", m_pNextCoarserLevel.InteriorTrim.CoarserGridPosMin);
			}
			else
			{
				m_pNormalMapUpdateEffect.ChangeTechnique("ComputeNormalsCoarsest");
			}
			const float ZMax = 1.0f;
			m_pNormalMapUpdateEffect.SetValue("NormalScaleFactor", 0.5f / (ZMax * m_nGridSpacing));

			m_pNormalMapTexture = m_pParentTerrain.RenderToTexture(
				m_pNormalMapTextureTarget, m_pNormalMapUpdateEffect,
				0, Settings.NORMAL_MAP_TEXTURE_SIZE,
				0, Settings.NORMAL_MAP_TEXTURE_SIZE);
		}

		private void UpdateColourMapTexture()
		{
			// render to texture here
			m_pColourMapUpdateEffect.SetValue("ElevationTexture", m_pElevationTexture);
			m_pColourMapUpdateEffect.SetValue("ElevationTextureSizeInverse", Settings.ELEVATION_TEXTURE_SIZE_INVERSE);
			m_pColourMapUpdateEffect.SetValue("ColourMapTextureSize", Settings.COLOUR_MAP_TEXTURE_SIZE);

			m_pColourMapTexture = m_pParentTerrain.RenderToTexture(
				m_pColourMapTextureTarget, m_pColourMapUpdateEffect,
				0, Settings.COLOUR_MAP_TEXTURE_SIZE,
				0, Settings.COLOUR_MAP_TEXTURE_SIZE);
		}

		#endregion

		#region Render methods

		public override void Draw(ExtendedEffect pEffect)
		{
			// apply vertex texture
			pEffect.SetValue("ElevationTexture", m_pElevationTexture);

			pEffect.SetValue("ViewerPos", (m_pParentTerrain.Viewer.Position2D - m_tPositionMin) / m_nGridSpacing);
			pEffect.SetValue("AlphaOffset", Settings.AlphaOffset);
			pEffect.SetValue("OneOverWidth", Settings.TransitionWidthInverse);
			pEffect.SetValue("GridSize", Settings.GRID_SIZE_N);

			//pEffect.SetValue("GrassTexture", m_pParentTerrain.GrassTexture);

			pEffect.SetValue("NormalMapTexture", m_pNormalMapTexture);

			/*Vector2 tCoarserGridPosMin = new Vector2();
			if (m_pNextCoarserLevel != null)
				tCoarserGridPosMin = m_pNextCoarserLevel.InteriorTrim.CoarserGridPosMin;
			pEffect.SetValue("CoarserNormalMapTextureOffset", tCoarserGridPosMin);*/

			//pEffect.SetValue("CoarserNormalMapTexture", (m_pNextCoarserLevel != null) ? m_pNextCoarserLevel.NormalMapTexture : null);
			pEffect.SetValue("NormalMapTextureSizeInverse", Settings.NORMAL_MAP_TEXTURE_SIZE_INVERSE);
			pEffect.SetValue("NormalMapTextureSize", Settings.NORMAL_MAP_TEXTURE_SIZE);

			pEffect.SetValue("ColourMapTexture", m_pColourMapTexture);
			pEffect.SetValue("ColourMapTextureSize", Settings.COLOUR_MAP_TEXTURE_SIZE);

			//pEffect.SetValue("LightDirection", Vector3.Normalize(new Vector3(0.5f, 1, 0.5f)));

			Vector2 tToroidalOriginScaled = (Vector2) m_tToroidalOrigin / (float) Settings.ELEVATION_TEXTURE_SIZE;
			Vector2 tGridSizeScaled = new Vector2(((float) Settings.GRID_SIZE_N / (float) Settings.ELEVATION_TEXTURE_SIZE));
			pEffect.SetValue("ToroidalOffsets", new Vector4(tToroidalOriginScaled, tGridSizeScaled.X, tGridSizeScaled.Y));
			pEffect.SetValue("ElevationTextureSize", Settings.ELEVATION_TEXTURE_SIZE);

			#region Render blocks

			pEffect.SetValue("Shading", new Vector4(0.7f, 0.0f, 0.0f, 1.0f));

			// we set the vertices and indices here because they are the same for all blocks
			GraphicsDevice.Vertices[0].SetSource(
				Block.SharedVertexBuffer,
				0,
				TerrainVertex.Shared.SizeInBytes);
			GraphicsDevice.Indices = Block.SharedIndexBuffer;

			pEffect.Begin();
			foreach (EffectPass pEffectPass in pEffect.CurrentTechnique.Passes)
			{
				pEffectPass.Begin();

				foreach (Block pBlock in m_pBlocks)
				{
					pBlock.Draw(pEffect);
				}

				pEffectPass.End();
			}
			pEffect.End();

			#endregion

			#region Render ring fix-ups

			pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
			pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
			pEffect.SetValue("Shading", new Vector4(0.0f, 0.7f, 0.0f, 1.0f));

			m_pRingFixups.Draw(pEffect);

			#endregion

			#region Render interior trim

			pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
			pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
			pEffect.SetValue("Shading", new Vector4(0.0f, 0.0f, 0.7f, 1.0f));

			m_pInteriorTrim.Draw(pEffect);

			#endregion

			#region Render centre blocks for finest level

			if (m_bFinestLevel)
			{
				pEffect.SetValue("Shading", new Vector4(0.7f, 0.7f, 0.0f, 1.0f));

				// we set the vertices and indices here because they are the same for all blocks
				GraphicsDevice.Vertices[0].SetSource(
					Block.SharedVertexBuffer,
					0,
					TerrainVertex.Shared.SizeInBytes);
				GraphicsDevice.Indices = Block.SharedIndexBuffer;

				pEffect.Begin();
				foreach (EffectPass pEffectPass in pEffect.CurrentTechnique.Passes)
				{
					pEffectPass.Begin();

					foreach (Block pBlock in m_pCentreBlocks)
					{
						pBlock.Draw(pEffect);
					}

					pEffectPass.End();
				}
				pEffect.End();

				pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
				pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
				pEffect.SetValue("Shading", new Vector4(0.0f, 0.0f, 0.7f, 1.0f));

				m_pCentreInteriorTrim.Draw(pEffect);
			}

			#endregion

			#region Render edge stitches

			pEffect.SetValue("FineBlockOrig", new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE, 0.0f, 0.0f));
			pEffect.SetValue("FineBlockOrig2", Vector2.Zero);
			pEffect.SetValue("Shading", new Vector4(1f, 0.7f, 0.7f, 1.0f));

			m_pEdgeStitches.Draw(pEffect);

			#endregion

			if (m_nGridSpacing == 1)
			{
				_randomTextureBatch.Begin();
				_randomTextureBatch.Draw(_randomTexture, Vector2.Zero, Color.White);
				_randomTextureBatch.End();
			}
		}

		#endregion

		#endregion
	}
}
