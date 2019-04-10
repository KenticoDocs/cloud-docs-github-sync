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
        public async Task SaveCodeFragmentsAsync_StoresFragmentsToRepository()
        {
            var eventDataRepository = Substitute.For<IEventDataRepository>();
            var expectedCodeFraments = new List<RepositoryModels.CodeFragment>
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
            var expectedCodeFragmentsEntity = new RepositoryModels.CodeFragmentsEntity
            {
                CodeFragments = expectedCodeFraments,
                Mode = FunctionMode.Initialize
            };
            var eventDataService = new EventDataService(eventDataRepository);

            await eventDataService.SaveCodeFragmentsAsync(
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
                .StoreAsync(Arg.Is<RepositoryModels.CodeFragmentsEntity>(codeFragmentsEntity => codeFragmentsEntity.DoesEqual(expectedCodeFragmentsEntity)));
        }
    }
}