﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.20506.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gallio.MSTestAdapter.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gallio.MSTestAdapter.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Cannot run tests because the MSTest executable was not found.
        /// </summary>
        internal static string MSTestController_MSTestExecutableNotFound {
            get {
                return ResourceManager.GetString("MSTestController_MSTestExecutableNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Running MSTest tests.
        /// </summary>
        internal static string MSTestController_RunningMSTestTests {
            get {
                return ResourceManager.GetString("MSTestController_RunningMSTestTests", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Ignore attribute was applied to this class..
        /// </summary>
        internal static string MSTestExplorer_IgnoreAttributeWasAppliedToClass {
            get {
                return ResourceManager.GetString("MSTestExplorer_IgnoreAttributeWasAppliedToClass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Ignore attribute was applied to this test..
        /// </summary>
        internal static string MSTestExplorer_IgnoreAttributeWasAppliedToTest {
            get {
                return ResourceManager.GetString("MSTestExplorer_IgnoreAttributeWasAppliedToTest", resourceCulture);
            }
        }
    }
}
