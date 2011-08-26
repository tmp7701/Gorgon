#region MIT.
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
// Created: Friday, June 24, 2011 10:05:35 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using GorgonLibrary.Math;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Enumeration for joystick axis directions.
	/// </summary>
	[Flags()]
	public enum JoystickDirections
	{
		/// <summary>
		/// Axis is centered.
		/// </summary>
		Center = 1,
		/// <summary>
		/// Axis is up.
		/// </summary>
		Up = 2,
		/// <summary>
		/// Axis is down.
		/// </summary>
		Down = 4,
		/// <summary>
		/// Axis is left.
		/// </summary>
		Left = 8,
		/// <summary>
		/// Axis is right.
		/// </summary>
		Right = 16,
		/// <summary>
		/// Axis is less than center position.
		/// </summary>
		LessThanCenter = 32,
		/// <summary>
		/// Axis is greater than center position.
		/// </summary>
		MoreThanCenter = 64,
		/// <summary>
		/// Axis is a horizontal axis.
		/// </summary>
		Horizontal = 128,
		/// <summary>
		/// Axis is a vertical axis.
		/// </summary>
		Vertical = 256,
		/// <summary>
		/// Axis is a vector.
		/// </summary>
		Vector = 512
	}

	/// <summary>
	/// Flags to indicate which extra capabilties are supported by the joystick.
	/// </summary>
	[Flags]
	public enum JoystickCapabilityFlags
	{
		/// <summary>
		/// No extra capabilities.  This flag is mutally exclusive.
		/// </summary>
		None = 0,
		/// <summary>
		/// Supports point of view controls.
		/// </summary>
		SupportsPOV = 1,
		/// <summary>
		/// Supports continuous degree bearings for the point of view controls.
		/// </summary>
		SupportsContinuousPOV = 2,
		/// <summary>
		/// Supports discreet values (up, down, left, right and center) for the point of view controls.
		/// </summary>
		SupportsDiscreetPOV = 4,
		/// <summary>
		/// Supports a rudder control.
		/// </summary>
		SupportsRudder = 8,
		/// <summary>
		/// Supports a throttle (or Z-axis) control.
		/// </summary>
		SupportsThrottle = 16,
		/// <summary>
		/// Supports vibration.
		/// </summary>
		SupportsVibration = 32,
		/// <summary>
		/// Supports a secondary X axis.
		/// </summary>
		SupportsSecondaryXAxis = 64,
		/// <summary>
		/// Supports a secondary Y axis.
		/// </summary>
		SupportsSecondaryYAxis = 128
	}

	/// <summary>
	/// A joystick/gamepad interface.
	/// </summary>
	/// <remarks>This is not like the other input interfaces in that the data in this object is a snapshot of its state and not polled automatically.  
	/// The user must call the <see cref="M:GorgonLibrary.Input.GorgonJoystick.Poll">Poll</see> method in order to update the state of the device.
	/// <para>Note that while this object supports 6 axis devices, it will be limited to the number of axes present on the physical device, thus some values will always return 0.</para>
	/// </remarks>
	public abstract class GorgonJoystick
		: GorgonInputDevice
	{
		#region Value Types.
		/// <summary>
		/// Button state data.
		/// </summary>
		public struct JoystickButtonState
			: INamedObject
		{
			#region Variables.
			private string _name;
			private bool _state;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return whether the button is pressed or not.
			/// </summary>
			public bool IsPressed
			{
				get
				{
					return _state;
				}
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickButtonState"/> struct.
			/// </summary>
			/// <param name="name">The name of the button.</param>
			/// <param name="state">State of the button, TRUE for pressed, FALSE if not.</param>
			public JoystickButtonState(string name, bool state)
			{
				GorgonDebug.AssertParamString(name, "name");
				_name = name;
				_state = state;
			}
			#endregion

			#region INamedObject Members
			/// <summary>
			/// Property to return the name of the button.
			/// </summary>
			public string Name
			{
				get 
				{
					return _name;
				}
			}
			#endregion
		}
		#endregion

		#region Classes.
		/// <summary>
		/// Contains the capabilities of the joystick.
		/// </summary>
		/// <remarks>Implementors must implement this to fill in the capabilities of the device.</remarks>
		public abstract class JoystickCapabilities
		{
			#region Variables.
			private GorgonMinMax _xAxisRange = GorgonMinMax.Empty;			// X axis range.
			private GorgonMinMax _yAxisRange = GorgonMinMax.Empty;			// Y axis range.
			private GorgonMinMax _x2AxisRange = GorgonMinMax.Empty;			// Secondary X axis range.
			private GorgonMinMax _y2AxisRange = GorgonMinMax.Empty;			// Secondary Y axis range.
			private GorgonMinMax _throttleAxisRange = GorgonMinMax.Empty;	// Throttle axis range.
			private GorgonMinMax _rudderAxisRange = GorgonMinMax.Empty;		// Rudder axis range.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return the X axis mid range.
			/// </summary>
			internal int XAxisMidRange
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the Y axis mid range.
			/// </summary>
			internal int YAxisMidRange
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the throttle axis mid range.
			/// </summary>
			internal int ThrottleAxisMidRange
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the rudder axis mid range.
			/// </summary>
			internal int RudderAxisMidRange
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the secondary X axis mid range.
			/// </summary>
			internal int SecondaryXAxisMidRange
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to set or return the secondary Y axis mid range.
			/// </summary>
			internal int SecondaryYAxisMidRange
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the extra capabilities of this device.
			/// </summary>
			public JoystickCapabilityFlags ExtraCapabilities
			{
				get;
				protected set;
			}

			/// <summary>
			/// Property to return the number of buttons for the device.
			/// </summary>
			public int ButtonCount
			{
				get;
				protected set;
			}

			/// <summary>
			/// Property to return the number of axes for the device.
			/// </summary>
			public int AxisCount
			{
				get;
				protected set;
			}

			/// <summary>
			/// Property to return the X axis range.
			/// </summary>
			public GorgonMinMax XAxisRange
			{
				get
				{
					return _xAxisRange;
				}
				protected set
				{
					_xAxisRange = value;
					XAxisMidRange = CalculateMidRange(value);
				}
			}

			/// <summary>
			/// Property to return the Y axis range.
			/// </summary>
			public GorgonMinMax YAxisRange
			{
				get
				{
					return _yAxisRange;
				}
				protected set
				{
					_yAxisRange = value;
					YAxisMidRange = CalculateMidRange(value);
				}
			}

			/// <summary>
			/// Property to return the secondary Y axis range.
			/// </summary>
			public GorgonMinMax SecondaryYAxisRange
			{
				get
				{
					return _y2AxisRange;
				}
				protected set
				{
					_y2AxisRange = value;
					SecondaryYAxisMidRange = CalculateMidRange(value);
				}
			}

			/// <summary>
			/// Property to return the secondary X axis range.
			/// </summary>
			public GorgonMinMax SecondaryXAxisRange
			{
				get
				{
					return _x2AxisRange;
				}
				protected set
				{
					_x2AxisRange = value;
					SecondaryXAxisMidRange = CalculateMidRange(value);
				}
			}

			/// <summary>
			/// Property to return the throttle axis range.
			/// </summary>
			public GorgonMinMax ThrottleAxisRange
			{
				get
				{
					return _throttleAxisRange;
				}
				protected set
				{
					_throttleAxisRange = value;
					ThrottleAxisMidRange = CalculateMidRange(value);
				}
			}

			/// <summary>
			/// Property to return the rudder axis range.
			/// </summary>
			public GorgonMinMax RudderAxisRange
			{
				get
				{
					return _rudderAxisRange;
				}
				protected set
				{
					_rudderAxisRange = value;
					RudderAxisMidRange = CalculateMidRange(value);
				}
			}

			/// <summary>
			/// Property to return the number of vibration motor controls for the device.
			/// </summary>
			public int VibrationMotorCount
			{
				get
				{
					return VibrationMotorRanges.Count;
				}
			}

			/// <summary>
			/// Property to return the vibration motor ranges.
			/// </summary>
			public IList<GorgonMinMax> VibrationMotorRanges
			{
				get;
				protected set;
			}

			/// <summary>
			/// Property to return the manufacturer ID.
			/// </summary>
			public int ManufacturerID
			{
				get;
				protected set;
			}

			/// <summary>
			/// Property to return the product ID.
			/// </summary>
			public int ProductID
			{
				get;
				protected set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to calculate the mid point for a range value.
			/// </summary>
			/// <param name="range">Range to calculate.</param>
			/// <returns>The mid point of the range.</returns>
			private int CalculateMidRange(GorgonMinMax range)
			{
				if (range == GorgonMinMax.Empty)
					return 0;

				return range.Minimum + (range.Range / 2);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickCapabilities"/> class.
			/// </summary>
			protected JoystickCapabilities()
			{
				VibrationMotorRanges = new GorgonMinMax[0];
				AxisCount = 0;
				ButtonCount = 0;
				XAxisRange = GorgonMinMax.Empty;
				YAxisRange = GorgonMinMax.Empty;
				SecondaryXAxisRange = GorgonMinMax.Empty;
				SecondaryYAxisRange = GorgonMinMax.Empty;
				ThrottleAxisRange = GorgonMinMax.Empty;
				RudderAxisRange = GorgonMinMax.Empty;
				ManufacturerID = 0;
				ProductID = 0;

				ExtraCapabilities = JoystickCapabilityFlags.None;
			}
			#endregion
		}

		/// <summary>
		/// Defines the dead zone for a given axis.
		/// </summary>
		public class JoystickDeadZoneAxes
		{
			#region Properties.
			/// <summary>
			/// Property to set or return the deadzone for the X axis.
			/// </summary>
			public GorgonMinMax X
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the deadzone for the Y axis.
			/// </summary>
			public GorgonMinMax Y
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the deadzone for the secondary X axis.
			/// </summary>
			public GorgonMinMax SecondaryX
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the deadzone for the secondary Y axis.
			/// </summary>
			public GorgonMinMax SecondaryY
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the deadzone for the throttle axis.
			/// </summary>
			public GorgonMinMax Throttle
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the deadzone for the rudder axis.
			/// </summary>
			public GorgonMinMax Rudder
			{
				get;
				set;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickDeadZoneAxes"/> class.
			/// </summary>
			internal JoystickDeadZoneAxes()
			{
				X = GorgonMinMax.Empty;
				Y = GorgonMinMax.Empty;
				SecondaryX = GorgonMinMax.Empty;
				SecondaryY = GorgonMinMax.Empty;
				Throttle = GorgonMinMax.Empty;
				Rudder = GorgonMinMax.Empty;
			}
			#endregion
		}

		/// <summary>
		/// Defines which direction each axis is pointing at.
		/// </summary>
		public class JoystickAxisDirections
		{
			#region Properties.
			/// <summary>
			/// Property to return the direction that the X axis is pointed towards.
			/// </summary>
			/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
			/// property.  If the axis position is within the dead-zone range only the 
			/// <see cref="GorgonLibrary.Input.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
			public JoystickDirections X
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the direction that the Y axis is pointed towards.
			/// </summary>
			/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
			/// property.  If the axis position is within the dead-zone range only the 
			/// <see cref="GorgonLibrary.Input.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
			public JoystickDirections Y
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the direction that the secondary X axis is pointed towards.
			/// </summary>
			/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
			/// property.  If the axis position is within the dead-zone range only the 
			/// <see cref="GorgonLibrary.Input.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
			public JoystickDirections SecondaryX
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the direction that the secondary Y axis is pointed towards.
			/// </summary>
			/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
			/// property.  If the axis position is within the dead-zone range only the 
			/// <see cref="GorgonLibrary.Input.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
			public JoystickDirections SecondaryY
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the direction that the throttle axis is pointed towards.
			/// </summary>
			/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
			/// property.  If the axis position is within the dead-zone range only the 
			/// <see cref="GorgonLibrary.Input.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
			public JoystickDirections Throttle
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the direction that the rudder axis is pointed towards.
			/// </summary>
			/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
			/// property.  If the axis position is within the dead-zone range only the 
			/// <see cref="GorgonLibrary.Input.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
			public JoystickDirections Rudder
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the direction that the POV is pointed towards.
			/// </summary>
			public JoystickDirections POV
			{
				get;
				internal set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to reset the directions.
			/// </summary>
			internal void Reset()
			{
				X = JoystickDirections.Center;
				Y = JoystickDirections.Center;
				SecondaryX = JoystickDirections.Center;
				SecondaryY = JoystickDirections.Center;
				Throttle = JoystickDirections.Center;
				Rudder = JoystickDirections.Center;
				POV = JoystickDirections.Center;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickAxisDirections"/> class.
			/// </summary>
			internal JoystickAxisDirections()
			{
				Reset();
			}
			#endregion
		}		

		/// <summary>
		/// Defines the state of the buttons for the device.
		/// </summary>
		public abstract class JoystickButtons
			: GorgonBaseNamedObjectCollection<JoystickButtonState>
		{
			#region Properties.
			/// <summary>
			/// Property to return a button state by its index.
			/// </summary>
			public JoystickButtonState this[int index]
			{
				get
				{
					return GetItem(index);
				}
				protected set
				{
					SetItem(value.Name, value);
				}
			}

			/// <summary>
			/// Property to return a button state by the button name.
			/// </summary>
			public JoystickButtonState this[string name]
			{
				get
				{
					return GetItem(name);
				}
				protected set
				{
					SetItem(name, value);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to define a new button for the device.
			/// </summary>
			/// <param name="name">Name of the button to define.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the name parameter is empty.
			/// <para>-or-</para>
			/// <para>Thrown when the name already exists in the list.</para>
			/// </exception>
			protected void DefineButton(string name)
			{
				GorgonDebug.AssertParamString(name, "name");

				AddItem(new JoystickButtonState(name, false));
			}

			/// <summary>
			/// Function to reset the button states.
			/// </summary>
			internal void Reset()
			{
				for (int i = 0; i < Count; i++)
					this[i] = new JoystickButtonState(this[i].Name, false);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickButtons"/> class.
			/// </summary>
			protected JoystickButtons()
				: base(false)
			{				
			}
			#endregion
		}
		#endregion

		#region Variables.
		private bool _deviceLost = false;												// Flag to indicate that the device was in a lost state.
		private JoystickAxisDirections _directions = null;								// Axis directions.
		#endregion

		#region Properties.		
		/// <summary>
		/// Property to set or return flag to indicate that the device is in a lost state.
		/// </summary>
		protected bool DeviceLost
		{
			get
			{
				if (!AllowBackground)
					return _deviceLost;
				else
					return false;
			}
			set
			{
				if (AllowBackground)
					_deviceLost = false;
				else
					_deviceLost = value;
			}
		}

		/// <summary>
		/// Property to return the direction each axis (and POV) are pointed at.
		/// </summary>
		public JoystickAxisDirections Direction
		{
			get
			{
				return _directions;
			}
			private set
			{
				_directions = value;
			}
		}

		/// <summary>
		/// Property to return the dead zones for the axes of the joystick.
		/// </summary>
		public JoystickDeadZoneAxes DeadZone
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the joystick capabilities.
		/// </summary>
		public JoystickCapabilities Capabilities
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the point of view value for continuous bearing.
		/// </summary>
		/// <remarks>This will return an integer value of -1 for center, or 0 to 35999.  The user must divide by 100 to get the angle in degrees for a continuous bearing POV.</remarks>
		public int POV
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return a button state.
		/// </summary>
		public JoystickButtons Button
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the x coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the X axis range will be returned (center position of the axis).</remarks>
		public int X
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the y coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Y axis range will be returned (center position of the axis).</remarks>
		public int Y
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the throttle coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the throttle axis range will be returned (center position of the axis).</remarks>
		public int Throttle
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the secondary X axis for the joystick (if applicable).
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Z axis range will be returned (center position of the axis).</remarks>
		public int SecondaryX
		{
			get;
			protected set;
		}


		/// <summary>
		/// Property to return the secondary Y axis for the joystick (if applicable).
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Z axis range will be returned (center position of the axis).</remarks>
		public int SecondaryY
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the rudder coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.Input.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the rudder range will be returned (center position of the axis).</remarks>
		public int Rudder
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the joystick is connected.
		/// </summary>
		public bool IsConnected
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply a deadzone to a value.
		/// </summary>
		/// <param name="value">Value to dead zone.</param>
		/// <param name="deadZone">Dead zone range.</param>
		/// <param name="midRange">Mid point for the range.</param>
		/// <returns>The actual axis data if it falls outside of the dead zone, or the center position value.</returns>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the joystick has not been initialized.</exception>
		private int DeadZoneValue(int value, GorgonMinMax deadZone, int midRange)
		{
			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The joystick is not initialized.");

			// The dead zone range needs to be within the range of the axis.
			if ((!deadZone.Contains(value)) || (deadZone == GorgonMinMax.Empty))
				return value;

			return midRange;
		}

		/// <summary>
		/// Function to get an orientation for a value.
		/// </summary>
		/// <param name="value">Value to evaluate.</param>
		/// <param name="deadZone">The dead zone.</param>
		/// <param name="orientation">Orientation of the axis.</param>
		/// <param name="midRange">Mid point for the range.</param>
		/// <returns>The direction that the axis is pointed at.</returns>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the joystick has not been initialized.</exception>
		private JoystickDirections GetDirection(int value, GorgonMinMax deadZone, JoystickDirections orientation, int midRange)
		{
			JoystickDirections result = JoystickDirections.Center;

			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The joystick is not initialized.");

			if ((deadZone == GorgonMinMax.Empty) || (!deadZone.Contains(value)))
			{
				switch (orientation)
				{
					case JoystickDirections.Horizontal:
						if (value > midRange)
							result = JoystickDirections.Right;
						if (value < midRange)
							result = JoystickDirections.Left;
						break;
					case JoystickDirections.Vertical:
						if (value < midRange)
							result = JoystickDirections.Down;
						if (value > midRange)
							result = JoystickDirections.Up;
						break;
					default:
						if (orientation != JoystickDirections.Vector)
							orientation = JoystickDirections.Vector;
						if (value > midRange)
							result = JoystickDirections.MoreThanCenter;
						if (value < midRange)
							result = JoystickDirections.LessThanCenter;
						break;
				}
			}

			//result |= orientation;

			return result;
		}

		/// <summary>
		/// Function to reset the various joystick axes and buttons and POV settings to default values.
		/// </summary>
		private void SetDefaults()
		{
			Direction.Reset();
			X = 0;
			Y = 0;
			SecondaryX = 0;
			SecondaryY = 0;
			Throttle = 0;
			Rudder = 0;
			POV = 0;
			Button.Reset();
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		/// <remarks>Implementors must override this to return data from the joystick.</remarks>
		protected abstract void PollJoystick();

		/// <summary>
		/// Function to retrieve the capabilities of the joystick/gamepad.
		/// </summary>
		/// <returns>The capabilities data for the joystick/gamepad.</returns>
		/// <remarks>Implementors must implement this function so the object can determine constraints about the device.</remarks>
		protected abstract JoystickCapabilities GetCapabilities();

		/// <summary>
		/// Function to retrieve the buttons for the joystick/gamepad.
		/// </summary>
		/// <returns>The list of buttons for the joystick/gamepad.</returns>
		/// <remarks>Implementors must implement this function so the object can get the list of buttons for the device.</remarks>
		protected abstract JoystickButtons GetButtons();

		/// <summary>
		/// Function to initalize the data for the joystick.
		/// </summary>
		/// <remarks>Implementors must call this function after the object has been created or during the construction of the object.</remarks>
		protected void Initialize()
		{
			DeadZone = new JoystickDeadZoneAxes();
			Capabilities = GetCapabilities();
			Button = GetButtons();
			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The device was not initialized successfully.");
			SetDefaults();
		}

		/// <summary>
		/// Function to perform device vibration.
		/// </summary>
		/// <param name="motorIndex">Index of the motor to start.</param>
		/// <param name="value">Value to set.</param>
		/// <remarks>Implementors should implement this function if the device supports vibration.</remarks>
		protected virtual void VibrateDevice(int motorIndex, int value)
		{
		}

		/// <summary>
		/// Function to set the device to vibrate.
		/// </summary>
		/// <param name="motorIndex">Index of the motor to start.</param>
		/// <param name="value">Value to set.</param>
		/// <remarks>This will activate the vibration motor(s) in the joystick/gamepad.  The <paramref name="motorIndex"/> should be within the 
		/// <see cref="P:GorgonLibrary.Input.GorgonJoystick.JoystickCapabilities.VibrationMotorCount">VibrationMotorCount</see>, or else an exception will be thrown.  Check the
		/// <see cref="P:GorgonLibrary.Input.GorgonJoystick.JoystickCapabilities.ExtraCapabilities">ExtraCapabilities</see> property to see if vibration is supported by the device.
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the device has not been initialized.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the motorIndex parameter is less than 0 or greater than or equal to the VibrationMotorCount range.</exception>
		public void Vibrate(int motorIndex, int value)
		{
			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The joystick device has not been initialized.");

			if (((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsVibration) != JoystickCapabilityFlags.SupportsVibration) || (motorIndex < 0) || (motorIndex >= Capabilities.VibrationMotorCount))
				throw new ArgumentException("motorIndex", "There is no motor at index " + motorIndex.ToString());

			if (Capabilities.VibrationMotorRanges[motorIndex].Contains(value))
				VibrateDevice(motorIndex, value);
		}

		/// <summary>
		/// Function to read the joystick state.
		/// </summary>
		/// <remarks>Users must call this in order to retrieve the state of the joystick/gamepad at any given time.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the device has not been initialized.</exception>
		public void Poll()
		{
			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The joystick device has not been initialized.");

			if ((DeviceLost) && (!BoundControl.Focused))
				return;
			else
				_deviceLost = false;

			if ((!Enabled) || (!Acquired))
				return;

			// Set the values back to their defaults.
			SetDefaults();
			
			// Get the data.
			PollJoystick();

			// Apply dead zones and get directions.
			X = DeadZoneValue(X, DeadZone.X, Capabilities.XAxisMidRange);
			Direction.X = GetDirection(X, DeadZone.X, JoystickDirections.Horizontal, Capabilities.XAxisMidRange);

			Y = DeadZoneValue(Y, DeadZone.Y, Capabilities.YAxisMidRange);
			Direction.Y = GetDirection(Y, DeadZone.Y, JoystickDirections.Vertical, Capabilities.YAxisMidRange);

			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
			{
				SecondaryX = DeadZoneValue(SecondaryX, DeadZone.SecondaryX, Capabilities.SecondaryXAxisMidRange);
				Direction.SecondaryX = GetDirection(SecondaryX, DeadZone.SecondaryX, JoystickDirections.Horizontal, Capabilities.SecondaryXAxisMidRange);
			}
			else
			{
				SecondaryX = 0;
				Direction.SecondaryX = JoystickDirections.Center;
			}

			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
			{
				SecondaryY = DeadZoneValue(SecondaryY, DeadZone.SecondaryY, Capabilities.SecondaryXAxisMidRange);
				Direction.SecondaryY = GetDirection(SecondaryY, DeadZone.SecondaryY, JoystickDirections.Vertical, Capabilities.SecondaryYAxisMidRange);
			}
			else
			{
				SecondaryY = 0;
				Direction.SecondaryY = JoystickDirections.Center;
			}

			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
			{
				Throttle = DeadZoneValue(Throttle, DeadZone.Throttle, Capabilities.ThrottleAxisMidRange);
				Direction.Throttle = GetDirection(Throttle, DeadZone.Throttle, JoystickDirections.Vector, Capabilities.ThrottleAxisMidRange);
			}
			else
			{
				Throttle = 0;
				Direction.Throttle = JoystickDirections.Center;
			}

			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
			{
				Rudder = DeadZoneValue(Rudder, DeadZone.Rudder, Capabilities.RudderAxisMidRange);
				Direction.Rudder = GetDirection(Rudder, DeadZone.Rudder, JoystickDirections.Vector, Capabilities.RudderAxisMidRange);
			}
			else
			{
				Rudder = 0;
				Direction.Rudder = JoystickDirections.Center;
			}

			// Wrap POV if it's higher than 359.99 degrees.
			if (POV > 35999)
				POV = -1;

			// Get POV direction.
			if (POV == -1)
				Direction.POV = JoystickDirections.Center;
			else
			{
				// Determine direction.
				if ((POV < 18000) && (POV > 9000))
					Direction.POV = JoystickDirections.Down | JoystickDirections.Right;
				if ((POV > 18000) && (POV < 27000))
					Direction.POV = JoystickDirections.Down | JoystickDirections.Left;
				if ((POV > 27000) && (POV < 36000))
					Direction.POV = JoystickDirections.Up | JoystickDirections.Left;
				if ((POV > 0) && (POV < 9000))
					Direction.POV = JoystickDirections.Up | JoystickDirections.Right;

				if (POV == 18000)
					Direction.POV = JoystickDirections.Down;
				if (POV == 0)
					Direction.POV = JoystickDirections.Up;
				if (POV == 9000)
					Direction.POV = JoystickDirections.Right;
				if (POV == 27000)
					Direction.POV = JoystickDirections.Left;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystick"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the input device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		protected GorgonJoystick(GorgonInputFactory owner, string deviceName, Control boundWindow)
			: base(owner, deviceName, boundWindow)
		{
			Direction = new JoystickAxisDirections();
		}
		#endregion
	}
}