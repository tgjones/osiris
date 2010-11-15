using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Osiris.Builder
{
	public partial class OsirisBuilderForm : Form
	{
		#region Fields

		private OsirisGame _game;

		#endregion

		#region Properties

		public Panel XnaPanel
		{
			get { return pnlXna; }
		}

		public int PanelWidth
		{
			get { return pnlXna.Width; }
		}

		public int PanelHeight
		{
			get { return pnlXna.Height; }
		}

		public IntPtr PanelHandle
		{
			get { return pnlXna.IsHandleCreated ? pnlXna.Handle : IntPtr.Zero; }
		}

		#endregion

		public OsirisBuilderForm(OsirisGame game)
		{
			InitializeComponent();

			_game = game;

			TreeNode rootNode = new TreeNode("Game");
			rootNode.Tag = _game;
			foreach (GameComponent gameComponent in _game.Components)
			{
				TreeNode treeNode = new TreeNode(gameComponent.ToString());
				treeNode.Tag = gameComponent;
				rootNode.Nodes.Add(treeNode);
			}
			trvObjects.Nodes.Add(rootNode);
			trvObjects.ExpandAll();
		}

		private void trvObjects_AfterSelect(object sender, TreeViewEventArgs e)
		{
			prgObjectProperties.SelectedObject = e.Node.Tag;
		}
	}
}