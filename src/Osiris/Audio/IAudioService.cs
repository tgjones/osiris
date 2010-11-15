using System;
using Microsoft.Xna.Framework.Audio;

namespace Osiris.Audio
{
	public interface IAudioService
	{
		void PlaySound(string cueName);
		Cue GetCue(string cueName);
	}
}
