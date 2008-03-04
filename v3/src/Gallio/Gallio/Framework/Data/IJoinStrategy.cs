// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
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

using System.Collections.Generic;

namespace Gallio.Framework.Data
{
    /// <summary>
    /// <para>
    /// A join strategy combines rows from multiple providers into products
    /// according to some algorithm.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A join strategy performs much the same purpose as a "JOIN" in
    /// query languages.  It specifies how rows from multiple providers are to
    /// be correlated to create a sequence of aggregate rows.
    /// </para>
    /// </remarks>
    /// <seealso cref="JoinedDataSet"/>
    public interface IJoinStrategy
    {
        /// <summary>
        /// Joins the rows from each provider into a sequence of aggregate rows.
        /// </summary>
        /// <remarks>
        /// The number of rows in each row-list must equal the number
        /// of providers in the <paramref name="providers"/> list because
        /// each row-list should contain exactly one row taken from each
        /// provider.
        /// </remarks>
        /// <param name="providers">The list of providers</param>
        /// <param name="bindingsPerProvider">The list of bindings per provider</param>
        /// <returns>An enumeration of row-lists consisting of exactly one row from
        /// each provider and indexed in the same order as the <paramref name="providers"/>
        /// <param name="includeDynamicRows">If true, includes rows that may be dynamically
        /// generated in the result set.  Otherwise excludes such rows and only returns
        /// those that are statically known a priori.</param>
        /// collection</returns>
        IEnumerable<IList<IDataRow>> Join(IList<IDataProvider> providers, IList<ICollection<DataBinding>> bindingsPerProvider,
            bool includeDynamicRows);
    }
}
