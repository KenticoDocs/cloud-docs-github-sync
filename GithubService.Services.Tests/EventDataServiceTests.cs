using System.Collections.Generic;
using System.Threading.Tasks;
using GithubService.Models;
using GithubService.Repository;
using GithubService.Services.Tests.Utils;
using NSubstitute;
using NUnit.Framework;
using RepositoryModels = GithubService.Repository.Models;

namespace GithubService.Services.Tests
{
    public class EventDataServiceTests
    {
        [Test]
        public async Task SaveCodeFragmentEventAsync_StoresFragmentEventToRepository()
        {
            var eventDataRepository = Substitute.For<IEventDataRepository>();
            var expectedCodeFragments = new List<RepositoryModels.CodeFragment>
            {
                new RepositoryModels.CodeFragment
                {
                    Content = "using namespace System;",
                    Identifier = "csharp_using",
                    Language = CodeFragmentLanguage.CSharp,
                    Platform = CodeFragmentPlatform.Net,
                    Status = CodeFragmentStatus.Added
                },
                new RepositoryModels.CodeFragment
                {
                    Content = "import { axios } from 'axios';",
                    Identifier = "javascript_import",
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript,
                    Status = CodeFragmentStatus.Added
                },
                new RepositoryModels.CodeFragment
                {
                    Content = "include vars.php",
                    Identifier = "php_include",
                    Language = CodeFragmentLanguage.Php,
                    Platform = CodeFragmentPlatform.Php,
                    Status = CodeFragmentStatus.Modified
                },
                new RepositoryModels.CodeFragment
                {
                    Content = "import module",
                    Identifier = "python_import",
                    Language = CodeFragmentLanguage.Python,
                    Status = CodeFragmentStatus.Removed
                }
            };
            var expectedCodeFragmentEvent = new RepositoryModels.CodeFragmentEvent
            {
                CodeFragments = expectedCodeFragments,
                Mode = FunctionMode.Initialize
            };
            var eventDataService = new EventDataService(eventDataRepository);

            await eventDataService.SaveCodeFragmentEventAsync(
                FunctionMode.Initialize,
                addedCodeFragments: new[]
                {
                    new CodeFragment
                    {
                        Content = "using namespace System;",
                        Identifier = "csharp_using",
                        Language = CodeFragmentLanguage.CSharp,
                        Platform = CodeFragmentPlatform.Net
                    },
                    new CodeFragment
                    {
                        Content = "import { axios } from 'axios';",
                        Identifier = "javascript_import",
                        Language = CodeFragmentLanguage.JavaScript,
                        Platform = CodeFragmentPlatform.JavaScript
                    }
                },
                modifiedCodeFragments: new[]
                {
                    new CodeFragment
                    {
                        Content = "include vars.php",
                        Identifier = "php_include",
                        Language = CodeFragmentLanguage.Php,
                        Platform = CodeFragmentPlatform.Php
                    }
                },
                removedCodeFragments: new[]
                {
                    new CodeFragment
                    {
                        Content = "import module",
                        Identifier = "python_import",
                        Language = CodeFragmentLanguage.Python,
                    }
                });

            await eventDataRepository
                .Received()
                .StoreAsync(Arg.Is<RepositoryModels.CodeFragmentEvent>(codeFragmentEvent => codeFragmentEvent.DoesEqual(expectedCodeFragmentEvent)));
        }

        [Test]
        public async Task SaveCodeFragmentEventAsync_NoFragments_DoesNotStoreFragmentEventToRepository()
        {
            var eventDataRepository = Substitute.For<IEventDataRepository>();
            var eventDataService = new EventDataService(eventDataRepository);

            await eventDataService.SaveCodeFragmentEventAsync(
                FunctionMode.Initialize,
                addedCodeFragments: new CodeFragment[0],
                modifiedCodeFragments: new CodeFragment[0],
                removedCodeFragments: new CodeFragment[0]);

            await eventDataRepository
                .DidNotReceive()
                .StoreAsync(Arg.Any<RepositoryModels.CodeFragmentEvent>());
        }

        [Test]
        public async Task SaveCodeFragmentEventAsync_OnlyAddedFragments_StoresFragmentEventToRepository()
        {
            var eventDataRepository = Substitute.For<IEventDataRepository>();
            var eventDataService = new EventDataService(eventDataRepository);
            var expectedCodeFragments = new List<RepositoryModels.CodeFragment>
            {
                new RepositoryModels.CodeFragment
                {
                    Content = "using namespace System;",
                    Identifier = "csharp_using",
                    Language = CodeFragmentLanguage.CSharp,
                    Platform = CodeFragmentPlatform.Net,
                    Status = CodeFragmentStatus.Added
                },
                new RepositoryModels.CodeFragment
                {
                    Content = "import { axios } from 'axios';",
                    Identifier = "javascript_import",
                    Language = CodeFragmentLanguage.JavaScript,
                    Platform = CodeFragmentPlatform.JavaScript,
                    Status = CodeFragmentStatus.Added
                }
            };
            var expectedCodeFragmentEvent = new RepositoryModels.CodeFragmentEvent
            {
                CodeFragments = expectedCodeFragments,
                Mode = FunctionMode.Update
            };

            await eventDataService.SaveCodeFragmentEventAsync(
                FunctionMode.Update,
                new[]
                {
                    new CodeFragment
                    {
                        Content = "using namespace System;",
                        Identifier = "csharp_using",
                        Language = CodeFragmentLanguage.CSharp,
                        Platform = CodeFragmentPlatform.Net
                    },
                    new CodeFragment
                    {
                        Content = "import { axios } from 'axios';",
                        Identifier = "javascript_import",
                        Language = CodeFragmentLanguage.JavaScript,
                        Platform = CodeFragmentPlatform.JavaScript
                    }
                }
            );

            await eventDataRepository
                .Received()
                .StoreAsync(Arg.Is<RepositoryModels.CodeFragmentEvent>(codeFragmentEvent =>
                    codeFragmentEvent.DoesEqual(expectedCodeFragmentEvent)));
        }
    }
}