#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, June 24, 2011 10:04:07 AM
// 
#endregion

using System;
using System.Drawing;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Pointing device event arguments.
	/// </summary>
	public class PointingDeviceEventArgs
		: EventArgs
	{
		#region Variables.
		private readonly PointingDeviceButtons _button;			// Buttons that are pressed while the mouse is being moved.
		private readonly PointingDeviceButtons _shiftButtons;	// Other buttons being held down.
		private readonly PointF _position;				        // Mouse position.
		private readonly int _wheelPosition;				    // Wheel position.
		private readonly PointF _relative;				        // Relative mouse position.
		private readonly int _wheelDelta;				        // Wheel delta.
		private readonly int _clickCount;				        // Number of clicks in a timed period.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return buttons that were pressed during mouse movement.
		/// </summary>
		public PointingDeviceButtons Buttons
		{
			get
			{
				return _button;
			}
		}

		/// <summary>
		/// Property to return the buttons that are being held down during the event.
		/// </summary>
		public PointingDeviceButtons ShiftButtons
		{
			get
			{
				return _shiftButtons;
			}
		}

		/// <summary>
		/// Property to return the position of the mouse.
		/// </summary>
		public PointF Position
		{
			get
			{
				return _position;
			}
		}

		/// <summary>
		/// Property to return the wheel position.
		/// </summary>
		public int WheelPosition
		{
			get
			{
				return _wheelPosition;
			}
		}

		/// <summary>
		/// Property to return the amount that the mouse has moved since it last moved.
		/// </summary>
		public PointF RelativePosition
		{
			get
			{
				return _relative;
			}
		}

		/// <summary>
		/// Property to return the amount that the wheel has moved since the last update.
		/// </summary>
		public int WheelDelta
		{
			get
			{
				return _wheelDelta;
			}
		}

		/// <summary>
		/// Property to return if we've double clicked.
		/// </summary>
		public bool DoubleClick
		{
			get
			{
				return (_clickCount > 1);
			}
		}

		/// <summary>
		/// Property to return the number of full clicks.
		/// </summary>
		public int ClickCount
		{
			get
			{
				return _clickCount;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="buttons">Buttons that are pressed during mouse event.</param>
		/// <param name="shiftButtons">Buttons that are held down during the mouse evnet.</param>
		/// <param name="position">Position of the mouse.</param>
		/// <param name="wheelPosition">Position of the wheel.</param>
		/// <param name="relativePosition">Relative position of the mouse.</param>
		/// <param name="wheelDelta">Relative position of the wheel.</param>
		/// <param name="clickCount">Number of clicks in a timed period.</param>
		public PointingDeviceEventArgs(PointingDeviceButtons buttons, PointingDeviceButtons shiftButtons, PointF position, int wheelPosition, PointF relativePosition, int wheelDelta, int clickCount)
		{
			_button = buttons;
			_shiftButtons = shiftButtons;
			_position = position;
			_wheelPosition = wheelPosition;
			_relative = relativePosition;
			_wheelDelta = wheelDelta;
			_clickCount = clickCount;
		}
		#endregion
	}

	/// <summary>
	/// Keyboard event arguments.
	/// </summary>
	public class KeyboardEventArgs
		: EventArgs
	{
		#region Variables.
		private readonly KeyboardKeys _key;						// Key that is pressed.
		private readonly KeyboardKeys _modifierKey;				// Other keys being held down.
		private readonly int _scan;							    // Scan code information.
		private readonly GorgonKeyboard.KeyCharMap _character;	// Character that the key represents.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the character that the key represents.
		/// </summary>
		public GorgonKeyboard.KeyCharMap CharacterMapping
		{
			get
			{
				return _character;
			}
		}

		/// <summary>
		/// Property to return key that is pressed.
		/// </summary>
		public KeyboardKeys Key
		{
			get
			{
				return _key;
			}
		}

		/// <summary>
		/// Property to return the keys that are being held down during the event.
		/// </summary>
		public KeyboardKeys ModifierKeys
		{
			get
			{
				return _modifierKey;
			}
		}

		/// <summary>
		/// Property to return if ALT is pressed or not.
		/// </summary>
		public bool Alt
		{
			get
			{
				return (_modifierKey & KeyboardKeys.Alt) == KeyboardKeys.Alt;
			}
		}

		/// <summary>
		/// Property to return if Ctrl is pressed or not.
		/// </summary>
		public bool Ctrl
		{
			get
			{
				return (_modifierKey & KeyboardKeys.Control) == KeyboardKeys.Control;
			}
		}

		/// <summary>
		/// Property to return if Shift is pressed or not.
		/// </summary>
		public bool Shift
		{
			get
			{
				return (_modifierKey & KeyboardKeys.Shift) == KeyboardKeys.Shift;
			}
		}

		/// <summary>
		/// Property to return the scan code data.
		/// </summary>
		public int ScanCodeData
		{
			get
			{
				return _scan;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="key">Key that is pressed.</param>
		/// <param name="modifierKey">Keys that are held down during the event.</param>
		/// <param name="character">Character that the key represents.</param>
		/// <param name="scanData">Scan code data.</param>
		public KeyboardEventArgs(KeyboardKeys key, KeyboardKeys modifierKey, GorgonKeyboard.KeyCharMap character, int scanData)
		{
			_key = key;
			_modifierKey = modifierKey;
			_character = character;
			_scan = scanData;
		}
		#endregion
	}
}