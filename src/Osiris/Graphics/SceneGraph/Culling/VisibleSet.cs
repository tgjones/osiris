using System;
using System.Collections.Generic;
using System.Collections;
using Osiris.Graphics.Effects;
using Osiris.Graphics.SceneGraph.Geometries;

namespace Osiris.Graphics.SceneGraph.Culling
{
	public class VisibleSet
	{
		public List<Geometry> VisibleGeometries;
		public List<VisibleShaderEffect> VisibleShaderEffects;

		public VisibleSet()
		{
			VisibleGeometries = new List<Geometry>();
			VisibleShaderEffects = new List<VisibleShaderEffect>();
		}

		public void Clear()
		{
			VisibleGeometries.Clear();
			VisibleShaderEffects.Clear();
		}
	}
}
