using System;
using Microsoft.Xna.Framework;
using Osiris.Graphics;
using Osiris.Graphics.SimpleObjects;
using Microsoft.Xna.Framework.Graphics;

namespace Torq2.GameComponents
{
	public class Checkpoint
	{
		private Cube _cube;
		private CheckpointStatus _status;
		private Checkpoint _nextCheckpoint;

		public CheckpointStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}

		public Checkpoint NextCheckpoint
		{
			set { _nextCheckpoint = value; }
		}

		public Checkpoint(Game game, Vector2 position)
		{
			_cube = new Cube(game, new Vector3(position.X, 0, position.Y), Matrix.CreateScale(1, 1000, 1));
			_cube.Initialize();

			_status = CheckpointStatus.Inactive;
		}

		public void Update(GameTime gameTime, Vector3 carPosition)
		{
			if (_status == CheckpointStatus.Active && _cube.BoundingBox.Contains(carPosition) != ContainmentType.Disjoint)
			{
				_status = CheckpointStatus.Complete;
				if (_nextCheckpoint != null)
					_nextCheckpoint.Status = CheckpointStatus.Active;
			}

			switch (_status)
			{
				case CheckpointStatus.Inactive :
					_cube.Diffuse = Color.TransparentWhite;
					break;
				case CheckpointStatus.Active :
					_cube.Diffuse = new Color(255, 0, 0, 128);
					break;
				case CheckpointStatus.Complete:
					_cube.Diffuse = new Color(0, 255, 0, 128);
					break;
			}

			_cube.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			_cube.Draw(gameTime);
		}
	}

	public enum CheckpointStatus
	{
		Inactive,
		Active,
		Complete
	}
}
