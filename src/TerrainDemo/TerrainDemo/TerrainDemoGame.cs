#region File Description
//-----------------------------------------------------------------------------
// GeneratedGeometry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Osiris.Graphics;
using System;
using Osiris.Terrain.Graphics;

#endregion

namespace TerrainDemo
{
	/// <summary>
	/// Sample showing how to use geometry that is programatically
	/// generated as part of the content pipeline build process.
	/// </summary>
	public class TerrainDemoGame : Microsoft.Xna.Framework.Game
	{
		#region Fields

		GraphicsDeviceManager graphics;
		TerrainComponent terrain;

		SpriteBatch spriteBatch;
		SpriteFont spriteFont;

		KeyboardState lastKeyboardState = new KeyboardState();
		GamePadState lastGamePadState = new GamePadState();
		MouseState lastMousState = new MouseState();
		KeyboardState currentKeyboardState = new KeyboardState();
		GamePadState currentGamePadState = new GamePadState();
		MouseState currentMouseState = new MouseState();

		Ship ship;
		ChaseCamera camera;

		Model shipModel;

		bool cameraSpringEnabled = true;
		bool wireframeEnabled = false;

		#endregion

		#region Initialization

		public TerrainDemoGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			IsMouseVisible = true;

#if WINDOWS_PHONE
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;
#else
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
#endif

			// Create the chase camera
			camera = new ChaseCamera();
			Services.AddService(typeof (ICameraService), camera);

			// Set the camera offsets
			//camera.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
			//camera.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);
			camera.DesiredPositionOffset = new Vector3(0.0f, 20.0f, 35.0f);
			camera.LookAtOffset = new Vector3(0.0f, 1.5f, 0.0f);

			// Set camera perspective
			camera.NearPlaneDistance = 1.0f;
			camera.FarPlaneDistance = 10000.0f;

			//TODO: Set any other camera invariants here such as field of view

			terrain = new TerrainComponent(this, @"Terrain\HeightMap");
		}

		#endregion

		protected override void Initialize()
		{
			ship = new Ship(GraphicsDevice, terrain);

			// Set the camera aspect ratio
			// This must be done after the class to base.Initalize() which will
			// initialize the graphics device.
			camera.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
				graphics.GraphicsDevice.Viewport.Height;


			// Perform an inital reset on the camera so that it starts at the resting
			// position. If we don't do this, the camera will start at the origin and
			// race across the world to get behind the chased object.
			// This is performed here because the aspect ratio is needed by Reset.
			UpdateCameraChaseTarget();
			camera.Reset();

			terrain.Initialize();

			ship.Initialize();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("gameFont");

			shipModel = Content.Load<Model>("Ship");

			base.LoadContent();
		}

		#region Update and Draw

		/// <summary>
		/// Allows the game to run logic.
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			lastKeyboardState = currentKeyboardState;
			lastGamePadState = currentGamePadState;
			lastMousState = currentMouseState;

#if WINDOWS_PHONE
            currentKeyboardState = new KeyboardState();
#else
			currentKeyboardState = Keyboard.GetState();
#endif
			currentGamePadState = GamePad.GetState(PlayerIndex.One);
			currentMouseState = Mouse.GetState();


			// Exit when the Escape key or Back button is pressed
			if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
				currentGamePadState.Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}

			bool touchTopLeft = currentMouseState.LeftButton == ButtonState.Pressed &&
					lastMousState.LeftButton != ButtonState.Pressed &&
					currentMouseState.X < GraphicsDevice.Viewport.Width / 10 &&
					currentMouseState.Y < GraphicsDevice.Viewport.Height / 10;


			// Pressing the A button or key toggles the spring behavior on and off
			if (lastKeyboardState.IsKeyUp(Keys.A) &&
				(currentKeyboardState.IsKeyDown(Keys.A)) ||
				(lastGamePadState.Buttons.A == ButtonState.Released &&
				currentGamePadState.Buttons.A == ButtonState.Pressed) ||
				touchTopLeft)
			{
				cameraSpringEnabled = !cameraSpringEnabled;
			}

			// Pressing the B button or key toggles terrain wireframe on and off
			if (lastKeyboardState.IsKeyUp(Keys.B) &&
				(currentKeyboardState.IsKeyDown(Keys.B)) ||
				(lastGamePadState.Buttons.B == ButtonState.Released &&
				currentGamePadState.Buttons.B == ButtonState.Pressed))
			{
				wireframeEnabled = !wireframeEnabled;
			}

			// Reset the ship on R key or right thumb stick clicked
			if (currentKeyboardState.IsKeyDown(Keys.R) ||
				currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
			{
				ship.Reset();
				camera.Reset();
			}

			// Update the ship
			ship.Update(gameTime);

			// Update the camera to chase the new target
			UpdateCameraChaseTarget();

			// The chase camera's update behavior is the springs, but we can
			// use the Reset method to have a locked, spring-less camera
			if (cameraSpringEnabled)
				camera.Update(gameTime);
			else
				camera.Reset();

			terrain.Update(gameTime);

			base.Update(gameTime);
		}

		/// <summary>
		/// Update the values to be chased by the camera
		/// </summary>
		private void UpdateCameraChaseTarget()
		{
			camera.ChasePosition = ship.Position;
			camera.ChaseDirection = ship.Direction;
			camera.Up = ship.Up;
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

			terrain.Draw(gameTime);

			if (wireframeEnabled)
				GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

			//GraphicsDevice.BlendState = BlendState.AlphaBlend;

			DrawModel(shipModel, ship.World);

			DrawOverlayText();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Simple model drawing method. The interesting part here is that
		/// the view and projection matrices are taken from the camera object.
		/// </summary>        
		private void DrawModel(Model model, Matrix world)
		{
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (Effect effect in mesh.Effects)
				{
					if (effect is BasicEffect)
					{
						((BasicEffect) effect).EnableDefaultLighting();
					}
					if (effect is IEffectMatrices)
					{
						IEffectMatrices effectMatrices = (IEffectMatrices)effect;
						effectMatrices.World = transforms[mesh.ParentBone.Index] * world;
						effectMatrices.View = camera.View;
						effectMatrices.Projection = camera.Projection;
					}
				}
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

			string text = "-Touch, Right Trigger, or Spacebar = thrust\n" +
						  "-Screen edges, Left Thumb Stick,\n  or Arrow keys = steer\n" +
						  "-Press A or touch the top left corner\n  to toggle camera spring (" + (cameraSpringEnabled ?
							  "on" : "off") + ")";

			// Draw the string twice to create a drop shadow, first colored black
			// and offset one pixel to the bottom right, then again in white at the
			// intended position. This makes text easier to read over the background.
			spriteBatch.DrawString(spriteFont, text, new Vector2(65, 65), Color.Black);
			spriteBatch.DrawString(spriteFont, text, new Vector2(64, 64), Color.White);

			spriteBatch.End();
		}

		#endregion
	}


	#region Entry Point

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	static class Program
	{
		static void Main()
		{
			using (TerrainDemoGame game = new TerrainDemoGame())
			{
				game.Run();
			}
		}
	}

	#endregion
}