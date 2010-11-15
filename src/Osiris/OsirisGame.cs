using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Osiris
{
	public class OsirisGame : Microsoft.Xna.Framework.Game
	{
		private GraphicsDeviceManager _graphicsDeviceManager;
		private bool _useWinForm = false;
		private bool _applyDeviceChanges = false;
		private Osiris.Builder.OsirisBuilderForm _builderForm;

		public bool UseWinForms
		{
			get { return _useWinForm; }
		}

		public OsirisGame(bool useWinForm)
		{
			_graphicsDeviceManager = new GraphicsDeviceManager(this);
			_useWinForm = useWinForm;

			Content.RootDirectory = "Content";

			this.IsFixedTimeStep = false;

			_graphicsDeviceManager.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphicsDeviceManager_PreparingDeviceSettings);
			_graphicsDeviceManager.PreferMultiSampling = false;
			_graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
			_graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
			//_graphicsDeviceManager.IsFullScreen = true;
			//_graphicsDeviceManager.PreferredBackBufferWidth = 1280;
			//_graphicsDeviceManager.PreferredBackBufferHeight = 800;
		}

		private void graphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			e.GraphicsDeviceInformation.PresentationParameters.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8;
			e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = SurfaceFormat.Color;
			e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 1024;
			e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 768;
			e.GraphicsDeviceInformation.PresentationParameters.EnableAutoDepthStencil = true;
			e.GraphicsDeviceInformation.PresentationParameters.PresentOptions = PresentOptions.DiscardDepthStencil;
			e.GraphicsDeviceInformation.PresentationParameters.SwapEffect = SwapEffect.Discard;
			//e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples;
			//e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 2;
		}

		public void ApplyResolutionChange(int width, int height)
		{
			int resolutionWidth = width;
			int resolutionHeight = height;

			if (resolutionWidth <= 0 || resolutionWidth <= 0)
			{
				resolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
				resolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			}

			_graphicsDeviceManager.PreferredBackBufferWidth = resolutionWidth;
			_graphicsDeviceManager.PreferredBackBufferHeight = resolutionHeight;
			//_graphicsDeviceManager.IsFullScreen = GameSettings.Default.Fullscreen;

			_applyDeviceChanges = true;
		}

#if !XBOX360
		private void xnaPanel_Resize(object sender, EventArgs e)
		{
			System.Windows.Forms.Panel panel = (System.Windows.Forms.Panel) sender;
			ApplyResolutionChange(panel.Width, panel.Height);
		}

		private void builderForm_Resize(object sender, EventArgs e)
		{
			Osiris.Builder.OsirisBuilderForm form = (Osiris.Builder.OsirisBuilderForm) sender;
			ApplyResolutionChange(form.PanelWidth, form.PanelHeight);
		}

		private void builderForm_HandleDestroyed(object sender, EventArgs e)
		{
			this.Exit();
		}

		private void gameWindowForm_Shown(object sender, EventArgs e)
		{
			((System.Windows.Forms.Form) sender).Hide();
		}
#endif

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			this.Services.AddService(typeof(ContentManager), this.Content);

			base.Initialize();

#if !XBOX360
			if (_useWinForm)
			{
				System.Windows.Forms.Form gameWindowForm = (System.Windows.Forms.Form) System.Windows.Forms.Form.FromHandle(this.Window.Handle);
				gameWindowForm.Shown += new EventHandler(gameWindowForm_Shown);

				_builderForm = new Osiris.Builder.OsirisBuilderForm(this);
				_builderForm.HandleDestroyed += new EventHandler(builderForm_HandleDestroyed);
				_builderForm.Resize += new EventHandler(builderForm_Resize);
				_builderForm.XnaPanel.Resize += new EventHandler(xnaPanel_Resize);
				_builderForm.Show();
			}
#endif
		}

		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			base.Draw(gameTime);

			// Apply device changes
			if (_applyDeviceChanges)
			{
				_graphicsDeviceManager.ApplyChanges();
				_applyDeviceChanges = false;
			}
#if !XBOX360
			if (_useWinForm)
			{
				GraphicsDevice.Present(_builderForm.PanelHandle);
			}
#endif
		}
	}
}