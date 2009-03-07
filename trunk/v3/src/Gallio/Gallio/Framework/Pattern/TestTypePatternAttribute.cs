// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
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
using System.Collections.Generic;
using System.Reflection;
using Gallio.Collections;
using Gallio.Framework.Data;
using Gallio.Model;
using Gallio.Reflection;

namespace Gallio.Framework.Pattern
{
    /// <summary>
    /// Declares that a type represents an test.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Subclasses of this attribute can control what happens with the type.
    /// </para>
    /// <para>
    /// At most one attribute of this type may appear on any given class.
    /// </para>
    /// <para>
    /// A test type has no timeout by default.
    /// </para>
    /// </remarks>
    /// <seealso cref="TestTypeDecoratorPatternAttribute"/>
    [AttributeUsage(PatternAttributeTargets.TestType, AllowMultiple = false, Inherited = true)]
    public class TestTypePatternAttribute : PatternAttribute
    {
        private static readonly Key<ObjectCreationSpec> FixtureObjectCreationSpecKey = new Key<ObjectCreationSpec>("FixtureObjectCreationSpec");

        /// <summary>
        /// Gets an instance of the test type pattern attribute to use when no
        /// other pattern consumes the type.  If the type can be inferred to be a
        /// test type then the pattern will behave as if the type has a test type pattern attribute
        /// applied to it.  Otherwise it will simply recurse into nested types.
        /// </summary>
        /// <seealso cref="InferTestType"/>
        public static readonly TestTypePatternAttribute AutomaticInstance = new AutomaticImpl();

        /// <summary>
        /// <para>
        /// Gets or sets a number that defines an ordering for the test with respect to its siblings.
        /// </para>
        /// <para>
        /// Unless compelled otherwise by test dependencies, tests with a lower order number than
        /// their siblings will run before those siblings and tests with the same order number
        /// as their siblings with run in an arbitrary sequence with respect to those siblings.
        /// </para>
        /// </summary>
        /// <value>The test execution order with respect to siblings, initially zero.</value>
        public int Order { get; set; }

        /// <inheritdoc />
        public override bool IsPrimary
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool IsTest(IPatternEvaluator evaluator, ICodeElementInfo codeElement)
        {
            return true;
        }

        /// <inheritdoc />
        public override void Consume(IPatternScope containingScope, ICodeElementInfo codeElement, bool skipChildren)
        {
            ITypeInfo type = codeElement as ITypeInfo;
            Validate(containingScope, type);

            IPatternScope typeScope = containingScope.CreateChildTestScope(type.Name, type);
            typeScope.TestBuilder.Kind = TestKinds.Fixture;
            typeScope.TestBuilder.Order = Order;
                
            InitializeTest(typeScope, type);
            SetTestSemantics(typeScope.TestBuilder, type);

            typeScope.TestBuilder.ApplyDeferredActions();
        }

        /// <summary>
        /// Verifies that the attribute is being used correctly.
        /// </summary>
        /// <param name="containingScope">The containing scope</param>
        /// <param name="type">The type</param>
        /// <exception cref="PatternUsageErrorException">Thrown if the attribute is being used incorrectly</exception>
        protected virtual void Validate(IPatternScope containingScope, ITypeInfo type)
        {
            if (!containingScope.CanAddChildTest || type == null)
                ThrowUsageErrorException("This attribute can only be used on a test type within a test assembly.");
            if (!type.IsClass || type.ElementType != null)
                ThrowUsageErrorException("This attribute can only be used on a class.");
        }

        /// <summary>
        /// <para>
        /// Initializes a test for a type after it has been added to the test model.
        /// </para>
        /// <para>
        /// The members of base types are processed before those of subtypes.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The default implementation processes all public members of the type including
        /// the first constructor found, then recurses to process all public and non-public
        /// nested types.  Non-public members other than nested types are ignored.
        /// </remarks>
        /// <param name="typeScope">The type scope</param>
        /// <param name="type">The type</param>
        protected virtual void InitializeTest(IPatternScope typeScope, ITypeInfo type)
        {
            string xmlDocumentation = type.GetXmlDocumentation();
            if (xmlDocumentation != null)
                typeScope.TestBuilder.AddMetadata(MetadataKeys.XmlDocumentation, xmlDocumentation);

            typeScope.Process(type);

            if (type.IsGenericTypeDefinition)
            {
                foreach (IGenericParameterInfo parameter in type.GenericArguments)
                    typeScope.Consume(parameter, false, DefaultGenericParameterPattern);
            }

            ConsumeMembers(typeScope, type);
            ConsumeConstructors(typeScope, type);
            ConsumeNestedTypes(typeScope, type);
        }

        /// <summary>
        /// Consumes type members including fields, properties, methods and events.
        /// </summary>
        /// <param name="typeScope">The scope to be used as the containing scope</param>
        /// <param name="type">The type whose members are to be consumed</param>
        protected void ConsumeMembers(IPatternScope typeScope, ITypeInfo type)
        {
            BindingFlags bindingFlags = GetMemberBindingFlags(type);

            // TODO: We should probably process groups of members in sorted order working outwards
            //       from the base type, like an onion.
            foreach (IFieldInfo field in CodeElementSorter.SortMembersByDeclaringType(type.GetFields(bindingFlags)))
                typeScope.Consume(field, false, DefaultFieldPattern);

            foreach (IPropertyInfo property in CodeElementSorter.SortMembersByDeclaringType(type.GetProperties(bindingFlags)))
                typeScope.Consume(property, false, DefaultPropertyPattern);

            foreach (IMethodInfo method in CodeElementSorter.SortMembersByDeclaringType(type.GetMethods(bindingFlags)))
                typeScope.Consume(method, false, DefaultMethodPattern);

            foreach (IEventInfo @event in CodeElementSorter.SortMembersByDeclaringType(type.GetEvents(bindingFlags)))
                typeScope.Consume(@event, false, DefaultEventPattern);
        }

        /// <summary>
        /// Consumes type constructors.
        /// </summary>
        /// <param name="typeScope">The scope to be used as the containing scope</param>
        /// <param name="type">The type whose constructors are to be consumed</param>
        protected void ConsumeConstructors(IPatternScope typeScope, ITypeInfo type)
        {
            // Note: We only consider instance members of concrete types because abstract types
            //       cannot be instantiated so the members cannot be accessed.  An abstract type
            //       might yet be a static test fixture so we still consider its static members.
            if (!type.IsAbstract && !type.IsInterface)
            {
                foreach (IConstructorInfo constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public))
                {
                    typeScope.Consume(constructor, false, DefaultConstructorPattern);

                    // FIXME: Currently we arbitrarily choose the first constructor and throw away the rest.
                    //        This should be replaced by a more intelligent mechanism that supports a constructor
                    //        selection policy based on some criterion.
                    break;
                }
            }
        }

        /// <summary>
        /// Consumes nested types.
        /// </summary>
        /// <param name="typeScope">The scope to be used as the containing scope</param>
        /// <param name="type">The type whose nested types are to be consumed</param>
        protected void ConsumeNestedTypes(IPatternScope typeScope, ITypeInfo type)
        {
            foreach (ITypeInfo nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                typeScope.Consume(nestedType, false, DefaultNestedTypePattern);
        }

        /// <summary>
        /// <para>
        /// Applies semantic actions to a test to estalish its runtime behavior.
        /// </para>
        /// <para>
        /// This method is called after <see cref="InitializeTest" />.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default behavior for a <see cref="TestTypePatternAttribute" />
        /// is to configure the test actions as follows:
        /// <list type="bullet">
        /// <item><see cref="IPatternTestInstanceHandler.BeforeTestInstance" />: Set the
        /// fixture instance name and <see cref="PatternTestInstanceState.FixtureType" />.</item>
        /// <item><see cref="IPatternTestInstanceHandler.InitializeTestInstance" />: Create
        /// the fixture instance and set the <see cref="PatternTestInstanceState.FixtureInstance" />
        /// property accordingly.</item>
        /// <item><see cref="IPatternTestInstanceHandler.DisposeTestInstance" />: If the fixture type
        /// implements <see cref="IDisposable" />, disposes the fixture instance.</item>
        /// <item><see cref="IPatternTestInstanceHandler.DecorateChildTest" />: Decorates the child's
        /// <see cref="IPatternTestInstanceHandler.BeforeTestInstance" /> to set its <see cref="PatternTestInstanceState.FixtureInstance" />
        /// and <see cref="PatternTestInstanceState.FixtureType" /> properties to those
        /// of the fixture.  The child test may override these values later on but this
        /// is a reasonable default setting for test methods within a fixture.</item>
        /// </list>
        /// </para>
        /// <para>
        /// You can override this method to change the semantics as required.
        /// </para>
        /// </remarks>
        /// <param name="testBuilder">The test builder</param>
        /// <param name="type">The test type</param>
        protected virtual void SetTestSemantics(ITestBuilder testBuilder, ITypeInfo type)
        {
            testBuilder.TestInstanceActions.BeforeTestInstanceChain.After(
                delegate(PatternTestInstanceState testInstanceState)
                {
                    ObjectCreationSpec spec = testInstanceState.GetFixtureObjectCreationSpec(type);
                    testInstanceState.Data.SetValue(FixtureObjectCreationSpecKey, spec);

                    testInstanceState.FixtureType = spec.ResolvedType;

                    if (!testInstanceState.IsReusingPrimaryTestStep)
                        testInstanceState.TestStep.Name = spec.Format(testInstanceState.TestStep.Name, testInstanceState.Formatter);
                });

            testBuilder.TestInstanceActions.InitializeTestInstanceChain.After(
                delegate(PatternTestInstanceState testInstanceState)
                {
                    if (!type.IsAbstract && !type.IsInterface)
                    {
                        ObjectCreationSpec spec = testInstanceState.Data.GetValue(FixtureObjectCreationSpecKey);

                        testInstanceState.FixtureInstance = spec.CreateInstance();
                    }
                });

            testBuilder.TestInstanceActions.DisposeTestInstanceChain.After(
                delegate(PatternTestInstanceState testInstanceState)
                {
                    IDisposable dispose = testInstanceState.FixtureInstance as IDisposable;
                    if (dispose != null)
                    {
                        TestContext context = TestContext.CurrentContext;
                        context.Sandbox.Run(context.LogWriter, delegate { dispose.Dispose(); }, "Dispose Fixture");
                    }
                });

            testBuilder.TestInstanceActions.DecorateChildTestChain.After(
                delegate(PatternTestInstanceState testInstanceState, PatternTestActions decoratedTestActions)
                {
                    decoratedTestActions.TestInstanceActions.BeforeTestInstanceChain.Before(delegate(PatternTestInstanceState childTestInstanceState)
                    {
                        IMemberInfo member = childTestInstanceState.Test.CodeElement as IMemberInfo;
                        if (member != null)
                        {
                            ITypeInfo memberDeclaringType = member.DeclaringType;
                            if (memberDeclaringType != null)
                            {
                                if (type.Equals(memberDeclaringType) || type.IsSubclassOf(memberDeclaringType))
                                {
                                    childTestInstanceState.FixtureType = testInstanceState.FixtureType;
                                    childTestInstanceState.FixtureInstance = testInstanceState.FixtureInstance;
                                }
                            }
                        }
                    });
                });
        }

        /// <summary>
        /// Gets the default pattern to apply to generic parameters that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see cref="TestParameterPatternAttribute.DefaultInstance" />.
        /// </remarks>
        protected virtual IPattern DefaultGenericParameterPattern
        {
            get { return TestParameterPatternAttribute.DefaultInstance; }
        }

        /// <summary>
        /// Gets the default pattern to apply to methods that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <c>null</c>.
        /// </remarks>
        protected virtual IPattern DefaultMethodPattern
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the default pattern to apply to events that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <c>null</c>.
        /// </remarks>
        protected virtual IPattern DefaultEventPattern
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the default pattern to apply to fields that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see cref="TestParameterPatternAttribute.AutomaticInstance" />.
        /// </remarks>
        protected virtual IPattern DefaultFieldPattern
        {
            get { return TestParameterPatternAttribute.AutomaticInstance; }
        }

        /// <summary>
        /// Gets the default pattern to apply to properties that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see cref="TestParameterPatternAttribute.AutomaticInstance" />.
        /// </remarks>
        protected virtual IPattern DefaultPropertyPattern
        {
            get { return TestParameterPatternAttribute.AutomaticInstance; }
        }

        /// <summary>
        /// Gets the default pattern to apply to constructors that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see cref="TestConstructorPatternAttribute.DefaultInstance" />.
        /// </remarks>
        protected virtual IPattern DefaultConstructorPattern
        {
            get { return TestConstructorPatternAttribute.DefaultInstance; }
        }

        /// <summary>
        /// Gets the default pattern to apply to nested types that do not have a primary pattern, or null if none.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see cref="TestTypePatternAttribute.AutomaticInstance"/>.
        /// </remarks>
        protected virtual IPattern DefaultNestedTypePattern
        {
            get { return AutomaticInstance; }
        }

        /// <summary>
        /// Gets the binding flags that should be used to enumerate non-nested type members
        /// of the type for determining their contribution to the test fixture.  Instance members are
        /// only included if the type is not abstract.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The binding flags for enumerating members</returns>
        protected virtual BindingFlags GetMemberBindingFlags(ITypeInfo type)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            if (!type.IsAbstract)
                bindingFlags |= BindingFlags.Instance;
            return bindingFlags;
        }

        /// <summary>
        /// Infers whether the type is a test type based on its structure.
        /// Returns true if the type any associated patterns, if it has
        /// non-nested type members (subject to <see cref="GetMemberBindingFlags" />)
        /// with patterns, or if it has generic parameters with patterns.
        /// </summary>
        /// <param name="evaluator">The pattern evaluator</param>
        /// <param name="type">The type</param>
        /// <returns>True if the type is likely a test type</returns>
        protected virtual bool InferTestType(IPatternEvaluator evaluator, ITypeInfo type)
        {
            if (evaluator.HasPatterns(type))
                return true;

            BindingFlags bindingFlags = GetMemberBindingFlags(type);
            return HasCodeElementWithPattern(evaluator, type.GetMethods(bindingFlags))
                || HasCodeElementWithPattern(evaluator, type.GetProperties(bindingFlags))
                || HasCodeElementWithPattern(evaluator, type.GetFields(bindingFlags))
                || HasCodeElementWithPattern(evaluator, type.GetConstructors(bindingFlags))
                || HasCodeElementWithPattern(evaluator, type.GetEvents(bindingFlags))
                || (type.IsGenericTypeDefinition && HasCodeElementWithPattern(evaluator, type.GenericArguments));
        }

        private static bool HasCodeElementWithPattern<T>(IPatternEvaluator evaluator, IEnumerable<T> elements)
            where T : ICodeElementInfo
        {
            foreach (T element in elements)
                if (evaluator.HasPatterns(element))
                    return true;
            return false;
        }

        private sealed class AutomaticImpl : TestTypePatternAttribute
        {
            public override void Consume(IPatternScope containingScope, ICodeElementInfo codeElement, bool skipChildren)
            {
                ITypeInfo type = codeElement as ITypeInfo;
                if (type != null)
                {
                    if (InferTestType(containingScope.Evaluator, type))
                    {
                        base.Consume(containingScope, codeElement, skipChildren);
                    }
                    else
                    {
                        ConsumeNestedTypes(containingScope, type);
                    }
                }
            }
        }
    }
}