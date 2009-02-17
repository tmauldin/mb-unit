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
using System.Drawing;
using Gallio.Icarus.Controllers.EventArgs;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Runtime.Logging;
using Gallio.Utilities;

namespace Gallio.Icarus.Controllers
{
    public class RuntimeLogController : BaseLogger, IRuntimeLogController
    {
        public event EventHandler<RuntimeLogEventArgs> LogMessage;

        protected override void LogImpl(LogSeverity severity, string message, Exception exception)
        {
            Color color = Color.Black;
            switch (severity)
            {
                case LogSeverity.Error:
                    color = Color.Red;
                    break;

                case LogSeverity.Warning:
                    color = Color.Gold;
                    break;

                case LogSeverity.Important:
                    color = Color.Black;
                    break;

                case LogSeverity.Info:
                    color = Color.Gray;
                    break;

                case LogSeverity.Debug:
                    color = Color.DarkGray;
                    break;
            }

            if (LogMessage == null)
                return;

            LogMessage(this, new RuntimeLogEventArgs(message, color));

            if (exception != null)
                LogMessage(this, new RuntimeLogEventArgs(ExceptionUtils.SafeToString(exception), color));
        }
    }
}