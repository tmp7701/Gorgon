﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gorgon.Input.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gorgon.Input.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not create the input factory for &apos;{0}&apos;..
        /// </summary>
        internal static string GORINP_CANNOT_CREATE {
            get {
                return ResourceManager.GetString("GORINP_CANNOT_CREATE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device requested already exists, but is not of the type &apos;{0}&apos;..
        /// </summary>
        internal static string GORINP_DEVICE_ALREADY_EXISTS_TYPE_MISMATCH {
            get {
                return ResourceManager.GetString("GORINP_DEVICE_ALREADY_EXISTS_TYPE_MISMATCH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot enumerate the devices when input device objects are active..
        /// </summary>
        internal static string GORINP_ERR_CANNOT_ENUM_DEVICES_PRESENT {
            get {
                return ResourceManager.GetString("GORINP_ERR_CANNOT_ENUM_DEVICES_PRESENT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device is not polled and requires a forwarder method. Did you forget to override RegisterForwarder?.
        /// </summary>
        internal static string GORINP_ERR_DEVICE_NEEDS_FORWARDER {
            get {
                return ResourceManager.GetString("GORINP_ERR_DEVICE_NEEDS_FORWARDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The HID property &apos;{0}&apos; is NULL..
        /// </summary>
        internal static string GORINP_ERR_HID_PROPERTY_NULL {
            get {
                return ResourceManager.GetString("GORINP_ERR_HID_PROPERTY_NULL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no gaming device axis at the specified index..
        /// </summary>
        internal static string GORINP_ERR_JOYSTICK_AXES_INVALID {
            get {
                return ResourceManager.GetString("GORINP_ERR_JOYSTICK_AXES_INVALID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A window is required to bind with the {0} device..
        /// </summary>
        internal static string GORINP_ERR_NO_WINDOW_TO_BIND {
            get {
                return ResourceManager.GetString("GORINP_ERR_NO_WINDOW_TO_BIND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The parameter must not be empty..
        /// </summary>
        internal static string GORINP_ERR_PARAMETER_MUST_NOT_BE_EMPTY {
            get {
                return ResourceManager.GetString("GORINP_ERR_PARAMETER_MUST_NOT_BE_EMPTY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error retrieving the cursor information.
        ///
        ///Win32 Error Code: {0}.
        /// </summary>
        internal static string GORINP_ERR_WIN32_CURSOR_INFO {
            get {
                return ResourceManager.GetString("GORINP_ERR_WIN32_CURSOR_INFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find the HID &apos;{0}&apos;..
        /// </summary>
        internal static string GORINP_HID_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_HID_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no motor at index [{0}]..
        /// </summary>
        internal static string GORINP_JOYSTICK_MOTOR_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_JOYSTICK_MOTOR_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find the joystick &apos;{0}&apos;..
        /// </summary>
        internal static string GORINP_JOYSTICK_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_JOYSTICK_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The joystick was not initialized..
        /// </summary>
        internal static string GORINP_JOYSTICK_NOT_INITIALIZED {
            get {
                return ResourceManager.GetString("GORINP_JOYSTICK_NOT_INITIALIZED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Keyboard key &apos;{0}&apos; has not been assigned to a mapping..
        /// </summary>
        internal static string GORINP_KEY_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_KEY_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Keyboard key &apos;{0}&apos; has not been assigned to a mapping..
        /// </summary>
        internal static string GORINP_KEYBOARD_KEY_NO_MAPPING {
            get {
                return ResourceManager.GetString("GORINP_KEYBOARD_KEY_NO_MAPPING", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find the keyboard &apos;{0}&apos;..
        /// </summary>
        internal static string GORINP_KEYBOARD_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_KEYBOARD_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The parameter must not be NULL or empty..
        /// </summary>
        internal static string GORINP_PARAMETER_NULL_EMPTY {
            get {
                return ResourceManager.GetString("GORINP_PARAMETER_NULL_EMPTY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The plug-in &apos;{0}&apos; was not found in any of the loaded plug-in assemblies or was not the correct type..
        /// </summary>
        internal static string GORINP_PLUGIN_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_PLUGIN_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find the pointing device &apos;{0}&apos;..
        /// </summary>
        internal static string GORINP_POINTINGDEVICE_NOT_FOUND {
            get {
                return ResourceManager.GetString("GORINP_POINTINGDEVICE_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Joystick axis: {0}, Range: {1} - {2}..
        /// </summary>
        internal static string GORINP_TOSTR_JOYSTICKAXIS {
            get {
                return ResourceManager.GetString("GORINP_TOSTR_JOYSTICKAXIS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Virtual Key: {0}, Character: {1}, Shift+Character: {2}.
        /// </summary>
        internal static string GORINP_TOSTR_KEYCHARMAP {
            get {
                return ResourceManager.GetString("GORINP_TOSTR_KEYCHARMAP", resourceCulture);
            }
        }
    }
}
