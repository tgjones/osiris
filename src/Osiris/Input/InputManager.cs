using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Osiris.Input
{
	public class InputManager : GameComponent, IInputService
	{
		private Dictionary<string, Keys[]> _registeredKeys;
		private Dictionary<string, Buttons[]> _registeredButtons;

		private KeyboardState _previousKeyboardState, _currentKeyboardState;
		private GamePadState _previousGamePadState, _currentGamePadState;

#if TARGET_WINDOWS
		private string _mouseListener;
		private MouseStyle _mouseStyle;
		private MouseState _mouseState;
#endif

		public InputManager(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(IInputService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);

			_registeredKeys = new Dictionary<string, Keys[]>();
			_registeredButtons = new Dictionary<string, Buttons[]>();

			_currentKeyboardState = Keyboard.GetState();
			_currentGamePadState = GamePad.GetState(PlayerIndex.One);

			UpdateOrder = 1;
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(IInputService));
		}

		public void RegisterMapping(string name, params Keys[] keys)
		{
			_registeredKeys.Add(name, keys);
		}

		public void RegisterMapping(string name, params Buttons[] buttons)
		{
			_registeredButtons.Add(name, buttons);
		}

#if TARGET_WINDOWS
		public void RegisterMouseListener(string name, MouseStyle style)
		{
			// ignore request if we're in the Builder app
			if (((OsirisGame) Game).UseWinForms)
				return;

			if (_mouseListener != null)
				throw new ArgumentException("Mouse listener is already registered", "name");

			_mouseListener = name;
			_mouseStyle = style;

			if (style == MouseStyle.FirstPersonShooter)
				ResetMousePosition();
		}
#endif

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_previousKeyboardState = _currentKeyboardState;
			_currentKeyboardState = Keyboard.GetState();

			_previousGamePadState = _currentGamePadState;
			_currentGamePadState = GamePad.GetState(PlayerIndex.One);

#if TARGET_WINDOWS
			if (_mouseListener != null)
			{
				_mouseState = Mouse.GetState();

				// if FPS-style mouse has been requested, translate mouse position into relative coordinates
				if (_mouseStyle == MouseStyle.FirstPersonShooter)
				{
					// translate mouse position into relative coordinates
					int mouseMoveX = _mouseState.X - (this.Game.Window.ClientBounds.Width / 2);
					int mouseMoveY = _mouseState.Y - (this.Game.Window.ClientBounds.Height / 2);
					_mouseState = new MouseState(mouseMoveX, mouseMoveY, _mouseState.ScrollWheelValue, _mouseState.LeftButton,
						_mouseState.MiddleButton, _mouseState.RightButton, _mouseState.XButton1, _mouseState.XButton2);

					ResetMousePosition();
				}
			}
#endif
		}

		public bool IsButtonDown(string name)
		{
			Keys[] keys;
			if (_registeredKeys.TryGetValue(name, out keys))
				foreach (Keys key in keys)
					if (!_currentKeyboardState.IsKeyDown(key))
						return false;
			if (_currentGamePadState.IsConnected)
			{
				Buttons[] buttons;
				if (_registeredButtons.TryGetValue(name, out buttons))
					foreach (Buttons button in buttons)
						if (!_currentGamePadState.IsButtonDown(button))
							return false;
			}
			return true;
		}

		public bool IsButtonPressed(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsButtonUp(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

#if TARGET_WINDOWS
		public MouseState GetMouseState()
		{
			return _mouseState;
		}

		/// <summary>
		/// Resets the mouse cursor to the centre of the window
		/// </summary>
		private void ResetMousePosition()
		{
			OsirisGame game = (OsirisGame) Game;
			if (Mouse.GetState().LeftButton == ButtonState.Pressed || !game.UseWinForms)
				Mouse.SetPosition(this.Game.Window.ClientBounds.Width / 2, this.Game.Window.ClientBounds.Height / 2);
		}
#endif
	}
}
