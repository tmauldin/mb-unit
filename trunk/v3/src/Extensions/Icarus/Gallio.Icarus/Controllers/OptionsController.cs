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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using Gallio.Common.IO;
using Gallio.Icarus.Controllers.Interfaces;
using Gallio.Icarus.Options;
using Gallio.Model;
using Gallio.Icarus.Utilities;
using Gallio.Runtime.Logging;

namespace Gallio.Icarus.Controllers
{
    public sealed class OptionsController : NotifyController, IOptionsController
    {
        private readonly IFileSystem fileSystem;
        private MRUList recentProjects;

        private readonly List<string> unselectedTreeViewCategoriesList = new List<string>();

        public bool AlwaysReloadAssemblies
        {
            get { return Settings.AlwaysReloadAssemblies; }
            set { Settings.AlwaysReloadAssemblies = value; }
        }

        public bool RunTestsAfterReload
        {
            get { return Settings.RunTestsAfterReload; }
            set { Settings.RunTestsAfterReload = value; }
        }

        public string TestStatusBarStyle
        {
            get { return Settings.TestStatusBarStyle; }
            set { Settings.TestStatusBarStyle = value; }
        }

        public bool ShowProgressDialogs
        {
            get { return Settings.ShowProgressDialogs; }
            set { Settings.ShowProgressDialogs = value; }
        }

        public bool RestorePreviousSettings
        {
            get { return Settings.RestorePreviousSettings; }
            set { Settings.RestorePreviousSettings = value; }
        }

        public string TestRunnerFactory
        {
            get { return Settings.TestRunnerFactory; }
            set
            {
                Settings.TestRunnerFactory = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TestRunnerFactory"));
            }
        }

        public BindingList<string> PluginDirectories { get; private set; }

        public BindingList<string> SelectedTreeViewCategories { get; private set; }

        public BindingList<string> UnselectedTreeViewCategories { get; private set; }

        public Color PassedColor
        {
            get { return Color.FromArgb(Settings.PassedColor); }
            set { Settings.PassedColor = value.ToArgb(); }
        }

        public Color FailedColor
        {
            get { return Color.FromArgb(Settings.FailedColor); }
            set { Settings.FailedColor = value.ToArgb(); }
        }

        public Color InconclusiveColor
        {
            get { return Color.FromArgb(Settings.InconclusiveColor); }
            set { Settings.InconclusiveColor = value.ToArgb(); }
        }

        public Color SkippedColor
        {
            get { return Color.FromArgb(Settings.SkippedColor); }
            set { Settings.SkippedColor = value.ToArgb(); }
        }

        public double UpdateDelay
        {
            get { return 1000; }
        }

        public Size Size
        {
            get { return Settings.Size; }
            set { Settings.Size = value; }
        }

        public Point Location
        {
            get { return Settings.Location; }
            set { Settings.Location = value; }
        }

        public MRUList RecentProjects
        {
            get
            {
                if (recentProjects == null)
                {
                    // remove any dead projects
                    var list = new List<string>();
                    foreach(var proj in Settings.RecentProjects)
                    {
                        if (fileSystem.FileExists(proj))
                            list.Add(proj);
                    }
                    Settings.RecentProjects.Clear();
                    Settings.RecentProjects.AddRange(list);

                    recentProjects = new MRUList(Settings.RecentProjects, 10);

                    recentProjects.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == "Items")
                                OnPropertyChanged(new PropertyChangedEventArgs("RecentProjects"));
                        };
                }
                return recentProjects;
            }
        }

        public LogSeverity MinLogSeverity
        {
            get { return Settings.MinLogSeverity; }
            set { Settings.MinLogSeverity = value; }
        }

        public bool AnnotationsShowErrors
        {
            get { return Settings.AnnotationsShowErrors; }
            set { Settings.AnnotationsShowErrors = value; }
        }

        public bool AnnotationsShowWarnings
        {
            get { return Settings.AnnotationsShowWarnings; }
            set { Settings.AnnotationsShowWarnings = value; }
        }

        public bool AnnotationsShowInfos
        {
            get { return Settings.AnnotationsShowInfos; }
            set { Settings.AnnotationsShowInfos = value; }
        }

        public BindingList<string> TestRunnerExtensions { get; private set; }

        public bool TestTreeSplitNamespaces
        {
            get { return Settings.TestTreeSplitNamespaces; }
            set { Settings.TestTreeSplitNamespaces = value; }
        }

        public Settings Settings
        {
            get { return OptionsManager.Settings; }
        }

        public OptionsController(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void Load()
        {
            if (Settings.TreeViewCategories.Count == 0)
            {
                // add default categories
                Settings.TreeViewCategories.AddRange(new[] { "Namespace", MetadataKeys.AuthorName, 
                    MetadataKeys.Category, MetadataKeys.Importance, MetadataKeys.TestsOn });
            }

            // add unselected categories for treeview (test explorer)
            unselectedTreeViewCategoriesList.Clear();
            foreach (FieldInfo fi in typeof(MetadataKeys).GetFields())
            {
                if (!Settings.TreeViewCategories.Contains(fi.Name))
                    unselectedTreeViewCategoriesList.Add(fi.Name);
            }

            // set up bindable lists (for options dialogs)
            PluginDirectories = new BindingList<string>(Settings.PluginDirectories);
            SelectedTreeViewCategories = new BindingList<string>(Settings.TreeViewCategories);
            UnselectedTreeViewCategories = new BindingList<string>(unselectedTreeViewCategoriesList);
            TestRunnerExtensions = new BindingList<string>(Settings.TestRunnerExtensions);
        }

        public void Save()
        {
            OptionsManager.Save();
        }

        public bool GenerateReportAfterTestRun
        {
            get { return Settings.GenerateReportAfterTestRun; }
            set { Settings.GenerateReportAfterTestRun = value; }
        }

        public void Cancel()
        {
            OptionsManager.Load();
        }
    }
}