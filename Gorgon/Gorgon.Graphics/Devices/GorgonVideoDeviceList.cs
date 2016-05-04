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
// Created: Saturday, February 23, 2013 4:00:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3DCommon = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using System.Collections;
using System.Threading;
using Gorgon.Collections;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Functionality to retrieve information about the installed video devices on the system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this to retrieve a list of video devices available on the system. A video device may be a discreet video card, or a device on the motherboard.
	/// </para>
	/// <para>
	/// This list will contain <see cref="GorgonVideoDeviceInfo"/> objects which can then be passed to a <see cref="GorgonGraphics"/> instance. This allows applications or users to pick and choose which device 
	/// they wish to use.
	/// </para>
	/// <para>
	/// This list will also allow enumeration of the WARP/Reference devices.  WARP is a high performance software device that will emulate much of the functionality that a real video device would have. The 
	/// reference device is a fully featured device for debugging driver issues.
	/// </para>
	/// <para>
	/// <note type="caution">
	/// <para>
	/// Since the reference device is primarily a diagnostic object, it will have very poor performance. Please use this only for diagnostic purposes.
	/// </para>
	/// <para>
	/// Please note that the reference is device is only available when the Windows SDK is installed and Gorgon is compiled in DEBUG mode, the application will crash if SDK is not installed or Gorgon is compiled 
	/// as in RELEASE mode.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	public class GorgonVideoDeviceList
		: IGorgonVideoDeviceList
	{
		#region Variables.
		// The backing store for the device list.
		private List<GorgonVideoDeviceInfo> _devices = new List<GorgonVideoDeviceInfo>();
		// Log used for debugging info.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the keys are case sensitive.
		/// </summary>
		bool IGorgonNamedObjectReadOnlyList<GorgonVideoDeviceInfo>.KeysAreCaseSensitive => true;

		/// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements in the collection. 
		/// </returns>
		public int Count => _devices.Count;

		/// <summary>
		/// Gets the element at the specified index in the read-only list.
		/// </summary>
		/// <returns>
		/// The element at the specified index in the read-only list.
		/// </returns>
		/// <param name="index">The zero-based index of the element to get. </param>
		public GorgonVideoDeviceInfo this[int index] => _devices[index];

		/// <summary>
		/// Property to return an item in this list by its name.
		/// </summary>
		public GorgonVideoDeviceInfo this[string name]
		{
			get
			{
				int index = IndexOf(name);

				if (index == -1)
				{
					throw new KeyNotFoundException(string.Format(Resources.GORGFX_ERR_NO_VIDEO_DEVICE_WITH_NAME, name));
				}

				return _devices[index];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add the WARP software device.
		/// </summary>
		/// <param name="index">Index of the device.</param>
		/// <returns>The video device used for WARP software rendering.</returns>
		private GorgonVideoDeviceInfo GetWARPSoftwareDevice(int index)
		{
			D3DCommon.FeatureLevel[] featureLevels =
			{
				D3DCommon.FeatureLevel.Level_11_0, 
				D3DCommon.FeatureLevel.Level_10_1, 
				D3DCommon.FeatureLevel.Level_11_0, 
			};
			D3D11.DeviceCreationFlags flags = D3D11.DeviceCreationFlags.None;

			if (GorgonGraphics.IsDebugEnabled)
			{
				flags = D3D11.DeviceCreationFlags.Debug;
			}

			using (var D3DDevice = new D3D11.Device(D3DCommon.DriverType.Warp, flags, featureLevels))
			{
                using (var giDevice = D3DDevice.QueryInterface<DXGI.Device1>())
				{
					using (var adapter = giDevice.GetParent<DXGI.Adapter1>())
					{
						DeviceFeatureLevel featureLevel = GetFeatureLevel(D3DDevice);

						if (featureLevel == DeviceFeatureLevel.Unsupported)
						{
							return null;
						}

						var result =  new GorgonVideoDeviceInfo(index, adapter, featureLevel, new GorgonNamedObjectList<GorgonVideoOutputInfo>(), VideoDeviceType.Software);

						PrintLog(result);

						return result;
					}
				}
			}
		}

#if DEBUG
		/// <summary>
		/// Function to add the reference device.
		/// </summary>
		/// <param name="index">Index of the device.</param>
        /// <returns>The video device used for reference software rendering.</returns>
		private GorgonVideoDeviceInfo GetRefSoftwareDevice(int index)
		{
			using (var D3DDevice = new D3D11.Device(D3DCommon.DriverType.Reference, D3D11.DeviceCreationFlags.Debug))
			{
				using (var giDevice = D3DDevice.QueryInterface<DXGI.Device1>())
                {
                    using (var adapter = giDevice.GetParent<DXGI.Adapter1>())
					{
						DeviceFeatureLevel featureLevel = GetFeatureLevel(D3DDevice);

						if (featureLevel == DeviceFeatureLevel.Unsupported)
						{
							return null;
						}

						var result = new GorgonVideoDeviceInfo(index, adapter, featureLevel, new GorgonNamedObjectList<GorgonVideoOutputInfo>(), VideoDeviceType.ReferenceRasterizer);
						PrintLog(result);
						return result;
					}
				}
			}
		}
#endif

		/// <summary>
		/// Function to print device log information.
		/// </summary>
		/// <param name="device">Device to print.</param>
		private void PrintLog(GorgonVideoDeviceInfo device)
		{
			_log.Print($"Device found: {device.Name}", LoggingLevel.Simple);
			_log.Print("===================================================================", LoggingLevel.Simple);
			_log.Print($"Supported feature level: {device.SupportedFeatureLevel}", LoggingLevel.Simple);
			_log.Print($"Video memory: {device.DedicatedVideoMemory.FormatMemory()}", LoggingLevel.Simple);
			_log.Print($"System memory: {device.DedicatedSystemMemory.FormatMemory()}", LoggingLevel.Intermediate);
			_log.Print($"Shared memory: {device.SharedSystemMemory.FormatMemory()}", LoggingLevel.Intermediate);
			_log.Print($"Device ID: 0x{device.DeviceID.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Sub-system ID: 0x{device.SubSystemID.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Vendor ID: 0x{device.VendorID.FormatHex()}", LoggingLevel.Verbose);
			_log.Print($"Revision: {device.Revision}", LoggingLevel.Verbose);
			_log.Print($"Unique ID: 0x{device.Luid.FormatHex()}", LoggingLevel.Verbose);
			_log.Print("===================================================================", LoggingLevel.Simple);

			foreach (GorgonVideoOutputInfo output in device.Outputs)
			{
				_log.Print($"Found output '{output.Name}'.", LoggingLevel.Simple);
				_log.Print("===================================================================", LoggingLevel.Verbose);
				_log.Print($"Output bounds: ({output.DesktopBounds.Left}x{output.DesktopBounds.Top})-({output.DesktopBounds.Right}x{output.DesktopBounds.Bottom})",
						   LoggingLevel.Verbose);
				_log.Print($"Monitor handle: 0x{output.MonitorHandle.FormatHex()}", LoggingLevel.Verbose);
				_log.Print($"Attached to desktop: {output.IsAttachedToDesktop}", LoggingLevel.Verbose);
				_log.Print($"Monitor rotation: {output.Rotation}", LoggingLevel.Verbose);
				_log.Print("===================================================================", LoggingLevel.Simple);

				_log.Print($"Retrieving video modes for output '{output.Name}'...", LoggingLevel.Simple);
				_log.Print("===================================================================", LoggingLevel.Simple);

				foreach (GorgonVideoMode mode in output.VideoModes)
				{
					_log.Print($"Size: {mode.Width}x{mode.Height}, Format: {mode.Format}, Refresh Rate: {mode.RefreshRate.Numerator}/{mode.RefreshRate.Denominator}, " +
							   $"Scaling: {mode.Scaling}, Scanline order: {mode.ScanlineOrdering}",
							   LoggingLevel.Verbose);
				}

				_log.Print("===================================================================", LoggingLevel.Verbose);
				_log.Print($"Found {output.VideoModes.Count} video modes for output '{output.Name}'.", LoggingLevel.Simple);
				_log.Print("===================================================================", LoggingLevel.Simple);
			}
		}

		/// <summary>
		/// Function to retrieve the video modes for an output.
		/// </summary>
		/// <param name="D3DDevice">D3D device for filtering supported display modes.</param>
		/// <param name="giOutput">Output that contains the video modes.</param>
		/// <returns>A list of display compatible full screen video modes.</returns>
		private static IReadOnlyList<GorgonVideoMode> GetVideoModes(D3D11.Device D3DDevice, DXGI.Output giOutput)
		{
			var formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));

			var result = new List<GorgonVideoMode>();

			// Test each format for display compatibility.
			foreach (var format in formats)
			{
				var giFormat = (DXGI.Format)format;
				DXGI.ModeDescription[] modes = giOutput.GetDisplayModeList(giFormat, DXGI.DisplayModeEnumerationFlags.Scaling | DXGI.DisplayModeEnumerationFlags.Interlaced);

				if ((modes == null) || (modes.Length <= 0))
				{
					continue;
				}

				IEnumerable<GorgonVideoMode> videoModes = (from mode in modes
				                                           where (D3DDevice.CheckFormatSupport(giFormat) & D3D11.FormatSupport.Display) == D3D11.FormatSupport.Display
				                                           select new GorgonVideoMode(mode)).ToArray();


				result.AddRange(videoModes);
			}

			return result;
		}

		/// <summary>
		/// Function to retrieve the highest feature level for a video device.
		/// </summary>
		/// <param name="device">The D3D device to use.</param>
		/// <returns>The highest available feature level for the device.</returns>
		private DeviceFeatureLevel GetFeatureLevel(D3D11.Device device)
		{
			D3DCommon.FeatureLevel result = device.FeatureLevel;

			if (Enum.IsDefined(typeof(DeviceFeatureLevel), (int)result))
			{
				return (DeviceFeatureLevel)device.FeatureLevel;
			}

			_log.Print("This video device is not supported by Gorgon and will be skipped.", LoggingLevel.Verbose);
			return DeviceFeatureLevel.Unsupported;
		}

		/// <summary>
		/// Function to retrieve the outputs attached to a video device.
		/// </summary>
		/// <param name="device">The Direct 3D device used to filter display modes.</param>
		/// <param name="adapter">The adapter to retrieve the outputs from.</param>
		/// <param name="outputCount">The number of outputs for the device.</param>
		/// <returns>A list if video output info values.</returns>
		private IGorgonNamedObjectReadOnlyList<GorgonVideoOutputInfo> GetOutputs(D3D11.Device device, DXGI.Adapter1 adapter, int outputCount)
		{
			var result = new GorgonNamedObjectList<GorgonVideoOutputInfo>();

			// Devices created under RDP/TS do not support output selection.
			if (SystemInformation.TerminalServerSession)
			{
				_log.Print("Devices under terminal services and WARP devices devices do not use outputs, no outputs enumerated.", LoggingLevel.Intermediate);
				return result;
			}

			for (int i = 0; i < outputCount; ++i)
			{
				using (DXGI.Output output = adapter.GetOutput(i))
				{

					IReadOnlyList<GorgonVideoMode> videoModes = GetVideoModes(device, output);

					if (videoModes.Count <= 0)
					{
						_log.Print($"Output '{output.Description.DeviceName.Replace("\0", string.Empty)}' on adapter '{adapter.Description1.Description.Replace("\0", string.Empty)}' has no full screen video modes.",
						           LoggingLevel.Intermediate);
					}

					result.Add(new GorgonVideoOutputInfo(i, output, videoModes));
				}
			}

			return result;
		}

		/// <summary>
		/// Function to return a video device by its unique identifier.
		/// </summary>
		/// <returns>A <see cref="GorgonVideoDeviceInfo"/> for the device with the specified LUID, or <b>null</b> (<i>Nothing</i> in VB.Net) if no device was found with the appropriate <paramref name="luid"/>.</returns>
		public GorgonVideoDeviceInfo GetByLuid(long luid)
		{
			return _devices.FirstOrDefault(item => item.Luid == luid);
		}

		/// <summary>
		/// Function to perform an enumeration of the video devices attached to the system and populate this list.
		/// </summary>
		/// <param name="enumerateWARPDevice">[Optional] <b>true</b> to enumerate the WARP software device, or <b>false</b> to exclude it.</param>
		/// <param name="enumerateRefRasterizer">[Optional] <b>true</b> to enumerate the reference rasterizer device, or <b>false</b> to exclude it.</param>
		/// <remarks>
		/// <para>
		/// Use this method to populate this list with information about the video devices installed in the system.
		/// </para>
		/// <para>
		/// You may include the WARP device, which is a software based device that emulates most of the functionality of a video device, by setting the <paramref name="enumerateWARPDevice"/> to <b>true</b>.
		/// </para>
		/// <para>
		/// If Gorgon is <i>not</i> compiled in DEBUG mode, then the <paramref name="enumerateRefRasterizer"/> parameter is ignored as it only applies to debugging scenarios.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// If the <paramref name="enumerateRefRasterizer"/> is set to <b>true</b> then you must only use this device in DEBUG mode and for diagnostic issues. The reference rasterizer is meant for diagnosing 
		/// driver problems and has extremely poor performance.
		/// </para>
		/// <para>
		/// If the D3D11SDKLayers.dll is not on the system, then the application may crash when enumerating a reference rasterizer. 
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Enumerate(bool enumerateWARPDevice = false, bool enumerateRefRasterizer = false)
		{
			// Turn on object tracking if it's not already enabled.
			var devices = new List<GorgonVideoDeviceInfo>();

			using (var factory = new DXGI.Factory1())
			{
				int adapterCount = factory.GetAdapterCount1();

				_log.Print("Enumerating video devices...", LoggingLevel.Simple);

				// Begin gathering device information.
				for (int i = 0; i < adapterCount; i++)
				{
					// Get the video device information.
					using (var adapter = factory.GetAdapter1(i))
					{
						if (((adapter.Description1.Flags & DXGI.AdapterFlags.Remote) != 0)
							|| ((adapter.Description1.Flags & DXGI.AdapterFlags.Software) != 0))
						{
							continue;
						}

						// We create a D3D device here to filter out unsupported video modes from the format list.
						using (var D3DDevice = new D3D11.Device(adapter))
						{
							D3DDevice.DebugName = "Output enumerator device.";

							DeviceFeatureLevel featureLevel = GetFeatureLevel(D3DDevice);

							// Do not enumerate this device if its feature level is not supported.
							if (featureLevel == DeviceFeatureLevel.Unsupported)
							{
								continue;
							}

							IGorgonNamedObjectReadOnlyList<GorgonVideoOutputInfo> outputs = GetOutputs(D3DDevice, adapter, adapter.GetOutputCount());

							if (outputs.Count <= 0)
							{
								_log.Print($"WARNING: Video device {adapter.Description1.Description.Replace("\0", string.Empty)} has no outputs. Full screen mode will not be possible.", LoggingLevel.Verbose);
							}

							var videoDevice = new GorgonVideoDeviceInfo(i, adapter, featureLevel, outputs, VideoDeviceType.Hardware);
							devices.Add(videoDevice);
							PrintLog(videoDevice);
						}
					}
				}

				// Get software devices.
				if (enumerateWARPDevice)
				{
					var device = GetWARPSoftwareDevice(devices.Count);

					if (device != null)
					{
						devices.Add(device);
					}
				}

#if DEBUG
				if (enumerateRefRasterizer)
				{
					var device = GetRefSoftwareDevice(devices.Count);

					if (device != null)
					{
						devices.Add(device);
					}
				}
#endif
			}

			if (devices.Count == 0)
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORGFX_DEVICE_ERR_CANNOT_FIND_DEVICES);
			}

			Interlocked.Exchange(ref _devices, devices);

			_log.Print("Found {0} video devices.", LoggingLevel.Simple, Count);
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		public bool Contains(string name) => _devices.Any(item => string.Equals(name, item.Name, StringComparison.CurrentCultureIgnoreCase));

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>
		/// The index of <paramref name="name"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(string name)
		{
			for (int i = 0; i < _devices.Count; ++i)
			{
				if (string.Equals(name, _devices[i].Name, StringComparison.CurrentCultureIgnoreCase))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="item">The object to locate in the list.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(GorgonVideoDeviceInfo item) => _devices.IndexOf(item);

		/// <summary>
		/// Determines whether the list contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the list.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the list; otherwise, false.
		/// </returns>
		public bool Contains(GorgonVideoDeviceInfo item) => _devices.Contains(item);

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonVideoDeviceInfo> GetEnumerator()
		{
			return _devices.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_devices).GetEnumerator();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonVideoDeviceList"/> class.
		/// </summary>
		/// <param name="log">[Optional] A logging object used for debugging.</param>
		public GorgonVideoDeviceList(IGorgonLog log = null)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;
		}
		#endregion
	}
}
