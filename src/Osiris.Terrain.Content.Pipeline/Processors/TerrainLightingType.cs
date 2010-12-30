namespace Osiris.Terrain.Content.Pipeline.Processors
{
	public enum TerrainLightingType
	{
		/// <summary>
		/// Lighting is baked into terrain texture.
		/// </summary>
		PreBaked,

		/// <summary>
		/// Lighting is calculated at build-time from the directional light settings for this terrain.
		/// </summary>
		// Static - Coming soon...
	}
}