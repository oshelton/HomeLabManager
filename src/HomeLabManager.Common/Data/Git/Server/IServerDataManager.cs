using LibGit2Sharp;

namespace HomeLabManager.Common.Data.Git.Server;

/// <summary>
/// Class for accessing and updating the data stored in a Home Lab Git Repo.
/// </summary>
public interface IServerDataManager
{
    /// <summary>
    /// Get a list of the server information in the git HomeLab directory.
    /// </summary>
    IReadOnlyList<ServerHostDto> GetServers();

    /// <summary>
    /// Add a new server to the repo or update an existing one.
    /// </summary>
    void AddUpdateServer(ServerHostDto server);

    /// <summary>
    /// Delete the passed in server.
    /// </summary>
    void DeleteServer(ServerHostDto server);

    /// <summary>
    /// Map the passed in repo status to human readable infformation.
    /// </summary>
    /// <param name="status">Current status of the repo.</param>
    /// <returns>A list of human readable descriptions.</returns>
    IReadOnlyList<string> MapChangesToHumanReadableInfo(RepositoryStatus status);
}
