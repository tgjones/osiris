using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using System.Collections.Generic;

namespace Osiris.Sky
{
	public class SkyDome : Osiris.DrawableGameComponent
	{
		private VertexDeclaration _vertexDeclaration;
		private VertexBuffer _vertexBuffer;
		private IndexBuffer _indexBuffer;
		private int _numVertices, _numPrimitives;

		public override BoundingBox BoundingBox
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public SkyDome(Game game)
			: base(game, @"Sky\Sky", false, false, false)
		{
			IsAlwaysVisible = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_vertexDeclaration = new VertexDeclaration(this.GraphicsDevice, SkyDomeVertex.VertexElements);

			// Marco - skydome generator that only creates what is necessary
			// Radius = PlanetRadius + 200000
			// Y offset is PlanetRadius
			// Don't need anything below the horizon (i.e. Y < 0)

			const int NUM_RINGS = 160;
			const int NUM_SEGMENTS = 20;

			// set vertex and index count
			const int NUM_VERTICES = (NUM_RINGS + 1) * (NUM_SEGMENTS + 1);
			const int NUM_INDICES = 2 * NUM_RINGS * (NUM_SEGMENTS + 1);

			// establish constants used in sphere generation
			const float DELTA_RING_ANGLE = (MathHelper.Pi) / NUM_RINGS;
			//const float DELTA_RING_ANGLE = (MathHelper.PiOver4 / 2) / NUM_RINGS;
			const float DELTA_SEGMENT_ANGLE = MathHelper.TwoPi / NUM_SEGMENTS;

			SkyDomeVertex[] vertices = new SkyDomeVertex[NUM_VERTICES];
			short[] indices = new short[NUM_INDICES];

			IAtmosphereService atmosphereService = GetService<IAtmosphereService>();
			float skydomeRadius = atmosphereService.SkydomeRadius;
			float earthRadius = atmosphereService.EarthRadius;
			float atmosphereDepth = atmosphereService.AtmosphereDepth;

			// generate the group of rings for the sphere
			int vertexCounter = 0, indexCounter = 0; short index = 0;
			for (int ring = 0; ring < NUM_RINGS + 1; ring++)
			{
				float r0 = MathsHelper.Sin(ring * DELTA_RING_ANGLE);
				float y0 = MathsHelper.Cos(ring * DELTA_RING_ANGLE);

				// generate the group of segments for the current ring
				for (int segment = 0; segment < NUM_SEGMENTS + 1; segment++)
				{
					float x0 = r0 * MathsHelper.Sin(segment * DELTA_SEGMENT_ANGLE);
					float z0 = r0 * MathsHelper.Cos(segment * DELTA_SEGMENT_ANGLE);

					// add one vertex to the strip which makes up the sphere
					vertices[vertexCounter++] = new SkyDomeVertex(
						x0 * skydomeRadius,
						y0 * (skydomeRadius),
						z0 * skydomeRadius);

					// add two indices except for last ring
					if (ring != NUM_RINGS)
					{
						indices[indexCounter++] = (short)(index + NUM_SEGMENTS + 1);
						indices[indexCounter++] = index;

						index++;
					}
				}
			}

			// create the vertex buffer
			_vertexBuffer = new VertexBuffer(this.GraphicsDevice, typeof(SkyDomeVertex),
				NUM_VERTICES, BufferUsage.WriteOnly);

			// create the index buffer
			_indexBuffer = new IndexBuffer(this.GraphicsDevice, typeof(short),
				NUM_INDICES, BufferUsage.WriteOnly);

			// set data
			_vertexBuffer.SetData<SkyDomeVertex>(vertices);
			_indexBuffer.SetData<short>(indices);

			_numVertices = NUM_VERTICES;
			//_numPrimitives = index / 2;
			_numPrimitives = indexCounter / 2;
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			IAtmosphereService atmosphereService = GetService<IAtmosphereService>();
			atmosphereService.SetEffectParameters(_effect, true);
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			this.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, SkyDomeVertex.SizeInBytes);
			this.GraphicsDevice.Indices = _indexBuffer;
			this.GraphicsDevice.VertexDeclaration = _vertexDeclaration;

			_effect.Begin();
			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Begin();

				this.GraphicsDevice.DrawIndexedPrimitives(
					PrimitiveType.TriangleStrip, 0, 0,
					_numVertices, 0, _numPrimitives);

				pass.End();
			}
			_effect.End();
		}
	}
}
