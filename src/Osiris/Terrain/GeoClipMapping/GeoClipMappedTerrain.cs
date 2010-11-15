using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Content;
using Osiris.Graphics;
using Osiris.Sky;

namespace Osiris.Terrain.GeoClipMapping
{
	public class GeoClipMappedTerrain : Terrain
	{
		#region Fields

		private VertexDeclaration _vertexDeclaration;

		private Level[] m_pLevels;

		private IntVector2 m_tPreviousViewerPosition;

		private IViewer m_pViewer;

		private SpriteBatch m_pLevelHeightMap;

		private RenderTarget2D m_pHeightReadbackTextureTarget;
		private Texture2D m_pHeightReadbackTexture, m_pHeightReadbackOffsetsTexture;
		private ExtendedEffect m_pHeightReadbackEffect;

		#endregion

		#region Properties

		public override BoundingBox BoundingBox
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public IViewer Viewer
		{
			get { return m_pViewer; }
			set { m_pViewer = value; }
		}

		public ExtendedEffect Effect
		{
			get { return _effect; }
		}

		private Level FinestLevel
		{
			get { return m_pLevels[m_pLevels.Length - 1]; }
		}

		#endregion

		#region Constructor

		public GeoClipMappedTerrain(Game pGame)
			: base(pGame, @"Terrain\GeoClipMapping\Terrain")
		{
			const int NUM_LEVELS = 11;
			m_pLevels = new Level[NUM_LEVELS];
			int nCounter = 0;
			for (int i = NUM_LEVELS - 1; i >= 0; i--)
			{
				m_pLevels[nCounter++] = new Level(pGame, this, MathsHelper.Pow(2, i));
			}

			IsAlwaysVisible = true;
		}

		#endregion

		#region Methods

		public override void GetHeightAndNormalAtPoints(Vector2[] p, out float[] heights, out Vector3[] normals)
		{
			// we assume that p is located within the finest level
			Vector2[] textureOffsets = new Vector2[p.Length];
			for (int i = 0; i < p.Length; i++)
				textureOffsets[i] = p[i] - FinestLevel.PositionMin;
			m_pHeightReadbackOffsetsTexture.SetData<Vector2>(textureOffsets);

			m_pHeightReadbackEffect.SetValue("ElevationTexture", FinestLevel.ElevationTexture);
			m_pHeightReadbackEffect.SetValue("NormalMapTexture", FinestLevel.NormalMapTexture);
			m_pHeightReadbackEffect.SetValue("TextureSize", Settings.ELEVATION_TEXTURE_SIZE);
			m_pHeightReadbackEffect.SetValue("HeightReadbackTextureSize", new Vector2(p.Length, 1));
			m_pHeightReadbackEffect.SetValue("OffsetsTexture", m_pHeightReadbackOffsetsTexture);

			m_pHeightReadbackTexture = RenderToTexture(
				m_pHeightReadbackTextureTarget, m_pHeightReadbackEffect,
				0, (short) p.Length,
				0, 1);

			// read value out of texture - the X component is the height; (Y, Z, W) is the normal
			Vector4[] data = new Vector4[p.Length];
			m_pHeightReadbackTexture.GetData<Vector4>(data);

			heights = new float[p.Length]; normals = new Vector3[p.Length];
			for (int i = 0; i < data.Length; i++)
			{
				heights[i] = data[i].X;
				normals[i] = new Vector3(data[i].Y, data[i].Z, data[i].W);
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_vertexDeclaration = new VertexDeclaration(GraphicsDevice, TerrainVertex.VertexElements);

			for (int i = 0, length = m_pLevels.Length; i < length; i++)
			{
				Level pLevel = m_pLevels[i];

				Level pNextFinerLevel = null, pNextCoarserLevel = null;
				if (i != length - 1)
					pNextFinerLevel = m_pLevels[i + 1];
				if (i > 0)
					pNextCoarserLevel = m_pLevels[i - 1];

				pLevel.Create(pNextFinerLevel, pNextCoarserLevel, (IntVector2) m_pViewer.Position2D);
			}

			foreach (Level pLevel in m_pLevels)
				pLevel.Create2();

			m_pLevelHeightMap = new SpriteBatch(GraphicsDevice);

			_effect.SetValue("ColourTexture1", AssetLoader.LoadAsset<Texture2D>(@"Terrain\grass", Game));
			_effect.SetValue("ColourTexture2", AssetLoader.LoadAsset<Texture2D>(@"Terrain\mud", Game));
			_effect.SetValue("ColourTexture3", AssetLoader.LoadAsset<Texture2D>(@"Terrain\dirt", Game));
			_effect.SetValue("ColourTexture4", AssetLoader.LoadAsset<Texture2D>(@"Terrain\stone", Game));

			Texture2D heightColourMappingTexture = new Texture2D(GraphicsDevice, 4, 1, 1, TextureUsage.None, SurfaceFormat.Color);
			Color[] colourMappings = new Color[4];
			colourMappings[0] = new Color(255, 0, 0, 0);
			colourMappings[1] = new Color(0, 255, 0, 0);
			colourMappings[2] = new Color(0, 0, 255, 0);
			colourMappings[3] = new Color(0, 0, 0, 255);
			heightColourMappingTexture.SetData<Color>(colourMappings);
			_effect.SetValue("HeightColourMappingTexture", heightColourMappingTexture);

			m_pHeightReadbackTextureTarget = new RenderTarget2D(GraphicsDevice,
				5,
				1,
				1,
				SurfaceFormat.Vector4,
				RenderTargetUsage.PreserveContents); // TODO: Remove this flag

			m_pHeightReadbackEffect = new ExtendedEffect(AssetLoader.LoadAsset<Effect>(@"Terrain\GeoClipMapping\HeightReadback", Game));

			m_pHeightReadbackOffsetsTexture = new Texture2D(GraphicsDevice, 5, 1, 1, TextureUsage.None, SurfaceFormat.Vector2);
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			IAtmosphereService atmosphereService = GetService<IAtmosphereService>();
			atmosphereService.SetEffectParameters(_effect, false);

			IntVector2 tViewerPosition2D = (IntVector2) m_pViewer.Position2D;
			IntVector2 tDeltaViewerPosition = tViewerPosition2D - m_tPreviousViewerPosition;
			m_tPreviousViewerPosition = tViewerPosition2D;

			// only bother updating levels if viewer has moved at all
			/*if (tDeltaViewerPosition != IntVector2.Zero)
			{*/
				// update levels in coarse-to-fine order
				foreach (Level pLevel in m_pLevels)
				{
					pLevel.Update(m_pViewer.Position, tViewerPosition2D);
				}
			//}
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			GraphicsDevice.RenderState.DepthBufferEnable = true;
			foreach (Level pLevel in m_pLevels)
				pLevel.Draw(_effect);
		}

		#endregion
	}
}
