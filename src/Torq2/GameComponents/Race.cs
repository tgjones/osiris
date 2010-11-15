using System;
using Microsoft.Xna.Framework;

namespace Torq2.GameComponents
{
	public class Race : Microsoft.Xna.Framework.DrawableGameComponent
	{
		private Vehicle _vehicle;
		private Checkpoint[] _checkpoints;

		public bool Won
		{
			get
			{
				foreach (Checkpoint checkpoint in _checkpoints)
					if (checkpoint.Status != CheckpointStatus.Complete)
						return false;
				return true;
			}
		}

		public Race(Game game, Vehicle vehicle)
			: base(game)
		{
			UpdateOrder = 3000;
			DrawOrder = 3000;

			_vehicle = vehicle;

			// create checkpoints
			_checkpoints = new Checkpoint[1];
			_checkpoints[0] = new Checkpoint(game, new Vector2(0, -300));
			//_checkpoints[1] = new Checkpoint(game, new Vector2(100, -1000));
			//_checkpoints[2] = new Checkpoint(game, new Vector2(1000, -200));

			//_checkpoints[0].NextCheckpoint = _checkpoints[1];
			//_checkpoints[1].NextCheckpoint = _checkpoints[2];

			_checkpoints[0].Status = CheckpointStatus.Active;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// check whether car has intersected with any of the checkpoints
			foreach (Checkpoint checkpoint in _checkpoints)
				checkpoint.Update(gameTime, _vehicle.Position);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			foreach (Checkpoint checkpoint in _checkpoints)
				checkpoint.Draw(gameTime);
		}
	}
}
