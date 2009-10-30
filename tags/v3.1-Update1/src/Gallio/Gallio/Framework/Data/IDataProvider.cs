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

namespace Gallio.Framework.Data
{
    /// <summary>
    /// A data provider generates an enumeration of <see cref="IDataItem" />s
    /// given a collection of <see cref="DataBinding"/>s to satisfy.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Gets an enumeration of data items that can supply values for a given collection of bindings.
        /// It should produce items even if some of the requested bindings cannot be fulfilled.
        /// </summary>
        /// <param name="bindings">The bindings that are requested.</param>
        /// <param name="includeDynamicItems">If true, includes items that may be dynamically
        /// generated in the result set.  Otherwise excludes such items and only returns
        /// those that are statically known a priori.</param>
        /// <returns>The enumeration of data items.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bindings"/> is null.</exception>
        IEnumerable<IDataItem> GetItems(ICollection<DataBinding> bindings, bool includeDynamicItems);
    }
}
