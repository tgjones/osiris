using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Osiris.Graphics;

namespace Osiris.Gui
{
	/// <summary>
	/// Based on http://www.ziggyware.com/readarticle.php?article_id=163
	/// </summary>
	public class ConsoleMenu : Microsoft.Xna.Framework.DrawableGameComponent
	{
		#region Fields

		// graphics
		private ContentManager _content;
		private ResolveTexture2D _texture;
		private SpriteBatch _batch;
		private SpriteFont _font;

		// menu
		private bool _active;
		private int _width, _height;
		private string _command = string.Empty, _prefix = ">", _line = "------------" + Environment.NewLine, _logPrefix = "--> ", _message = "Type HELP to begin";
		private float _prefixHeight;
		private List<string> _log;

		// input
		private KeyboardState _currentKeyState, _prevKeyState;
		private Keys[] _alphabetKeys = { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z };

		#endregion

		#region Constructor

		public ConsoleMenu(Game game)
			: base(game)
		{
			_content = new ContentManager(game.Services, "Content");

			_log = new List<string>();
		}

		#endregion

		#region Methods

		public override void Initialize()
		{
			_log.Add(_message);
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_batch = new SpriteBatch(GraphicsDevice);
			_font = _content.Load<SpriteFont>("Fonts/Console");
			_prefixHeight = _font.MeasureString(_prefix).Y;

			_texture = new ResolveTexture2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1, GraphicsDevice.DisplayMode.Format);

			_width = GraphicsDevice.Viewport.Width;
			_height = (GraphicsDevice.Viewport.Height / 2) - 100;

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			CheckInput();
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (_active)
			{
				// save a copy of the current screen into the texture. this allows the menu to be transparent.
				GraphicsDevice.ResolveBackBuffer(_texture);

				_batch.Begin();

				// draw the texture without any tint
				_batch.Draw(_texture, new Rectangle(0, 0, _width, GraphicsDevice.Viewport.Height), Color.White);

				// draw transparent menu
				_batch.Draw(_texture, new Rectangle(0, 0, _width, _height), new Rectangle(0, 0, _width, _height), Color.Gray);

				// draw command string
				_batch.DrawString(_font, _line + _prefix + _command, new Vector2(10, _height - (_prefixHeight * 2) - 4), Color.White);

				// draw log
				for (int i = 0; i < _log.Count; i++)
					_batch.DrawString(_font, _log[i], new Vector2(10, _height - 34 - (_prefixHeight * -i) - (_prefixHeight * _log.Count)), Color.Silver);

				_batch.End();
			}

			base.Draw(gameTime);
		}

		private void CheckInput()
		{
			_prevKeyState = _currentKeyState;
			_currentKeyState = Keyboard.GetState();

			// toggle the console menu on or off
			if (_currentKeyState.IsKeyDown(Keys.OemTilde) && _prevKeyState.IsKeyUp(Keys.OemTilde))
			{
				_active = !_active;

				// clear the log when the menu closes
				if (!_active)
				{
					_log.Clear();
					_log.Add(_message);
				}
			}

			if (_active)
			{
				// check input for alphabetical letters
				foreach (Keys key in _alphabetKeys)
				{
					if (_currentKeyState.IsKeyDown(key) && _prevKeyState.IsKeyUp(key))
					{
						string letter = key.ToString();

						if (_currentKeyState.IsKeyUp(Keys.LeftShift) && _currentKeyState.IsKeyUp(Keys.RightShift))
							letter = letter.ToLower();

						_command += letter;
					}
				}

				// check input for spacebar
				if (_currentKeyState.IsKeyDown(Keys.Space) && _prevKeyState.IsKeyUp(Keys.Space) && _command != string.Empty && _command[_command.Length - 1] != ' ')
					_command += " ";

				// check input for backspace
				if (_currentKeyState.IsKeyDown(Keys.Back) && _prevKeyState.IsKeyUp(Keys.Back) && _command != string.Empty && _command != _prefix)
					_command = _command.Remove(_command.Length - 1, 1);

				// check input for enter
				if (_currentKeyState.IsKeyDown(Keys.Enter) && _prevKeyState.IsKeyUp(Keys.Enter) && _command != string.Empty)
				{
					_log.Add(string.Empty);
					_log.Add(_command);
					ExecuteCommand();
					_command = string.Empty;
				}
			}
		}

		private void ExecuteCommand()
		{
			string command = _command.ToLower();

			if (command == "help")
			{
				_log.Add(_logPrefix + "COMMANDS - Displays a list of commands");
				_log.Add(_logPrefix + "COMPONENTS - Displays a list of components to use with the commands");
			}
			else if (command == "commands")
			{
				_log.Add(_logPrefix + "TOGGLE component - Turns the component on or off");
				_log.Add(_logPrefix + "QUIT - Exits the program");
			}
			else if (command == "components")
			{
				_log.Add(_logPrefix + "FPS - Displays the current frames per second");
			}
			else if (command == "toggle fps")
			{
				FrameRateCounter fpsComponent = null;
				foreach (IGameComponent gameComponent in Game.Components)
					if (gameComponent is FrameRateCounter)
						fpsComponent = (FrameRateCounter) gameComponent;

				fpsComponent.Enabled = !fpsComponent.Enabled;
				fpsComponent.Visible = !fpsComponent.Visible;

				_log.Add(_logPrefix + "FPS: " + (fpsComponent.Visible ? "ON" : "OFF"));
			}
			else if (command == "quit")
			{
				Game.Exit();
			}
			else
			{
				_log.Add(_logPrefix + "Command does not exist");
			}
		}

		#endregion
	}
}
