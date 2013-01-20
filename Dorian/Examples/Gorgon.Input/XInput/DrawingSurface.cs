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
// Created: Saturday, January 12, 2013 4:44:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Examples
{
	/// <summary>
	/// The drawing surface for our joystick.
	/// </summary>
	class DrawingSurface
		: IDisposable
	{
		#region Variables.
		private Control _control = null;						// Control we're drawing on.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private Graphics _controlGraphics = null;				// Graphics interface for the control.
		private Graphics _surfaceGraphics = null;				// Graphics interface for the surface.
		private Image _sufaceBuffer = null;						// Buffer for the surface.
		private Image _drawing = null;							// Image that will contain the drawing.
		private Graphics _imageGraphics = null;					// Graphics interface for the drawing image.
		private BufferedGraphicsContext _context = null;		// Buffered context.
		private BufferedGraphics _buffer = null;				// Buffered graphics.
		private float _cursorFlash = 256.0f;					// Cursor flash direction.
		private float _cursorTint = 0.0f;						// Cursor tinting.
		private Image _cursor = null;							// Cursor image.
		private ColorMatrix _colorMatrix = null;				// Color matrix.
		private ImageAttributes _cursorAttribs = null;			// Cursor image attributes.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the cursor size.
		/// </summary>
		public Size CursorSize
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Resize event of the surfaceControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void surfaceControl_Resize(object sender, EventArgs e)
		{
			Image tempImage = null;
			Form parentForm = _control.FindForm();

			if (parentForm.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			if (_imageGraphics != null)
			{
				_imageGraphics.Dispose();
				_imageGraphics = null;
			}

			// Copy the old image into the new buffer.
			tempImage = new Bitmap(_control.ClientSize.Width, _control.ClientSize.Height, _drawing.PixelFormat);
			_imageGraphics = Graphics.FromImage(tempImage);
			if (_drawing != null)
			{								
				_imageGraphics.DrawImage(_drawing, Point.Empty);

				_drawing.Dispose();				
			}
			_drawing = tempImage;

			GetResources(false);
		}

		/// <summary>
		/// Function to perform clean up on the objects within this object.
		/// </summary>
		/// <param name="clearDrawing">TRUE to destroy the drawing image, FALSE to leave alone.</param>
		private void CleanUp(bool clearDrawing)
		{
			if (_buffer != null)
			{
				_buffer.Dispose();
				_buffer = null;
			}

			if (_context != null)
			{
				_context.Dispose();
				_context = null;
			}

			if (_surfaceGraphics != null)
			{
				_surfaceGraphics.Dispose();
				_surfaceGraphics = null;
			}

			if (_controlGraphics != null)
			{
				_controlGraphics.Dispose();
				_controlGraphics = null;
			}

			if (_sufaceBuffer != null)
			{
				_sufaceBuffer.Dispose();
				_sufaceBuffer = null;
			}

			if (clearDrawing)
			{
				if (_imageGraphics != null)
				{
					_imageGraphics.Dispose();
					_imageGraphics = null;
				}

				if (_drawing != null)
				{
					_drawing.Dispose();
					_drawing = null;
				}
			}
		}

		/// <summary>
		/// Function to allocate resources for our graphics.
		/// </summary>
		/// <param name="clearDrawing">TRUE to clear the drawing, FALSE to leave alone.</param>
		private void GetResources(bool clearDrawing)
		{
			CleanUp(clearDrawing);

			_sufaceBuffer = new Bitmap(_control.ClientSize.Width, _control.ClientSize.Height, PixelFormat.Format32bppArgb);
			_controlGraphics = Graphics.FromHwnd(_control.Handle);
			_surfaceGraphics = Graphics.FromImage(_sufaceBuffer);
			
			_context = BufferedGraphicsManager.Current;
			_buffer = _context.Allocate(_surfaceGraphics, _control.ClientRectangle);

			if (clearDrawing)
			{
				_drawing = new Bitmap(_control.ClientSize.Width, _control.ClientSize.Height, PixelFormat.Format32bppArgb);
				_imageGraphics = Graphics.FromImage(_drawing);
			}
		}

		/// <summary>
		/// Function to clear the drawing.
		/// </summary>
		public void ClearDrawing()
		{
			_imageGraphics.Clear(Color.Transparent);
		}

		/// <summary>
		/// Function to clear the control surface.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		public void Clear(Color color)
		{
			_buffer.Graphics.Clear(color);
			_buffer.Graphics.DrawImage(_drawing, Point.Empty);
		}

		/// <summary>
		/// Function to draw a point on the screen.
		/// </summary>
		/// <param name="position">Position to draw at.</param>
		/// <param name="color">Color of the point.</param>
		/// <param name="size">Size of the point.</param>
		public void DrawPoint(Point position, Color color, float size)
		{
			using (var brush = new SolidBrush(color))
			{
				float halfSize = size / 2.0f;
				_imageGraphics.FillEllipse(brush, new Rectangle(position.X - (int)halfSize, position.Y - (int)halfSize, (int)size, (int)size));
			}
		}

		/// <summary>
		/// Function to draw the cursor image on the surface.
		/// </summary>
		/// <param name="position">Position to draw the cursor image at.</param>
		/// <param name="color">Color to use.</param>
		public void DrawCursor(Point position, Color color)
		{			
			position = new Point(position.X - CursorSize.Width / 2,
									position.Y - CursorSize.Height / 2);

			_cursorTint += GorgonTiming.Delta * (_cursorFlash / 256.0f);

			if ((_cursorTint > 1.0f) || (_cursorTint < 0.0f))
			{
				if (_cursorTint < 0.0f)
				{
					_cursorTint = 0.0f;
				}

				if (_cursorTint > 1.0f)
				{
					_cursorTint = 1.0f;
				}

				_cursorFlash *= -1.0f;
			}

			_colorMatrix.Matrix00 = 1.0f;
			_colorMatrix.Matrix11 = 1.0f;
			_colorMatrix.Matrix22 = 1.0f;
			_colorMatrix.Matrix33 = 1.0f;
			_colorMatrix.Matrix44 = 1.0f;
			_colorMatrix.Matrix40 = (color.R * _cursorTint) / 255.0f;
			_colorMatrix.Matrix41 = (color.G * _cursorTint) / 255.0f;
			_colorMatrix.Matrix42 = (color.B * _cursorTint) / 255.0f;

			_cursorAttribs.SetColorMatrix(_colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			// Draw the cursor image.			
			_buffer.Graphics.DrawImage(_cursor, new Rectangle(position, CursorSize), 0, 0, CursorSize.Width, CursorSize.Height, GraphicsUnit.Pixel, _cursorAttribs);
		}

		/// <summary>
		/// Function to render the graphics interface.
		/// </summary>
		public void Render()
		{
			_buffer.Render();
			_buffer.Render(_controlGraphics);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DrawingSurface" /> class.
		/// </summary>
		/// <param name="surfaceControl">The control that contains the surface to draw on.</param>
		public DrawingSurface(Control surfaceControl)
		{
			_colorMatrix = new ColorMatrix();
			_cursorAttribs = new ImageAttributes();			
			_cursor = Properties.Resources.device_gamepad_48x48;
			CursorSize = new Size(_cursor.Width, _cursor.Height);
			_control = surfaceControl;
			_control.Resize += surfaceControl_Resize;
			GetResources(true);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{					
					_control.Resize -= surfaceControl_Resize;

					if (_cursorAttribs != null)
					{
						_cursorAttribs.Dispose();
						_cursorAttribs = null;
					}

					CleanUp(true);
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}