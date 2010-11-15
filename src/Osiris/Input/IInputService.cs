using System;
using Microsoft.Xna.Framework.Input;

namespace Osiris.Input
{
	public interface IInputService
	{
		void RegisterMapping(string name, params Keys[] keys);
		void RegisterMapping(string name, params Buttons[] buttons);

		void RegisterMouseListener(string name, MouseStyle style);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="group"></param>
		/// <param name="name"></param>
		/// <returns>true if button was pressed since the last update, otherwise false</returns>
		bool IsButtonDown(string name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="group"></param>
		/// <param name="name"></param>
		/// <returns>true if button is currently pressed</returns>
		bool IsButtonPressed(string name);

		bool IsButtonUp(string name);

#if TARGET_WINDOWS
		MouseState GetMouseState();
#endif
	}
}
