using System;
using Microsoft.Xna.Framework;

namespace Osiris.Physics.Particles
{
	public class GroundCollisionConstraint : Constraint
	{
		private Particle _particle;
		private float _terrainHeight;

		public GroundCollisionConstraint(ParticleSystem particleSystem, Particle particle, float terrainHeight, float stiffness)
			: base(particleSystem, stiffness)
		{
			_particle = particle;
			_terrainHeight = terrainHeight;
		}

		public GroundCollisionConstraint(ParticleSystem particleSystem, Particle particle, float terrainHeight)
			: this(particleSystem, particle, terrainHeight, 1)
		{

		}

		public override void Project()
		{
			if (_particle.CandidatePosition.Y < _terrainHeight)
				_particle.CandidatePosition.Y = _terrainHeight;
		}
	}
}
