using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Osiris.Input;
using Osiris.Sky;

namespace Osiris.Graphics.Cameras
{
	/// <summary>
	/// Implements a first-person shooter style camera using the mouse for looking,
	/// and the keyboard (WASD) for translation
	/// </summary>
	public class GodCamera : Camera
	{
		#region Fields

		private const string KEY_MOVE_FORWARD = "Move Forward";
		private const string KEY_MOVE_BACKWARD = "Move Backward";
		private const string KEY_MOVE_LEFT = "Move Left";
		private const string KEY_MOVE_RIGHT = "Move Right";
		private const string KEY_MOVE_UP = "Move Up";
		private const string KEY_MOVE_DOWN = "Move Down";

		private const float ROTATION_SPEED = 0.0001f;
		private const float GAMEPAD_ROTATION_SPEED = ROTATION_SPEED * 10.0f;
		private const float TRANSLATION_SPEED = 0.25f;
		private const float GAMEPAD_TRANSLATION_SPEED = TRANSLATION_SPEED * 10.0f;
		private const float PITCH_CLAMP_ANGLE = MathHelper.PiOver2 - 0.1f;

		private readonly Vector3 m_tCameraReference;

		private Vector3 m_tPosition;
		private Vector3 m_tLookAt;
		private readonly Vector3 m_tUp;

		private float m_fPitch;
		private float m_fYaw;

		#endregion

		#region Properties

		public override Vector3 Position
		{
			get { return m_tPosition; }
		}

		public override Vector3 Direction
		{
			get { return Vector3.Normalize(m_tLookAt - m_tPosition); }
		}

		#endregion

		#region Constructor

		public GodCamera(Game pGame, Vector3 initialPosition)
			: base(pGame)
		{
			m_tCameraReference = Vector3.Forward;

			m_fPitch = 0;
			m_fYaw = 0;

			m_tPosition = initialPosition;
			m_tLookAt = new Vector3(0, 0, 0);
			m_fYaw = MathHelper.Pi;
			m_tUp = Vector3.Up;

			IInputService input = (IInputService) Game.Services.GetService(typeof(IInputService));
			input.RegisterMouseListener("GodCamera", MouseStyle.FirstPersonShooter);

			UpdateOrder = 10;
		}

		#endregion

		#region Methods

		public override void Initialize()
		{
			base.Initialize();

			// register keys
			IInputService input = (IInputService) Game.Services.GetService(typeof(IInputService));

			input.RegisterMapping(KEY_MOVE_FORWARD, Keys.W);
			input.RegisterMapping(KEY_MOVE_BACKWARD, Keys.S);
			input.RegisterMapping(KEY_MOVE_LEFT, Keys.A);
			input.RegisterMapping(KEY_MOVE_RIGHT, Keys.D);
			input.RegisterMapping(KEY_MOVE_UP, Keys.T);
			input.RegisterMapping(KEY_MOVE_DOWN, Keys.G);

			input.RegisterMapping(KEY_MOVE_FORWARD, Buttons.DPadUp);
			input.RegisterMapping(KEY_MOVE_BACKWARD, Buttons.DPadDown);
			input.RegisterMapping(KEY_MOVE_LEFT, Buttons.DPadLeft);
			input.RegisterMapping(KEY_MOVE_RIGHT, Buttons.DPadRight);
			input.RegisterMapping(KEY_MOVE_UP, Buttons.DPadUp, Buttons.A);
			input.RegisterMapping(KEY_MOVE_DOWN, Buttons.DPadDown, Buttons.A);
		}

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
		public override void Update(GameTime gameTime)
		{
			float fDeltaTime = (float) gameTime.ElapsedGameTime.TotalMilliseconds;

			GamePadState pGamePadState = GamePad.GetState(PlayerIndex.One);

			IInputService input = (IInputService) Game.Services.GetService(typeof(IInputService));

#if TARGET_WINDOWS
			MouseState mouseState = input.GetMouseState();

			if (mouseState.LeftButton == ButtonState.Released)
			{
				// look left and right
				m_fYaw -= mouseState.X * ROTATION_SPEED * fDeltaTime;

				// look up and down
				m_fPitch -= mouseState.Y * ROTATION_SPEED * fDeltaTime;
			}
#endif

			if (pGamePadState.IsConnected && pGamePadState.Buttons.LeftShoulder == ButtonState.Released)
			{
				// look left and right
				m_fYaw -= pGamePadState.ThumbSticks.Right.X * GAMEPAD_ROTATION_SPEED * fDeltaTime;

				// look up and down
				m_fPitch -= -pGamePadState.ThumbSticks.Right.Y * GAMEPAD_ROTATION_SPEED * fDeltaTime;
			}

			// clamp pitch to between vertically up and down
			m_fPitch = MathHelper.Clamp(m_fPitch, -PITCH_CLAMP_ANGLE, PITCH_CLAMP_ANGLE);

			// construct rotation matrix
			Matrix tYawMatrix = Matrix.CreateRotationY(m_fYaw);
			Matrix tPitchMatrix = Matrix.CreateRotationX(m_fPitch);
			Matrix tRotationMatrix = tPitchMatrix * tYawMatrix;

			// move forward and backward
			if (input.IsButtonDown(KEY_MOVE_FORWARD))
			{
				Vector3 tMovement = Vector3.Forward * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition += tMovement * fDeltaTime;
			}
			else if (input.IsButtonDown(KEY_MOVE_BACKWARD))
			{
				Vector3 tMovement = Vector3.Backward * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition += tMovement * fDeltaTime;
			}

#if TARGET_WINDOWS
			// use left button and mouse move to "zoom"
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				Vector3 tMovement = Vector3.Forward * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition -= tMovement * mouseState.Y * fDeltaTime;
			}
#endif

			// strafe left and right
			if (input.IsButtonDown(KEY_MOVE_LEFT))
			{
				Vector3 tMovement = Vector3.Left * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition += tMovement * fDeltaTime;
			}
			else if (input.IsButtonDown(KEY_MOVE_RIGHT))
			{
				Vector3 tMovement = Vector3.Right * TRANSLATION_SPEED;
				tMovement = Vector3.Transform(tMovement, tRotationMatrix);
				m_tPosition += tMovement * fDeltaTime;
			}

			// move up and down
			if (input.IsButtonDown(KEY_MOVE_UP))
			{
				m_tPosition.Y += TRANSLATION_SPEED * fDeltaTime;
			}
			else if (input.IsButtonDown(KEY_MOVE_DOWN))
			{
				m_tPosition.Y -= TRANSLATION_SPEED * fDeltaTime;
			}

			if (pGamePadState.IsConnected)
			{
				if (pGamePadState.Buttons.LeftShoulder == ButtonState.Released)
				{
					// strafe left and right
					Vector3 tMovement = new Vector3(GAMEPAD_TRANSLATION_SPEED, 0, 0);
					tMovement = Vector3.Transform(tMovement, tRotationMatrix);
					m_tPosition -= -tMovement * pGamePadState.ThumbSticks.Left.X * fDeltaTime;

					// move forward and backward
					tMovement = new Vector3(0, GAMEPAD_TRANSLATION_SPEED, 0);
					tMovement = Vector3.Transform(tMovement, tRotationMatrix);
					m_tPosition += tMovement * pGamePadState.ThumbSticks.Left.Y * fDeltaTime;
				}

				// use left shoulder button and right stick to "zoom"
				if (pGamePadState.Buttons.LeftShoulder == ButtonState.Pressed)
				{
					Vector3 tMovement = new Vector3(0, GAMEPAD_TRANSLATION_SPEED * 10.0f, 0);
					tMovement = Vector3.Transform(tMovement, tRotationMatrix);
					m_tPosition += tMovement * pGamePadState.ThumbSticks.Right.Y * fDeltaTime;
				}
			}

			// calculate the direction vector for the camera
			Vector3 tTransformedReference = Vector3.Transform(m_tCameraReference, tRotationMatrix);
			m_tLookAt = m_tPosition + tTransformedReference;

			_view = Matrix.CreateLookAt(m_tPosition, m_tLookAt, m_tUp);
			_boundingFrustum = null;
		}

		#endregion
	}
}
