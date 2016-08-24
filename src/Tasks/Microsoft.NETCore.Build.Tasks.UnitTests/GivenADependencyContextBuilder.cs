﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions.Json;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NuGet.ProjectModel;
using Xunit;

namespace Microsoft.NETCore.Build.Tasks.UnitTests
{
    public class GivenADependencyContextBuilder
    {
        /// <summary>
        /// Tests that DependencyContextBuilder generates DependencyContexts correctly.
        /// </summary>
        [Theory]
        [InlineData("dotnet.new", "1.0.0")]
        [InlineData("simple.dependencies", "1.0.0")]
        public void ItBuildsDependencyContextsFromProjectLockFiles(string mainProjectName, string mainProjectVersion)
        {
            LockFile lockFile = TestLockFiles.GetLockFile(mainProjectName);

            DependencyContext dependencyContext = new DependencyContextBuilder().Build(
                mainProjectName,
                mainProjectVersion,
                compilerOptions: null,
                lockFile: lockFile,
                framework: FrameworkConstants.CommonFrameworks.NetCoreApp10,
                runtime: null);

            JObject result = Save(dependencyContext);
            JObject baseline = ReadJson($"{mainProjectName}.deps.json");

            baseline
                .Should()
                .BeEquivalentTo(result);
        }

        private static JObject ReadJson(string path)
        {
            using (JsonTextReader jsonReader = new JsonTextReader(File.OpenText(path)))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<JObject>(jsonReader);
            }
        }

        private JObject Save(DependencyContext dependencyContext)
        {
            using (var memoryStream = new MemoryStream())
            {
                new DependencyContextWriter().Write(dependencyContext, memoryStream);
                using (var readStream = new MemoryStream(memoryStream.ToArray()))
                {
                    using (var textReader = new StreamReader(readStream))
                    {
                        using (var reader = new JsonTextReader(textReader))
                        {
                            return JObject.Load(reader);
                        }
                    }
                }
            }
        }
    }
}
