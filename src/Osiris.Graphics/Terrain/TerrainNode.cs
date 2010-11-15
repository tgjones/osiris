using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Graphics;
using Osiris.SceneObjects.SceneGraph;
using Osiris.Maths;
using Osiris.Graphics.SceneGraph.Nodes;

namespace Osiris.Graphics.Terrain
{
	public class TerrainNode : Node, ITerrainService
	{
		#region Fields

		private Int2 m_tPreviousViewerPosition;

		//private ScreenSpaceQuadRenderer _heightReadbackRenderer;
		//private Texture2D m_pHeightReadbackTexture, m_pHeightReadbackOffsetsTexture;

		#endregion

		#region Constructor

		public TerrainNode(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			game.Services.AddService(typeof(ITerrainService), this);
		}

		#endregion

		#region Methods

		public override void LoadContent()
		{
			_geometry = Game.Content.Load<TerrainGeometry>(_terrainDescriptionAssetName);

			// create levels
			for (int i = _geometry.NumLevels - 1; i >= 0; i--)
				Nodes.Add(new Level(Game, this, MathsHelper.Pow(2, i)));

			// give each level information about its next finer and next coarser levels
			for (int i = 0, length = Nodes.Count; i < length; i++)
			{
				Level pLevel = (Level) Nodes[i];

				if (i != length - 1)
					pLevel.NextFinerLevel = (Level) Nodes[i + 1];
				if (i > 0)
					pLevel.NextCoarserLevel = (Level) Nodes[i - 1];
			}

			base.LoadContent();

			/*Texture2D heightColourMappingTexture = new Texture2D(Game.GraphicsDevice, 4, 1, 1, TextureUsage.None, SurfaceFormat.Color);
			Color[] colourMappings = new Color[4];
			colourMappings[0] = new Color(255, 0, 0, 0);
			colourMappings[1] = new Color(0, 255, 0, 0);
			colourMappings[2] = new Color(0, 0, 255, 0);
			colourMappings[3] = new Color(0, 0, 0, 255);
			heightColourMappingTexture.SetData<Color>(colourMappings);
			Geometry.Effect.InnerEffect.SetValue("HeightColourMappingTexture", heightColourMappingTexture);*/

			_heightReadbackRenderer = new ScreenSpaceQuadRenderer(Game.GraphicsDevice, 5, 1, SurfaceFormat.Vector4, Game.Content,
				@"Effects\Terrain\HeightReadback");

			_heightReadbackRenderer.Effect.SetValue("TextureSize", Geometry.Settings.ElevationTextureSize);

			m_pHeightReadbackOffsetsTexture = new Texture2D(Game.GraphicsDevice, 5, 1, 1, TextureUsage.None, SurfaceFormat.Vector2);
		}

		public void Update(Int2 viewerPosition)
		{
			//IAtmosphereService atmosphereService = GetService<IAtmosphereService>();
			//atmosphereService.SetEffectParameters(_effect, false);

			Int2 tDeltaViewerPosition = viewerPosition - m_tPreviousViewerPosition;
			m_tPreviousViewerPosition = viewerPosition;

			// only bother updating levels if viewer has moved at all
			//if (tDeltaViewerPosition != Int2.Zero)
			//{
			// update levels in coarse-to-fine order
			base.Update(gameTime);
			//}
		}

		public void GetHeightAndNormalAtPoints(Vector2[] p, out float[] heights, out Vector3[] normals)
		{
			throw new NotImplementedException();

			/*Level finestLevel = (Level) Nodes[Nodes.Count - 1];

			// we assume that p is located within the finest level
			Vector2[] textureOffsets = new Vector2[p.Length];
			for (int i = 0; i < p.Length; i++)
				textureOffsets[i] = p[i] - finestLevel.PositionMin;
			m_pHeightReadbackOffsetsTexture.SetData<Vector2>(textureOffsets);

			_heightReadbackRenderer.Effect.SetValue("ElevationTexture", finestLevel.ElevationTexture);
			_heightReadbackRenderer.Effect.SetValue("NormalMapTexture", finestLevel.NormalMapTexture);
			_heightReadbackRenderer.Effect.SetValue("HeightReadbackTextureSize", new Vector2(p.Length, 1));
			_heightReadbackRenderer.Effect.SetValue("OffsetsTexture", m_pHeightReadbackOffsetsTexture);

			m_pHeightReadbackTexture = _heightReadbackRenderer.RenderToTexture(
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
			}*/
		}

		#endregion
	}
}