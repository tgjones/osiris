using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Osiris.Input;
using Osiris.Sky;

namespace Osiris.Graphics.Cameras
{
	/// <summary>
	/// Implements a tracker camera using the mouse for looking around the target
	/// </summary>
	public class TrackerCamera : Camera
	{
		#region Fields

		private const float ROTATION_SPEED = 0.0001f;
		private const float GAMEPAD_ROTATION_SPEED = ROTATION_SPEED * 10.0f;
		private const float TRANSLATION_SPEED = 0.25f;
		private const float GAMEPAD_TRANSLATION_SPEED = TRANSLATION_SPEED * 10.0f;
		private const float PITCH_CLAMP_ANGLE = MathHelper.PiOver2 - 0.1f;

		private readonly Vector3 m_tCameraReference;

		private Vector3 m_tPosition;
		private readonly Vector3 m_tUp;

		private float m_fPitch;
		private float m_fYaw;

		private IViewer m_pTarget;

		#endregion

		#region Properties

		public override Vector3 Position
		{
			get { return m_tPosition; }
		}

		public override Vector3 Direction
		{
			get { return Vector3.Normalize(m_pTarget.Position - m_tPosition); }
		}

		public IViewer Target
		{
			get { return m_pTarget; }
			set
			{
				m_pTarget = value;
				m_tPosition = value.Position;
				m_tPosition.X -= 10.0f;
			}
		}

		#endregion

		#region Constructor

		public TrackerCamera(Game pGame)
			: base(pGame)
		{
			m_tCameraReference = Vector3.Forward;

			m_fPitch = 0;
			m_fYaw = 0;

			m_tPosition = new Vector3();
			m_fYaw = MathHelper.Pi;
			m_tUp = Vector3.Up;

			IInputService input = (IInputService)Game.Services.GetService(typeof(IInputService));
			input.RegisterMouseListener("TrackerCamera", MouseStyle.FirstPersonShooter);

			UpdateOrder = 2000;
		}

		#endregion

		#region Methods

		public override void Update(GameTime gameTime)
		{
			float fDeltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

			GamePadState pGamePadState = GamePad.GetState(PlayerIndex.One);

			IInputService input = (IInputService)Game.Services.GetService(typeof(IInputService));

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

			// clamp pitch to between vertically up and down
			m_fPitch = MathHelper.Clamp(m_fPitch, -PITCH_CLAMP_ANGLE, PITCH_CLAMP_ANGLE);

			// construct rotation matrix
			/*Matrix tYawMatrix = Matrix.CreateRotationY(m_fYaw);
			Matrix tPitchMatrix = Matrix.CreateRotationX(m_fPitch);
			Matrix tRotationMatrix = tPitchMatrix * tYawMatrix;*/

			// move camera a little bit closer to position behind car
			Vector3 carToCameraTargetDirection = Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(-m_pTarget.Direction.Y)));
			Vector3 vectorToCar = (m_pTarget.Position + carToCameraTargetDirection * 6.0f) - m_tPosition;
			float distanceToCar = vectorToCar.Length();
			m_tPosition += Vector3.Normalize(vectorToCar) * distanceToCar / 4.0f;

			Vector3 tempPosition = m_tPosition;
			tempPosition.Y += 3;

			// calculate the direction vector for the camera
			//Vector3 tTransformedReference = Vector3.Transform(m_tCameraReference, tRotationMatrix);
			//m_tPosition = m_pTarget.Position - tTransformedReference * 10.0f;

			_view = Matrix.CreateLookAt(tempPosition, m_pTarget.Position + new Vector3(0, 2, 0), m_tUp);
			_boundingFrustum = null;
		}

		#endregion
	}
}
