using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Osiris.Diagnostics;

namespace ShapeVisualizerDemo
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class ShapeVisualizerGame : Game
	{
		private SpriteBatch _spriteBatch;
		private SpriteFont _spriteFont;
		private int _activeShape;

		public ShapeVisualizerGame()
		{
			new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_spriteFont = Content.Load<SpriteFont>("gameFont");

			ShapeVisualizer.GraphicsDevice = GraphicsDevice;
		}

		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				Exit();

			var currentKeyboardState = Keyboard.GetState();

			// Check for model change.
			if (currentKeyboardState.IsKeyDown(Keys.D1))
				_activeShape = 0;
			else if (currentKeyboardState.IsKeyDown(Keys.D2))
				_activeShape = 1;
			else if (currentKeyboardState.IsKeyDown(Keys.D3))
				_activeShape = 2;
			else if (currentKeyboardState.IsKeyDown(Keys.D4))
				_activeShape = 3;
			else if (currentKeyboardState.IsKeyDown(Keys.D5))
				_activeShape = 4;
			else if (currentKeyboardState.IsKeyDown(Keys.D6))
				_activeShape = 5;
			else if (currentKeyboardState.IsKeyDown(Keys.D7))
				_activeShape = 6;
			else if (currentKeyboardState.IsKeyDown(Keys.D8))
				_activeShape = 7;
			else if (currentKeyboardState.IsKeyDown(Keys.D9))
				_activeShape = 8;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			Vector3 cameraPosition = Vector3.Backward * 2;
			Matrix cameraView = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
			Matrix cameraProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(55),
				GraphicsDevice.Viewport.AspectRatio, 1, 100);
			float rotationAngle = (float)gameTime.TotalGameTime.TotalSeconds / 3.0f;
			Quaternion rotation = Quaternion.CreateFromYawPitchRoll(rotationAngle, 0, 0);

			switch (_activeShape)
			{
				case 0:
					ShapeVisualizer.DrawWireframeBox(cameraPosition, cameraView, cameraProjection,
						Vector3.Zero, Vector3.One, rotation, Color.Red);
					break;
				case 1:
				{
					var matrix = Matrix.CreateLookAt(new Vector3(-5, 1, -5), new Vector3(0, 0, -10), Vector3.Up) *
						Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 10);
					var frustum = new BoundingFrustum(matrix);
					
					ShapeVisualizer.DrawWireframeFrustum(cameraView, cameraProjection, frustum, Color.Gray);

					break;
				}
				case 2 :
				{
					var corners = new[]
					{
						new Vector3(-1, -1, 0),
						new Vector3(1, -1, -1),
						new Vector3(-1, 1, 0),
						new Vector3(1, 1, -3)
					};
					ShapeVisualizer.DrawSolidRectangle(cameraView, cameraProjection, corners, Color.Blue);
					break;
				}
				case 3:
				{
					for (int i = 0; i < 360; i += 30)
						ShapeVisualizer.DrawLine(cameraView, cameraProjection,
							Vector3.Zero,
							Vector3.TransformNormal(Vector3.Up, Matrix.CreateRotationZ(MathHelper.ToRadians(i))),
							Color.Yellow);
					break;
				}
				case 4 :
				{
					ShapeVisualizer.DrawWireframeDisc(cameraPosition, cameraView, cameraProjection,
						Vector3.Zero, Vector3.Forward, 0.8f, Color.White, false);
					break;
				}
				case 5 :
				{
					ShapeVisualizer.DrawWireframeSphere(cameraPosition, cameraView, cameraProjection,
						Vector3.Zero, Vector3.Forward, 0.8f, rotation * Quaternion.CreateFromYawPitchRoll(0.2f, 0.4f, 0.1f), Color.White);
					break;
				}
			}
			
			DrawOverlayText();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Displays an overlay showing what the controls are,
		/// and which settings are currently selected.
		/// </summary>
		private void DrawOverlayText()
		{
			_spriteBatch.Begin();

			const string text = "Press the number keys (1-9) to switch between shapes..";

			// Draw the string twice to create a drop shadow, first colored black
			// and offset one pixel to the bottom right, then again in white at the
			// intended position. This makes text easier to read over the background.
			_spriteBatch.DrawString(_spriteFont, text, new Vector2(10, 10), Color.Black);
			_spriteBatch.DrawString(_spriteFont, text, new Vector2(9, 9), Color.White);

			_spriteBatch.End();
		}
	}
}
