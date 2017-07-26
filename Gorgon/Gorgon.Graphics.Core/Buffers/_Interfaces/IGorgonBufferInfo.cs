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
// Created: July 4, 2017 10:06:48 PM
// 
#endregion

using System;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The type of binding to use when binding to the GPU.
    /// </summary>
    [Flags]
    public enum BufferBinding
    {
        /// <summary>
        /// <para>
        /// No GPU access. This buffer will only be used on the CPU.
        /// </para>
        /// <para>
        /// This flag is mutally exclusive.
        /// </para>
        /// </summary>
        None = 0,
        /// <summary>
        /// The GPU will have access via the shaders as a shader resource.
        /// </summary>
        Shader = 1,
        /// <summary>
        /// The GPU will have access via the shaders using unordered access.
        /// </summary>
        UnorderedAccess = 2,
        /// <summary>
        /// The GPU will bind this buffer as a stream out buffer used to receive data.
        /// </summary>
        StreamOut = 4
    }

    /// <summary>
    /// Provides the necessary information required to set up a generic unstructured buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides an immutable view of the buffer information so that it cannot be modified after the buffer is created.
    /// </para>
    /// </remarks>
    public interface IGorgonBufferInfo
    {
        /// <summary>
        /// Property to return the intended usage for binding to the GPU.
        /// </summary>
        D3D11.ResourceUsage Usage
        {
            get;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </para>
        /// </remarks>
        int SizeInBytes
        {
            get;
        }

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The type of binding should be used to determine what type of view to apply to the buffer when accessing it from shaders. This will also help determine how data will be interpreted.
        /// </para>
        /// <para>
        /// Different bindings may be applied at the same time by OR'ing the <see cref="BufferBinding"/> flags together.
        /// </para>
        /// <para>
        /// If the <see cref="Usage"/> is set to <c>Staging</c>, then this value must be set to <see cref="BufferBinding.None"/>, otherwise an exception will be raised when the buffer is created.
        /// </para>
        /// <para>
        /// The default value is <see cref="BufferBinding.Shader"/>
        /// </para>
        /// </remarks>
        BufferBinding Binding
        {
            get;
        }

        /// <summary>
        /// Property to return the format for the default shader view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this to define a default <see cref="GorgonBufferView"/> for the buffer. This default view will allow shaders to access the buffer without needing to create an additional view. If this 
        /// value is set to <c>Unknown</c>, then no default shader view will be created.
        /// </para>
        /// <para>
        /// The default shader view will expose the entire buffer to the shader. To limit view to only a portion of the buffer, call the <see cref="GorgonBuffer.GetShaderResourceView"/> with the appropriate 
        /// element constraints.
        /// </para>
        /// <para>
        /// The format must not be typeless, if it is, an exception will be thrown on buffer creation.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This property is only used if the <see cref="Binding"/> property has a <see cref="BufferBinding.Shader"/> flag.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// The default value for this property is <c>Unknown</c>.
        /// </para>
        /// </remarks>
        DXGI.Format DefaultShaderViewFormat
        {
            get;
        }
    }
}
