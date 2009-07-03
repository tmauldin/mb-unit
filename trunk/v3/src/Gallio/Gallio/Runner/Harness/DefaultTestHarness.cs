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
using System.Globalization;
using System.Threading;
using Gallio.Common;
using Gallio.Common.IO;
using Gallio.Common.Concurrency;
using Gallio.Model.Messages;
using Gallio.Model.Serialization;
using Gallio.Runtime.Loader;
using Gallio.Runtime.ProgressMonitoring;
using Gallio.Runtime;
using Gallio.Model;
using Gallio.Model.Execution;
using Gallio.Common.Reflection;

namespace Gallio.Runner.Harness
{
    /// <summary>
    /// Default implementation of a test harness.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="RuntimeAccessor" /> must be initialized prior to using this factory
    /// because the tests will run within the current <see cref="AppDomain" /> and
    /// <see cref="RuntimeAccessor"/>.
    /// </para>
    /// </remarks>
    public class DefaultTestHarness : ITestHarness
    {
        private readonly ITestContextTracker contextTracker;
        private readonly ILoader loader;
        private readonly ITestEnvironmentManager environmentManager;
        private readonly ITestFrameworkManager frameworkManager;

        private bool isDisposed;

        private string workingDirectory;
        private TestExplorationContext explorationContext;
        private TestModel model;

        /// <summary>
        /// Creates a test harness.
        /// </summary>
        /// <param name="contextTracker">The test context tracker.</param>
        /// <param name="loader">The loader.</param>
        /// <param name="environmentManager">The test environments manager.</param>
        /// <param name="frameworkManager">The test framework manager.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contextTracker"/>,
        /// <paramref name="loader "/>, <paramref name="environmentManager"/>
        /// or <paramref name="frameworkManager"/> is null.</exception>
        public DefaultTestHarness(ITestContextTracker contextTracker, ILoader loader,
            ITestEnvironmentManager environmentManager, ITestFrameworkManager frameworkManager)
        {
            if (contextTracker == null)
                throw new ArgumentNullException("contextTracker");
            if (loader == null)
                throw new ArgumentNullException("loader");
            if (environmentManager == null)
                throw new ArgumentNullException("environmentManager");
            if (frameworkManager == null)
                throw new ArgumentNullException("frameworkManager");

            this.contextTracker = contextTracker;
            this.loader = loader;
            this.environmentManager = environmentManager;
            this.frameworkManager = frameworkManager;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                explorationContext = null;
                model = null;
            }
        }

        /// <inheritdoc />
        public TestExplorationContext TestExplorationContext
        {
            get
            {
                ThrowIfDisposed();
                return explorationContext;
            }
        }

        /// <inheritdoc />
        public TestModel TestModel
        {
            get
            {
                ThrowIfDisposed();
                return model;
            }
        }

        /// <inheritdoc />
        public void Load(TestPackageConfig packageConfig, IProgressMonitor progressMonitor)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException(@"progressMonitor");
            if (packageConfig == null)
                throw new ArgumentNullException("packageConfig");

            ThrowIfDisposed();

            if (explorationContext != null)
                throw new InvalidOperationException("A package has already been loaded.");

            using (progressMonitor.BeginTask("Loading test package.", 10))
            {
                progressMonitor.SetStatus("Performing pre-processing.");

                workingDirectory = packageConfig.WorkingDirectory ?? Environment.CurrentDirectory;

                using (SwitchWorkingDirectory())
                {
                    explorationContext = new TestExplorationContext(packageConfig, Reflector.NativeReflectionPolicy, loader);

                    foreach (string path in packageConfig.HintDirectories)
                        loader.AssemblyResolverManager.AddHintDirectory(path);

                    foreach (string assemblyFile in packageConfig.Files)
                        loader.AssemblyResolverManager.AddHintDirectory(FileUtils.GetFullPathOfParentDirectory(assemblyFile));

                    progressMonitor.Worked(1);

                    using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(8))
                        LoadAssemblies(packageConfig.Files, subProgressMonitor);
                }
            }
        }

        private void LoadAssemblies(ICollection<string> assemblyFiles, IProgressMonitor progressMonitor)
        {
            if (assemblyFiles.Count == 0)
                return;

            using (progressMonitor.BeginTask("Loading test assemblies.", assemblyFiles.Count))
            {
                foreach (string assemblyFile in explorationContext.Config.Files)
                {
                    progressMonitor.SetStatus("Loading: " + assemblyFile + ".");

                    LoadAssemblyFrom(assemblyFile);

                    progressMonitor.Worked(1);
                }
            }
        }

        private void LoadAssemblyFrom(string assemblyFile)
        {
            if (assemblyFile == null)
                throw new ArgumentNullException(@"assemblyFile");

            try
            {
                IAssemblyInfo assembly = Reflector.Wrap(loader.LoadAssemblyFrom(assemblyFile));
                explorationContext.AddAssembly(assembly);
            }
            catch (Exception ex)
            {
                throw new RunnerException(String.Format(CultureInfo.CurrentCulture,
                    "Could not load test assembly from '{0}'.", assemblyFile), ex);
            }
        }

        /// <inheritdoc />
        public void Explore(TestExplorationOptions testExplorationOptions, ITestExplorationListener testExplorationListener, IProgressMonitor progressMonitor)
        {
            if (testExplorationOptions == null)
                throw new ArgumentNullException("testExplorationOptions");
            if (progressMonitor == null)
                throw new ArgumentNullException("progressMonitor");

            ThrowIfDisposed();

            if (explorationContext == null)
                throw new InvalidOperationException("No test package has been loaded.");

            using (progressMonitor.BeginTask("Building test model.", 10))
            {
                using (SwitchWorkingDirectory())
                {
                    model = new TestModel(explorationContext);

                    ITestExplorer explorer = frameworkManager.GetTestExplorer(frameworkId => explorationContext.Config.IsFrameworkRequested(frameworkId));

                    TestSource source = new TestSource();
                    foreach (IAssemblyInfo assembly in explorationContext.Assemblies)
                        source.AddAssembly(assembly);

                    explorer.Explore(model, source, null);
                }

                foreach (Annotation annotation in model.Annotations)
                    testExplorationListener.NotifyAnnotationAdded(new AnnotationData(annotation));

                testExplorationListener.NotifySubtreeMerged(null, new TestData(model.RootTest));
            }
        }

        /// <inheritdoc />
        public void Run(TestExecutionOptions testExecutionOptions, ITestExecutionListener testExecutionListener, IProgressMonitor progressMonitor)
        {
            if (testExecutionOptions == null)
                throw new ArgumentNullException("testExecutionOptions");
            if (testExecutionListener == null)
                throw new ArgumentNullException("testExecutionListener");
            if (progressMonitor == null)
                throw new ArgumentNullException(@"progressMonitor");

            ThrowIfDisposed();

            if (model == null)
                throw new InvalidOperationException("The test model has not been built.");

            using (progressMonitor.BeginTask("Running tests.", 100))
            {
                using (SwitchWorkingDirectory())
                {
                    IDisposable environmentState = null;
                    try
                    {
                        progressMonitor.SetStatus("Setting up the test environment.");
                        environmentState = environmentManager.SetUpAppDomain();
                        progressMonitor.Worked(1);

                        progressMonitor.SetStatus("Sorting tests.");
                        ObservableTestContextManager contextManager =
                            new ObservableTestContextManager(contextTracker, testExecutionListener);
                        ITestCommand rootTestCommand = TestCommandFactory.BuildCommands(model,
                            testExecutionOptions.FilterSet, testExecutionOptions.ExactFilter, contextManager);
                        progressMonitor.Worked(2);
                        progressMonitor.SetStatus(@"");

                        using (IProgressMonitor subProgressMonitor = progressMonitor.CreateSubProgressMonitor(96))
                            RunAllTestCommands(rootTestCommand, testExecutionOptions, subProgressMonitor);
                    }
                    finally
                    {
                        progressMonitor.SetStatus("Tearing down the test environment.");
                        if (environmentState != null)
                            environmentState.Dispose();
                        progressMonitor.Worked(1);
                        progressMonitor.SetStatus(@"");
                    }
                }
            }
        }

        private void RunAllTestCommands(ITestCommand rootTestCommand, TestExecutionOptions options, IProgressMonitor progressMonitor)
        {
            Action action = delegate
            {
                if (rootTestCommand != null)
                {
                    using (contextTracker.EnterContext(null))
                    {
                        Func<ITestController> rootTestControllerFactory = rootTestCommand.Test.TestControllerFactory;

                        if (rootTestControllerFactory != null)
                        {
                            using (ITestController controller = rootTestControllerFactory())
                                controller.RunTests(rootTestCommand, null, options, progressMonitor);
                        }
                    }
                }
            };

            if (options.SingleThreaded)
            {
                try
                {
                    using (environmentManager.SetUpThread())
                    {
                        action();
                    }
                }
                catch (Exception ex)
                {
                    throw new RunnerException("A fatal exception occurred while running all test commands.", ex);
                }
            }
            else
            {
                var task = new TestThreadTask("Test Runner", action, environmentManager);

                // Use STA as the default for all tests.  A test framework may of course choose
                // to create its own threads with different apartment states.
                task.ApartmentState = ApartmentState.STA;
                task.Run(null);

                if (task.Result.Exception != null)
                    throw new RunnerException("A fatal exception occurred while running all test commands.",
                        task.Result.Exception);
            }
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException("The test harness has been disposed.");
        }

        /// <inheritdoc />
        public void Unload(IProgressMonitor progressMonitor)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException(@"progressMonitor");

            ThrowIfDisposed();

            using (progressMonitor.BeginTask("Unloading tests.", 1))
            {
                workingDirectory = null;
                explorationContext = null;
                model = null;
            }
        }

        private CurrentDirectorySwitcher SwitchWorkingDirectory()
        {
            if (String.IsNullOrEmpty(workingDirectory))
                return null;
            return new CurrentDirectorySwitcher(workingDirectory);
        }
    }
}
