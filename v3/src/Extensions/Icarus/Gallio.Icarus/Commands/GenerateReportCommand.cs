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
using System.IO;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Icarus.Controllers.Interfaces;

namespace Gallio.Icarus.Commands
{
    internal class GenerateReportCommand : ICommand
    {
        private readonly IProjectController projectController;
        private readonly ITestController testController;
        private readonly IReportController reportController;

        public GenerateReportCommand(IProjectController projectController, ITestController testController,
            IReportController reportController)
        {
            this.projectController = projectController;
            this.testController = testController;
            this.reportController = reportController;
        }

        public void Execute(IProgressMonitor progressMonitor)
        {
            using (progressMonitor.BeginTask("Generating report", 100))
            {
                var reportFolder = Path.Combine(Path.GetDirectoryName(projectController.ProjectFileName),
                    "Reports");

                if (progressMonitor.IsCanceled)
                    throw new OperationCanceledException();

                testController.ReadReport(report => reportController.GenerateReport(report, 
                    reportFolder, progressMonitor));
            }
        }
    }
}
