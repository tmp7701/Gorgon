﻿#region MIT.
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
// Created: Friday, June 24, 2011 10:04:46 AM
// 
#endregion

using System;
using System.Windows.Forms;
using GorgonLibrary.Input.Raw.Properties;
using GorgonLibrary.Native;

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Object representing a message loop filter.
	/// </summary>
	internal class MessageFilter
		: IMessageFilter
	{
		#region Events.
		/// <summary>
		/// Event fired when a raw input keyboard event occours.
		/// </summary>
		public event EventHandler<RawInputKeyboardEventArgs> RawInputKeyboardData = null;
		/// <summary>
		/// Event fired when a pointing device event occurs.
		/// </summary>
		public event EventHandler<RawInputPointingDeviceEventArgs> RawInputPointingDeviceData = null;
		/// <summary>
		/// Event fired when an HID event occurs.
		/// </summary>
		public event EventHandler<RawInputHIDEventArgs> RawInputHIDData = null;
		#endregion

		#region Variables.
		private readonly int _headerSize = DirectAccess.SizeOf<RAWINPUTHEADER>();	// Size of the input data in bytes.
		#endregion

		#region IMessageFilter Members
	    /// <summary>
	    /// Filters out a message before it is dispatched.
	    /// </summary>
	    /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
	    /// <returns>
	    /// true to filter the message and stop it from being dispatched; false to allow the message to continue to the next filter or control.
	    /// </returns>
	    public unsafe bool PreFilterMessage(ref Message m)
	    {
			int dataSize = 0;

	        // Get data size.			
	        int result = Win32API.GetRawInputData(m.LParam, RawInputCommand.Input, IntPtr.Zero, ref dataSize, _headerSize);

	        if (result == -1)
	        {
	            throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_CANNOT_READ_DATA);
	        }

	        // Get actual data.
	        var rawInputPtr = stackalloc byte[dataSize];
	        result = Win32API.GetRawInputData(m.LParam,
	                                          RawInputCommand.Input,
	                                          (IntPtr)rawInputPtr,
	                                          ref dataSize,
	                                          _headerSize);

	        if ((result == -1)
	            || (result != dataSize))
	        {
	            throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_CANNOT_READ_DATA);
	        }

	        var rawInput = *((RAWINPUT*)rawInputPtr);

	        switch (rawInput.Header.Type)
	        {
	            case RawInputType.Mouse:
	                if (RawInputPointingDeviceData != null) 
	                {
	                    RawInputPointingDeviceData(this,
	                                               new RawInputPointingDeviceEventArgs(rawInput.Header.Device,
	                                                                                   ref rawInput.Union.Mouse));
	                }
	                break;
	            case RawInputType.Keyboard:
	                if (RawInputKeyboardData != null)
	                {
	                    RawInputKeyboardData(this,
	                                         new RawInputKeyboardEventArgs(rawInput.Header.Device,
	                                                                       ref rawInput.Union.Keyboard));
	                }
	                break;
	            default:
	                if (RawInputHIDData != null)
	                {
	                    var HIDData = new byte[rawInput.Union.HID.Size * rawInput.Union.HID.Count];
	                    var hidDataPtr = rawInputPtr + _headerSize + 8;

	                    fixed(byte* buffer = &HIDData[0])
	                    {
	                        DirectAccess.MemoryCopy(buffer, hidDataPtr, HIDData.Length);
	                    }

	                    RawInputHIDData(this,
	                                    new RawInputHIDEventArgs(rawInput.Header.Device, ref rawInput.Union.HID, HIDData));
	                }
	                break;
	        }

	        return false;
	    }

	    #endregion
	}
}
