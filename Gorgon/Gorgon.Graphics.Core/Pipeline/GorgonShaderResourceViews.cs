﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 4, 2016 1:05:13 AM
// 
#endregion

using System;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of shader resource views to apply to the pipeline.
	/// </summary>
	public sealed class GorgonShaderResourceViews
		: GorgonMonitoredArray<GorgonShaderResourceView>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed shader resources that can be bound at the same time.
		/// </summary>
		public const int MaximumShaderResourceViewCount = 32;
		#endregion

		#region Variables.
		// The native resource bindings.
		private readonly D3D11.ShaderResourceView[] _native = new D3D11.ShaderResourceView[MaximumShaderResourceViewCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the native shader resource views.
		/// </summary>
		internal D3D11.ShaderResourceView[] Native => _native;
		#endregion

		#region Methods.
#if DEBUG
		/// <summary>
		/// Function to validate an item being assigned to a slot.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="view">The view to validate.</param>
		protected override void OnValidate(int index, GorgonShaderResourceView view)
		{
			if (view == null)
			{
				return;
			}

			GorgonShaderResourceView startView = this.FirstOrDefault(item => item != null);

			// If no other views are assigned, then leave.
			if (startView == null)
			{
				return;
			}

			// Only check if we have more than 1 shader resource view being applied.
			for (int i = 0; i < Count; i++)
			{
				var other = this[i] as GorgonTextureShaderView;

				if (other == null)
				{
					continue;
				}

				if ((other == view) && (i != index))
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_VIEW_ALREADY_BOUND, other.Texture.Name));
				}
			}
		}
#endif

		/// <summary>
		/// Function to clear the list of native binding objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementing class must implement this in order to unassign items from the native binding object list when the <see cref="GorgonMonitoredArray{T}.Clear"/> method is called.
		/// </para>
		/// </remarks>
		protected override void OnClear()
		{
			Array.Clear(_native, 0, MaximumShaderResourceViewCount);
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		protected override void OnItemSet(int index, GorgonShaderResourceView item)
		{
			_native[index] = item.D3DView;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderResourceViews"/> class.
		/// </summary>
		public GorgonShaderResourceViews()
			: base(MaximumShaderResourceViewCount)
		{
		}
		#endregion
	}
}
