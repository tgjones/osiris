using System;
using Microsoft.Xna.Framework;

namespace Osiris.Terrain.GeoClipMapping
{
	public static class Settings
	{
		#region Geoclipmap settings

		/// <summary>
		/// <para>Because the outer perimeter of each level must lie on the grid of
		/// the next-coarser level, the grid size n must be odd.</para>
		/// 
		/// <para>We use 2^k - 1 so that the finer level is never exactly centred
		/// with respect to its parent next-coarser level. It is always offset
		/// by 1 grid unit either left or right, as well as either top or bottom,
		/// depending on the position of the viewpoint.</para>
		/// 
		/// <para>This is because it is necessary to allow a finer level to shift
		/// while its next-coarser level stays fixed, so we need to handle the
		/// off-centre case anyway.</para>
		/// 
		/// <para>Another point to note is that because we use 16-bit indices to render
		/// the blocks, this allows for a maximum block size of m = 256 and
		/// therefore a maximum clipmap size of n = 1023. This is because 2^16 = 65536,
		/// and the square root of 65536 is 256, so that's the maximum number
		/// of vertices in a square block that we can index. Also, I'm not sure
		/// what the maximum texture size on modern graphics cards is, but I would imagine
		/// going much over 1024 isn't a good idea.</para>
		/// </summary>
		public const short GRID_SIZE_N = 255; // 2^k - 1; min of 15 and max of 1023

		/// <summary>
		/// Defined as an optimisation
		/// </summary>
		public const short GRID_SIZE_N_MINUS_ONE = GRID_SIZE_N - 1;

		/// <summary>
		/// Used to create the textures for each level that store the elevation data
		/// </summary>
		public const short ELEVATION_TEXTURE_SIZE = GRID_SIZE_N;

		/// <summary>
		/// This value is regularly used so it's here as a convenience and an optimisation.
		/// </summary>
		public const float ELEVATION_TEXTURE_SIZE_INVERSE = 1.0f / (float) ELEVATION_TEXTURE_SIZE;

		/// <summary>
		/// Used to create the textures for each level that store the normal data
		/// </summary>
		public const short NORMAL_MAP_TEXTURE_SIZE = ELEVATION_TEXTURE_SIZE;

		/// <summary>
		/// This value is regularly used so it's here as a convenience and an optimisation.
		/// </summary>
		public const float NORMAL_MAP_TEXTURE_SIZE_INVERSE = 1.0f / (float) NORMAL_MAP_TEXTURE_SIZE;

		/// <summary>
		/// Used to create the textures for each level that store the diffuse colour
		/// </summary>
		public const short COLOUR_MAP_TEXTURE_SIZE = ELEVATION_TEXTURE_SIZE;

		/// <summary>
		/// Each clipmap is split into 12 blocks. This both reduces memory costs,
		/// and enables view frustum culling.
		/// </summary>
		public const short BLOCK_SIZE_M = (GRID_SIZE_N + 1) / 4;

		/// <summary>
		/// This value is regularly used so it's here as a convenience and an optimisation.
		/// </summary>
		public const short BLOCK_SIZE_M_MINUS_ONE = BLOCK_SIZE_M - 1;

		/// <summary>
		/// <para>The number of indices in a block includes the necessary indices
		/// for degenerate triangles. An example m x m block is shown below:</para>
		/// 
		/// <para>.  .  .  .</para>
		/// <para>.  .  .  .</para>
		/// <para>.  .  .  .</para>
		/// <para>.  .  .  .</para>
		/// 
		/// <para>For the top and bottom rows, each point will be indexed once.
		/// For the middle rows, each point will be indexed twice.
		/// For the middle rows, 2 additional indices are needed for the
		/// degenerate triangles.</para>
		/// 
		/// <para>So we have:</para>
		/// <para>m * 2          top and bottom rows</para>
		/// <para>2m * (m - 2)   middle rows</para>
		/// <para>2 * (m - 2)    indices for degenerate triangles</para>
		/// 
		/// <para>Summing up:</para>
		/// <para>(m * 2) + (2m * (m - 2)) + (2 * m - 2)) = 2m^2 - 4</para>
		/// 
		/// <para>So for the example above where m = 4, the number of indices is 28.</para>
		/// </summary>
		public const short BLOCK_NUM_INDICES = (2 * (BLOCK_SIZE_M * BLOCK_SIZE_M)) - 4;

		/// <summary>
		/// This is used when rendering; it's set as a const just for optimisation.
		/// </summary>
		public const short BLOCK_NUM_PRIMITIVES = BLOCK_NUM_INDICES - 2;

		/// <summary>
		/// The number of vertices is simply the square of the block size.
		/// </summary>
		public const short BLOCK_NUM_VERTICES = BLOCK_SIZE_M * BLOCK_SIZE_M;

		/// <summary>
		/// There are four ring-fixups, which are necessary because the blocks don't
		/// completely cover the ring. There is a gap of (n - 1) - ((m - 1) * 4) = 2
		/// quads at the middle of each ring side. We patch these gaps using four m * 3
		/// fix-up regions. We encode these regions using one vertex and index buffer.
		/// 
		/// The number of indices, therefore, includes all the indices to render each
		/// of the four ring fix-ups, and for the degenerate triangles used to link the
		/// ring fix-ups.
		/// 
		/// For each left and right ring fix-up, we have:
		/// m * 2          top and bottom rows
		/// 2m             middle row
		/// 2              degenerates
		/// 
		/// Total for left and right ring fix-ups:
		/// (2m + 2m + 2) * 2 = 8m + 4
		/// 
		/// For each top and bottom ring fix-up, we have:
		/// 3 * 2             top and bottom rows
		/// 3 * 2 * (m - 2)   middle rows
		/// 2 * (m - 2)       degenerates
		/// 
		/// Total for top and bottom ring fix-ups:
		/// (6 + (6 * (m - 2)) + (2 * (m - 2))) * 2 = 16m - 20
		/// 
		/// So for all four ring fix-ups, the indices count is:
		/// (8m + 4) + (16m - 20) = 24m - 16
		/// 
		/// Then we need to add the 6 indices, used to create the linking degenerate triangles.
		/// So the total is:
		/// 24m - 10
		/// </summary>
		public const short RING_FIXUPS_NUM_INDICES = (24 * BLOCK_SIZE_M) - 10;

		/// <summary>
		/// This is used when rendering; it's set as a const just for optimisation.
		/// </summary>
		public const short RING_FIXUPS_NUM_PRIMITIVES = RING_FIXUPS_NUM_INDICES - 2;

		/// <summary>
		/// The number of vertices for all 4 ring-fixups in a level. We include all
		/// four because the vertex buffer includes all four.
		/// </summary>
		public const short RING_FIXUPS_NUM_VERTICES = BLOCK_SIZE_M * 3 * 4;

		/// <summary>
		/// 
		/// </summary>
		public const short EDGE_STITCHES_NUM_INDICES = (GRID_SIZE_N * 4) + 1;

		/// <summary>
		/// This is used when rendering; it's set as a const just for optimisation.
		/// </summary>
		public const short EDGE_STITCHES_NUM_PRIMITIVES = EDGE_STITCHES_NUM_INDICES - 2;

		/// <summary>
		/// 
		/// </summary>
		public const short EDGE_STITCHES_NUM_VERTICES = (GRID_SIZE_N_MINUS_ONE * 4) + 1;

		/// <summary>
		/// This will depend on whether we use triangle lists or strips. Currently we're
		/// using strips, just to be consistent, but it's not necessarily the most efficient
		/// method.
		/// 
		/// Don't ask how I arrived at this number of indices. Trust me, it works.
		/// </summary>
		public const short INTERIOR_TRIM_NUM_INDICES = (16 * BLOCK_SIZE_M) - 4;

		/// <summary>
		/// This is used when rendering; it's set as a const just for optimisation.
		/// </summary>
		public const short INTERIOR_TRIM_NUM_PRIMITIVES = INTERIOR_TRIM_NUM_INDICES - 2;

		/// <summary>
		/// ((2m + 1) * 4) - 4 + 2
		/// </summary>
		public const short INTERIOR_TRIM_NUM_VERTICES = (BLOCK_SIZE_M * 8) + 2;

		public static readonly IntVector2 CentralSquareMin = new IntVector2(Settings.BLOCK_SIZE_M_MINUS_ONE * 2);
		public static readonly IntVector2 CentralSquareMax = new IntVector2((Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 1);

		/// <summary>
		/// Defines the width of the blending transition at the edge of a level, between
		/// the height of the vertices of the finer level and the height of the vertices
		/// of the next coarser level.
		/// </summary>
		public const float TRANSITION_WIDTH = (float) GRID_SIZE_N / 10.0f;

		/// <summary>
		/// This is used when rendering; it's set as a const just for optimisation.
		/// </summary>
		public const float TRANSITION_WIDTH_INVERSE = 1.0f / TRANSITION_WIDTH;

		/// <summary>
		/// This is used when rendering; it's set as a const just for optimisation.
		/// </summary>
		public const float ALPHA_OFFSET = ((float) GRID_SIZE_N_MINUS_ONE / 2.0f) - TRANSITION_WIDTH - 1.0f;

		public static readonly Vector2 TransitionWidthInverse = new Vector2(TRANSITION_WIDTH_INVERSE);
		public static readonly Vector2 AlphaOffset = new Vector2(ALPHA_OFFSET);

		#endregion
	}
}
