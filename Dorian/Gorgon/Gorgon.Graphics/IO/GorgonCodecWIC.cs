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
// Created: Wednesday, February 13, 2013 10:15:28 PM
// 

// This code was adapted from:
// SharpDX by Alexandre Mutel (http://sharpdx.org)
// DirectXTex by Chuck Walburn (http://directxtex.codeplex.com)

#region SharpDX/DirectXTex licenses
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTex http://directxtex.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.
#endregion
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIC = SharpDX.WIC;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// Special case flags for decoding images.
	/// </summary>
	[Flags]
	public enum WICFlags
	{
		/// <summary>
		/// No special flags.
		/// </summary>
		None = 0,
		/// <summary>
		/// Loads BGR formats as R8G8B8A8_UintNormal.
		/// </summary>
		ForceRGB = 0x1,
		/// <summary>
		/// Loads R10G10B10_XR_BIAS_A2_UIntNormal as R10G10B10A2_UIntNormal.
		/// </summary>
		NoX2Bias = 0x2,
		/// <summary>
		/// Loads 565, 5551, and 4444 as R8G8B8A8_UIntNormal.
		/// </summary>
		No16BPP = 0x4,
		/// <summary>
		/// Loads 1-bit monochrome as 8 bit greyscale.
		/// </summary>
		AllowMono = 0x8
	}

	/// <summary>
	/// Base class for the WIC based file formats (PNG, GIF, BMP, WMP, TIF).
	/// </summary>
	/// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// </remarks>
	public unsafe class GorgonCodecWIC
		: GorgonImageCodec
	{
		#region Variables.
		private string _codec = string.Empty;				// Codec name.
		private string _description = string.Empty;			// Codec description.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the flags used for decoding the image.
		/// </summary>
		/// <remarks>This will alter the conversion process for an image when changing formats.
		/// <para>This property applied to decoding of image data only.</para>
		/// <para>The default value is None.</para>
		/// </remarks>
		public WICFlags DecodeFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether all frames in a multi-frame image should be encoded/decoded or not.
		/// </summary>
		/// <remarks>This property will encode or decode multiple frames from or into an array.  Note that this is only supported on codecs that support multiple frames (e.g. animated Gif).  
		/// Images that do not support multiple frames will ignore this flag.
		/// <para>This property applies to both encoding and decoding of image data.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool UseAllFrames
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. PNG).
		/// </summary>
		public override string Codec
		{
			get
			{
				return _codec;
			}
		}

		/// <summary>
		/// Property to return the friendly description of the format.
		/// </summary>
		public override string CodecDescription
		{
			get
			{
				return _description;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve meta data from the image.
		/// </summary>
		/// <param name="wic">WIC interface.</param>
		/// <param name="decoder">Decoder for the image.</param>
		/// <param name="frame">Frame in the image to decode.</param>
		/// <param name="bestFormatMatch">The best match for the pixel format.</param>
		/// <returns>Settings for the new image.</returns>
		private IImageSettings ReadMetaData(GorgonWICImage wic, WIC.BitmapDecoder decoder, WIC.BitmapFrameDecode frame, ref Guid bestFormatMatch)
		{
			GorgonTexture2DSettings settings = new GorgonTexture2DSettings();
			
			return new GorgonTexture2DSettings()
			{
				Width = frame.Size.Width,
				Height = frame.Size.Height,
				MipCount = 1,
				ArrayCount = (UseAllFrames) ? decoder.FrameCount : 1,
				Format = wic.FindBestFormat(frame.PixelFormat, DecodeFlags, ref bestFormatMatch)
			};
		}

		/// <summary>
		/// Function to read the data from a frame.
		/// </summary>
		/// <param name="wic">WIC interface.</param>
		/// <param name="data">Image data to populate.</param>
		/// <param name="srcFormat">Source image format.</param>
		/// <param name="convertFormat">Conversion format.</param>
		/// <param name="frame">Frame containing the image data.</param>
		private void ReadFrame(GorgonWICImage wic, GorgonImageData data, Guid srcFormat, Guid convertFormat, WIC.BitmapFrameDecode frame)
		{
			var buffer = data[0];

			// We don't need to convert, so just leave.
			if ((convertFormat == Guid.Empty) || (srcFormat == convertFormat))
			{
				frame.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data.BasePointer, buffer.PitchInformation.SlicePitch);
				return;
			}

			// Perform conversion.
			using (var converter = new WIC.FormatConverter(wic.Factory))
			{
				Tuple<WIC.Palette, double, WIC.BitmapPaletteType> paletteInfo = GetPaletteInfo(true);

				try
				{					
					converter.Initialize(frame, convertFormat, WIC.BitmapDitherType.None, paletteInfo.Item1, paletteInfo.Item2, paletteInfo.Item3);
					converter.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data.BasePointer, buffer.PitchInformation.SlicePitch);
				}
				finally
				{
					if ((paletteInfo != null) && (paletteInfo.Item1 != null))
					{
						paletteInfo.Item1.Dispose();
					}
				}				
			}
		}

		/// <summary>
		/// Function to retrieve palette information for indexed images.
		/// </summary>
		/// <param name="decoding">TRUE if decoding, FALSE if not.</param>
		/// <returns>A tuple containing the palette data, alpha percentage and the type of palette.</returns>
		internal virtual Tuple<WIC.Palette, double, WIC.BitmapPaletteType> GetPaletteInfo(bool decoding)
		{
			return new Tuple<WIC.Palette, double, WIC.BitmapPaletteType>(null, 0, WIC.BitmapPaletteType.Custom);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the data to load.</param>
		/// <param name="size">Size of the data to read, in bytes.</param>
		/// <returns>
		/// The image data that was in the stream.
		/// </returns>
		protected internal override GorgonImageData LoadFromStream(GorgonDataStream stream, int size)
		{
			GorgonImageData result = null;
			Guid bestFormat = Guid.Empty;

			// Get our WIC interface.
			using (var wic = new GorgonWICImage())
			{
				using (var decoder = new WIC.BitmapDecoder(wic.Factory, stream, WIC.DecodeOptions.CacheOnDemand))
				{
					using (var frame = decoder.GetFrame(0))
					{
						var settings = ReadMetaData(wic, decoder, frame, ref bestFormat);

						if (settings.Format == BufferFormat.Unknown)
						{
							throw new System.IO.IOException("Cannot decode the " + Codec + " file.  Format is not supported.");
						}

						// Create our image data.
						result = new GorgonImageData(settings);

						if (settings.ArrayCount > 1)
						{
							// TODO: Read multi-frame images.
						}
						else
						{
							ReadFrame(wic, result, frame.PixelFormat, bestFormat, frame);
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to persist image data to a stream.
		/// </summary>
		/// <param name="imageData"><see cref="GorgonLibrary.Graphics.GorgonImageData">Gorgon image data</see> to persist.</param>
		/// <param name="stream">Stream that will contain the data.</param>
		protected internal override void SaveToStream(GorgonImageData imageData, System.IO.Stream stream)
		{
			
		}

		/// <summary>
		/// Function to read file meta data.
		/// </summary>
		/// <param name="stream">Stream used to read the metadata.</param>
		/// <returns>
		/// The image meta data as a <see cref="GorgonLibrary.Graphics.IImageSettings">IImageSettings</see> value.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>Thrown when the stream cannot perform seek operations.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
		public override IImageSettings GetMetaData(System.IO.Stream stream)
		{
			Guid bestFormat = Guid.Empty;

			// Get our WIC interface.
			using (var wic = new GorgonWICImage())
			{
				using (var decoder = new WIC.BitmapDecoder(wic.Factory, stream, WIC.DecodeOptions.CacheOnDemand))
				{
					using (var frame = decoder.GetFrame(0))
					{
						var settings = ReadMetaData(wic, decoder, frame, ref bestFormat);

						if (settings.Format == BufferFormat.Unknown)
						{
							throw new System.IO.IOException("Cannot decode the " + Codec + " file.  Format is not supported.");
						}

						return settings;
					}
				}
			}
		}

		/// <summary>
		/// Function to determine if this codec can read the file or not.
		/// </summary>
		/// <param name="stream">Stream used to read the file information.</param>
		/// <returns>
		/// TRUE if the codec can read the file, FALSE if not.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>Thrown when the stream cannot perform seek operations.</para>
		/// </exception>		
		public override bool CanBeRead(System.IO.Stream stream)
		{
			long position = 0;
			Guid bestFormat = Guid.Empty;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new System.IO.IOException("Stream is write-only.");
            }

            if (!stream.CanSeek)
            {
                throw new System.IO.IOException("The stream cannot perform seek operations.");
            }

			position = stream.Position;

			try
			{
				// Get our WIC interface.
				using (var wic = new GorgonWICImage())
				{
					using (var decoder = new WIC.BitmapDecoder(wic.Factory, stream, WIC.DecodeOptions.CacheOnDemand))
					{
						using (var frame = decoder.GetFrame(0))
						{
							var settings = ReadMetaData(wic, decoder, frame, ref bestFormat);

							return (settings.Format != BufferFormat.Unknown);
						}
					}
				}
			}
			catch (System.IO.EndOfStreamException)
			{
				// Catch end of stream as a sign that we can't read this file.
				return false;
			}
			finally
			{
				stream.Position = position;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWIC" /> class.
		/// </summary>
		/// <param name="codec">Codec name.</param>
		/// <param name="description">Description for the codec.</param>
		/// <param name="extensions">Common extension(s) for the codec.</param>
		internal GorgonCodecWIC(string codec, string description, string[] extensions)
		{
			DecodeFlags = WICFlags.None;
			UseAllFrames = false;
			_codec = codec;
			_description = description;
			CodecCommonExtensions = extensions;
		}
		#endregion
	}
}
