using System;
using Osiris.Graphics.Rendering;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Osiris.Graphics.Effects;
using Osiris.Graphics.SceneGraph.Geometries;
using System.Collections.Generic;
using Osiris.Graphics.Rendering.Cameras;

namespace Osiris.Graphics.SceneGraph.Culling
{
	public class Culler
	{
		// The input camera has information that might be needed during the
		// culling pass over the scene.
		public Camera Camera;

		// A copy of the view frustum for the input camera.  This allows various
    // subsystems to change the frustum parameters during culling (for
    // example, the portal system) without affecting the camera, whose initial
    // state is needed by the renderer.
    public BoundingFrustum Frustum;

		// The potentially visible set for a call to GetVisibleSet.
		private VisibleSet _visibleSet;

		public VisibleSet VisibleSet
		{
			get { return _visibleSet; }
		}

		public Culler()
		{
			_visibleSet = new VisibleSet();
		}

		// Compare the object's world bounding volume against the culling planes.
    // Only Spatial calls this function.
		public bool IsVisible(BoundingSphere worldBound)
		{
			return Frustum.Contains(worldBound) != ContainmentType.Disjoint;
		}

		// This is the main function you should use for culling within a scene
		// graph.  Traverse the scene and construct the potentially visible set
		// relative to the world planes.
		public void ComputeVisibleSet(Spatial scene)
		{
			Frustum = Camera.Frustum;
			_visibleSet.Clear();
			scene.OnGetVisibleSet(this, false);
		}
	}
}
