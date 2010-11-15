using System;
using Microsoft.Xna.Framework;

namespace Osiris
{
	public interface IViewer
	{
		Vector3 Position
		{
			get;
		}

		Vector2 Position2D
		{
			get;
		}

		Vector3 Direction
		{
			get;
		}
	}
}
