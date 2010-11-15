using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Osiris.Terrain.ChunkedLod
{
	public class Chunk
	{
		private int _scale;
		private Vector2 _offset;
		private List<Chunk> _children;
		private bool _shouldDraw;
		private BoundingBox _boundingBox;
		private float delta;

		public int Scale
		{
			get { return _scale; }
		}

		public Vector2 Offset
		{
			get { return _offset; }
		}

		public List<Chunk> Children
		{
			get { return _children; }
		}

		public bool ShouldDraw
		{
			get { return _shouldDraw; }
		}

		public BoundingBox BoundingBox
		{
			get { return _boundingBox; }
			set { _boundingBox = value; }
		}

		public Chunk(int scale, Vector2 offset)
		{
			_scale = scale;
			_offset = offset;

			_children = new List<Chunk>();
		}

		public void Update()
		{
			//_shouldDraw = Rho(node, viewpoint) <= tau;
		}
	}
}
