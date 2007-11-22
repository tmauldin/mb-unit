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
using System.Reflection;
using Gallio.Model;

namespace Gallio.Model.Filters
{
    /// <summary>
    /// A filter that matches objects whose <see cref="IModelComponent.CodeReference" />
    /// contains the specified type.
    /// </summary>
    [Serializable]
    public class TypeFilter<T> : Filter<T> where T : IModelComponent
    {
        private readonly string typeName;
        private readonly bool includeDerivedTypes;

        /// <summary>
        /// Creates a type filter.
        /// </summary>
        /// <param name="typeName">The type name specified in one of the following ways.
        /// In each case, the type name must exactly match the value obtained via reflection
        /// on the type.
        /// <list type="bullet">
        /// <item>Fully qualified by namespace and assembly as returned by <see cref="Type.AssemblyQualifiedName" /></item>
        /// <item>Qualified by namespace as returned by <see cref="Type.FullName" /></item>
        /// <item>Unqualified as returned by the type's <see cref="MemberInfo.Name" /></item>
        /// </list>
        /// </param>
        /// <param name="includeDerivedTypes">If true, subclasses and interface implementations of the specified
        /// type are also matched by the filter if they can be located</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="typeName"/> is null</exception>
        public TypeFilter(string typeName, bool includeDerivedTypes)
        {
            //TODO: Localize this message
            if (String.IsNullOrEmpty(typeName))
                throw new ArgumentException(@"The Type's name can't be an empty string.");

            this.typeName = typeName;
            this.includeDerivedTypes = includeDerivedTypes;
        }

        /// <inheritdoc />
        public override bool IsMatch(T value)
        {
            Type type;
            try
            {
                type = value.CodeReference.ResolveType();
            }
            catch (Exception)
            {
                return false;
            }

            // TODO: Handle case where the type cannot be loaded using plain
            //       old strings.  Naturally we won't be able to
            //       handle includeDerivedTypes.

            if (IsMatchForType(type))
                return true;

            if (includeDerivedTypes)
            {
                for (Type baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
                {
                    if (IsMatchForType(baseType))
                        return true;
                }

                foreach (Type @interface in type.GetInterfaces())
                    if (IsMatchForType(@interface))
                        return true;
            }

            return false;
        }

        private bool IsMatchForType(Type type)
        {
            return typeName == type.AssemblyQualifiedName
                || typeName == type.FullName
                || typeName == type.Name;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return " Type(" + typeName + ") ";
        }
    }
}