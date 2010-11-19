using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ModelsDemo
{
	/// <summary>
	/// This class displays the imported model and animates it rotating. 
	/// It is useful for testing importer.
	/// </summary>
	public class ModelsGame : Microsoft.Xna.Framework.Game
	{
		#region Fields

		private GraphicsDeviceManager graphics;

		private Model[] _models;
		private BoundingSphere[] _modelBoundingSpheres;
		private int _activeModel = 0;

		private SpriteBatch spriteBatch;
		private SpriteFont spriteFont;

		KeyboardState lastKeyboardState = new KeyboardState();
		KeyboardState currentKeyboardState = new KeyboardState();

		private bool wireframeEnabled;

		#endregion

		#region Initialization


		public ModelsGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}


		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("gameFont");

			string[] modelNames = new[]
			{
				@"Tank",
				@"75Cathedral-model",
				@"Primitives\Cube",
				@"Primitives\Cylinder",
				@"Primitives\Plane",
				@"Primitives\Sphere",
				@"Primitives\Teapot",
				@"Primitives\Torus",
				@"Primitives\Combined",
			};
			_models = new Model[modelNames.Length];
			_modelBoundingSpheres = new BoundingSphere[modelNames.Length];
			for (int i = 0; i < modelNames.Length; ++i)
			{
				_models[i] = Content.Load<Model>(modelNames[i]);

				BoundingSphere boundingSphere = new BoundingSphere();
				foreach (ModelMesh mesh in _models[i].Meshes)
					boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
				_modelBoundingSpheres[i] = boundingSphere;
			}
		}

		#endregion

		#region Update and Draw

		/// <summary>
		/// Allows the game to run logic.
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			lastKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();

			GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

			// Check for exit.
			if (currentKeyboardState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed)
				Exit();

			// Check for model change.
			if (currentKeyboardState.IsKeyDown(Keys.D1))
				_activeModel = 0;
			else if (currentKeyboardState.IsKeyDown(Keys.D2))
				_activeModel = 1;
			else if (currentKeyboardState.IsKeyDown(Keys.D3))
				_activeModel = 2;
			else if (currentKeyboardState.IsKeyDown(Keys.D4))
				_activeModel = 3;
			else if (currentKeyboardState.IsKeyDown(Keys.D5))
				_activeModel = 4;
			else if (currentKeyboardState.IsKeyDown(Keys.D6))
				_activeModel = 5;
			else if (currentKeyboardState.IsKeyDown(Keys.D7))
				_activeModel = 6;
			else if (currentKeyboardState.IsKeyDown(Keys.D8))
				_activeModel = 7;
			else if (currentKeyboardState.IsKeyDown(Keys.D9))
				_activeModel = 8;

			// Pressing the B button or key toggles terrain wireframe on and off
			if (lastKeyboardState.IsKeyUp(Keys.W) && currentKeyboardState.IsKeyDown(Keys.W))
				wireframeEnabled = !wireframeEnabled;
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice device = graphics.GraphicsDevice;

			device.Clear(Color.CornflowerBlue);

			if (wireframeEnabled)
				GraphicsDevice.RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };

			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			DrawModel(gameTime, true);

			GraphicsDevice.BlendState = BlendState.AlphaBlend;
			DrawModel(gameTime, false);

			if (wireframeEnabled)
				GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

			DrawOverlayText();

			base.Draw(gameTime);
		}

		private void DrawModel(GameTime gameTime, bool drawOpaqueOnly)
		{
			// Animated the model rotating
			float modelRotation = (float)gameTime.TotalGameTime.TotalSeconds / 5.0f;

			// Set the positions of the camera in world space, for our view matrix.
			Vector3 cameraPosition = new Vector3(0.0f, _modelBoundingSpheres[_activeModel].Radius * 2, _modelBoundingSpheres[_activeModel].Radius * 2.0f);
			Vector3 lookAt = new Vector3(0.0f, 0, 0.0f);

			// Copy any parent transforms.
			Matrix[] transforms = new Matrix[_models[_activeModel].Bones.Count];
			_models[_activeModel].CopyAbsoluteBoneTransformsTo(transforms);
			// Draw the model. A model can have multiple meshes, so loop.
			foreach (ModelMesh mesh in _models[_activeModel].Meshes)
			{
				bool skipDrawing = false;
				// This is where the mesh orientation is set,
				// as well as our camera and projection.
				foreach (BasicEffect effect in mesh.Effects)
				{
					if (effect.Alpha < 1.0f && drawOpaqueOnly)
					{
						skipDrawing = true;
						break;
					}

					if (effect.Alpha == 1.0f && !drawOpaqueOnly)
					{
						skipDrawing = true;
						break;
					}

					effect.EnableDefaultLighting();
					effect.World = transforms[mesh.ParentBone.Index] *
						Matrix.CreateRotationY(modelRotation);
					effect.View = Matrix.CreateLookAt(cameraPosition, lookAt,
						Vector3.Up);
					effect.Projection = Matrix.CreatePerspectiveFieldOfView(
						MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000.0f);
				}

				if (skipDrawing)
					continue;

				// Draw the mesh, using the effects set above.
				mesh.Draw();
			}
		}

		/// <summary>
		/// Displays an overlay showing what the controls are,
		/// and which settings are currently selected.
		/// </summary>
		private void DrawOverlayText()
		{
			spriteBatch.Begin();

			string text = "Press the number keys (1-9) to switch between models.\nPress 'w' to toggle wireframe.";

			// Draw the string twice to create a drop shadow, first colored black
			// and offset one pixel to the bottom right, then again in white at the
			// intended position. This makes text easier to read over the background.
			spriteBatch.DrawString(spriteFont, text, new Vector2(65, 65), Color.Black);
			spriteBatch.DrawString(spriteFont, text, new Vector2(64, 64), Color.White);

			spriteBatch.End();
		}

		#endregion
	}
}
