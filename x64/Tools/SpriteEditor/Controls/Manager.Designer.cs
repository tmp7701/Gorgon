#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Tuesday, June 05, 2007 10:11:17 AM
// 
#endregion

using System;
using System.ComponentModel;

namespace GorgonLibrary.Graphics.Tools.Controls
{
	partial class Manager
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (!DesignMode)
			{
				if (disposing)
				{
					// Unbind events.
					if (_bindingItem != null)
						_bindingItem.Click -= new EventHandler(_bindingItem_Click);

					if (_dockWindow != null)
					{
						_dockWindow.Docking -= new EventHandler(_dockWindow_Docking);
						_dockWindow.WindowClosing -= new CancelEventHandler(_dockWindow_WindowClosing);
					}
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelManagerCaption = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelManager = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.labelManagerClose = new System.Windows.Forms.Label();
			this.panelManagerCaption.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelManagerCaption
			// 
			this.panelManagerCaption.Controls.Add(this.panel1);
			this.panelManagerCaption.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelManagerCaption.Location = new System.Drawing.Point(0, 0);
			this.panelManagerCaption.Margin = new System.Windows.Forms.Padding(4);
			this.panelManagerCaption.Name = "panelManagerCaption";
			this.panelManagerCaption.Size = new System.Drawing.Size(618, 23);
			this.panelManagerCaption.TabIndex = 3;
			this.panelManagerCaption.Visible = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.labelManager);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(618, 23);
			this.panel1.TabIndex = 4;
			// 
			// labelManager
			// 
			this.labelManager.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.labelManager.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelManager.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.labelManager.Location = new System.Drawing.Point(0, 0);
			this.labelManager.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelManager.Name = "labelManager";
			this.labelManager.Size = new System.Drawing.Size(579, 23);
			this.labelManager.TabIndex = 3;
			this.labelManager.Text = "Manager";
			this.labelManager.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelManager.VisibleChanged += new System.EventHandler(this.labelManager_VisibleChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.labelManagerClose);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel2.Location = new System.Drawing.Point(579, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(39, 23);
			this.panel2.TabIndex = 4;
			// 
			// labelManagerClose
			// 
			this.labelManagerClose.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.labelManagerClose.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelManagerClose.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.labelManagerClose.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.CloseButton1;
			this.labelManagerClose.Location = new System.Drawing.Point(0, 0);
			this.labelManagerClose.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelManagerClose.Name = "labelManagerClose";
			this.labelManagerClose.Size = new System.Drawing.Size(39, 23);
			this.labelManagerClose.TabIndex = 4;
			this.labelManagerClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelManagerClose.MouseLeave += new System.EventHandler(this.labelManagerClose_MouseLeave);
			this.labelManagerClose.Click += new System.EventHandler(this.labelManagerClose_Click);
			this.labelManagerClose.MouseEnter += new System.EventHandler(this.labelManagerClose_MouseEnter);
			// 
			// Manager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelManagerCaption);
			this.Name = "Manager";
			this.Size = new System.Drawing.Size(618, 525);
			this.panelManagerCaption.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelManagerCaption;
		private System.Windows.Forms.Panel panel1;
		protected System.Windows.Forms.Label labelManager;
		private System.Windows.Forms.Panel panel2;
		protected System.Windows.Forms.Label labelManagerClose;
	}
}
