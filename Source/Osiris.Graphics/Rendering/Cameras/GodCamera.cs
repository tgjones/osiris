using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering.Cameras
{
	/// <summary>
	/// Implements a first-person shooter style camera using the mouse for looking,
	/// and the keyboard (WASD) for translation
	/// </summary>
	public class GodCamera : Camera
	{
		#region Fields

		private const float ROTATION_SPEED = 1f;
		private const float GAMEPAD_ROTATION_SPEED = ROTATION_SPEED * 10.0f;
		private const float TRANSLATION_SPEED = 0.025f;
		private const float GAMEPAD_TRANSLATION_SPEED = TRANSLATION_SPEED * 10.0f;
		private const float PITCH_CLAMP_ANGLE = MathHelper.PiOver2 - 0.1f;

		private readonly Vector3 m_tCameraReference;

		private Vector3 m_tLookAt;
		private readonly Vector3 m_tUp;

		private float m_fPitch;
		private float m_fYaw;

		#endregion

		#region Properties

		public Vector3 Direction
		{
			get { return Vector3.Normalize(m_tLookAt - Position); }
		}

		#endregion

		#region Constructor

		public GodCamera(GraphicsDevice graphicsDevice)
		{
			float aspectRatio = (float) graphicsDevice.Viewport.Width / (float) graphicsDevice.Viewport.Height;
			float projectionLeft = -0.5f * aspectRatio;
			float projectionRight = 0.5f * aspectRatio;
			float projectionBottom = -0.5f;
			float projectionTop = 0.5f;
			float near = 1;
			float far = 1000;

			ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(
				projectionLeft,
				projectionRight,
				projectionBottom,
				projectionTop,
				near,
				far);

			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
							aspectRatio, 1.0f, 10000.0f);

			m_tCameraReference = Vector3.Forward;

			m_fPitch = 0;
			m_fYaw = 0;

			Position = new Vector3(-8, 0, 8);
			m_tLookAt = new Vector3(0, 0, 0);
			m_fYaw = 0;
			m_tUp = Vector3.Up;
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method implements a sort-of first-person-shooter camera,
		/// using these keys:
		/// W - Move forward (along line of sight)
		/// S - Move backward
		/// A - Strafe left
		/// D - Strafe right
		/// Q - Rotate left
		/// E - Rotate right
		/// R - Look up
		/// F - Look down
		/// T - Move up
		/// G - Move down
		/// </summary>
		/// <param name="fDeltaTime"></param>
		public void Update(GameTime gameTime, GodCameraInputState inputState)
		{
			float fDeltaTime = (float) gameTime.ElapsedGameTime.TotalMilliseconds;

			Vector3 position = Position;

#if TARGET_WINDOWS

			Vector2 panning = godCameraInput.GetDirection(GodCameraButtons.Pan);

			// look left and right
			m_fYaw -= panning.X * ROTATION_SPEED * fDeltaTime;

			// look up and down
			m_fPitch -= panning.Y * ROTATION_SPEED * fDeltaTime;
#endif

			// clamp pitch to between vertically up and down
			m_fPitch = MathHelper.Clamp(m_fPitch, -PITCH_CLAMP_ANGLE, PITCH_CLAMP_ANGLE);

			// construct rotation matrix
			Matrix tYawMatrix = Matrix.CreateRotationY(m_fYaw);
			Matrix tPitchMatrix = Matrix.CreateRotationX(m_fPitch);
			Matrix tRotationMatrix = tPitchMatrix * tYawMatrix;

			// move forward and backward
			if (inputState.MoveForward)
			{
				Vector3 tMovement = Vector3.Forward * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				position += tMovement * fDeltaTime;
			}
			else if (inputState.MoveBackward)
			{
				Vector3 tMovement = Vector3.Backward * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				position += tMovement * fDeltaTime;
			}

			// strafe left and right
			if (inputState.MoveLeft)
			{
				Vector3 tMovement = Vector3.Left * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				position += tMovement * fDeltaTime;
			}
			else if (inputState.MoveRight)
			{
				Vector3 tMovement = Vector3.Right * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				position += tMovement * fDeltaTime;
			}

			// move up and down
			if (inputState.MoveUp)
			{
				position.Y += TRANSLATION_SPEED * fDeltaTime;
			}
			else if (inputState.MoveDown)
			{
				position.Y -= TRANSLATION_SPEED * fDeltaTime;
			}

#if TARGET_WINDOWS
			/*// use left button and mouse move to "zoom"
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				Vector3 tMovement = Vector3.Forward * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				position -= tMovement * mouseState.Y * fDeltaTime;
			}*/
#endif

			// calculate the direction vector for the camera
			Vector3 tTransformedReference = Vector3.Transform(m_tCameraReference, tRotationMatrix);
			m_tLookAt = Position + tTransformedReference;

			ViewMatrix = Matrix.CreateLookAt(position, m_tLookAt, m_tUp);

			Position = position;
		}

		#endregion
	}

	public class GodCameraInputState
	{
		public bool MoveForward;
		public bool MoveBackward;
		public bool MoveLeft;
		public bool MoveRight;
		public bool MoveUp;
		public bool MoveDown;
	}
}