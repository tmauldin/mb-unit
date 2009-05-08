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
using Gallio.Model;
using Gallio.Common.Reflection;

namespace Gallio.Framework.Pattern
{
    /// <summary>
    /// <para>
    /// Associates an annotation message with a code element.  When the code element
    /// is processed as part of building some other test, the annotation message will
    /// be emitted.  This may be used to signal error, warning, and informational
    /// messages associated with the code element in question.
    /// </para>
    /// <para>
    /// Many annotations are automatically generated by the framework.  For example,
    /// improperly used attributes generally cause error annotations to be emitted.
    /// This attribute provides an easy way for test authors to leverage the annotation
    /// mechanism for other discretionary purposes where it is desirable to draw
    /// attention to a particular code element.
    /// </para>
    /// <para>
    /// Note that the annotation will only be emitted if it is encountered during the
    /// process of building a test.  If for some reason the annotated code element
    /// is not processed, then the attribute will have no effect.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public abstract class AnnotationPatternAttribute : PatternAttribute
    {
        private readonly AnnotationType type;
        private readonly string message;
        private string details;

        /// <summary>
        /// Associates an annotation message of the specified type with the code element.
        /// </summary>
        /// <param name="type">The annotation type</param>
        /// <param name="message">The annotation message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null</exception>
        public AnnotationPatternAttribute(AnnotationType type, string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            this.type = type;
            this.message = message;
        }

        /// <summary>
        /// Gets the annotation type.
        /// </summary>
        public AnnotationType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the annotation message.
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Gets or sets optional detail text of the annotation, or null if none.
        /// </summary>
        public string Details
        {
            get { return details; }
            set { details = value; }
        }

        /// <inheritdoc />
        public override void Process(IPatternScope scope, ICodeElementInfo codeElement)
        {
            scope.TestModelBuilder.AddAnnotation(new Annotation(type, codeElement, message, details));
        }
    }
}
