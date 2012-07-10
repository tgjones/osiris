using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Osiris.Diagnostics;

namespace ShapeVisualizerDemo
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class ShapeVisualizerGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public ShapeVisualizerGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			ShapeVisualizer.GraphicsDevice = GraphicsDevice;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			Vector3 cameraPosition = Vector3.Backward * 2;
			Matrix cameraView = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
			Matrix cameraProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(55),
				GraphicsDevice.Viewport.AspectRatio, 1, 100);
			float rotationAngle = (float)gameTime.TotalGameTime.TotalSeconds / 3.0f;
			Quaternion rotation = Quaternion.CreateFromYawPitchRoll(rotationAngle, 0, 0);

			ShapeVisualizer.DrawWireframeBox(cameraPosition, cameraView, cameraProjection,
				Vector3.Zero, Vector3.One, rotation, Color.Red);

			base.Draw(gameTime);
		}
	}
}
