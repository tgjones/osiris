using System;
using Microsoft.Xna.Framework;

namespace Osiris.Physics.Particles
{
	public class DistanceConstraint : Constraint
	{
		private Particle _particleA;
		private Particle _particleB;
		private float _restLength;

		public Particle ParticleA
		{
			get { return _particleA; }
		}

		public Particle ParticleB
		{
			get { return _particleB; }
		}

		public float RestLength
		{
			get { return _restLength; }
		}

		public DistanceConstraint(ParticleSystem particleSystem, Particle particleA, Particle particleB, float restLength, float stiffness)
			: base(particleSystem, stiffness)
		{
			_particleA = particleA;
			_particleB = particleB;
			_restLength = restLength;
		}

		public DistanceConstraint(ParticleSystem particleSystem, Particle particleA, Particle particleB, float restLength)
			: this(particleSystem, particleA, particleB, restLength, 1)
		{

		}

		public override void Project()
		{
			// calculate mass ratios
			float totalInverseMass = _particleA.InverseMass + _particleB.InverseMass;
			float massRatioA = _particleA.InverseMass / totalInverseMass;
			float massRatioB = _particleB.InverseMass / totalInverseMass;

			// calculate deltas
			Vector3 delta = _particleA.CandidatePosition - _particleB.CandidatePosition;
			float deltaLength = delta.Length();

			// calculate coefficient, which is the same for both position deltas
			Vector3 coefficient = (delta / deltaLength) * (deltaLength - _restLength);

			// calculate final corrections
			Vector3 deltaA = -massRatioA * coefficient;
			Vector3 deltaB =  massRatioB * coefficient;

			// move particles
			float scaledStiffness = CalculateScaledStiffness();
			_particleA.CandidatePosition += deltaA * scaledStiffness;
			_particleB.CandidatePosition += deltaB * scaledStiffness;
		}

		public override string ToString()
		{
			return string.Format("A-Pos:{0} B-Pos:{1}", _particleA.Position, _particleB.Position) + base.ToString();
		}
	}
}
