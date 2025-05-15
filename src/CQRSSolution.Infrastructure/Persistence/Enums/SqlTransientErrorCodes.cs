namespace CQRSSolution.Infrastructure.Persistence.Enums;

/// <summary>
///     Defines specific SQL Server error codes that are typically transient
///     and can be retried.
/// </summary>
public enum SqlTransientErrorCodes
{
    /// <summary>
    ///     SQL Error Code: 40613
    ///     Database on server is not currently available. Please retry the connection later.
    ///     If the connection fails to be re-established, a connection timeout strategy might be needed.
    /// </summary>
    DatabaseUnavailable = 40613,

    /// <summary>
    ///     SQL Error Code: 40501
    ///     The service is currently busy. Retry the request after 10 seconds.
    /// </summary>
    ServiceBusy = 40501,

    /// <summary>
    ///     SQL Error Code: 40197
    ///     The service has encountered an error processing your request. Please try again.
    /// </summary>
    RequestProcessingError = 40197,

    /// <summary>
    ///     SQL Error Code: 1205
    ///     Deadlock found when trying to get lock; try restarting transaction.
    /// </summary>
    Deadlock = 1205,

    /// <summary>
    ///     SQL Error Code: -1 / SQL_ERROR
    ///     Connection Timeout Expired. The timeout period elapsed while attempting to consume the pre-login handshake
    ///     acknowledgement.
    ///     This could be due to the pre-login handshake failing or the server not being able to respond back in time.
    /// </summary>
    ConnectionTimeout = -1,

    /// <summary>
    ///     SQL Error Code: -2
    ///     Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.
    ///     This error can also occur if the client can't connect to the server.
    /// </summary>
    GeneralNetworkErrorOrTimeout = -2,

    /// <summary>
    ///     SQL Error Code: 233
    ///     The client was unable to establish a connection because of an error during connection initialization process before
    ///     login.
    ///     This error can occur when the client cannot connect to the server.
    /// </summary>
    ConnectionInitializationError = 233,

    /// <summary>
    ///     SQL Error Code: 10928
    ///     Specific Azure SQL Database resource limit error: Resource ID: %d. The %s limit for the database is %d and has been
    ///     reached.
    /// </summary>
    AzureResourceLimitReached = 10928,

    /// <summary>
    ///     SQL Error Code: 10929
    ///     Specific Azure SQL Database resource limit error: Resource ID: %d. The %s minimum guarantee is %d, maximum limit is
    ///     %d and current usage file %d.
    /// </summary>
    AzureResourceMinimumNotMet = 10929,

    /// <summary>
    ///     SQL Error Code: 10053
    ///     A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - An
    ///     established connection was aborted by the software in your host machine.)
    /// </summary>
    ConnectionAbortedByHost = 10053,

    /// <summary>
    ///     SQL Error Code: 10054
    ///     A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An
    ///     existing connection was forcibly closed by the remote host.)
    /// </summary>
    ConnectionForciblyClosedByRemoteHost = 10054,

    /// <summary>
    ///     SQL Error Code: 10060
    ///     A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was
    ///     not found or was not accessible.
    /// </summary>
    ServerNotFoundOrNotAccessible = 10060
}