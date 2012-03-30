﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, February 22, 2012 4:36:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Extensions to the main graphics interface.
	/// </summary>
	public static class GorgonGraphics2DExtensions
	{
		#region Methods.
		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="window">Window to use for rendering.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <remarks>This method creates an internal swap chain and uses that for the display.  To have more control over the initial render target, use the <see cref="M:GorgonLibrary.Graphics.GorgonGraphics2DExtensions.Create2DRenderer(GorgonLibrary.Graphics.GorgonGraphics, GorgonLibrary.Graphics.GorgonRenderTarget)">Create2DRenderer(GorgonRenderTarget)</see> extension overload.
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public static Gorgon2D Create2DRenderer(this GorgonGraphics graphics, Control window)
		{
			if (window == null)
				window = Gorgon.ApplicationForm;

			GorgonDebug.AssertNull<Control>(window, "window");

			return Create2DRenderer(graphics, window, window.ClientSize.Width, window.ClientSize.Height, BufferFormat.Unknown, true, BufferFormat.Unknown);
		}

		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="window">Window to use for rendering.</param>
		/// <param name="width">Width of the video mode used for rendering.</param>
		/// <param name="height">Height of the video mode used for rendering.</param>
		/// <param name="format">Format of the video mode used for rendering.</param>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to to use full screen mode.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <remarks>This method creates an internal swap chain and uses that for the display.  To have more control over the initial render target, use the <see cref="M:GorgonLibrary.Graphics.GorgonGraphics2DExtensions.Create2DRenderer(GorgonLibrary.Graphics.GorgonGraphics, GorgonLibrary.Graphics.GorgonRenderTarget)">Create2DRenderer(GorgonRenderTarget)</see> extension overload.
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="format"/> parameter cannot be used by the video device for displaying data or for the depth/stencil buffer.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public static Gorgon2D Create2DRenderer(this GorgonGraphics graphics, Control window, int width, int height, BufferFormat format, bool isWindowed)
		{
			return Create2DRenderer(graphics, window, width, height, format, isWindowed, BufferFormat.Unknown);
		}

		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="window">Window to use for rendering.</param>
		/// <param name="width">Width of the video mode used for rendering.</param>
		/// <param name="height">Height of the video mode used for rendering.</param>
		/// <param name="format">Format of the video mode used for rendering.</param>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to to use full screen mode.</param>
		/// <param name="depthStencilFormat">Depth/stencil buffer format.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <remarks>This method creates an internal swap chain and uses that for the display.  To have more control over the initial render target, use the <see cref="M:GorgonLibrary.Graphics.GorgonGraphics2DExtensions.Create2DRenderer(GorgonLibrary.Graphics.GorgonGraphics, GorgonLibrary.Graphics.GorgonRenderTarget)">Create2DRenderer(GorgonRenderTarget)</see> extension overload.
		/// <para>The depth/stencil buffer is optional, and will only be used when <paramref name="depthStencilFormat"/> is not set to Unknown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="format"/> parameter cannot be used by the video device for displaying data or for the depth/stencil buffer.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public static Gorgon2D Create2DRenderer(this GorgonGraphics graphics, Control window, int width, int height, BufferFormat format, bool isWindowed, BufferFormat depthStencilFormat)
		{
			GorgonSwapChain swapChain = null;

			swapChain = graphics.Output.CreateSwapChain("Gorgon2D.DefaultSwapChain", new GorgonSwapChainSettings()
			{
				BufferCount = 2,
				DepthStencilFormat = depthStencilFormat,
				Flags = SwapChainUsageFlags.RenderTarget,
				Format = format,
				Height = height,
				IsWindowed = isWindowed,
				MultiSample = new GorgonMultisampling(1, 0),
				SwapEffect = SwapEffect.Discard,
				Width = width,
				Window = window
			});

			return Create2DRenderer(graphics, swapChain);
		}

		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="target">Default target for the renderer.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.</exception>
		public static Gorgon2D Create2DRenderer(this GorgonGraphics graphics, GorgonRenderTarget target)
		{
			Gorgon2D result = null;

			GorgonDebug.AssertNull<GorgonRenderTarget>(target, "target");

#if DEBUG
			if (target.Graphics != graphics)
				throw new ArgumentException("The target '" + target.Name + "' was not created by the same graphics interface as this one.", "target");
#endif

			result = new Gorgon2D(target);
			graphics.AddTrackedObject(result);

			return result;
		}
		#endregion
	}
}