using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Osiris.Graphics.Rendering;
using Osiris.Graphics.SceneGraph.Nodes;
using Osiris.Graphics.SceneGraph.Culling;
using Osiris.Graphics.Rendering.Cameras;
using Osiris.Graphics.Rendering.GlobalStates;
using Osiris.Graphics.Shaders;

namespace Test
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		private Renderer _renderer;
		private Node _sceneRoot;
		private Culler _culler;
		private Camera _camera;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			Services.AddService(typeof(GraphicsDeviceManager), graphics);
			Services.AddService(typeof(ContentManager), Content);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
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

			ShaderCatalog shaderCatalog = Content.Load<ShaderCatalog>("ShaderCombinations");
			Services.AddService(typeof(IShaderCatalogService), shaderCatalog);

			_camera = new GodCamera(GraphicsDevice);

			_renderer = new Renderer(GraphicsDevice);
			_renderer.Camera = _camera;

			_camera.Position = new Vector3(-8, 120, 1000);

			_sceneRoot = new ModelNode(Content.ServiceProvider, Content.Load<Model>(@"Models\p1_wedge"));
			//_sceneRoot.GlobalStates.Attach(new WireframeState { Enabled = true });

			// Build shaders.
			_sceneRoot.BuildShader(new Stack<Osiris.Graphics.Effects.ShaderEffect>());

			// Initial update of objects.
			_sceneRoot.UpdateGeometricState();
			_sceneRoot.UpdateRenderState();

			// Initial culling of scene.
			_culler = new Culler();
			_culler.Camera = _camera;
			_culler.ComputeVisibleSet(_sceneRoot);
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

			GodCameraInputState godCameraInputState = new GodCameraInputState();
			godCameraInputState.MoveBackward = true;
			((GodCamera) _camera).Update(gameTime, godCameraInputState);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			_renderer.DrawScene(_culler.VisibleSet);

			base.Draw(gameTime);
		}
	}
}
