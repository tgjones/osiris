using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Content;
using Osiris.Graphics.Cameras;
using Microsoft.Xna.Framework.Input;

namespace Osiris.Graphics.SimpleObjects
{
	public class Cube : Osiris.DrawableGameComponent, IViewer
	{
		private Vector3 _position;
		private Matrix _scaling;

		private VertexDeclaration _vertexDeclaration;
		private VertexBuffer _vertexBuffer;

		private Color _diffuse;

		public override BoundingBox BoundingBox
		{
			get
			{
				return new BoundingBox(
					_position - Vector3.Transform(Vector3.One, _scaling),
					_position + Vector3.Transform(Vector3.One, _scaling));
			}
		}

		public Vector3 Direction
		{
			get { return Vector3.Zero; }
		}

		public Color Diffuse
		{
			set { _diffuse = value; }
		}

		public Cube(Game game, Vector3 centre, Matrix scaling)
			: base(game, @"Graphics\SimpleObjects\Cube", true, true, false)
		{
			_position = centre;
			_scaling = scaling;

			_diffuse = Color.Blue;
		}

		protected override void LoadContent()
		{
			InitializeEffect();
			InitializeCube();

			base.LoadContent();
		}

		private void InitializeEffect()
		{
			_vertexDeclaration = new VertexDeclaration(
				this.GraphicsDevice, VertexPositionNormalTexture.VertexElements);
		}

		private void InitializeCube()
		{
			VertexPositionNormalTexture[] cubeVertices = new VertexPositionNormalTexture[36];

			Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, 1.0f);
			Vector3 bottomLeftFront = new Vector3(-1.0f, -1.0f, 1.0f);
			Vector3 topRightFront = new Vector3(1.0f, 1.0f, 1.0f);
			Vector3 bottomRightFront = new Vector3(1.0f, -1.0f, 1.0f);
			Vector3 topLeftBack = new Vector3(-1.0f, 1.0f, -1.0f);
			Vector3 topRightBack = new Vector3(1.0f, 1.0f, -1.0f);
			Vector3 bottomLeftBack = new Vector3(-1.0f, -1.0f, -1.0f);
			Vector3 bottomRightBack = new Vector3(1.0f, -1.0f, -1.0f);

			Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
			Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
			Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
			Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

			Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
			Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
			Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
			Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
			Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

			// Front face.
			cubeVertices[0] = new VertexPositionNormalTexture(topLeftFront, frontNormal, textureTopLeft);
			cubeVertices[1] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);
			cubeVertices[2] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
			cubeVertices[3] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
			cubeVertices[4] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);
			cubeVertices[5] = new VertexPositionNormalTexture(bottomRightFront, frontNormal, textureBottomRight);

			// Back face.
			cubeVertices[6] = new VertexPositionNormalTexture(topLeftBack, backNormal, textureTopRight);
			cubeVertices[7] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
			cubeVertices[8] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
			cubeVertices[9] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
			cubeVertices[10] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
			cubeVertices[11] = new VertexPositionNormalTexture(bottomRightBack, backNormal, textureBottomLeft);

			// Top face.
			cubeVertices[12] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
			cubeVertices[13] = new VertexPositionNormalTexture(topLeftBack, topNormal, textureTopLeft);
			cubeVertices[14] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);
			cubeVertices[15] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);
			cubeVertices[16] = new VertexPositionNormalTexture(topRightFront, topNormal, textureBottomRight);
			cubeVertices[17] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);

			// Bottom face. 
			cubeVertices[18] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
			cubeVertices[19] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
			cubeVertices[20] = new VertexPositionNormalTexture(bottomLeftBack, bottomNormal, textureBottomLeft);
			cubeVertices[21] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
			cubeVertices[22] = new VertexPositionNormalTexture(bottomRightFront, bottomNormal, textureTopRight);
			cubeVertices[23] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);

			// Left face.
			cubeVertices[24] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);
			cubeVertices[26] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
			cubeVertices[25] = new VertexPositionNormalTexture(bottomLeftFront, leftNormal, textureBottomRight);
			cubeVertices[27] = new VertexPositionNormalTexture(topLeftBack, leftNormal, textureTopLeft);
			cubeVertices[29] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
			cubeVertices[28] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);

			// Right face. 
			cubeVertices[30] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
			cubeVertices[31] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);
			cubeVertices[32] = new VertexPositionNormalTexture(bottomRightFront, rightNormal, textureBottomLeft);
			cubeVertices[33] = new VertexPositionNormalTexture(topRightBack, rightNormal, textureTopRight);
			cubeVertices[34] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);
			cubeVertices[35] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);

			_vertexBuffer = new VertexBuffer(
					this.GraphicsDevice,
					typeof(VertexPositionNormalTexture),
					cubeVertices.Length,
					BufferUsage.None);

			_vertexBuffer.SetData<VertexPositionNormalTexture>(cubeVertices);
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			float movement = (float) gameTime.ElapsedGameTime.TotalMilliseconds * 1f;
			KeyboardState keyboardState = Keyboard.GetState();
			
			if (keyboardState.IsKeyDown(Keys.NumPad8))
				_position.Z += movement;
			else if (keyboardState.IsKeyDown(Keys.NumPad5))
				_position.Z -= movement;
			
			if (keyboardState.IsKeyDown(Keys.NumPad4))
				_position.X += movement;
			else if (keyboardState.IsKeyDown(Keys.NumPad6))
				_position.X -= movement;

			_worldMatrix = _scaling * Matrix.CreateTranslation(_position);

			/*ILoggerService logger = GetService<ILoggerService>();
			logger.WriteLine("Cube Position: " + _position.ToString());*/

			_effect.SetValue("Diffuse", _diffuse);
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			//this.GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
			this.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			this.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);

			// This code would go between a device 
			// BeginScene-EndScene block.
			_effect.Begin();
			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Begin();

				this.GraphicsDevice.DrawPrimitives(
						PrimitiveType.TriangleList,
						0,
						12);

				pass.End();
			}
			_effect.End();

			//this.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
		}

		#region IViewer Members

		public Vector3 Position
		{
			get { return _position; }
		}

		public Vector2 Position2D
		{
			get { return new Vector2(_position.X, _position.Z); }
		}

		#endregion
	}
}