﻿// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
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
using System.Text;
using Gallio;
using Gallio.Framework;
using System.Collections;

namespace MbUnit.Framework
{
    /// <summary>
    /// <para>
    /// A general-purpose structural equality comparer that defines a fully customizable equality operation without the 
    /// need to implement <see cref="IEquatable{T}"/>.
    /// </para>
    /// <para>
    /// That equality comparer can be used in any MbUnit assertion that takes an <see cref="IEqualityComparer{T}"/> 
    /// object as argument, such as <see cref="Assert.AreEqual{T}(T, T, IEqualityComparer{T})"/>.
    /// </para>
    /// <para>
    /// The comparer must be initialized with a list of one or several matching criteria. Two instances are considered
    /// equal by the comparer if and only if all the criteria are true for that pair of instances.
    /// </para>
    /// <para>
    /// <example>
    /// The following example contains a test fixture that checks for the equality between two <strong>Foo</strong> 
    /// objects by using the well known <see cref="Assert.AreEqual{T}(T, T, IEqualityComparer{T})"/> assertion. The custom equality comparer which 
    /// is provided to the assertion method, declares two <strong>Foo</strong> objects equal when the <strong>Value</strong> 
    /// fields have the same parity, and when the <strong>Text</strong> fields are equal (case insensitive):
    /// <code><![CDATA[
    /// public class Foo
    /// {
    ///     public int Value;
    ///     public string Text;
    /// }
    /// 
    /// [TestFixture]
    /// public class FooTest
    /// {
    ///     [Test]
    ///     public void MyTest()
    ///     {
    ///         var foo1 = new Foo() { Value = 123, Text = "Hello" };    
    ///         var foo2 = new Foo() { Value = 789, Text = "hElLo" };
    ///         
    ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
    ///         {
    ///             { x => x.Value, (x, y) => x % 2 == y % 2 },
    ///             { x => x.Text, (x, y) => String.Compare(x, y, true) == 0 }
    ///         });
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the compared objects.</typeparam>
    public class StructuralEqualityComparer<T> : IEqualityComparer<T>, IEnumerable<EqualityComparison<T>>
    {
        private readonly List<EqualityComparison<T>> predicates = new List<EqualityComparison<T>>();

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The values returned by the accessor are compared by using a default comparison evaluator.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int Value;
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Value = 123 };    
        ///         var foo2 = new Foo() { Value = 123 };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { x => x.Value },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value returned by the accessor.</typeparam>
        /// <param name="accessor">An accessor that gets a value from the tested object.</param>
        /// <exception cref="ArgumentNullException">The specified accessor argument is a null reference.</exception>
        public void Add<TValue>(Accessor<T, TValue> accessor)
        {
            Add(accessor, (x, y) => ComparisonSemantics.Equals<TValue>(x, y));
        }

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The values returned by the accessor are compared by using the specified comparer object.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int Value;
        /// }
        /// 
        /// public class MyComparer : IEqualityComparer<int>
        /// {
        ///     public bool Equals(int x, int y)
        ///     {
        ///         return x == y;    
        ///     }
        ///     
        ///     public int GetHashCode(int obj)
        ///     {
        ///         return obj;
        ///     }
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Value = 123 };    
        ///         var foo2 = new Foo() { Value = 123 };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { x => x.Value, new MyComparer() },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value returned by the accessor.</typeparam>
        /// <param name="accessor">An accessor that gets a value from the tested object.</param>
        /// <param name="comparer">A comparer instance for the values returned by the accessor.</param>
        /// <exception cref="ArgumentNullException">One of the specified arguments is null reference.</exception>
        public void Add<TValue>(Accessor<T, TValue> accessor, IEqualityComparer<TValue> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            Add(accessor, (x, y) => comparer.Equals(x, y));
        }

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The values returned by the accessor are compared by using the specified comparison delegate.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int Value;
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Value = 123 };    
        ///         var foo2 = new Foo() { Value = 123 };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { x => x.Value, (x, y) => x == y },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value returned by the accessor.</typeparam>
        /// <param name="accessor">An accessor that gets a value from the tested object.</param>
        /// <param name="comparison">A equality comparison delegate to compare the values returned by the accessor.</param>
        /// <exception cref="ArgumentNullException">One of the specified arguments is null reference.</exception>
        public void Add<TValue>(Accessor<T, TValue> accessor, EqualityComparison<TValue> comparison)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException("accessor");
            }

            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }

            predicates.Add((x, y) => comparison(accessor(x), accessor(y)));
        }

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The evaluation process is done through the specified comparison delegate.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int Value;
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Value = 123 };    
        ///         var foo2 = new Foo() { Value = 123 };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { (x, y) => x.Value == y.Value },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="comparison">An equality comparison delegate to directly compare two instances..</param>
        /// <exception cref="ArgumentNullException">The specified comparison argument is a null reference.</exception>
        public void Add(EqualityComparison<T> comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }

            predicates.Add(comparison);
        }

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The evaluation process is done through the specified comparer object.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int Value;
        /// }
        /// 
        /// public class MyComparer : IEqualityComparer<Foo>
        /// {
        ///     public bool Equals(Foo x, Foo y)
        ///     {
        ///         return x.Value == y.Value;    
        ///     }
        ///     
        ///     public int GetHashCode(int obj)
        ///     {
        ///         return obj;
        ///     }
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Value = 123 };    
        ///         var foo2 = new Foo() { Value = 123 };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { new MyComparer() },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <param name="comparer">An comparer object to directly compare two instances.</param>
        /// <exception cref="ArgumentNullException">The specified comparer argument is a null reference.</exception>
        public void Add(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            predicates.Add(comparer.Equals);
        }

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The enumerations of values returned by the accessor are compared one by one, 
        /// by using the specified comparer.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int[] Values;
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Values = new int[] { 1, 2, 3, 4, 5 } };    
        ///         var foo2 = new Foo() { Values = new int[] { 1, 2, 3, 4, 5 } };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { x => x.Values,
        ///               new StructuralEqualityComparer<int> { { x => x } }
        ///             },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value returned by the accessor.</typeparam>
        /// <param name="accessor">An accessor that gets an enumeration of values from the tested object.</param>
        /// <param name="comparer">A comparer instance for the values returned by the accessor.</param>
        /// <exception cref="ArgumentNullException">One of the specified arguments is null reference.</exception>
        public void Add<TValue>(Accessor<T, IEnumerable<TValue>> accessor, IEqualityComparer<TValue> comparer)
        {
            Add(accessor, comparer, StructuralEqualityComparerOptions.Default);
        }

        /// <summary>
        /// <para>
        /// Adds a matching criterion to the structural equality comparer.
        /// </para>
        /// <para>
        /// The enumerations of values returned by the accessor are compared by using the specified comparer.
        /// <example>
        /// <code><![CDATA[
        /// public class Foo
        /// {
        ///     public int[] Values;
        /// }
        /// 
        /// [TestFixture]
        /// public class FooTest
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         var foo1 = new Foo() { Values = new int[] { 1, 2, 3, 4, 5 } };    
        ///         var foo2 = new Foo() { Values = new int[] { 5, 4, 3, 2, 1 } };
        ///         
        ///         Assert.AreEqual(foo1, foo2, new StructuralEqualityComparer<Foo>
        ///         {
        ///             { x => x.Values,
        ///               new StructuralEqualityComparer<int> { { x => x } },
        ///               StructuralEqualityComparerOptions.IgnoreEnumerableOrder
        ///             },
        ///         });
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// </para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value returned by the accessor.</typeparam>
        /// <param name="accessor">An accessor that gets an enumeration of values from the tested object.</param>
        /// <param name="comparer">A comparer instance for the values returned by the accessor.</param>
        /// <param name="options">Some options indicating how to compare the enumeration of values returned by the accessor.</param>
        /// <exception cref="ArgumentNullException">One of the specified arguments is null reference.</exception>
        public void Add<TValue>(Accessor<T, IEnumerable<TValue>> accessor, IEqualityComparer<TValue> comparer, StructuralEqualityComparerOptions options)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            if ((options & StructuralEqualityComparerOptions.IgnoreEnumerableOrder) != 0)
            {
                predicates.Add((x, y) => CompareEnumerablesIgnoringOrder(accessor(x), accessor(y), comparer));
            }
            else
            {
                predicates.Add((x, y) => CompareEnumerables(accessor(x), accessor(y), comparer));
            }
        }

        private bool CompareEnumerables<TValue>(IEnumerable<TValue> xEnumerable, IEnumerable<TValue> yEnumerable, IEqualityComparer<TValue> comparer)
        {
            var xEnumerator = xEnumerable.GetEnumerator();
            var yEnumerator = yEnumerable.GetEnumerator();

            while (xEnumerator.MoveNext())
            {
                if (!yEnumerator.MoveNext())
                {
                    return false;
                }

                TValue xValue = xEnumerator.Current;
                TValue yValue = yEnumerator.Current;

                if (!comparer.Equals(xValue, yValue))
                {
                    return false;
                }
            }

            if (yEnumerator.MoveNext())
            {
                return false;
            }

            return true;
        }

        private bool CompareEnumerablesIgnoringOrder<TValue>(IEnumerable<TValue> xEnumerable, IEnumerable<TValue> yEnumerable, IEqualityComparer<TValue> comparer)
        {
            var table = new MatchTable<TValue>(comparer.Equals);

            foreach (TValue xElement in xEnumerable)
                table.AddLeftValue(xElement);

            foreach (TValue yElement in yEnumerable)
                table.AddRightValue(yElement);

            return (table.NonEqualCount == 0);
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            return predicates.TrueForAll(predicate => predicate(x, y));
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        /// <summary>
        /// Returns a strongly-typed enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A strongly-typed enumerator that iterates through the collection.</returns>
        public IEnumerator<EqualityComparison<T>> GetEnumerator()
        {
            return predicates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var predicate in predicates)
            {
                yield return predicate;
            }
        }
    }
}