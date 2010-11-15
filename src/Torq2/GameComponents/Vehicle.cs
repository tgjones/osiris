using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Osiris;
using Osiris.Graphics;
using Osiris.Input;
using Osiris.Maths;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Terrain;
using Osiris.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Torq2.GameComponents
{
	public class Vehicle : Osiris.DrawableGameComponent, IViewer
	{
		private ExtendedModel _model;

		private const string KEY_ACCELERATE = "Accelerate";
		private const string KEY_STEER_LEFT = "Steer Left";
		private const string KEY_STEER_RIGHT = "Steer Right";
		private const string KEY_BRAKE = "Brake";

		private const int WHEEL_FL = 0;
		private const int WHEEL_FR = 1;
		private const int WHEEL_RL = 2;
		private const int WHEEL_RR = 3;

		private const float TRACK = 2.0f;
		private const float WHEELBASE = 4.3f;
		private const float TURN_RADIUS = 4.0f;
		private const float SPRING_RATE = 20.0f;
		private const float SPRING_STROKE = 0.4f;
		private const float RIDE_HEIGHT = 0.8f;
		private const float DAMPING = 0.4f;
		private const float TORQUE = 200.0f;
		private const float MASS = 20.0f;
		private const float GRIP = 20.0f;
		private const float AERO = 0.95f;

		private Vector3 _controlState;
		private Vector3 _orientation;
		private Vector3 _rotation;
		private Vector3 _velocity;
		private Vector3 _position;
		private Wheel[] _wheels;
		private bool _terrainIsGarbage;

		private Cue _engineSound;

		public Vector3 Orientation
		{
			get { return _orientation; }
		}

		public Vector3 Direction
		{
			get { return Orientation; }
		}

		public override BoundingBox BoundingBox
		{
			get
			{
				BoundingBox boundingBox = _model.BoundingBox;
				foreach (Wheel wheel in _wheels)
					boundingBox = BoundingBox.CreateMerged(boundingBox, wheel.BoundingBox);
				return boundingBox;
			}
		}

		public Vector3 Position
		{
			get { return _position; }
		}

		public Vector2 Position2D
		{
			get { return new Vector2(Position.X, Position.Z); }
		}

		public ExtendedEffect Effect
		{
			get { return _effect; }
		}

		public Vehicle(Game game, Vector3 initialPosition)
			: base(game, @"Graphics\Mesh", true, false, false)
		{
			UpdateOrder = 1000;

			IAudioService audio = GetService<IAudioService>();
			_engineSound = audio.GetCue("EngineMedium");

			IInputService lInputService = GetService<IInputService>();
			lInputService.RegisterMapping(KEY_ACCELERATE, Keys.Up);
			lInputService.RegisterMapping(KEY_STEER_LEFT, Keys.Left);
			lInputService.RegisterMapping(KEY_STEER_RIGHT, Keys.Right);
			lInputService.RegisterMapping(KEY_BRAKE, Keys.Down);

			_controlState = new Vector3();
			_orientation = new Vector3();
			_rotation = new Vector3();
			_velocity = new Vector3();
			_position = initialPosition;
			_terrainIsGarbage = true;

			_wheels = new Wheel[4];

			_wheels[WHEEL_FL] = new Wheel(game, this);
			_wheels[WHEEL_FR] = new Wheel(game, this);
			_wheels[WHEEL_RL] = new Wheel(game, this);
			_wheels[WHEEL_RR] = new Wheel(game, this);

			_wheels[WHEEL_FL].Position.X = TRACK / 2.0f;
			_wheels[WHEEL_FR].Position.X = TRACK / -2.0f;
			_wheels[WHEEL_RL].Position.X = TRACK / 2.0f;
			_wheels[WHEEL_RR].Position.X = TRACK / -2.0f;

			_wheels[WHEEL_FL].Position.Z = WHEELBASE / 2.0f;
			_wheels[WHEEL_FR].Position.Z = WHEELBASE / 2.0f;
			_wheels[WHEEL_RL].Position.Z = WHEELBASE / -2.0f;
			_wheels[WHEEL_RR].Position.Z = WHEELBASE / -2.0f;

			_wheels[WHEEL_FL].IsSteerable = true;
			_wheels[WHEEL_FR].IsSteerable = true;

			_model = new ExtendedModel(game, @"Models\ToyotaBody");
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			if (_terrainIsGarbage)
			{
				_terrainIsGarbage = false;
				return;
			}

			float lSecondsElapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
			IInputService lInputService = GetService<IInputService>();
			ILoggerService lLoggerService = GetService<ILoggerService>();
			
			// Assess the accel/brake control state
			Vector3 lControlState = new Vector3();
			if (lInputService.IsButtonDown(KEY_ACCELERATE))
				lControlState.Z = 1.0f;
			if (lInputService.IsButtonDown(KEY_BRAKE))
				lControlState.Z = -1.0f;

			// Assess the steering control state
			if (lInputService.IsButtonDown(KEY_STEER_LEFT))
				lControlState.X = -1.0f;
			if (lInputService.IsButtonDown(KEY_STEER_RIGHT))
				lControlState.X = 1.0f;

			_controlState += (lControlState - _controlState) * Math.Min(lSecondsElapsed * 4.0f, 1.0f);
			_model.WorldMatrix = _worldMatrix;

			// Map the control states
			for (int i = 0; i < _wheels.Length; i++)
			{
				_wheels[i].Force = new Vector3();
				_wheels[i].Force.Z = Math.Max(_controlState.Z, 0.0f) * TORQUE / 4.0f;
				if (_wheels[i].IsSteerable)
					_wheels[i].Orientation.Y = (float)Math.Asin(_controlState.X) * 0.4f;
			}

			// Deteriorate momentum
			_rotation *= (float)Math.Pow(AERO, lSecondsElapsed);
			_velocity *= (float)Math.Pow(AERO, lSecondsElapsed);

			// Evaluate sideslip
			for (int i = 0; i < _wheels.Length; i++)
			{
				// Car slip velocity
				float lWheelOrientation = _orientation.Y + _wheels[i].Orientation.Y;
				float lWheelSideSlip = _velocity.Z * (float)Math.Sin(lWheelOrientation)
														 + _velocity.X * (float)Math.Cos(lWheelOrientation);

				// Brake slip velocity
				float lWheelForwardSlip = 0.0f;
				if (_controlState.Z < 0.0f)
				{
					lWheelForwardSlip = Math.Min(_controlState.Z, 1.0f) * (
														-_velocity.Z * (float)Math.Cos(lWheelOrientation) +
														_velocity.X * (float)Math.Sin(lWheelOrientation));
				}

				// Rotation slip velocity
				lWheelSideSlip += (float)Math.Cos(_wheels[i].Orientation.Y) * (float)Math.Tan(_rotation.Y) * (WHEELBASE / 2) * (i > 1 ? 1 : -1);

				_wheels[i].Force.X -= MASS * lWheelSideSlip;
				_wheels[i].Force.Z -= MASS * lWheelForwardSlip;
			}

			// Evaluate grip limits
			for (int i = 0; i < 4; i++)
				if (_wheels[i].Force.Length() > GRIP * MASS / 4)
					_wheels[i].Force *= GRIP * MASS / 4 / _wheels[i].Force.Length();

			float[] lGroundHeights; Vector3[] lNormals;
			GetTerrainHeightsAndNormals(out lGroundHeights, out lNormals);
			float lGroundHeight = lGroundHeights[0];
			
			// Evaluate suspension
			for (int i = 0; i < 4; i++)
			{
				//if (lGroundHeight + (SPRING_STROKE / 2) > _position.Y)
				{
					//if (_orientation.X < (float)Math.PI / 2 || _orientation.X > 3 * (float)Math.PI / 2)
					{
						// Spring force
						float lVehiclePosition = _position.Y + ((float)Math.Sin(_orientation.X) * _wheels[i].Position.Z);
						float lDifference = lVehiclePosition - _wheels[i].TerrainHeight;

						if (lDifference < -SPRING_STROKE)
							lDifference = -SPRING_STROKE;

						if (lDifference < SPRING_STROKE)
						{
							_wheels[i].Force.Y -= SPRING_RATE * MASS * lDifference * lDifference * lDifference;

							// Damper force
							float lSpeed = _velocity.Y + ((float)Math.Sin(_rotation.X) * _wheels[i].Position.Z);
							_wheels[i].Force.Y -= (1.0f - DAMPING) * MASS * lSpeed;
						}
						else
						{
							_rotation.X = 0.0f;
						}
					}
				}
			}

			// Evaluate the total acceleration
			Vector3 lAcceleration = new Vector3();
			for (int i = 0; i < _wheels.Length; i++)
				lAcceleration += _wheels[i].TransformedForce / MASS;
			
			// Evaluate the rotational acceleration
			for (int i = 0; i < _wheels.Length; i++)
			{
				_rotation.Y += lSecondsElapsed * _wheels[i].TransformedForce.X / 10.0f * (i > 1 ? 1 : -1);
				_rotation.X -= lSecondsElapsed * _wheels[i].TransformedForce.Y / 2.0f * (i > 1 ? 1 : -1);
				_rotation.X += lSecondsElapsed * _wheels[i].TransformedForce.Z / 50.0f;
			}

			// Evaluate the total velocity
			_velocity.Z += lSecondsElapsed * (float)
				(lAcceleration.Z * Math.Cos(_orientation.Y) + lAcceleration.X * Math.Sin(_orientation.Y));
			_velocity.X += lSecondsElapsed * (float)
				(lAcceleration.X * Math.Cos(_orientation.Y) - lAcceleration.Z * Math.Sin(_orientation.Y));
			_velocity.Y += lSecondsElapsed * (lAcceleration.Y - 9.8f);
			
			// Evaluate the orientation and position
			_orientation += _rotation * lSecondsElapsed;
			_position += _velocity * lSecondsElapsed;

			GetTerrainHeightsAndNormals(out lGroundHeights, out lNormals);
			lGroundHeight = lGroundHeights[0] - (SPRING_STROKE / 2);
			Vector3 lNormal = lNormals[0];

			// Check we haven't fallen through
			if (lGroundHeight > _position.Y)
			{
				// Work out the change the normal force would have produced
				float lSpeed = _velocity.Length();
				Vector3 lPositionChange = lNormal * (lGroundHeight - _position.Y) / lNormal.Y;

				// Apply the change retrospectively
				_velocity += lPositionChange / lSecondsElapsed;
				_position += lPositionChange;

				// Limit the effect of this when on unrealistic inclines
				if (_velocity.Length() > lSpeed)
					_velocity *= lSpeed / _velocity.Length();

				GetTerrainHeightsAndNormals(out lGroundHeights, out lNormals);
			}

			// Locate body
			_orientation.Z = -(float) Math.Atan((_wheels[WHEEL_RL].TransformedHeight + _wheels[WHEEL_FL].TransformedHeight - (_wheels[WHEEL_RR].TransformedHeight + _wheels[WHEEL_FR].TransformedHeight)) / TRACK / 2)
				+ 0.5f * (float)Math.Asin(-lAcceleration.X / MASS / WHEELBASE);

			if (_orientation.X > Math.PI/2)
			{
				_orientation.X = (float)Math.PI/2;
				_rotation.X *= -1;
			}
			if (_orientation.X < -Math.PI / 2)
			{
				_orientation.X = -(float)Math.PI / 2;
				_rotation.X *= -1;
			}

			// Update the graphics
			_worldMatrix =
				Matrix.CreateRotationZ(-_orientation.Z) *
				Matrix.CreateRotationX(-_orientation.X) *
				Matrix.CreateRotationY(-_orientation.Y) *
				Matrix.CreateTranslation(0.0f, RIDE_HEIGHT, 0.0f) *
				Matrix.CreateTranslation(_position);

			for (int i = 0; i < _wheels.Length; i++)
				_wheels[i].Update();

			_engineSound.SetVariable("CarRPM", Math.Min(new Vector2(_velocity.X, _velocity.Z).Length() * 100, 7000));
			if (!_engineSound.IsPlaying)
				_engineSound.Play();
		}

		public void GetTerrainHeightsAndNormals(out float[] heights, out Vector3[] normals)
		{
			Vector2[] positions = new Vector2[]
			{
				new Vector2(_position.X, _position.Z),
				_wheels[WHEEL_FL].TransformedPosition,
				_wheels[WHEEL_FR].TransformedPosition,
				_wheels[WHEEL_RL].TransformedPosition,
				_wheels[WHEEL_RR].TransformedPosition
			};

			ITerrainService terrain = GetService<ITerrainService>();
			terrain.GetHeightAndNormalAtPoints(positions, out heights, out normals);

			for (int i = 0; i < _wheels.Length; i++)
				_wheels[i].TerrainHeight = heights[i + 1];
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			DrawMesh(GraphicsDevice, _effect, _model);

			for (int i = 0; i < _wheels.Length; i++)
				_wheels[i].Draw(shadowPass);
		}

		public static void DrawMesh(GraphicsDevice device, ExtendedEffect effect, ExtendedModel model)
		{
			foreach (ModelMesh lMesh in model.InnerModel.Meshes)
			{
				device.Indices = lMesh.IndexBuffer;

				foreach (ModelMeshPart lMeshPart in lMesh.MeshParts)
				{
					device.VertexDeclaration = lMeshPart.VertexDeclaration;
					device.Vertices[0].SetSource(lMesh.VertexBuffer, lMeshPart.StreamOffset, lMeshPart.VertexStride);

					Vector3 lColour = ((BasicEffect)lMeshPart.Effect).DiffuseColor;
					effect.SetValue("Diffuse", new Color(lColour).ToVector4());
					effect.Begin();

					foreach (EffectPass lPass in effect.CurrentTechnique.Passes)
					{
						lPass.Begin();

						device.DrawIndexedPrimitives(
							PrimitiveType.TriangleList,
							lMeshPart.BaseVertex,
							0,
							lMeshPart.NumVertices,
							lMeshPart.StartIndex,
							lMeshPart.PrimitiveCount);

						lPass.End();
					}

					effect.End();
				}
			}
		}
	}
}
