namespace HomeLabManager.Common.Data.Git;

/// <summary>
/// Class for accessing and updating the data stored in a Home Lab Git Repo.
/// </summary>
public interface IServerDataManager
{
    /// <summary>
    /// Get a list of the server information in the git HomeLab directory.
    /// </summary>
    public IReadOnlyList<ServerDto> GetServers();

    /// <summary>
    /// Add a new server to the repo.
    /// </summary>
    public void AddNewServer(ServerDto server);

    /// <summary>
    /// Add a new server to the repo.
    /// </summary>
    public void UpdateServer(ServerDto server);

    /// <summary>
    /// Delete the passed in server.
    /// </summary>
    public void DeleteServer(ServerDto server);
}
