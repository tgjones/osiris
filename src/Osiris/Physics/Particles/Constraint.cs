using System;

namespace Osiris.Physics.Particles
{
	public abstract class Constraint
	{
		private ParticleSystem _particleSystem;

		public float Stiffness;

		public Constraint(ParticleSystem particleSystem, float stiffness)
		{
			_particleSystem = particleSystem;
			Stiffness = stiffness;
		}

		public abstract void Project();

		public override string ToString()
		{
			return string.Format("Stiffness:{0}", Stiffness);
		}

		protected float CalculateScaledStiffness()
		{
			// calculate stiffness (differs based on number of solver iterations)
			float scaledStiffness = 1 - MathsHelper.Pow(1 - Stiffness, (1 / (float) _particleSystem.SolverIterations));
			return scaledStiffness;
		}
	}
}
