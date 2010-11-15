using System;
using Osiris.Graphics.Rendering;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.SceneGraph.Nodes
{
	/*public class CameraNode : Node
	{
		private Camera _camera;

		public Camera Camera
		{
			get { return _camera; }
			set
			{
				SetCamera(value);
				UpdateGeometricState();
			}
		}

		// Construction and destruction.  The node's world translation is used
    // as the camera's location.  The node's world rotation matrix is used
    // for the camera's coordinate axes.  Column 0 of the world rotation
    // matrix is the camera's direction vector, column 1 of the world rotation
    // matrix is the camera's up vector, and column 2 of the world rotation
    // matrix is the camera's right vector.
    //
    // On construction, the node's local transformation is set to the
    // camera's current coordinate system.
    //   local translation       = camera location
    //   local rotation column 0 = camera direction
    //   local rotation column 1 = camera up
    //   local rotation column 2 = camera right
		public CameraNode(Camera camera)
		{
			SetCamera(camera);
		}

		private void SetCamera(Camera camera)
		{
			_camera = camera;

			if (_camera != null)
			{
				Local.Translation = _camera.Location;
				Local.Forward = _camera.DirectionVector;
				Local.Up = _camera.UpVector;
				Local.Right = _camera.RightVector;
			}
		}

		protected override void UpdateWorldData(GameTime gameTime)
		{
			base.UpdateWorldData(gameTime);

			if (_camera != null)
				_camera.SetFrame(World.Translation, World.Forward, World.Up, World.Right);
		}
	}*/
}
