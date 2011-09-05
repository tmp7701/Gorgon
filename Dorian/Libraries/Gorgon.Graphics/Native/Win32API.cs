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
// Created: Sunday, July 24, 2011 10:15:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Native
{
	/// <summary>
	/// Win 32 API function calls.
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurity()]
	static class Win32API
	{
		/// <summary>
		/// Function to retrieve the nearest monitor to the window.
		/// </summary>
		/// <param name="hwnd">Handle to the window.</param>
		/// <param name="flags">Flags to pass in.</param>
		/// <returns></returns>
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorFlags flags);

		/// <summary>
		/// Function to retrieve the monitor with the largest portion of the window inside of it.
		/// </summary>
		/// <param name="window">Window to locate.</param>
		/// <returns>The handle to the monitor.</returns>
		public static IntPtr GetMonitor(Control window)
		{
			return MonitorFromWindow(window.Handle, MonitorFlags.MONITOR_DEFAULTTOPRIMARY);
		}
	}
}