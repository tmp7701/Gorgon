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
// Created: Tuesday, July 19, 2011 8:41:23 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The renderer interface.
	/// </summary>
	public abstract class GorgonRenderer
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _initialized = false;			// Flag to indicate that the renderer was initialized.
		private bool _disposed = false;				// Flag to indicate that the renderer was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that is bound to this renderer.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of custom settings for the renderer.
		/// </summary>
		public GorgonCustomValueCollection<string> CustomSettings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform shutdown for the renderer.
		/// </summary>
		private void Shutdown()
		{
			if (!_initialized)
				return;

			Gorgon.Log.Print("{0} shutting down...", Diagnostics.GorgonLoggingLevel.Simple, Name);

			ShutdownRenderer();
			Graphics = null; 
			_initialized = false;

			Gorgon.Log.Print("{0} shut down successfully", Diagnostics.GorgonLoggingLevel.Simple, Name);
		}

		/// <summary>
		/// Function to return a list of all video devices installed on the system.
		/// </summary>
		/// <returns>An enumerable list of video devices.</returns>
		protected abstract IEnumerable<GorgonVideoDevice> GetVideoDevices();

		/// <summary>
		/// Function to perform the actual initialization for the renderer from a plug-in.
		/// </summary>
		/// <remarks>Implementors must implement this method and perform any one-time set up for the renderer.</remarks>
		protected abstract void InitializeRenderer();

		/// <summary>
		/// Function to perform the actual shut down for the renderer from a plug-in.
		/// </summary>
		/// <remarks>Implementors must implement this method and perform any one-time tear down for the renderer.</remarks>
		protected abstract void ShutdownRenderer();

		/// <summary>
		/// Function to perform initialization for the renderer.
		/// </summary>
		/// <param name="owner">The graphics interface that owns this object.</param>
		internal void Initialize(GorgonGraphics owner)
		{
			Version minSMVersion = new Version(3, 0);		// Minimum shader model version.
			
			if (_initialized)
				Shutdown();

			Graphics = owner;

			Gorgon.Log.Print("{0} initializing...", Diagnostics.GorgonLoggingLevel.Simple, Name);
			InitializeRenderer();

			// TODO: Make GorgonGraphics abstract and then put device enumeration stuff in there.  
			//		 We shouldn't need to be doing all of this inside of the renderer, the renderer
			//		 should be for rendering and that's it.  Device enumeration sounds like a job
			//		 for the top level object.

			// Get a list of video devices.
			// TODO: Is there some way that we can defer the device enumeration to a method and use this to create just the interface needed to gather devices?
			//		 Required: Do not inherit and extend the collection.
			//       i.e.  devices = CreateDeviceList(); devices.Enumerate();
			var devices = GetVideoDevices();

			foreach (var device in devices)
				device.GetDeviceData();

			// Filter those that aren't supported by Gorgon (SM 3.0)
			devices = from device in devices
					  where (device.Capabilities.PixelShaderVersion >= minSMVersion) && (device.Capabilities.VertexShaderVersion >= minSMVersion)
					  select device;

			if (devices.Count() == 0)
				throw new GorgonException(GorgonResult.CannotEnumerate, "Cannot enumerate devices.  Could not find any applicable video device installed on the system.  Please ensure there is at least one video device that supports hardware acceleration and is capable of Shader Model 3.0 or better.");

			Graphics.VideoDevices = new GorgonVideoDeviceCollection(devices);

			_initialized = true;

			// Log device information.
			Gorgon.Log.Print("{0} video devices installed.", Diagnostics.GorgonLoggingLevel.Simple, devices.Count());
			foreach (GorgonVideoDevice device in devices)
			{
				Gorgon.Log.Print("Info for Video Device #{0} - {1}:", Diagnostics.GorgonLoggingLevel.Simple, device.Index, device.Name);
				Gorgon.Log.Print("\tHead count: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.Outputs.Count);
				Gorgon.Log.Print("\tDevice Name: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.DeviceName);
				Gorgon.Log.Print("\tDriver Name: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.DriverName);
				Gorgon.Log.Print("\tDriver Version: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.DriverVersion.ToString());
				Gorgon.Log.Print("\tRevision: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.Revision);
				Gorgon.Log.Print("\tDevice ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(device.DeviceID));
				Gorgon.Log.Print("\tSub System ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(device.SubSystemID));
				Gorgon.Log.Print("\tVendor ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(device.VendorID));
				Gorgon.Log.Print("\tDevice GUID: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.DeviceGUID);
				// TODO: Log device capabilities.
				// TODO: Log video modes.
			}

			Gorgon.Log.Print("{0} initialized successfully.", Diagnostics.GorgonLoggingLevel.Simple, Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderer"/> class.
		/// </summary>
		/// <param name="description">The description of the renderer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="description"/> parameter is NULL (or Nothing) in VB.NET.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="description"/> parameter is an empty string.</exception>
		protected GorgonRenderer(string description)
			: base(description)
		{
			CustomSettings = new GorgonCustomValueCollection<string>();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Shutdown();						
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}