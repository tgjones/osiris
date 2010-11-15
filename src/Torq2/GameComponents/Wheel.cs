using System;
using Microsoft.Xna.Framework;
using Osiris.Graphics;
using Osiris.Graphics.Lights;
using Osiris.Graphics.Cameras;
using Osiris.Graphics.Shadows;

namespace Torq2.GameComponents
{
	class Wheel
	{
		private Vehicle _vehicle;
		private ExtendedModel _model;

		public Vector3 Orientation;
		public Vector3 Position;
		public Vector3 Force;
		public bool IsSteerable;
		public float TerrainHeight;

		public Vector3 TransformedForce
		{
			get
			{
				return Vector3.Transform(this.Force, Matrix.CreateRotationY(this.Orientation.Y));
			}
		}

		public float TransformedHeight
		{
			get
			{
				return TerrainHeight - _vehicle.Position.Y;
			}
		}

		public Vector2 TransformedPosition
		{
			get
			{
				Vector2 lPosition = new Vector2(_vehicle.Position.X, _vehicle.Position.Z);
				lPosition.X -= (float) Math.Sin(_vehicle.Orientation.Y) * Position.Z;
				lPosition.X -= (float) Math.Cos(_vehicle.Orientation.Y) * Position.X;
				lPosition.Y += (float) Math.Sin(_vehicle.Orientation.Y) * Position.X;
				lPosition.Y += (float) Math.Cos(_vehicle.Orientation.Y) * Position.Z;
				return lPosition;
			}
		}

		public BoundingBox BoundingBox
		{
			get { return _model.BoundingBox; }
		}

		public Wheel(Game game, Vehicle vehicle)
		{
			_vehicle = vehicle;

			_model = new ExtendedModel(game, @"Models\ToyotaWheel");

			Orientation = new Vector3();
			Position = new Vector3();
			Force = new Vector3();

			IsSteerable = false;
		}

		public void Update()
		{
			this.Position.Y = TransformedHeight - _model.LocalBoundingBox.Min.Y;

			_model.WorldMatrix =
				Matrix.CreateRotationY(-this.Orientation.Y) *
				Matrix.CreateTranslation(this.Position) *
				Matrix.CreateRotationY(-_vehicle.Orientation.Y) *
				Matrix.CreateTranslation(_vehicle.Position);
		}

		public void Draw(bool shadowPass)
		{
			ILightService light = (ILightService) _vehicle.Game.Services.GetService(typeof(ILightService));
			if (shadowPass)
			{
				_vehicle.Effect.SetValue("LightWorldViewProjection", _model.WorldMatrix * light.ViewMatrix * light.ProjectionMatrix);
			}
			else
			{
				ICameraService camera = (ICameraService) _vehicle.Game.Services.GetService(typeof(ICameraService));
				_vehicle.Effect.SetValue("WorldViewProjection", _model.WorldMatrix * camera.ViewProjectionMatrix);
			}

			IShadowMapService shadowMap = (IShadowMapService) _vehicle.Game.Services.GetService(typeof(IShadowMapService));
			if (shadowMap != null)
			{
				_vehicle.Effect.SetValue("ShadowMapProjector", _model.WorldMatrix * light.ViewMatrix * light.ProjectionMatrix * _vehicle.TextureScaleAndOffsetMatrix);
				Vehicle.DrawMesh(_vehicle.GraphicsDevice, _vehicle.Effect, _model);
			}
		}
	}
}
