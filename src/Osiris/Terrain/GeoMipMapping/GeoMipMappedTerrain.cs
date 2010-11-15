using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Osiris.Content;
using Osiris.Graphics.Cameras;
using Osiris.Sky;
using Osiris.Graphics;
using System.ComponentModel;

namespace Osiris.Terrain.GeoMipMapping
{
	/// <summary>
	/// Summary description for Map.
	/// </summary>
	public class GeoMipMappedTerrain : Terrain
	{
		#region Variables

		private short _patchSize;
		private float _tau;

		private BoundingBox _boundingBox;

		private VertexDeclaration _vertexDeclaration;

		private int m_nNumPatchesY, m_nNumPatchesX;
		private Patch[,] m_pPatches;

		private IHeightMapService m_pHeightMap;

		#endregion

		#region Properties

		[Description("Error metric for geo-mipmapped terrain. Corresponds to the largest screen-space error that will be tolerated before switching to the next most detailed level"), DefaultValue(2.0f)]
		public float Tau
		{
			get { return _tau; }
			set
			{
				_tau = value;

				for (int y = 0; y < m_nNumPatchesY; y++)
					for (int x = 0; x < m_nNumPatchesX; x++)
						m_pPatches[x, y].RecalculateMinimumD(value);
			}
		}

		public short PatchSize
		{
			get { return _patchSize; }
		}

		public override BoundingBox BoundingBox
		{
			get { return _boundingBox; }
		}

		#endregion

		#region Constructors

		public GeoMipMappedTerrain(Game game)
			: base(game, @"Terrain\GeoMipMapping\Terrain")
		{
			_patchSize = 129;
			_tau = 2.0f;

			IsAlwaysVisible = true;
		}

		#endregion

		#region Methods

		protected override void LoadContent()
		{
			base.LoadContent();

			_effect.SetValue("LayerMap0Texture", AssetLoader.LoadAsset<Texture2D>(@"Tracks\Track 1\mud", Game));
			_effect.SetValue("LayerMap1Texture", AssetLoader.LoadAsset<Texture2D>(@"Tracks\Track 1\stone", Game));
			_effect.SetValue("LayerMap2Texture", AssetLoader.LoadAsset<Texture2D>(@"Tracks\Track 1\grass", Game));
			_effect.SetValue("BlendMapTexture", AssetLoader.LoadAsset<Texture2D>(@"Tracks\Track 1\blend_hm1", Game));

			_vertexDeclaration = new VertexDeclaration(this.GraphicsDevice, TerrainVertex.VertexElements);

			#region Patches

			m_pHeightMap = GetService<IHeightMapService>();

			ITerrainNormalsService terrainNormals = GetService<ITerrainNormalsService>();

			float fSunAngle = 25;
			//Vector3 tLightDirection = new Vector3(1, MathsHelper.Tan(fSunAngle), 0);
			Vector3 tLightDirection = new Vector3(1, -1, 0);
			tLightDirection.Normalize();
			LightMap pLightMap = new LightMap(m_pHeightMap, terrainNormals, fSunAngle, tLightDirection, 0.4f, this.GraphicsDevice);
			_effect.SetValue("LightTexture", pLightMap.Texture);
			pLightMap.Texture.Save("LightTexture.png", ImageFileFormat.Png);

			_boundingBox = new BoundingBox(Vector3.Zero, new Vector3(m_pHeightMap.Width, 255, -m_pHeightMap.Height));

			m_nNumPatchesX = (m_pHeightMap.Width - 1) / (_patchSize - 1);
			m_nNumPatchesY = (m_pHeightMap.Height - 1) / (_patchSize - 1);

			// create patches
			m_pPatches = new Patch[m_nNumPatchesX, m_nNumPatchesY];
			for (int y = 0; y < m_nNumPatchesY; y++)
				for (int x = 0; x < m_nNumPatchesX; x++)
				{
					m_pPatches[x, y] = new Patch(this.Game, this, m_pHeightMap, x, y);
					m_pPatches[x, y].Initialize();
				}

			// now set neighbours
			for (int y = 0; y < m_nNumPatchesY; y++)
			{
				for (int x = 0; x < m_nNumPatchesX; x++)
				{
					m_pPatches[x, y].SetNeighbours(
						GetPatch(x - 1, y),
						GetPatch(x + 1, y),
						GetPatch(x, y - 1),
						GetPatch(x, y + 1));
				}
			}

			#endregion
		}

		/*public override float GetHeight(float x, float z)
		{
			return m_pHeightMap[x, Math.Abs(z)];
		}*/

		public Patch GetPatch(int nX, int nY)
		{
			if (nX < 0 || nX > m_nNumPatchesX - 1 || nY < 0 || nY > m_nNumPatchesY - 1)
			{
				return null;
			}
			else
			{
				return m_pPatches[nX, nY];
			}
		}

		public short GetIndex(int nX, int nY)
		{
			// sanity check that x and y are valid
			System.Diagnostics.Debug.Assert(nX < _patchSize && nY < _patchSize);
			return (short) ((nY * _patchSize) + nX);
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			IAtmosphereService atmosphereService = GetService<IAtmosphereService>();
			atmosphereService.SetEffectParameters(_effect, false);

			_effect.SetValue("patchSize", _patchSize);
			_effect.SetValue("heightMapSize", m_pHeightMap.Width);

			for (int y = 0; y < m_nNumPatchesY; y++)
				for (int x = 0; x < m_nNumPatchesX; x++)
					m_pPatches[x, y].Update(gameTime);

			// TODO: Maybe don't need to update levels for invisible patches?
			// set preferred tesselation levels for each patch
			for (int y = 0; y < m_nNumPatchesY; y++)
				for (int x = 0; x < m_nNumPatchesX; x++)
					m_pPatches[x, y].UpdateLOD();

			// now, make sure that each patch is no more than 1 level different from its neighbours
			// we loop through all patches, and if any are changed, set a flag. continue the outer
			// loop until the "changed" flag is false for all patches
			bool bChanged;
			do
			{
				bChanged = false;

				for (int y = 0; y < m_nNumPatchesY; y++)
				{
					for (int x = 0; x < m_nNumPatchesX; x++)
					{
						// get the minimum level for neighbouring patches
						Patch pPatch = m_pPatches[x, y];
						int nLevel = pPatch.ActiveLevel;
						int nLeft = pPatch.LeftActiveLevel;
						int nRight = pPatch.RightActiveLevel;
						int nTop = pPatch.TopActiveLevel;
						int nBottom = pPatch.BottomActiveLevel;

						int nMinimumNeighbouringLevel = Math.Min(Math.Min(nLeft, nRight), Math.Min(nTop, nBottom));

						if (nLevel > nMinimumNeighbouringLevel + 1)
						{
							pPatch.ActiveLevel = nMinimumNeighbouringLevel + 1;
							bChanged = true;
						}
					}
				}
			}
			while (bChanged);

			// finally, update geometry to match LOD
			for (int y = 0; y < m_nNumPatchesY; y++)
				for (int x = 0; x < m_nNumPatchesX; x++)
					m_pPatches[x, y].UpdateTessellation();

			int numVisible = 0;
			for (int y = 0; y < m_nNumPatchesY; y++)
				for (int x = 0; x < m_nNumPatchesX; x++)
					if (m_pPatches[x, y].Visible)
						numVisible++;

			ILoggerService logger = GetService<ILoggerService>();
			logger.WriteLine(string.Format("Patches (Visible / Total): {0} / {1}", numVisible, m_pPatches.LongLength));
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			// set vertex buffer for terrain
			this.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			this.GraphicsDevice.Vertices[0].SetSource(
				Patch.SharedVertexBuffer,
				0,
				TerrainVertex.Shared.SizeInBytes);

			// this needs to change to render closest patches first, so that we
			// make proper use of the z-index
			for (int y = 0; y < m_nNumPatchesY; y++)
			{
				for (int x = 0; x < m_nNumPatchesX; x++)
				{
					if (m_pPatches[x, y].Visible)
					{
						_effect.SetValue("offset", m_pPatches[x, y].Offset);

						// start effect rendering
						_effect.Begin();
						foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
						{
							pass.Begin();
							m_pPatches[x, y].Draw(gameTime);
							pass.End();
						}
						_effect.End();
					}
				}
			}
		}

		#endregion
	}
}
