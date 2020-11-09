using System.Data.SqlClient;
using System.Threading.Tasks;

namespace handshake.Interfaces
{
  /// <summary>
  /// The <see cref="IAuthService"/> interface represents the authentication service.
  /// </summary>
  public interface IAuthService
  {
    #region Properties

    /// <summary>
    /// Retrives the current <see cref="SqlConnection"/>.
    /// </summary>
    /// <returns>the current connection.</returns>
    SqlConnection Connection { get; }

    /// <summary>
    /// Retrives the current username.
    /// </summary>
    /// <returns>the current connection.</returns>
    string Username { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Performs the Authentication and retrives a connection.
    /// </summary>
    /// <param name="username">The SQL username.</param>
    /// <param name="password">The SQL password.</param>
    /// <returns>An open connection.</returns>
    Task<SqlConnection> Authenticate(string username, string password);

    /// <summary>
    /// Performs the Authentication for the master database and retrives a connection.
    /// </summary>
    /// <param name="username">The SQL username.</param>
    /// <param name="password">The SQL password.</param>
    /// <returns>An open connection.</returns>
    Task<SqlConnection> AuthenticateMaster(string username, string password);

    #endregion Methods
  }
}