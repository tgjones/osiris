using System;
using Microsoft.Xna.Framework;
using Osiris.Graphics.Cameras;
using Microsoft.Xna.Framework.Input;

namespace Osiris.Graphics.Lights
{
	public class SimpleSunlight : Projector, ILightService
	{
		#region Fields

		private readonly Vector4 _diffuse;
		private readonly Vector4 _ambient;

		private float _angle;

		#endregion

		#region Properties

		public override Vector3 Direction
		{
			get { return Vector3.Normalize(Vector3.TransformNormal(Vector3.Normalize(new Vector3(0, 1, 0)), Matrix.CreateRotationZ(_angle))); }
		}

		public Vector4 Diffuse
		{
			get { return _diffuse; }
		}

		public Vector4 Ambient
		{
			get { return _ambient; }
		}

		public float Angle
		{
			get { return _angle; }
			set { _angle = value; }
		}

		#endregion

		public SimpleSunlight(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(ILightService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);

			this.UpdateOrder = 2500;

			_diffuse = new Vector4(1f, 1f, 1f, 1);
			_ambient = new Vector4(0.5f, 0.5f, 0.5f, 1);

			_angle = MathHelper.PiOver4;
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(ILightService));
		}

		public override void Update(GameTime gameTime)
		{
			// check for up/down arrow
			KeyboardState keyboardState = Keyboard.GetState();
			if (keyboardState.IsKeyDown(Keys.Add))
				_angle += 0.005f;
			else if (keyboardState.IsKeyDown(Keys.Subtract))
				_angle -= 0.005f;

			// create view matrix with position as the centre of the top face of the bounding box
			ISceneService scene = (ISceneService) this.Game.Services.GetService(typeof(ISceneService));
			Vector3[] boundingBoxCorners = scene.BoundingBox.GetCorners();
			Vector3 boundingBoxCentre = Vector3.Zero;
			foreach (Vector3 boundingBoxCorner in boundingBoxCorners)
				boundingBoxCentre += boundingBoxCorner;
			boundingBoxCentre /= BoundingBox.CornerCount;

			//boundingBoxCentre = boundingBoxCorners[0] + boundingBoxCorners[1] + boundingBoxCorners[4] + boundingBoxCorners[5];
			//boundingBoxCentre /= 4;

			_view = Matrix.CreateLookAt(
				boundingBoxCentre + (this.Direction),
				boundingBoxCentre,
				Vector3.Up);

			// transform bounding box vertices by light view matrix. this gives us the bounding box
			// in light view space, from which we can create an orthographic projection
			// based on the dimensions of this AABB
			Vector3[] transformedCorners = new Vector3[BoundingBox.CornerCount];
			Vector3.Transform(boundingBoxCorners, ref _view, transformedCorners);
			BoundingBox lightViewBoundingBox = BoundingBox.CreateFromPoints(transformedCorners);

			float farZ = lightViewBoundingBox.Max.Z - lightViewBoundingBox.Min.Z;

			// Essentially, for this situation you have to dynamically compute a temporary "working" position for your light.
			// One way to do it would be to transform the eight corners of the camera frustum into the light's "view" space.
			// By doing this you can compute the FarZ (far clip plane distance) which would be the difference between the max.z and min.z
			// corners of the camera frustum points in light view space.
			// You could then position the light by backing up from the centroid of the camera frustum in the opposite direction
			// of the light by the amount of max.z. Your near clip plane would be 0.0, your far clip plane would max.z - min.z,
			// your look at point would be the camera's frustum centroid, and your up vector would be anything orthogonal to
			// your light's direction. You would build an othographic projection and your max/min values would simply by max.x & max.y
			// and min.x & min.y.
			// Hopefully that makes some sense. :)

			_projection = Matrix.CreateOrthographicOffCenter(
				lightViewBoundingBox.Min.X,
				lightViewBoundingBox.Max.X,
				lightViewBoundingBox.Min.Y,
				lightViewBoundingBox.Max.Y,
				0,
				farZ);

			_view = Matrix.CreateLookAt(
				boundingBoxCentre + (this.Direction * lightViewBoundingBox.Max.Z),
				boundingBoxCentre,
				Vector3.Up);

			base.Update(gameTime);
		}
	}
}
