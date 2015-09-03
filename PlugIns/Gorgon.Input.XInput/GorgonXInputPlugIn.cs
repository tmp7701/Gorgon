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
// Created: Friday, July 15, 2011 6:22:48 AM
// 
#endregion

using Gorgon.Diagnostics;
using Gorgon.Input.XInput;
using Gorgon.Input.XInput.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// The entry point for the XInput plug-in.
	/// </summary>
	public class GorgonXInputPlugIn
		: GorgonInputServicePlugin
	{
		#region Properties.
		/// <summary>
		/// Property to return whether the plugin supports game devices like game pads, or joysticks.
		/// </summary>
		public override bool SupportsJoysticks => true;

		/// <summary>
		/// Property to return whether the plugin supports pointing devices like mice, trackballs, etc...
		/// </summary>
		public override bool SupportsMice => false;

		/// <summary>
		/// Property to return whether the plugin supports keyboard devices.
		/// </summary>
		public override bool SupportsKeyboards => false;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override GorgonInputService2 OnCreateInputService2(IGorgonLog log)
		{
			var coordinator = new XInputDeviceCoordinator();
			var registrar = new XInputDeviceRegistrar(log);

			// Unlike the other plugins, we don't need a processor because this thing only captures 
			// state via polling. We can just use our coordinator to determine if the window currently 
			// in focus is allowed to update state or not.

			return new GorgonXInputService(log, registrar, coordinator);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputPlugIn"/> class.
		/// </summary>
		public GorgonXInputPlugIn()
			: base(Resources.GORINP_XINP_SERVICEDESC)
		{
		}
		#endregion
	}
}
