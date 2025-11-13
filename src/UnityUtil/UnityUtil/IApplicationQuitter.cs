namespace UnityUtil;

/// <summary>
/// Provides methods for quitting the entire application.
/// </summary>
public interface IApplicationQuitter
{
    /// <summary>
    /// Quits the application with the specified exit code.
    /// </summary>
    /// <param name="exitCode"></param>
    void Quit(int exitCode = 0);
}
