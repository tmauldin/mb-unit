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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Gallio.Model.Serialization;
using Gallio.Utilities;

namespace Gallio.Hosting
{
    /// <summary>
    /// Provides configuration parameters for setting up the <see cref="Runtime" />.
    /// </summary>
    [Serializable]
    [XmlRoot("runtimeSetup", Namespace = SerializationUtils.XmlNamespace)]
    [XmlType(Namespace = SerializationUtils.XmlNamespace)]
    public sealed class RuntimeSetup
    {
        private readonly List<string> pluginDirectories;
        private string runtimeFactoryType;
        private string installationPath;
        private string configurationFilePath;

        /// <summary>
        /// Creates a default runtime setup.
        /// </summary>
        public RuntimeSetup()
        {
            pluginDirectories = new List<string>();
        }

        /// <summary>
        /// Gets or sets the full assembly-qualified name of a type that
        /// implements <see cref="IRuntimeFactory" /> that should be used
        /// to create the runtime.  If the value is null, the built-in
        /// default runtime factory will be used.
        /// </summary>
        [XmlElement("runtimeFactoryType", IsNullable=false)]
        public string RuntimeFactoryType
        {
            get { return runtimeFactoryType; }
            set { runtimeFactoryType = value; }
        }

        /// <summary>
        /// Gets list of relative or absolute paths of directories to be
        /// searched for plugin configuration files in addition to the
        /// primary Gallio directories.
        /// </summary>
        [XmlArray("pluginDirectories", IsNullable = false)]
        [XmlArrayItem("pluginDirectory", typeof(string), IsNullable = false)]
        public List<string> PluginDirectories
        {
            get { return pluginDirectories; }
        }

        /// <summary>
        /// Gets or sets the installation path, or null to determine it automatically
        /// based on the location of the primary runtime assemblies.  The installation
        /// path specifies where the standard runtime plugins are located.
        /// </summary>
        /// <value>
        /// The installation path.  Default is <c>null</c>.
        /// </value>
        [XmlAttribute("installationPath")]
        public string InstallationPath
        {
            get { return installationPath; }
            set { installationPath = value; }
        }

        /// <summary>
        /// Gets or sets the path of the primary configuration file to be
        /// loaded by the runtime (if it exists).  This is useful
        /// when Gallio is launched by a library instead of as a standalone
        /// executable.
        /// </summary>
        /// <value>
        /// The primary configuration file path.  Default is null to load the
        /// configuration from the <see cref="AppDomain" />.
        /// </value>
        [XmlAttribute("configurationFilePath")]
        public string ConfigurationFilePath
        {
            get { return configurationFilePath; }
            set { configurationFilePath = value; }
        }

        /// <summary>
        /// Creates a deep copy of the runtime setup parameters.
        /// </summary>
        /// <returns>The copy</returns>
        public RuntimeSetup Copy()
        {
            RuntimeSetup copy = new RuntimeSetup();
            copy.pluginDirectories.AddRange(pluginDirectories);
            copy.runtimeFactoryType = runtimeFactoryType;
            copy.installationPath = installationPath;
            copy.configurationFilePath = configurationFilePath;
            return copy;
        }

        /// <summary>
        /// Makes all paths in this instance absolute.
        /// </summary>
        /// <param name="baseDirectory">The base directory for resolving relative paths,
        /// or null to use the current directory</param>
        public void Canonicalize(string baseDirectory)
        {
            FileUtils.CanonicalizePaths(baseDirectory, pluginDirectories);
            installationPath = FileUtils.CanonicalizePath(baseDirectory, installationPath);
            configurationFilePath = FileUtils.CanonicalizePath(baseDirectory, configurationFilePath);
        }
    }
}
