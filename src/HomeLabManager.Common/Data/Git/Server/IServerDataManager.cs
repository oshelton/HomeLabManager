namespace HomeLabManager.Common.Data.Git.Server;

/// <summary>
/// Class for accessing and updating the data stored in a Home Lab Git Repo.
/// </summary>
public interface IServerDataManager
{
    /// <summary>
    /// Get a list of the server information in the git HomeLab directory.
    /// </summary>
    public IReadOnlyList<ServerHostDto> GetServers();

    /// <summary>
    /// Add a new server to the repo or update an existing one.
    /// </summary>
    public void AddUpdateServer(ServerHostDto server);

    /// <summary>
    /// Delete the passed in server.
    /// </summary>
    public void DeleteServer(ServerHostDto server);
}
