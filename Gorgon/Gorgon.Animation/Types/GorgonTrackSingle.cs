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
// Created: Wednesday, October 3, 2012 9:03:27 PM
// 
#endregion

using System;
using SlimMath;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// An animation track for single precision floating point values.
	/// </summary>
	/// <typeparam name="T">Type of object being animated.</typeparam>
	internal class GorgonTrackSingle<T>
		: GorgonAnimationTrack<T>
		where T : class
	{
		#region Variables.
	    private readonly Action<T, Single> _setProperty;		// Set property method.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		public override TrackInterpolationMode SupportedInterpolation
		{
			get
			{
				return TrackInterpolationMode.Spline | TrackInterpolationMode.Linear;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the a key with the proper type for this track.
		/// </summary>
		/// <returns>The key with the proper type for this track.</returns>
		protected override IKeyFrame MakeKey()
		{
			return new GorgonKeySingle(0, 0);
		}

		/// <summary>
		/// Function to set up the spline for the animation.
		/// </summary>
		protected internal override void SetupSpline()
		{
			base.SetupSpline();

			foreach (var key in KeyFrames.Times)
				Spline.Points.Add(new Vector4(((GorgonKeySingle)key.Value).Value, 0.0f, 0.0f, 1.0f));

			Spline.UpdateTangents();
		}

		/// <summary>
		/// Function to interpolate a new key frame from the nearest previous and next key frames.
		/// </summary>
		/// <param name="keyValues">Nearest previous and next key frames.</param>
		/// <param name="keyTime">The time to assign to the key.</param>
		/// <param name="unitTime">The time, expressed in unit time.</param>
		/// <returns>
		/// The interpolated key frame containing the interpolated values.
		/// </returns>
		protected override IKeyFrame GetTweenKey(ref NearestKeys keyValues, float keyTime, float unitTime)
		{
			var next = (GorgonKeySingle)keyValues.NextKey;
			var prev = (GorgonKeySingle)keyValues.PreviousKey;

			switch (InterpolationMode)
			{
				case TrackInterpolationMode.Linear:
					return new GorgonKeySingle(keyTime, prev.Value + (next.Value - prev.Value) * unitTime);
				case TrackInterpolationMode.Spline:
					return new GorgonKeySingle(keyTime, Spline.GetInterpolatedValue(keyValues.PreviousKeyIndex, unitTime).X);
				default:
					return prev;
			}
		}

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected internal override void ApplyKey(ref IKeyFrame key)
		{
			var value = (GorgonKeySingle)key;
			_setProperty(Animation.AnimationController.AnimatedObject, value.Value);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTrackSingle{T}" /> class.
		/// </summary>
		/// <param name="property">Property information.</param>
		internal GorgonTrackSingle(GorgonAnimatedProperty property)
			: base(property)
		{
			_setProperty = BuildSetAccessor<Single>();

			InterpolationMode = TrackInterpolationMode.Linear;
		}
		#endregion
	}
}
