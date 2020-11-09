using handshake.Interfaces;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OneClickStream.Services
{
  /// <summary>
  /// The <see cref="IAuthService"/> class is a service for authentication.
  /// </summary>
  internal class AuthService : IAuthService
  {
    #region Properties

    /// <summary>
    /// Retrives the current <see cref="SqlConnection"/>.
    /// </summary>
    /// <returns>the current connection.</returns>
    public SqlConnection Connection
    {
      get;
      private set;
    }

    /// <summary>
    /// Retrives the current username.
    /// </summary>
    /// <returns>the current connection.</returns>
    public string Username
    {
      get;
      private set;
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Performs the Authentication with the SQL Server and retrives a connection.
    /// </summary>
    /// <param name="username">The SQL username.</param>
    /// <param name="password">The SQL password.</param>
    /// <returns>An open connection.</returns>
    public async Task<SqlConnection> Authenticate(string username, string password)
    {
      await this.AuthenticateInternal(username, password, "handshake");
      return this.Connection;
    }

    /// <summary>
    /// Performs the Authentication for the master database and retrives a connection.
    /// </summary>
    /// <param name="username">The SQL username.</param>
    /// <param name="password">The SQL password.</param>
    /// <returns>An open connection.</returns>
    public async Task<SqlConnection> AuthenticateMaster(string username, string password)
    {
      await this.AuthenticateInternal(username, password, "master");
      return this.Connection;
    }

    private async Task AuthenticateInternal(string username, string password, string catalog)
    {
      if (this.Connection != null)
      {
        this.Connection.Dispose();
        this.Connection = null;
      }

      string connectionString = $"Server=tcp:server2.database.windows.net,1433;Initial Catalog={catalog};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

      this.Connection = await Task.Run(() =>
      {
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        this.Username = username;
        return connection;
      });
    }

    #endregion Methods
  }
}