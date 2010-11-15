using System;

namespace Osiris.Terrain.SeamlessPatches
{
	public static class Settings
	{
		/// <summary>
		/// Precision factor
		/// </summary>
		public const float RHO = 1.0f;

		public const int PATCH_SIZE = 4;

		/// <summary>
		/// Number of different resolutions for each patch
		/// </summary>
		public const int NUM_RESOLUTIONS = 1;
		public const int MAX_RESOLUTION = NUM_RESOLUTIONS - 1;

		/// <summary>
		/// Branching factor
		/// </summary>
		public const int R = 2 ^ (NUM_RESOLUTIONS - 1);
		public const int R_SQUARED = R * R;
	}
}
