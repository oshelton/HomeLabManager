using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeLabManager.Common.Data.CoreConfiguration;

public sealed record CoreConfigurationDto
{
    public string? HostDataPath { get; init; }
    public string? GitConfigFilePath { get; init; }
    public string? GithubUserName { get; init; }
    public string? GithubPat { get; init; }
}
