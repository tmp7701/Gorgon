﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 14, 2017 11:28:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core.Pipeline
{
	/// <summary>
	/// A native binding for a shader resource view.
	/// </summary>
	struct NativeSrvBinding
		: IEquatable<NativeSrvBinding>
	{
		/// <summary>
		/// The shader resource views to bind.
		/// </summary>
		public D3D11.ShaderResourceView[] Srvs;
		/// <summary>
		/// The starting slot to bind.
		/// </summary>
		public int StartSlot;
		/// <summary>
		/// The number of slots to bind.
		/// </summary>
		public int Count;

		/// <summary>
		/// Equalses the specified binding.
		/// </summary>
		/// <param name="binding">The binding.</param>
		/// <returns><b>true</b> if XXXX, <b>false</b> otherwise.</returns>
		public bool Equals(NativeSrvBinding binding)
		{
			return binding.Srvs == Srvs
			       && binding.StartSlot == StartSlot
			       && binding.Count == Count;
		}
	}
}
