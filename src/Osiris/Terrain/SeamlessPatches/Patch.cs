using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;
using Osiris.Content;

namespace Osiris.Terrain.SeamlessPatches
{
	public class Patch
	{
		#region Fields

		private static ExtendedEffect _effect;

		private static VertexBuffer[] _bottomTileVertices;
		private static IndexBuffer[] _bottomTileIndices;
		private static int[] _numVertices;
		private static int[] _numIndices;

		private BoundingSquare _boundingSquare;
		private Vector4 _scaleFactor;
		private int _requiredResolutionTop, _requiredResolutionBottom, _requiredResolutionLeft, _requiredResolutionRight;
		private List<Patch> _children;

		#endregion

		#region Properties

		public static ExtendedEffect Effect
		{
			get { return _effect; }
		}

		#endregion

		#region Constructor

		public Patch(BoundingSquare boundingSquare)
		{
			_boundingSquare = boundingSquare;

			// set scale factor
			// ScaleFactor.xy: size of current patch
			// ScaleFactor.zw: origin of current patch within world
			_scaleFactor.X = _scaleFactor.Y = boundingSquare.EdgeLength;
			_scaleFactor.Z = boundingSquare.Min.X;
			_scaleFactor.W = boundingSquare.Min.Y;
		}

		#endregion

		#region Methods

		public static void LoadContent(Game game)
		{
			GraphicsDevice device = ((IGraphicsDeviceService) game.Services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			_vertexDeclaration = new VertexDeclaration(device, TerrainVertex.VertexElements);
			_effect = new ExtendedEffect(AssetLoader.LoadAsset<Effect>(@"Terrain\SeamlessPatches\Effects\Terrain", Game));

			_bottomTileVertices = new VertexBuffer[Settings.NUM_RESOLUTIONS];
			_bottomTileIndices = new IndexBuffer[Settings.NUM_RESOLUTIONS];
			_numVertices = new int[Settings.NUM_RESOLUTIONS];
			_numIndices = new int[Settings.NUM_RESOLUTIONS];

			for (int i = 0; i < Settings.NUM_RESOLUTIONS; i++)
			{
				CreateBottomTileVerticesAndIndices(device, i);
				// ...
			}
		}

		private static void CreateBottomTileVerticesAndIndices(GraphicsDevice device, int resolution)
		{
			CreateTileVertexBuffer(device, resolution);
		}

		private static void CreateTileVertexBuffer(GraphicsDevice device, int resolution)
		{
			_numVertices[resolution] = 6;
			_numIndices[resolution] = 9;

			TerrainVertex[] vertices = new TerrainVertex[_numVertices[resolution]];
			short[] indices = new short[_numIndices[resolution]];
			short vertexCounter = 0, indexCounter = 0;

			// first create vertices for left half of triangle
			for (int z = 0; z > -(Settings.PATCH_SIZE / 2) + 1; z--)
			{
				vertices[vertexCounter] = new TerrainVertex(ScaleVertexCoordinate(1), ScaleVertexCoordinate(z));
				indices[indexCounter++] = vertexCounter++;

				for (int x = 2; x <= Settings.PATCH_SIZE / 2; x++)
				{
					vertices[vertexCounter] = new TerrainVertex(ScaleVertexCoordinate(x), ScaleVertexCoordinate(z - 1));
					indices[indexCounter++] = vertexCounter++;

					vertices[vertexCounter] = new TerrainVertex(ScaleVertexCoordinate(x), ScaleVertexCoordinate(z));
					indices[indexCounter++] = vertexCounter;

					if (x < Settings.PATCH_SIZE / 2)
						vertexCounter++;
				}

				indices[indexCounter++] = vertexCounter++;
			}

			indices[indexCounter++] = vertexCounter;
			indices[indexCounter++] = vertexCounter;

			// then create vertices for right half of triangle
			for (int x = Settings.PATCH_SIZE / 2; x < Settings.PATCH_SIZE - 1; x++)
			{
				vertices[vertexCounter] = new TerrainVertex(ScaleVertexCoordinate(x), ScaleVertexCoordinate(-1));
				indices[indexCounter++] = vertexCounter++;

				for (int z = -(Settings.PATCH_SIZE / 2) + 2; z <= 0; z++)
				{
					vertices[vertexCounter] = new TerrainVertex(ScaleVertexCoordinate(x + 1), ScaleVertexCoordinate(z));
					indices[indexCounter++] = vertexCounter++;

					vertices[vertexCounter] = new TerrainVertex(ScaleVertexCoordinate(x), ScaleVertexCoordinate(z));
					indices[indexCounter++] = vertexCounter;

					if (z < 0)
						vertexCounter++;
				}

				if (x < Settings.PATCH_SIZE - 2)
					indices[indexCounter++] = vertexCounter++;
			}

			// create shared vertex buffer
			_bottomTileVertices[resolution] = new VertexBuffer(
				device, typeof(TerrainVertex), _numVertices[resolution],
				BufferUsage.None);

			_bottomTileVertices[resolution].SetData<TerrainVertex>(vertices);

			// create shared index buffer
			_bottomTileIndices[resolution] = new IndexBuffer(
				device, typeof(short), _numIndices[resolution],
				BufferUsage.None);

			_bottomTileIndices[resolution].SetData<short>(indices);
		}

		private static float ScaleVertexCoordinate(float coord)
		{
			return coord;
			return coord / (float) Settings.PATCH_SIZE;
		}

		#region Update methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cameraPosition"></param>
		/// <returns>True if patch must be split</returns>
		public void Update(Vector3 cameraPosition)
		{
			_children = null;
			if (ComputeRequiredResolutions(cameraPosition))
			{
				_children = new List<Patch>(Settings.R_SQUARED);

				// traverse children
				int width = _boundingSquare.EdgeLength;
				for (int x = _boundingSquare.Min.X; x < _boundingSquare.Max.X; x += width / Settings.R)
				{
					for (int y = _boundingSquare.Min.Y; y > _boundingSquare.Max.Y; y -= width / Settings.R)
					{
						IntVector2 childMin = new IntVector2(x, y);
						BoundingSquare childBoundingSquare = new BoundingSquare(
							childMin, childMin + new IntVector2(width / Settings.R, -width / Settings.R));

						Patch childPatch = new Patch(childBoundingSquare);
						_children.Add(childPatch);

						childPatch.Update(cameraPosition);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cameraPosition"></param>
		/// <returns>True if patch must be split</returns>
		public bool ComputeRequiredResolutions(Vector3 cameraPosition)
		{
			bool requiresSplit = ComputeRequiredResolution(cameraPosition, _boundingSquare.TopEdgeMidpoint, out _requiredResolutionTop);
			if (requiresSplit) return true;

			requiresSplit = ComputeRequiredResolution(cameraPosition, _boundingSquare.BottomEdgeMidpoint, out _requiredResolutionBottom);
			if (requiresSplit) return true;

			requiresSplit = ComputeRequiredResolution(cameraPosition, _boundingSquare.LeftEdgeMidpoint, out _requiredResolutionLeft);
			if (requiresSplit) return true;

			requiresSplit = ComputeRequiredResolution(cameraPosition, _boundingSquare.RightEdgeMidpoint, out _requiredResolutionRight);
			if (requiresSplit) return true;

			return false;
		}

		private bool ComputeRequiredResolution(Vector3 cameraPos, Vector2 edgeMidpoint2D, out int requiredResolution)
		{
			Vector3 edgeMidpoint = new Vector3(edgeMidpoint2D.X, 0, edgeMidpoint2D.Y);

			// find distance of edge from viewpoint, based on midpoint of edge
			float distanceFromViewPoint = Vector3.Distance(cameraPos, edgeMidpoint);

			// calculate required resolution factor
			float requiredResolutionFactor = Settings.RHO * (_boundingSquare.EdgeLength / distanceFromViewPoint);

			// calculate required resolution by multiplying by max resolution and rounding up
			requiredResolution = MathsHelper.CeilingToInt(requiredResolutionFactor * Settings.NUM_RESOLUTIONS);
			requiredResolution -= 1;

			// if required resolution factor is greater than 1, patch needs to be split, as long as we're not at the max resolution already
			if (requiredResolutionFactor > 1)
			{
				if (requiredResolution > Settings.MAX_RESOLUTION)
				{
					requiredResolution = Settings.MAX_RESOLUTION;
					return false;
				}
				else
				{
					requiredResolution = -1;
					return true;
				}
			}

			return false;
		}

		#endregion

		public void Draw()
		{
			#warning Set vertex declaration

			if (_children == null || _children.Count == 0)
			{
				// draw this patch

				// draw bottom
				// we set the vertices and indices here because they are the same for all blocks
				_effect.GraphicsDevice.Vertices[0].SetSource(
					_bottomTileVertices[_requiredResolutionBottom],
					0,
					TerrainVertex.SizeInBytes);
				_effect.GraphicsDevice.Indices = _bottomTileIndices[_requiredResolutionBottom];

				_effect.Render(new RenderCallback(RenderBottomTile));
			}
			else
			{
				// we are drawing this patch's children
				foreach (Patch childPatch in _children)
					childPatch.Draw();
			}
		}

		public void RenderBottomTile(ExtendedEffect effect)
		{
			//pEffect.SetValue("FineBlockOrig", m_tFineBlockOrig);
			//pEffect.SetValue("FineBlockOrig2", m_tFineBlockOrig2);
			effect.SetValue("ScaleFactor", _scaleFactor);
			effect.CommitChanges();

			// render
			effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
				0,                              // base vertex
				0,                              // min vertex index
				_numVertices[_requiredResolutionBottom], // total num vertices - note that is NOT just vertices that are indexed, but all vertices
				0,                              // start index
				_numIndices[_requiredResolutionBottom] - 2); // primitive count
		}

		#endregion
	}
}
