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
// Created: Monday, April 02, 2007 12:24:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpUtilities.Utility;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form used to edit and create path/file names.
	/// </summary>
	public partial class formPathNameInput 
		: Form
	{
		#region Variables.
		private string _invalidChars = string.Empty;								// Invalid character list.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the path name.
		/// </summary>
		public string PathName
		{
			get
			{
				return textName.Text;
			}
			set
			{
				textName.Text = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyPress event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs"/> instance containing the event data.</param>
		private void textName_KeyPress(object sender, KeyPressEventArgs e)
		{
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				if ((invalidChar == e.KeyChar) && (e.KeyChar != '\b') && (e.KeyChar != '\r') && (e.KeyChar != 27))
				{
					UI.WarningBox(this, e.KeyChar.ToString() + " is an invalid character.\nThe following characters are not allowed:\n" + _invalidChars);
					e.Handled = true;
				}
			}

			ValidateForm();

			// Cancel on ESC.
			if (e.KeyChar == 27)
				DialogResult = DialogResult.Cancel;

			// Enter will accept the input.
			if ((e.KeyChar == '\r') && (buttonOK.Enabled))
				DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Handles the TextChanged event of the textName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textName_TextChanged(object sender, EventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Function to validate the form & controls.
		/// </summary>
		private void ValidateForm()
		{
			buttonOK.Enabled = false;
			if (textName.Text != string.Empty)
				buttonOK.Enabled = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formPathNameInput()
		{
			InitializeComponent();

			// Get list of invalid characters.
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				if (Convert.ToInt32(invalidChar) > 32)
				{
					if (_invalidChars != string.Empty)
						_invalidChars += ", ";
					_invalidChars += invalidChar.ToString();
				}
			}
		}
		#endregion
	}
}