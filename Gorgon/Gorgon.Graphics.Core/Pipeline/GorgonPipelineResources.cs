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
// Created: August 17, 2016 9:40:02 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Flags to indicate what part of the pipeline was changed.
	/// </summary>
	[Flags]
	public enum PipelineResourceChangeFlags
		: ulong
	{
		/// <summary>
		/// No changes.
		/// </summary>
		None = 0,
		/// <summary>
		/// Vertex buffer bindings changed.
		/// </summary>
		VertexBuffer = 0x1,
		/// <summary>
		/// Index buffer changes.
		/// </summary>
		IndexBuffer = 0x2,
		/// <summary>
		/// Render target views changed.
		/// </summary>
		RenderTargets = 0x4,
		/// <summary>
		/// Constant buffers for the pixel shader changed.
		/// </summary>
		PixelShaderConstantBuffer = 0x8,
		/// <summary>
		/// Constant buffers for the vertex shader changed.
		/// </summary>
		VertexShaderConstantBuffer = 0x10,
		/// <summary>
		/// Shader resources for the pixel shader changed.
		/// </summary>
		PixelShaderResource = 0x20,
		/// <summary>
		/// Shader resources for the vertex shader changed.
		/// </summary>
		VertexShaderResource = 0x40,
		/// <summary>
		/// Samplers for the pixel shader changed.
		/// </summary>
		PixelShaderSampler = 0x80,
		/// <summary>
		/// <para>
		/// Samplers for the vertex shader changed.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This only applies to an <see cref="IGorgonVideoDevice"/> that has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
		/// </para>
		/// </note>
		/// </para>
		/// </summary>
		VertexShaderSampler = 0x100,
		/// <summary>
		/// All states have changed.
		/// </summary>
		All = VertexBuffer 
			| IndexBuffer 
			| RenderTargets 
			| PixelShaderConstantBuffer 
			| VertexShaderConstantBuffer 
			| PixelShaderResource 
			| VertexShaderResource
			| PixelShaderSampler
			| VertexShaderSampler
	}

	/// <summary>
	/// Used to bind resource types (e.g. buffers, textures, etc...) to the GPU pipeline.
	/// </summary>
	public sealed class GorgonPipelineResources
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed shader resources that can be bound at the same time.
		/// </summary>
		public const int MaximumShaderResourceViewCount = D3D11.CommonShaderStage.InputResourceSlotCount;
		#endregion

		#region Variables.
		// The list of render target resources bound.
		private GorgonRenderTargetViews _renderTargets;
		// The list of constant buffers for a pixel shader.
		private GorgonConstantBuffers _pixelShaderConstantBuffers;
		// The list of constant buffers for a vertex shader.
		private GorgonConstantBuffers _vertexShaderConstantBuffers;
		// The index buffer.
		private GorgonIndexBuffer _indexBuffer;
		// The list of vertex buffers.
		private GorgonVertexBufferBindings _vertexBuffers;
		// The available changes on this resource list.
		private PipelineResourceChangeFlags _changes;
		#endregion

		#region Properties.

		/// <summary>
		/// Property to set or return the vertex buffers to bind to the pipeline.
		/// </summary>
		public GorgonVertexBufferBindings VertexBuffers
		{
			get
			{
				return _vertexBuffers;
			}
			set
			{
				if (_vertexBuffers == value)
				{
					return;
				}

				_vertexBuffers = value;
				_changes |= PipelineResourceChangeFlags.VertexBuffer;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer to bind to the pipeline.
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get
			{
				return _indexBuffer;
			}
			set
			{
				if (_indexBuffer == value)
				{
					return;
				}

				_indexBuffer = value;
				_changes |= PipelineResourceChangeFlags.IndexBuffer;
			}
		}

		/// <summary>
		/// Property to return the render targets to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// Once a set of <see cref="GorgonRenderTargetViews"/> are assigned to this property, it will be locked and cannot be changed until it is unassigned.
		/// </remarks>
		public GorgonRenderTargetViews RenderTargets
		{
			get
			{
				return _renderTargets;
			}
			set
			{
				SetRenderTargets(value, false);
			}
		}

		/// <summary>
		/// Property to return the pixel shader constant buffers to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// Once a set of <see cref="GorgonConstantBuffers"/> are assigned to this property, it will be locked and cannot be changed until it is unassigned.
		/// </remarks>
		public GorgonConstantBuffers PixelShaderConstantBuffers
		{
			get
			{
				return _pixelShaderConstantBuffers;
			}
			set
			{
				SetShaderConstantBuffers(value, false, ShaderType.Pixel);
			}
		}

		/// <summary>
		/// Property to return the vertex shader constant buffers to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// Once a set of <see cref="GorgonConstantBuffers"/> are assigned to this property, it will be locked and cannot be changed until it is unassigned.
		/// </remarks>
		public GorgonConstantBuffers VertexShaderConstantBuffers
		{
			get
			{
				return _vertexShaderConstantBuffers;
			}
			set
			{
				SetShaderConstantBuffers(value, false, ShaderType.Vertex);
			}
		}

		/// <summary>
		/// Property to return the vertex shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews VertexShaderResourceViews
		{
			get;
		} = new GorgonShaderResourceViews();

		/// <summary>
		/// Property to set or return the list of pixel shader resource views.
		/// </summary>
		public GorgonShaderResourceViews PixelShaderResourceViews
		{
			get;
		} = new GorgonShaderResourceViews();

		/// <summary>
		/// Property to return the pixel shader samplers to bind to the pipeline.
		/// </summary>
		public GorgonSamplerStates PixelShaderSamplers
		{
			get;
		} = new GorgonSamplerStates();

		/// <summary>
		/// Property to return the vertex shader samplers to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This only applies to an <see cref="IGorgonVideoDevice"/> that has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerStates VertexShaderSamplers
		{
			get;
		} = new GorgonSamplerStates();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to assign the vertex buffer bindings to the resource list.
		/// </summary>
		/// <param name="device">The Direct 3D 11 device context to use.</param>
		internal void SetVertexBuffers(D3D11.DeviceContext device)
		{
			if (VertexBuffers == null)
			{
				device.InputAssembler.InputLayout = null;
				device.InputAssembler.SetVertexBuffers(0);
				return;
			}

			// Ensure that we assign an input layout.
			if (device.InputAssembler.InputLayout != VertexBuffers.InputLayout.D3DInputLayout)
			{
				device.InputAssembler.InputLayout = VertexBuffers.InputLayout.D3DInputLayout;
			}
			
			ref NativeBinding<D3D11.VertexBufferBinding> bindings = ref VertexBuffers.GetNativeBindings();
			device.InputAssembler.SetVertexBuffers(bindings.StartSlot, bindings.Bindings);
		}

		/// <summary>
		/// Function to assign the render target views to the resource list.
		/// </summary>
		/// <param name="value">The resources to assign.</param>
		/// <param name="noLockChange"><b>true</b> to change locking state on the resource, <b>false</b> to leave alone.</param>
		private void SetRenderTargets(GorgonRenderTargetViews value, bool noLockChange)
		{
			if (_renderTargets == value)
			{
				return;
			}

			if ((_renderTargets != null)
				&& (!noLockChange))
			{
				_renderTargets.IsLocked = false;
			}

			_renderTargets = value;

			if ((_renderTargets != null)
				&& (!noLockChange))
			{
				_renderTargets.IsLocked = true;
			}

			_changes |= PipelineResourceChangeFlags.RenderTargets;
		}

		/// <summary>
		/// Function to assign the constant buffers to the resource list.
		/// </summary>
		/// <param name="value">The resources to assign.</param>
		/// <param name="noLockChange"><b>true</b> to change locking state on the resource, <b>false</b> to leave alone.</param>
		/// <param name="shaderType">The type of shader to update.</param>
		private void SetShaderConstantBuffers(GorgonConstantBuffers value, bool noLockChange, ShaderType shaderType)
		{
			switch (shaderType)
			{
				case ShaderType.Pixel:
					if (_pixelShaderConstantBuffers == value)
					{
						return;
					}

					if ((_pixelShaderConstantBuffers != null)
						&& (!noLockChange))
					{
						_pixelShaderConstantBuffers.IsLocked = false;
					}

					_pixelShaderConstantBuffers = value;
					_changes |= PipelineResourceChangeFlags.PixelShaderConstantBuffer;

					if ((_pixelShaderConstantBuffers != null)
						&& (!noLockChange))
					{
						_pixelShaderConstantBuffers.IsLocked = true;
					}
					break;
				case ShaderType.Vertex:
					if (_vertexShaderConstantBuffers == value)
					{
						return;
					}

					if ((_vertexShaderConstantBuffers != null)
						&& (!noLockChange))
					{
						_vertexShaderConstantBuffers.IsLocked = false;
					}

					_vertexShaderConstantBuffers = value;
					_changes |= PipelineResourceChangeFlags.VertexShaderConstantBuffer;

					if ((_vertexShaderConstantBuffers != null)
						&& (!noLockChange))
					{
						_vertexShaderConstantBuffers.IsLocked = true;
					}
					break;
			}
		}

		/// <summary>
		/// Function to assign the sampler states to the resource list.
		/// </summary>
		/// <param name="device">The Direct 3D 11 device context to use.</param>
		/// <param name="shaderType">The type of shader to set the resources on.</param>
		internal void SetShaderSamplers(D3D11.DeviceContext1 device, ShaderType shaderType)
		{
			switch (shaderType)
			{
				case ShaderType.Pixel:
					ref NativeBinding<D3D11.SamplerState> psBinding = ref PixelShaderSamplers.GetNativeBindings();
					device.PixelShader.SetSamplers(psBinding.StartSlot, psBinding.Count, psBinding.Bindings);
					break;
				case ShaderType.Vertex:
					ref NativeBinding<D3D11.SamplerState> vsBinding = ref VertexShaderSamplers.GetNativeBindings();
					device.PixelShader.SetSamplers(vsBinding.StartSlot, vsBinding.Count, vsBinding.Bindings);
					break;
			}
		}

		/// <summary>
		/// Function to set the shader resource views for a shader.
		/// </summary>
		/// <param name="device">The Direct 3D 11 device context to use.</param>
		/// <param name="shaderType">The type of shader to set the resources on.</param>
		internal void SetShaderResourceViews(D3D11.DeviceContext1 device, ShaderType shaderType)
		{
			switch (shaderType)
			{
				case ShaderType.Pixel:
					ref NativeBinding<D3D11.ShaderResourceView> psBinding = ref PixelShaderResourceViews.GetNativeBindings();
					device.PixelShader.SetShaderResources(psBinding.StartSlot, psBinding.Count, psBinding.Bindings);
					break;
				case ShaderType.Vertex:
					ref NativeBinding<D3D11.ShaderResourceView> vsBinding = ref VertexShaderResourceViews.GetNativeBindings();
					device.VertexShader.SetShaderResources(vsBinding.StartSlot, vsBinding.Count, vsBinding.Bindings);
					break;
			}
		}

		/// <summary>
		/// Function to determine if the shader resources on this resource list are the same as another.
		/// </summary>
		/// <param name="theseResources">The resources to compare from this resource list.</param>
		/// <param name="otherResources">The resources to compare from the other resource list.</param>
		/// <returns></returns>
		private bool ShaderResourcesChanged(GorgonShaderResourceViews theseResources, GorgonShaderResourceViews otherResources)
		{
			// If the resources are the same, then just check the dirty flag.
			if (theseResources == otherResources)
			{
				return theseResources.IsDirty;
			}

			ref NativeBinding<D3D11.ShaderResourceView> sourceViews = ref theseResources.GetNativeBindings();
			ref NativeBinding<D3D11.ShaderResourceView> destViews = ref otherResources.GetNativeBindings();

			// If there's a change in slots and counts, then we have a change.
			if ((sourceViews.StartSlot != destViews.StartSlot)
				|| (sourceViews.Count != destViews.Count))
			{
				return true;
			}

			// Otherwise, we'll have to go through and compare each element.
			for (int i = sourceViews.StartSlot; i < sourceViews.StartSlot + sourceViews.Count; ++i)
			{
				if (theseResources[i] != otherResources[i])
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Function to determine if the shader resources on this resource list are the same as another.
		/// </summary>
		/// <param name="theseResources">The resources to compare from this resource list.</param>
		/// <param name="otherResources">The resources to compare from the other resource list.</param>
		/// <returns></returns>
		private bool SamplersChanged(GorgonSamplerStates theseResources, GorgonSamplerStates otherResources)
		{
			// If the resources are the same, then just check the dirty flag.
			if (theseResources == otherResources)
			{
				return theseResources.IsDirty;
			}

			ref NativeBinding<D3D11.SamplerState> sourceViews = ref theseResources.GetNativeBindings();
			ref NativeBinding<D3D11.SamplerState> destViews = ref otherResources.GetNativeBindings();

			// If there's a change in slots and counts, then we have a change.
			if ((sourceViews.StartSlot != destViews.StartSlot)
				|| (sourceViews.Count != destViews.Count))
			{
				return true;
			}

			// Otherwise, we'll have to go through and compare each element.
			for (int i = sourceViews.StartSlot; i < sourceViews.StartSlot + sourceViews.Count; ++i)
			{
				if (theseResources[i] != otherResources[i])
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Function to determine if the vertex buffers on this resource list are the same as another.
		/// </summary>
		/// <param name="theseResources">The resources to compare from this resource list.</param>
		/// <param name="otherResources">The resources to compare from the other resource list.</param>
		/// <returns></returns>
		private bool VertexBuffersChanged(GorgonVertexBufferBindings theseResources, GorgonVertexBufferBindings otherResources)
		{
			// If the resources are the same, then just check the dirty flag.
			if (theseResources == otherResources)
			{
				return theseResources?.IsDirty ?? false;
			}

			if (((theseResources != null) && (otherResources == null))
				|| ((otherResources != null) && (theseResources == null)))
			{
				return true;
			}

			if (theseResources.InputLayout != otherResources.InputLayout)
			{
				return true;
			}

			ref NativeBinding<D3D11.VertexBufferBinding> sourceViews = ref theseResources.GetNativeBindings();
			ref NativeBinding<D3D11.VertexBufferBinding> destViews = ref otherResources.GetNativeBindings();

			// If there's a change in slots and counts, then we have a change.
			if (sourceViews.Count != destViews.Count)
			{
				return true;
			}

			// Otherwise, we'll have to go through and compare each element.
			for (int i = sourceViews.StartSlot; i < sourceViews.StartSlot + sourceViews.Count; ++i)
			{
				if (theseResources[i] != otherResources[i])
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Function to retrieve the differences between this pipeline resources and another.
		/// </summary>
		/// <param name="resources">The resources to compare with.</param>
		/// <returns>The differences between the resources.</returns>
		internal PipelineResourceChangeFlags GetDifference(GorgonPipelineResources resources)
		{
			// If we had no state prior to this, then return all states as changed.
			if (resources == null)
			{
				return PipelineResourceChangeFlags.All;
			}

			PipelineResourceChangeFlags changes = _changes;
			_changes = PipelineResourceChangeFlags.None;

			var result = PipelineResourceChangeFlags.None;

			if (IndexBuffer != resources.IndexBuffer)
			{
				result |= PipelineResourceChangeFlags.IndexBuffer;
			}

			if (RenderTargets != resources.RenderTargets)
			{
				result |= PipelineResourceChangeFlags.RenderTargets;
			}

			if (VertexBuffersChanged(VertexBuffers, resources.VertexBuffers))
			{
				result |= PipelineResourceChangeFlags.VertexBuffer;
			}

			if (VertexShaderConstantBuffers != resources.VertexShaderConstantBuffers)
			{
				result |= PipelineResourceChangeFlags.VertexShaderConstantBuffer;
			}

			if (ShaderResourcesChanged(VertexShaderResourceViews, resources.VertexShaderResourceViews))
			{
				result |= PipelineResourceChangeFlags.VertexShaderResource;
			}

			if (SamplersChanged(VertexShaderSamplers, resources.VertexShaderSamplers))
			{
				result |= PipelineResourceChangeFlags.VertexShaderSampler;
			}

			if (PixelShaderConstantBuffers != resources.PixelShaderConstantBuffers)
			{
				result |= PipelineResourceChangeFlags.PixelShaderConstantBuffer;
			}

			if (SamplersChanged(PixelShaderSamplers, resources.PixelShaderSamplers))
			{
				result |= PipelineResourceChangeFlags.PixelShaderSampler;
			}

			if (ShaderResourcesChanged(PixelShaderResourceViews, resources.PixelShaderResourceViews))
			{
				result |= PipelineResourceChangeFlags.PixelShaderResource;
			}

			return result | changes;
		}

		/// <summary>
		/// Function to copy the pipeline resources from another instance.
		/// </summary>
		/// <param name="resources">The resources to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="resources"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This will copy the items from another <see cref="GorgonPipelineResources"/> state into this state. This is a shallow copy, and the items hold the same references as the original 
		/// <paramref name="resources"/>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Exceptions are only thrown from this method when Gorgon is compiled as <b>DEBUG</b>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyResourceState(GorgonPipelineResources resources)
		{
			resources.ValidateObject(nameof(resources));

			if (resources.VertexBuffers == null)
			{
				VertexBuffers = null;
			}
			else
			{
				if (VertexBuffers == null)
				{
					VertexBuffers = new GorgonVertexBufferBindings(resources.VertexBuffers.InputLayout, resources.VertexBuffers.Count);					
				}

				VertexBuffers.CopyFrom(resources.VertexBuffers);
			}
			
			SetRenderTargets(resources.RenderTargets, true);
			PixelShaderSamplers.CopyFrom(resources.PixelShaderSamplers);
			VertexShaderSamplers.CopyFrom(resources.VertexShaderSamplers);
			SetShaderConstantBuffers(resources.PixelShaderConstantBuffers, true, ShaderType.Pixel);
			SetShaderConstantBuffers(resources.VertexShaderConstantBuffers, true, ShaderType.Vertex);
			PixelShaderResourceViews.CopyFrom(resources.PixelShaderResourceViews);
			VertexShaderResourceViews.CopyFrom(resources.VertexShaderResourceViews);

			IndexBuffer = resources.IndexBuffer;
		}

		/// <summary>
		/// Function to reset to an empty resources object.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Applications should call this after the <see cref="GorgonDrawCallBase"/> is submitted to the <see cref="GorgonGraphics"/> interface if the resources assigned need to be modified.
		/// </para>
		/// </remarks>
		public void Reset()
		{
			VertexBuffers = null;
			IndexBuffer = null;
			PixelShaderResourceViews.Clear();
			VertexShaderResourceViews.Clear();
			PixelShaderConstantBuffers = null;
			VertexShaderConstantBuffers = null;
			PixelShaderSamplers.Clear();
			RenderTargets = null;

			_changes = PipelineResourceChangeFlags.None;
		}
		#endregion
	}
}
