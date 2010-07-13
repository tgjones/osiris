using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Maths;
using Osiris.SceneObjects.SceneGraph;
using Osiris.Graphics.SceneGraph.Nodes;

namespace Osiris.Graphics.Terrain
{
	public class LevelNode : Node
	{
		#region Fields

		private readonly int m_nGridSpacing;
		private readonly int m_nHalfWidth;

		private Int2 m_tPositionMin;
		private Int2 m_tViewerPosGridCoords;

		private InteriorTrim m_pInteriorTrim;

		private Level m_pNextFinerLevel;
		private Level m_pNextCoarserLevel;
		private bool m_bFinestLevel;

		private bool m_bActive;

		#endregion

		#region Properties

		public Level NextFinerLevel
		{
			get { return m_pNextFinerLevel; }
			set { m_pNextFinerLevel = value; }
		}

		public Level NextCoarserLevel
		{
			get { return m_pNextCoarserLevel; }
			set { m_pNextCoarserLevel = value; }
		}

		public Int2 PositionMin
		{
			get { return m_tPositionMin; }
		}

		public Int2 PositionMax
		{
			get
			{
				return m_tPositionMin + new Int2(Settings.GridSizeNMinusOne * m_nGridSpacing);
			}
		}

		public Int2 ViewerPosGridCoords
		{
			get { return m_tViewerPosGridCoords; }
		}

		private bool Active
		{
			get { return m_bActive; }
		}

		private InteriorTrim InteriorTrim
		{
			get { return m_pInteriorTrim; }
		}

		public bool IsFinestLevel
		{
			get { return m_bFinestLevel; }
			private set
			{
				m_bFinestLevel = value;

				// enable centre interior trim and blocks
				foreach (Node node in Nodes)
				{
					if (node is InteriorTrim && ((InteriorTrim) node).IsCentreFill)
					{
						node.Enabled = value;
						node.Visible = value;
					}
					else if (node is Block && ((Block) node).IsCentreFill)
					{
						node.Enabled = value;
						node.Visible = value;
					}
				}
			}
		}

		private TerrainSettings Settings
		{
			get { return Parent.Geometry.Settings; }
		}

		#endregion

		#region Constructor

		public LevelNode(OsirisGame game, Terrain parent, int nGridSpacing)
			: base(game, parent)
		{
			m_nGridSpacing = nGridSpacing;
			m_nHalfWidth = (Settings.GridSizeNMinusOne / 2) * m_nGridSpacing;

			// TODO: comment these offsets!

			// blocks
			Nodes.Add(CreateBlock(new Vector2(0, 0), false));
			Nodes.Add(CreateBlock(new Vector2(0, Settings.BlockSizeMMinusOne), false));
			Nodes.Add(CreateBlock(new Vector2(0, (Settings.BlockSizeMMinusOne * 2) + 2), false));
			Nodes.Add(CreateBlock(new Vector2(0, (Settings.BlockSizeMMinusOne * 3) + 2), false));
			Nodes.Add(CreateBlock(new Vector2(Settings.BlockSizeMMinusOne, 0), false));
			Nodes.Add(CreateBlock(new Vector2(Settings.BlockSizeMMinusOne, (Settings.BlockSizeMMinusOne * 3) + 2), false));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeMMinusOne * 2) + 2, 0), false));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeMMinusOne * 2) + 2, (Settings.BlockSizeMMinusOne * 3) + 2), false));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeMMinusOne * 3) + 2, 0), false));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeMMinusOne * 3) + 2, Settings.BlockSizeMMinusOne), false));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeMMinusOne * 3) + 2, (Settings.BlockSizeMMinusOne * 2) + 2), false));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeMMinusOne * 3) + 2, (Settings.BlockSizeMMinusOne * 3) + 2), false));

			// centre blocks
			Nodes.Add(CreateBlock(new Vector2(Settings.BlockSizeM, Settings.BlockSizeM), true));
			Nodes.Add(CreateBlock(new Vector2(Settings.BlockSizeM, (Settings.BlockSizeM * 2) - 1), true));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeM * 2) - 1, Settings.BlockSizeM), true));
			Nodes.Add(CreateBlock(new Vector2((Settings.BlockSizeM * 2) - 1, (Settings.BlockSizeM * 2) - 1), true));

			// other terrain parts
			Nodes.Add(new RingFixups(game, this, Parent.Geometry.RingFixupsGeometry, m_nGridSpacing));

			m_pInteriorTrim = new InteriorTrim(game, this, Parent.Geometry.InteriorTrimGeometry, m_nGridSpacing, false);
			Nodes.Add(m_pInteriorTrim);

			Nodes.Add(new InteriorTrim(game, this, Parent.Geometry.InteriorTrimGeometry, m_nGridSpacing, true));
			Nodes.Add(new EdgeStitches(game, this, Parent.Geometry.EdgeStitchesGeometry, m_nGridSpacing));

			m_bActive = true;

			m_tToroidalOrigin = Int2.Zero;
		}

		private Block CreateBlock(Vector2 tGridSpaceOffset, bool isCentreFill)
		{
			return new Block(Game, this, Parent.Geometry.BlockGeometry, m_nGridSpacing, isCentreFill, tGridSpaceOffset, tGridSpaceOffset * m_nGridSpacing);
		}

		#endregion

		#region Methods

		public override void LoadContent()
		{
			// set initial min position of level
			Int2 tViewerPosGridCoords = ((Int2) Parent.Viewer.Position2D - m_tPositionMin) / m_nGridSpacing;
			Int2 tDeltaPositionTemp1 = tViewerPosGridCoords - Settings.CentralSquareMin;
			Int2 tDeltaPositionTemp2 = new Int2(Math.Abs(tDeltaPositionTemp1.X), Math.Abs(tDeltaPositionTemp1.Y));
			Int2 tDeltaPositionTemp3 = tDeltaPositionTemp1 - (tDeltaPositionTemp2 % 2);
			m_tPositionMin += tDeltaPositionTemp3 * m_nGridSpacing;

			_elevationTextureRenderer = new ScreenSpaceQuadRenderer(Game.GraphicsDevice,
				Settings.ElevationTextureSize, Settings.ElevationTextureSize,
				SurfaceFormat.Vector4, Game.Content, @"Effects\Terrain\UpdateElevation");

			INoiseService noise = Game.GetService<INoiseService>();
			_elevationTextureRenderer.Effect.SetValue("ElevationTextureSize", Settings.ElevationTextureSize);
			_elevationTextureRenderer.Effect.SetValue("PermutationTexture", noise.GetPermutationTexture());
			_elevationTextureRenderer.Effect.SetValue("GradientTexture", noise.GetGradientTexture());

			_normalMapTextureRenderer = new ScreenSpaceQuadRenderer(Game.GraphicsDevice,
				Settings.NormalMapTextureSize, Settings.NormalMapTextureSize,
				SurfaceFormat.Color, Game.Content, @"Effects\Terrain\ComputeNormals");

			_normalMapTextureRenderer.Effect.SetValue("ElevationTextureSizeInverse", Settings.ElevationTextureSizeInverse);
			_normalMapTextureRenderer.Effect.SetValue("NormalMapTextureSize", Settings.NormalMapTextureSize);

			_colourMapTextureRenderer = new ScreenSpaceQuadRenderer(Game.GraphicsDevice,
				Settings.ColourMapTextureSize, Settings.ColourMapTextureSize,
				SurfaceFormat.Color, Game.Content, @"Effects\Terrain\UpdateColourMap");

			_colourMapTextureRenderer.Effect.SetValue("ElevationTextureSizeInverse", Settings.ElevationTextureSizeInverse);
			_colourMapTextureRenderer.Effect.SetValue("ColourMapTextureSize", Settings.ColourMapTextureSize);

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			Int2 tViewerPosition2D = (Int2) Parent.Viewer.Position2D;

			// there is a central square of 2x2 grid units that we use to determine
			// if the level needs to move. if it doesn't need to move, we still might
			// need to update the interior trim position

			// each level only ever moves by TWICE the level's grid spacing

			// transform viewer position from world coords to grid coords
			m_tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;

			// check if viewer position is still in central square
			bool lUpdate = false;
			if (!(m_tViewerPosGridCoords >= Settings.CentralSquareMin && m_tViewerPosGridCoords <= Settings.CentralSquareMax))
			{
				// need to move level, so calculate new minimum position
				Int2 tDeltaPositionTemp1 = m_tViewerPosGridCoords - Settings.CentralSquareMin;
				Int2 tDeltaPositionTemp2 = new Int2(Math.Abs(tDeltaPositionTemp1.X), Math.Abs(tDeltaPositionTemp1.Y));
				Int2 tDeltaPositionTemp3 = tDeltaPositionTemp1 - (tDeltaPositionTemp2 % 2);
				m_tPositionMin += tDeltaPositionTemp3 * m_nGridSpacing;

				// recalculate viewer pos in grid coordinates
				m_tViewerPosGridCoords = (tViewerPosition2D - m_tPositionMin) / m_nGridSpacing;

				lUpdate = true;
			}

			// if this is the finest active level, we need to fill the hole with more blocks
			IsFinestLevel = (m_pNextFinerLevel == null || !m_pNextFinerLevel.Active);

			if (true || lUpdate)
			{
				UpdateElevationTexture();
				UpdateNormalMapTexture();
				//UpdateColourMapTexture();
			}

			// update child nodes
			base.Update(gameTime);
		}

		#region Render methods

		public override void Draw(GameTime gameTime)
		{
			Parent.Geometry.Effect.ViewerPos = (Parent.Viewer.Position2D - m_tPositionMin) / m_nGridSpacing;

			Vector2 tToroidalOriginScaled = (Vector2) m_tToroidalOrigin / (float) Settings.ElevationTextureSize;
			Vector2 tGridSizeScaled = new Vector2(((float) Settings.GridSizeN / (float) Settings.ElevationTextureSize));
			Parent.Geometry.Effect.ToroidalOffsets = new Vector4(tToroidalOriginScaled, tGridSizeScaled.X, tGridSizeScaled.Y);

			// draw all terrain sections for this level
			// TODO: make sure centre blocks work ok
			base.Draw(gameTime);
		}

		#endregion

		#endregion
	}
}