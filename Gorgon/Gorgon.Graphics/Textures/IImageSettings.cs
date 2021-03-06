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
// Created: Sunday, February 3, 2013 2:45:31 PM
// 
#endregion

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Type of image data.
	/// </summary>
	public enum ImageType
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Image is a 1 dimensional image.
		/// </summary>
		Image1D = 2,
		/// <summary>
		/// Image is a 2 dimensional image.
		/// </summary>
		Image2D = 3,
		/// <summary>
		/// Image is a 3 dimensional image.
		/// </summary>
		Image3D = 4,
		/// <summary>
		/// Image is a texture cube.
		/// </summary>
		ImageCube = 0xFF
	}

	/// <summary>
	/// Image settings to describe the characteristics of an image.
	/// </summary>
	public interface IImageSettings        
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of image data.
		/// </summary>
		ImageType ImageType
		{
			get;
		}

		/// <summary>
		/// Property to set or return the width of an image.
		/// </summary>
		int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of an image.
		/// </summary>
		/// <remarks>This applies to 2D and 3D images only.  This parameter will be ignored if applied to a 1D image.</remarks>
		int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth of a texture.
		/// </summary>
		/// <remarks>This applies to 3D images only.  This parameter will be ignored if applied to a 1D or 2D image.</remarks>
		int Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the pixel layout of an image.
		/// </summary>
		BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip map levels in the image.
		/// </summary>
		int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		bool IsPowerOfTwo
		{
			get;
		}

		/// <summary>
		/// Property to set or return the number of images there are in an image array.
		/// </summary>
		/// <remarks>This only applies to 1D and 2D images.  This parameter will be ignored if applied to a 3D image.</remarks>
		int ArrayCount
		{
			get;
			set;
		}
		#endregion

        #region Methods
        /// <summary>
        /// Function to clone these image settings.
        /// </summary>
        /// <returns>A clone of the image settings.</returns>
        IImageSettings Clone();
        #endregion
    }
}
