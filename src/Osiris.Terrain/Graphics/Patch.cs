using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.Graphics
{
	/// <summary>
	/// Summary description for Patch.
	/// </summary>
	public class Patch
	{
		#region Variables

		private readonly VertexBuffer _vertexBuffer;
		private readonly Level[] _levels;
		private readonly Vector3 _center;

		private Patch _left, _right, _top, _bottom;

		#endregion

		#region Properties

		public bool Visible { get; set; }
		public int ActiveLevel { get; set; }

		private bool LeftMoreDetailed
		{
			get { return (_left != null && _left.ActiveLevel < ActiveLevel); }
		}

		private bool RightMoreDetailed
		{
			get { return (_right != null && _right.ActiveLevel < ActiveLevel); }
		}

		private bool TopMoreDetailed
		{
			get { return (_top != null && _top.ActiveLevel < ActiveLevel); }
		}

		private bool BottomMoreDetailed
		{
			get { return (_bottom != null && _bottom.ActiveLevel < ActiveLevel); }
		}

		public int LeftActiveLevel
		{
			get {return (_left != null) ? _left.ActiveLevel : 1000;}
		}

		public int RightActiveLevel
		{
			get {return (_right != null) ? _right.ActiveLevel : 1000;}
		}

		public int TopActiveLevel
		{
			get {return (_top != null) ? _top.ActiveLevel : 1000;}
		}

		public int BottomActiveLevel
		{
			get {return (_bottom != null) ? _bottom.ActiveLevel : 1000;}
		}

		public Vector2 Offset { get; private set; }
		public BoundingBox BoundingBox { get; private set; }

		#endregion

		#region Constructors

		internal Patch(VertexBuffer vertexBuffer, Level[] levels, BoundingBox boundingBox, Vector3 center, Vector2 offset)
		{
			_vertexBuffer = vertexBuffer;
			_levels = levels;
			_center = center;
			Offset = offset;
			BoundingBox = boundingBox;
		}

		#endregion

		#region Methods

		internal void SetNeighbours(Patch pLeft, Patch pRight, Patch pTop, Patch pBottom)
		{
			_left = pLeft;
			_right = pRight;
			_top = pTop;
			_bottom = pBottom;
		}

		public void Initialize(float tau, ICameraService camera, GraphicsDevice graphicsDevice)
		{
			foreach (Level level in _levels)
				level.Initialize(tau, camera, graphicsDevice);
		}

		public void UpdateLevelOfDetail(ICameraService camera)
		{
			// calculate distance(sq) from centre of this patch to the camera
			Vector3 tDistance = _center - camera.Position;
			float fDistanceSq = tDistance.LengthSquared();

			// choose which level to use
			ActiveLevel = 0;
			for (short i = 1; i < _levels.Length; i++)
				if (fDistanceSq > _levels[i].MinimumDSq)
					ActiveLevel = i;
		}

		private static int GetNeighboursCode(bool bLeft, bool bRight, bool bTop, bool bBottom)
		{
			return ((bLeft) ? 1 : 0) | ((bRight) ? 2 : 0) | ((bTop) ? 4 : 0) | ((bBottom) ? 8 : 0);
		}

		public void UpdateTessellation()
		{
			// work out bitmask for neighbours
			int nCode = GetNeighboursCode(LeftMoreDetailed, RightMoreDetailed, TopMoreDetailed, BottomMoreDetailed);
			_levels[ActiveLevel].NeighboursCode = nCode;
		}

		public void Draw()
		{
			_vertexBuffer.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
			_levels[ActiveLevel].Draw(_vertexBuffer.VertexCount);
		}

		#endregion
	}
}
