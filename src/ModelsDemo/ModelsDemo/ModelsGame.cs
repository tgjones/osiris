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

		GraphicsDeviceManager graphics;

		Model model;
		private BoundingSphere modelBoundingSphere;

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
			model = Content.Load<Model>("Tank");
			//model = Content.Load<Model>("Nissan");

			BoundingSphere boundingSphere = new BoundingSphere();
			foreach (ModelMesh mesh in model.Meshes)
				boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
			modelBoundingSphere = boundingSphere;
		}


		#endregion

		#region Update and Draw


		/// <summary>
		/// Allows the game to run logic.
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			KeyboardState keyboardState = Keyboard.GetState();
			GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

			// Check for exit.
			if (keyboardState.IsKeyDown(Keys.Escape) ||
				gamePadState.Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice device = graphics.GraphicsDevice;

			device.Clear(Color.CornflowerBlue);

			// Animated the model rotating
			float modelRotation = (float)gameTime.TotalGameTime.TotalSeconds / 5.0f;

			// Set the positions of the camera in world space, for our view matrix.
			Vector3 cameraPosition = new Vector3(0.0f, modelBoundingSphere.Radius * 1.5f, modelBoundingSphere.Radius * 2.0f);
			Vector3 lookAt = new Vector3(0.0f, 35.0f, 0.0f);

			// Copy any parent transforms.
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			// Draw the model. A model can have multiple meshes, so loop.
			foreach (ModelMesh mesh in model.Meshes)
			{
				// This is where the mesh orientation is set,
				// as well as our camera and projection.
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.World = transforms[mesh.ParentBone.Index] *
						Matrix.CreateRotationY(modelRotation);
					effect.View = Matrix.CreateLookAt(cameraPosition, lookAt,
						Vector3.Up);
					effect.Projection = Matrix.CreatePerspectiveFieldOfView(
						MathHelper.ToRadians(45.0f), device.Viewport.AspectRatio, 1.0f, 10000.0f);
				}
				// Draw the mesh, using the effects set above.
				mesh.Draw();
			}


			base.Draw(gameTime);
		}


		#endregion
	}
}
