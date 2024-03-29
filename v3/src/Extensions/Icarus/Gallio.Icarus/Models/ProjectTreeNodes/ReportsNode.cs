// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
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

using System.Collections.Generic;
using Gallio.Common.IO;
using Gallio.Icarus.Commands;
using Gallio.UI.Menus;

namespace Gallio.Icarus.Models.ProjectTreeNodes
{
    internal sealed class ReportsNode : ProjectTreeNode
    {
        public ReportsNode(IProjectTreeModel projectTreeModel, 
            IFileSystem fileSystem)
        {
            // TODO: i18n
            Text = "Reports";
            Image = Properties.Resources.Report.ToBitmap();

            var deleteAllReportsCommand = new MenuCommand
            {
                Command = new DeleteAllReportsCommand(projectTreeModel, fileSystem),
                Text = "Delete all reports"
            };
            Commands = new List<MenuCommand> { deleteAllReportsCommand };
        }
    }
}
