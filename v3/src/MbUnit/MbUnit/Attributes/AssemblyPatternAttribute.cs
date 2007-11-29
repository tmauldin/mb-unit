// Copyright 2007 MbUnit Project - http://www.mbunit.com/
// Portions Copyright 2000-2004 Jonathan De Halleux, Jamie Cansdale
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Gallio.Model.Reflection;
using MbUnit.Model;

namespace MbUnit.Attributes
{
    /// <summary>
    /// <para>
    /// Declares that an assembly contains MbUnit tests.  Subclasses of this
    /// attribute can customize how template enumeration takes place within
    /// an assembly.
    /// </para>
    /// <para>
    /// At most one attribute of this type may appear in any given assembly.
    /// If the attribute is omitted, the assembly is scanned using the default
    /// reflection algorithm.
    /// </para>
    /// </summary>
    /// <seealso cref="AssemblyDecoratorPatternAttribute"/>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false, Inherited=true)]
    public abstract class AssemblyPatternAttribute : PatternAttribute
    {
        /// <summary>
        /// Gets a default instance of the assembly pattern attribute to use
        /// when the attribute is elided from a test assembly.
        /// </summary>
        public static readonly AssemblyPatternAttribute DefaultInstance = new DefaultImpl();

        /// <summary>
        /// Creates a test assembly template.
        /// This method is called when an assembly that appears to contain unit
        /// tests is discovered via reflection to create a new model object to represent it.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="frameworkTemplate">The containing framework template</param>
        /// <param name="assembly">The assembly</param>
        /// <returns>The test assembly template</returns>
        public virtual MbUnitAssemblyTemplate CreateTemplate(MbUnitTestBuilder builder,
            MbUnitFrameworkTemplate frameworkTemplate, IAssemblyInfo assembly)
        {
            return new MbUnitAssemblyTemplate(frameworkTemplate, assembly);
        }

        /// <summary>
        /// <para>
        /// Applies contributions to an assembly template.
        /// This method is called after the assembly template is linked to the template tree.
        /// </para>
        /// <para>
        /// Contributions are applied in a very specific order:
        /// <list type="bullet">
        /// <item>Assembly decorator attributes declared by the assembly sorted by order</item>
        /// <item>Metadata attributes declared by the assembly</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <remarks>
        /// A typical use of this method is to apply additional metadata to model
        /// objects in the template tree and to further expand the tree using
        /// declarative metadata derived via reflection.
        /// </remarks>
        /// <param name="builder">The builder</param>
        /// <param name="assemblyTemplate">The assembly template</param>
        public virtual void Apply(MbUnitTestBuilder builder, MbUnitAssemblyTemplate assemblyTemplate)
        {
            builder.ProcessAssemblyDecorators(assemblyTemplate);
            builder.ProcessMetadata(assemblyTemplate, assemblyTemplate.Assembly);

            ProcessTypes(builder, assemblyTemplate);
        }

        /// <summary>
        /// Processes all public types within the assembly via reflection.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="assemblyTemplate">The assembly template</param>
        protected virtual void ProcessTypes(MbUnitTestBuilder builder, MbUnitAssemblyTemplate assemblyTemplate)
        {
            foreach (ITypeInfo type in assemblyTemplate.Assembly.GetExportedTypes())
            {
                ProcessType(builder, assemblyTemplate, type);
            }
        }

        /// <summary>
        /// Processes a type via reflection.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="assemblyTemplate">The assembly template</param>
        /// <param name="type">The type</param>
        protected virtual void ProcessType(MbUnitTestBuilder builder, MbUnitAssemblyTemplate assemblyTemplate, ITypeInfo type)
        {
            builder.ProcessType(assemblyTemplate, type);
        }

        private class DefaultImpl : AssemblyPatternAttribute
        {
        }
    }
}
