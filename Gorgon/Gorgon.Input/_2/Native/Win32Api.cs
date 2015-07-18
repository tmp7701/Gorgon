﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Saturday, July 18, 2015 4:41:30 PM
// 
#endregion

using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Input.Native
{
	/// <summary>
	/// Win32 API function calls.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	static class Win32Api
	{
		/// <summary>
		/// Converts a virtual key code into a unicode character representation.
		/// </summary>
		/// <param name="keyCode">Key code</param>
		/// <param name="scanCode">Scan code</param>
		/// <param name="keyboardState">State</param>
		/// <param name="buffer">Buffer to populate</param>
		/// <param name="bufferSize">Size of the buffer, in bytes.</param>
		/// <param name="flags">Flags to pass.</param>
		/// <returns>The return code for the method.</returns>
		/// <remarks>
		/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646320(v=vs.85).aspx"/> for more info.
		/// </remarks>
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int ToUnicode(uint keyCode,
		                                    uint scanCode,
		                                    byte[] keyboardState,
		                                    [Out] [MarshalAs(UnmanagedType.LPArray)] char[] buffer,
		                                    int bufferSize,
		                                    uint flags);
	}
}
