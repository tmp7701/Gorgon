﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Saturday, March 2, 2013 12:47:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Data passed from the editor to the plug-in.
	/// </summary>
	public struct EditorPlugInData
	{
		/// <summary>
		/// The panel that contains the main content for the application.
		/// </summary>
		public readonly System.Windows.Forms.Panel ContentPanel;
		/// <summary>
		/// The main menu for the application.
		/// </summary>
		public readonly System.Windows.Forms.MenuStrip MainMenu;
		/// <summary>
		/// The graphics interface for the application.
		/// </summary>
		public readonly GorgonGraphics Graphics;
		/// <summary>
		/// The renderer interface for the application.
		/// </summary>
		public readonly Gorgon2D Renderer;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditorPlugInData"/> struct.
		/// </summary>
		/// <param name="contentPanel">The panel that contains the main content for the application.</param>
		/// <param name="mainMenu">The main menu for the application.</param>
		/// <param name="graphics">The graphics interface for the application.</param>
		/// <param name="renderer">The renderer for the application.</param>
		internal EditorPlugInData(System.Windows.Forms.Panel contentPanel, System.Windows.Forms.MenuStrip mainMenu, GorgonGraphics graphics, Gorgon2D renderer)
		{
			ContentPanel = contentPanel;
			MainMenu = mainMenu;
			Graphics = graphics;
			Renderer = renderer;
		}
	}

	/// <summary>
	/// An interface for editor plug-ins.
	/// </summary>
	public abstract class EditorPlugIn
		: GorgonPlugIn
	{
		#region Methods.
		/// <summary>
		/// Function to create the plug-in interface.
		/// </summary>
		/// <param name="editorObjects">Editor interfaces for the plug-in to manipulate.</param>
		protected internal abstract void CreateInterface(EditorPlugInData editorObjects);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>
		/// Objects that implement this base class should pass in a hard coded description on the base constructor.
		/// </remarks>
		protected EditorPlugIn(string description)
			: base(description)
		{
		}
		#endregion
	}
}