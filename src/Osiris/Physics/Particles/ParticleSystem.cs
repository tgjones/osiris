using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Terrain;

namespace Osiris.Physics.Particles
{
	public class ParticleSystem : Microsoft.Xna.Framework.DrawableGameComponent
	{
		private List<Particle> _particles;
		private List<Constraint> _constraints;

		VertexDeclaration _basicEffectVertexDeclaration;
		BasicEffect _basicEffect;

		public readonly int SolverIterations;

		public ParticleSystem(Game game, string configFile)
			: base(game)
		{
			SolverIterations = 1;
			_particles = new List<Particle>();
			_constraints = new List<Constraint>();

			LoadParticleSystem(configFile);

			this.UpdateOrder = 1000;
		}

		private void LoadParticleSystem(string configFile)
		{
			_particles = new List<Particle>();
			_constraints = new List<Constraint>();

			XmlDocument doc = new XmlDocument();
			doc.Load(configFile);

			// load particles
			foreach (XmlElement particleElement in doc.SelectNodes("/ParticleSystem/Particles/Particle"))
			{
				string positionString = particleElement.Attributes["Position"].Value;
				string[] positionArray = positionString.Split(',');
				Vector3 position = new Vector3(Convert.ToSingle(positionArray[0]), Convert.ToSingle(positionArray[1]), Convert.ToSingle(positionArray[2]));
				Particle particle = new Particle(position, this.Game);
				if (particleElement.HasAttribute("Mass"))
				{
					string massString = particleElement.Attributes["Mass"].Value;
					if (massString == "INFINITE_MASS")
						particle.Mass = Particle.INFINITE_MASS;
					else
						particle.Mass = Convert.ToSingle(massString);
				}
				_particles.Add(particle);
			}

			// load constraints
			foreach (XmlElement constraintElement in doc.SelectNodes("/ParticleSystem/Constraints/*"))
			{
				string constraintType = constraintElement.Name;
				Constraint constraint = null;

				switch (constraintType)
				{
					case "DistanceConstraint" :
						Particle particleA = _particles[Convert.ToInt32(constraintElement.Attributes["ParticleA"].Value)];
						Particle particleB = _particles[Convert.ToInt32(constraintElement.Attributes["ParticleB"].Value)];

						float restLength;
						if (constraintElement.HasAttribute("RestLength"))
							restLength = Convert.ToSingle(constraintElement.Attributes["RestLength"].Value);
						else
							restLength = Vector3.Distance(particleA.Position, particleB.Position);

						constraint = new DistanceConstraint(this, particleA, particleB, restLength);
						if (constraintElement.HasAttribute("Stiffness"))
							constraint.Stiffness = Convert.ToSingle(constraintElement.Attributes["Stiffness"].Value);

						break;
				}

				if (constraint != null)
					_constraints.Add(constraint);
			}
		}

		protected override void LoadContent()
		{
			_basicEffectVertexDeclaration = new VertexDeclaration(this.GraphicsDevice, VertexPositionColor.VertexElements);

			_basicEffect = new BasicEffect(this.GraphicsDevice, null);
			_basicEffect.Alpha = 1.0f;
			_basicEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f);
			_basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
			_basicEffect.SpecularPower = 5.0f;
			_basicEffect.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);
		}

		public override void Update(GameTime gameTime)
		{
			float deltaTime = (float) gameTime.ElapsedGameTime.TotalMilliseconds;
			if (deltaTime == 0)
				return;
			deltaTime = 1 / deltaTime;

			// line 5
			foreach (Particle p in _particles)
				p.Velocity += deltaTime * p.InverseMass * CalculateExternalForces(p);

			// line 6
			//DampVelocities();

			// line 7
			foreach (Particle p in _particles)
				p.CandidatePosition = p.Position + (deltaTime * p.Velocity);

			// line 8
			List<Constraint> collisionConstraints = new List<Constraint>();
			foreach (Particle p in _particles)
				GenerateCollisionConstraints(p, collisionConstraints);

			// lines 9-11
			for (int i = 0; i < SolverIterations; i++)
			{
				foreach (Constraint c in _constraints)
					c.Project();

				foreach (Constraint c in collisionConstraints)
					c.Project();
			}

			// lines 12-15
			foreach (Particle p in _particles)
			{
				p.Velocity = (p.CandidatePosition - p.Position) / deltaTime;
				p.Position = p.CandidatePosition;
			}

			// line 16
			//VelocityUpdate();

			ICameraService camera = (ICameraService) this.Game.Services.GetService(typeof(ICameraService));
			_basicEffect.World = Matrix.Identity;
			_basicEffect.View = camera.ViewMatrix;
			_basicEffect.Projection = camera.ProjectionMatrix;
		}

		private void GenerateCollisionConstraints(Particle p, List<Constraint> collisionConstraints)
		{
			IHeightMapService terrain = (IHeightMapService) this.Game.Services.GetService(typeof(ITerrainService));
			if (terrain != null)
			{
				float terrainHeight = terrain[p.CandidatePosition.X, p.CandidatePosition.Z];
				if (p.CandidatePosition.Y < terrainHeight)
					collisionConstraints.Add(new GroundCollisionConstraint(this, p, terrainHeight));
			}
		}

		private static Vector3 CalculateExternalForces(Particle p)
		{
			// not efficient at all
			return new Vector3(0, -1f * p.Mass, 0);
		}

		public override void Draw(GameTime gameTime)
		{
			// create particle vertices
			VertexPositionColor[] pointList = new VertexPositionColor[_particles.Count];
			int index = 0;
			foreach (Particle p in _particles)
				pointList[index++] = new VertexPositionColor(p.Position, Color.Red);

			// create constraint indices
			List<short> lineListIndices = new List<short>();
			foreach (Constraint c in _constraints)
			{
				if (c is DistanceConstraint)
				{
					DistanceConstraint dc = (DistanceConstraint) c;
					lineListIndices.Add((short) _particles.IndexOf(dc.ParticleA));
					lineListIndices.Add((short) _particles.IndexOf(dc.ParticleB));
				}
			}

			//this.GraphicsDevice.RenderState.PointSize = 10;
			this.GraphicsDevice.VertexDeclaration = _basicEffectVertexDeclaration;

			_basicEffect.Begin();
			foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
			{
				pass.Begin();

				/*// draw particles
				this.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
					PrimitiveType.PointList,
					pointList,
					0,  // index of the first vertex to draw
					pointList.Length); // number of primitives*/

				// draw constraints
				this.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
					PrimitiveType.LineList,
					pointList,
					0,  // vertex buffer offset to add to each element of the index buffer
					pointList.Length, // number of vertices in pointList
					lineListIndices.ToArray(), // the index buffer
					0,  // first index element to read
					lineListIndices.Count / 2); // number of primitives to draw

				pass.End();
			}
			_basicEffect.End();
		}
	}
}
