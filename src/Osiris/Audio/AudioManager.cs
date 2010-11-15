using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Osiris.Audio
{
	public class AudioManager : GameComponent, IAudioService
	{
		private AudioEngine _engine;
		private SoundBank _soundBank;
		private WaveBank _waveBank;

		public AudioManager(Game game)
			: base(game)
		{
			Game.Services.AddService(typeof(IAudioService), this);
			Game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(IAudioService));
		}

		public override void Initialize()
		{
			base.Initialize();

			// Initialize audio objects.
			_engine = new AudioEngine("Content\\Audio\\Torq2Audio.xgs");
			_soundBank = new SoundBank(_engine, "Content\\Audio\\Sound Bank.xsb");
			_waveBank = new WaveBank(_engine, "Content\\Audio\\Wave Bank.xwb");
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_engine.Update();
		}

		public void PlaySound(string cueName)
		{
			// Play the sound.
			_soundBank.PlayCue(cueName);
		}

		public Cue GetCue(string cueName)
		{
			return _soundBank.GetCue(cueName);
		}
	}
}
