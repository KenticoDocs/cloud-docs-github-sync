using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GithubService.Models;
using GithubService.Repository;
using GithubService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace GithubService.Services.Tests
{
    public class FileProcessorTests
    {
        private const string TestPath = "test";
        private IFileProcessor _fileProcessor;
        private IGithubService _githubService;
        private ICodeFileRepository _codeFileRepository;

        [SetUp]
        public void SetupBeforeEachTest()
        {
            _githubService = Substitute.For<IGithubService>();
            _codeFileRepository = Substitute.For<ICodeFileRepository>();
            _fileProcessor = new FileProcessor(_githubService, _codeFileRepository);
        }

        [Test]
        public async Task ProcessAddedFiles_OnEmptyAddedFiles_ReturnEmptyEnumerable()
        {
            var expectedResult = Enumerable.Empty<CodeFragment>();

            var result = await _fileProcessor.ProcessAddedFiles(new Collection<string>());

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        [Test]
        public async Task ProcessAddedFiles_MoreAddedFiles_ReturnAllCodeFragments()
        {
            var codeFragment1 = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var codeFragment2 = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = CodeFragmentLanguage.TypeScript,
                Platform = CodeFragmentPlatform.TypeScript,
            };

            var fragmentsList = new List<CodeFragment> {codeFragment1, codeFragment2};

            var codeFile = new CodeFile
            {
                FilePath = TestPath,
                CodeFragments = fragmentsList
            };

            _githubService
                .GetCodeFileAsync(TestPath)
                .Returns(codeFile);

            var result = await _fileProcessor.ProcessAddedFiles(new Collection<string> {TestPath});

            CollectionAssert.AreEquivalent(fragmentsList, result);
        }

        [Test]
        public async Task ProcessModifiedFiles_OnEmptyModifiedFiles_ReturnEmptyEnumerable()
        {
            var logger = Substitute.For<ILogger>();
            var emptyList = new List<CodeFragment>();

            var (
                addedFragments,
                modifiedFragments,
                deletedFragments
                ) = await _fileProcessor.ProcessModifiedFiles(new Collection<string>(), logger);

            CollectionAssert.AreEquivalent(emptyList, addedFragments);
            CollectionAssert.AreEquivalent(emptyList, modifiedFragments);
            CollectionAssert.AreEquivalent(emptyList, deletedFragments);
        }

        [Test]
        public async Task ProcessModifiedFiles_OnlyAddedFiles_ReturnFragmentsByOperation()
        {
            var logger = Substitute.For<ILogger>();
            var emptyList = new List<CodeFragment>();

            var codeFragment1 = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var fragmentsList = new List<CodeFragment> {codeFragment1};

            var codeFile = new CodeFile
            {
                FilePath = TestPath,
                CodeFragments = fragmentsList
            };

            _codeFileRepository
                .GetAsync(TestPath)
                .Returns((CodeFile) null);

            _githubService
                .GetCodeFileAsync(TestPath)
                .Returns(codeFile);

            var (
                addedFragments,
                modifiedFragments,
                deletedFragments
                ) = await _fileProcessor.ProcessModifiedFiles(new Collection<string> {TestPath}, logger);

            CollectionAssert.AreEquivalent(fragmentsList, addedFragments);
            CollectionAssert.AreEquivalent(emptyList, modifiedFragments);
            CollectionAssert.AreEquivalent(emptyList, deletedFragments);
        }

        [Test]
        public async Task ProcessModifiedFiles_MixedFiles_ReturnFragmentsByOperation()
        {
            var logger = Substitute.For<ILogger>();

            var codeFragment = new CodeFragment
            {
                Identifier = "test_identifier_modified",
                Content = "test content",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var removedCodeFragment = new CodeFragment
            {
                Identifier = "test_identifier_removed",
                Content = "test content",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var addedCodeFragment = new CodeFragment
            {
                Identifier = "test_identifier_added",
                Content = "test content",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var modifiedCodeFragment = new CodeFragment
            {
                Identifier = "test_identifier_modified",
                Content = "test content updated",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var oldFragmentsList = new List<CodeFragment> {codeFragment, removedCodeFragment};
            var newFragmentsList = new List<CodeFragment> {addedCodeFragment, modifiedCodeFragment};

            var newCodeFile = new CodeFile
            {
                FilePath = TestPath,
                CodeFragments = newFragmentsList,
            };

            var oldCodeFile = new CodeFile
            {
                FilePath = TestPath,
                CodeFragments = oldFragmentsList,
            };

            _codeFileRepository
                .GetAsync(TestPath)
                .Returns(oldCodeFile);

            _githubService
                .GetCodeFileAsync(TestPath)
                .Returns(newCodeFile);

            var (
                addedFragments,
                modifiedFragments,
                deletedFragments
                ) = await _fileProcessor.ProcessModifiedFiles(new Collection<string> {TestPath}, logger);

            CollectionAssert.AreEquivalent(new List<CodeFragment> {addedCodeFragment}, addedFragments);
            CollectionAssert.AreEquivalent(new List<CodeFragment> {modifiedCodeFragment}, modifiedFragments);
            CollectionAssert.AreEquivalent(new List<CodeFragment> {removedCodeFragment}, deletedFragments);
        }

        [Test]
        public async Task ProcessRemovedFiles_OnEmptyRemovedFiles_ReturnEmptyEnumerable()
        {
            var expectedResult = Enumerable.Empty<CodeFragment>();

            var result = await _fileProcessor.ProcessRemovedFiles(new Collection<string>());

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        [Test]
        public async Task ProcessRemovedFiles_MoreRemovedFiles_ReturnAllCodeFragments()
        {
            var codeFragment1 = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = CodeFragmentLanguage.CSharp,
                Platform = CodeFragmentPlatform.Net,
            };

            var codeFragment2 = new CodeFragment
            {
                Identifier = "test_identifier",
                Content = "test content",
                Language = CodeFragmentLanguage.TypeScript,
                Platform = CodeFragmentPlatform.TypeScript,
            };

            var fragmentsList = new List<CodeFragment> {codeFragment1, codeFragment2};

            var codeFile = new CodeFile
            {
                FilePath = TestPath,
                CodeFragments = fragmentsList
            };

            _codeFileRepository
                .ArchiveAsync(TestPath)
                .Returns(codeFile);

            var result = await _fileProcessor.ProcessRemovedFiles(new Collection<string> {TestPath});

            CollectionAssert.AreEquivalent(fragmentsList, result);
        }

        [Test]
        public async Task ProcessRemovedFiles_NoneArchivedFile_ReturnEmptyCodeFragments()
        {
            _codeFileRepository
                .ArchiveAsync(TestPath)
                .Returns((CodeFile) null);

            var result = await _fileProcessor.ProcessRemovedFiles(new Collection<string> {TestPath});

            CollectionAssert.AreEquivalent(Enumerable.Empty<CodeFragment>(), result);
        }
    }
}