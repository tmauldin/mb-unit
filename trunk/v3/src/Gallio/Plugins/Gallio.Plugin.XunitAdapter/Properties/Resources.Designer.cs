﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gallio.Plugin.XunitAdapter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gallio.Plugin.XunitAdapter.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Running xUnit.Net tests..
        /// </summary>
        internal static string XunitTestController_RunningXunitTests {
            get {
                return ResourceManager.GetString("XunitTestController_RunningXunitTests", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run test: {0}..
        /// </summary>
        internal static string XunitTestController_StatusMessages_RunningTest {
            get {
                return ResourceManager.GetString("XunitTestController_StatusMessages_RunningTest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to xUnit.Net v{0}.
        /// </summary>
        internal static string XunitTestExplorer_FrameworkNameWithVersionFormat {
            get {
                return ResourceManager.GetString("XunitTestExplorer_FrameworkNameWithVersionFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to xUnit.Net.
        /// </summary>
        internal static string XunitTestFramework_XunitFrameworkName {
            get {
                return ResourceManager.GetString("XunitTestFramework_XunitFrameworkName", resourceCulture);
            }
        }
    }
}
