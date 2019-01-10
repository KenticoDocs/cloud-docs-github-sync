﻿using System.Collections.Generic;
using GithubService.Models;
using GithubService.Models.CodeSamples;

namespace GithubService.Services.Interfaces
{
    public interface IGithubService
    {
        IEnumerable<FileCodeSamples> GetCodeSamplesFiles();

        FileCodeSamples GetCodeSamplesFile(string path);
    }
}
