#region MIT.
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
// Created: Wednesday, May 29, 2013 11:08:06 PM
// 
#endregion

using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An unordered access buffer view.
	/// </summary>
	/// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
	/// <para>Ordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
	/// have a format that is the same bit-depth and in the same group as its bound resource.</para>
	/// <para>Unlike a <see cref="GorgonLibrary.Graphics.GorgonBufferShaderView">GorgonBufferShaderView</see>, only one unordered access view may be applied to a resource.</para>
	/// </remarks>
	public class GorgonBufferUnorderedAccessView
		: GorgonUnorderedAccessView
	{
		#region Properties.
		/// <summary>
		/// Property to return the offset of the view from first element in the buffer.
		/// </summary>
		public int ElementStart
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of elements in the view.
		/// </summary>
		public int ElementCount
		{
			get;
			private set;
		}
        
        /// <summary>
        /// Property to return whether this is a raw view or not.
        /// </summary>
        public bool IsRaw
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void OnInitialize()
        {
			Gorgon.Log.Print("Creating buffer unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);
			var desc = new SharpDX.Direct3D11.UnorderedAccessViewDescription
				{
					Dimension = SharpDX.Direct3D11.UnorderedAccessViewDimension.Buffer,
					Buffer =
						{
							FirstElement = ElementStart,
							ElementCount = ElementCount,
							Flags = IsRaw ? D3D.UnorderedAccessViewBufferFlags.Raw : D3D.UnorderedAccessViewBufferFlags.None
						},
					Format = (SharpDX.DXGI.Format)Format
				};

			D3DView = new SharpDX.Direct3D11.UnorderedAccessView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
				{
					DebugName = "Gorgon Unordered Access View for " + Resource.Name
				};

			Resource.Graphics.AddTrackedObject(this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferUnorderedAccessView"/> class.
		/// </summary>
		/// <param name="resource">The buffer to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="firstElement">The first element in the buffer.</param>
		/// <param name="elementCount">The number of elements to view.</param>
		/// <param name="isRaw">TRUE if the view should be a raw view, FALSE if not.</param>
		internal GorgonBufferUnorderedAccessView(GorgonResource resource, BufferFormat format, int firstElement, int elementCount, bool isRaw)
			: base(resource, format)
		{
		    IsRaw = isRaw;
			ElementStart = firstElement;
			ElementCount = elementCount;
		}
		#endregion
	}
}