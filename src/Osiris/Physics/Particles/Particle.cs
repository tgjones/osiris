using System;
using Microsoft.Xna.Framework;
using Osiris.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Physics.Particles
{
	public class Particle
	{
		public const int INFINITE_MASS = int.MinValue;

		private Vector3 _position;
		private float _mass;
		private float _inverseMass;

		public Vector3 Velocity;
		public Vector3 CandidatePosition;

		private ExtendedModel _model;

		public Vector3 Position
		{
			get { return _position; }
			set
			{
				_position = value;
				_model.WorldMatrix = Matrix.CreateScale(0.03f) * Matrix.CreateTranslation(_position);
			}
		}

		public float Mass
		{
			get { return _mass; }
			set
			{
				_mass = value;

				if (value == INFINITE_MASS)
					_inverseMass = 0;
				else
					_inverseMass = 1 / value;
			}
		}

		public float InverseMass
		{
			get { return _inverseMass; }
		}

		public Particle(Vector3 position, float mass, Game game)
		{
			IGameComponent
			_model = new ExtendedModel(game, @"Graphics\SimpleObjects\Sphere0");
			game.Components.Add(_mesh);

			Position = position;
			Velocity = Vector3.Zero;
			CandidatePosition = Vector3.Zero;
			Mass = mass;
		}

		public Particle(Vector3 position, Game game) : this(position, 1, game)
		{

		}

		public override string ToString()
		{
			return string.Format("P:{0} V:{1} CP:{2}", Position, Velocity, CandidatePosition);
		}
	}
}
