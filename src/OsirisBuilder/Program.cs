using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Osiris;

namespace OsirisBuilder
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			OsirisGame osirisGame = new OsirisGame(true);
			osirisGame.Run();
		}
	}
}