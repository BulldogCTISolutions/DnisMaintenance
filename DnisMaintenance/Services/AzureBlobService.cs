/*
 * See: https://learn.microsoft.com/en-us/rest/api/storageservices/create-user-delegation-sas
 * as an option.
 */
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

using BlazorApplicationInsights;

using DnisMaintenance.Models;

using Microsoft.AspNetCore.Components;

namespace DnisMaintenance.Services;

public sealed class AzureBlobService
{
    [Inject] private IApplicationInsights ApplicationInsights { get; set; }

    private readonly ILogger<AzureBlobService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    /// <summary>
    ///  Represents the user defined settings in 'appSettings.downloadedJson'.
    /// </summary>
    private readonly Settings _settings;
    private const string ContainerName = "dnis-maintenance";

    /// <summary>
    ///  Constructor.
    /// </summary>
    /// <param name="logger">
    ///  Dependency Injected Logger.
    /// </param>
    /// <param name="configuration">
    ///  Dependency Injected Configuration.
    /// </param>
    public AzureBlobService( ILogger<AzureBlobService> logger,
                             IConfiguration configuration,
                             IServiceScopeFactory scopeFactory )
    {
        this._logger = logger;
        this._configuration = configuration;
        this._scopeFactory = scopeFactory;
        this._settings = this.LoadSettingsFromAppConfigFile();
    }

    /// <summary>
    ///  Retrieves the DNIS JSON from Azure Blob Storage.
    ///  Uses properties from the <see cref="Settings"/> DTO.
    /// </summary>
    /// <param name="fileName">
    ///  The name of the file to fetch from Azure.
    /// </param>
    /// <param name="cancellationToken">
    ///  Propagates notification that this operations should be canceled.
    /// </param>
    /// <returns>
    ///  The JSON file as a string.
    /// </returns>
    public async Task<string> GetDNISFileFromAzureBlobAsync( string fileName, CancellationToken cancellationToken )
    {
        string uri = $"{this._settings.BlobContainerUrl}/{ContainerName}/{fileName}?{this._settings.SASTokenUrl}";
        _ = this.ProcessMessageAsync( $"{Environment.NewLine}Downloading file from Blob Storage: {uri.Split( '?' )[0]}{Environment.NewLine}" );

        string downloadedJson = string.Empty;
        using( IServiceScope? scope = this._scopeFactory.CreateScope() )
        {
            try
            {
                HttpClient? httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                downloadedJson = await httpClient.GetStringAsync( uri, cancellationToken ).ConfigureAwait( false );
            }
            catch( Exception ex )
            {
                _ = this.ProcessMessageAsync( $"Exception in GetDNISFileFromAzureBlobAsync(): ", ex );
            }
        }
        return downloadedJson;
    }

    /// <summary>
    ///  Uploads the modified DNIS JSON to Azure Blob Storage.
    ///  Uses properties from the <see cref="Settings"/> DTO to determine which Upload() method is used.
    /// </summary>
    /// <param name="fileName">
    ///  The name of the file.
    /// </param>
    /// <param name="json">
    ///  The jsonified string to upload to the file.
    /// </param>
    /// <param name="cancellationToken">
    ///  Propagates notification that this operations should be canceled.
    /// </param>
    /// <returns>
    ///  Was the upload successful?
    /// </returns>
    public async Task<bool> PutDNISFileToAzureBlobAsync( string fileName, string json, CancellationToken cancellationToken )
    {
        //  Guard Conditions
        if( string.IsNullOrEmpty( fileName ) || string.IsNullOrEmpty( json ) )
        {
            return false;
        }

        bool success = false;
        string uri = $"{this._settings.BlobContainerUrl}/{ContainerName}/{fileName}?{this._settings.SASTokenUrl}";
        _ = this.ProcessMessageAsync( $"{Environment.NewLine}Uploading file to Blob Storage: {uri.Split( '?' )[0]}{Environment.NewLine}" );

        using( IServiceScope? scope = this._scopeFactory.CreateScope() )
        {
            try
            {
                HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                //  Build the Request Message
                using( HttpRequestMessage httpRequestMessage = new HttpRequestMessage( HttpMethod.Put, uri )
                {
                    Content = new StringContent( json, MediaTypeHeaderValue.Parse( "application/json" ) )
                } )
                {
                    //  Add the request headers for x-ms-blob-type.
                    httpRequestMessage.Headers.Add( "x-ms-blob-type", "BlockBlob" );
                    DateTime now = DateTime.UtcNow;
                    httpRequestMessage.Headers.Add( "x-ms-date", now.ToString( "R", CultureInfo.InvariantCulture ) );
                    httpRequestMessage.Headers.Add( "x-ms-version", "2021-12-02" );

                    //  Add host header
                    httpRequestMessage.Headers.Host = httpRequestMessage.RequestUri.Host;
                    httpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse( "application/json" );
                    httpRequestMessage.Content.Headers.ContentLength = json.Length;

                    //  Send the request.
                    HttpResponseMessage httpResponseMessage = await httpClient.SendAsync( httpRequestMessage,
                                                                                          cancellationToken )
                                                                              .ConfigureAwait( false );

                    // Successful if status code = 200/201.
                    if( httpResponseMessage.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created )
                    {
                        _ = this.ProcessMessageAsync( $"Upload of ({fileName}) completed successfully! " + $"Status = ({httpResponseMessage.StatusCode}), " + $"Reason = ({httpResponseMessage.ReasonPhrase})" );
                        success = true;
                    }
                    else
                    {
                        _ = this.ProcessMessageAsync( $"Upload of ({fileName}) failed! " + $"Status = ({httpResponseMessage.StatusCode}), " + $"Reason = ({httpResponseMessage.ReasonPhrase})." );
                    }
                }
            }
            catch( Exception ex )
            {
                _ = this.ProcessMessageAsync( $"Exception in PutDNISFileToAzureBlobAsync(): ", ex );
            }
        }
        return success;
    }

    /// <summary>
    ///  Parses the items read by <see cref="IConfiguration"/> of the 'appSettings.downloadedJson' file.
    /// </summary>
    /// <returns>
    ///  A <see cref="Settings"/> DTO.
    /// </returns>
    /// <remarks>
    ///  Sample code from here: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
    /// </remarks>
    private Settings LoadSettingsFromAppConfigFile()
    {
        //  Get the configuration file settings.
        Settings settings = new Settings();

        //  Start with the SAS Token URL.
        (settings.BlobContainerUrl, settings.SASTokenUrl) = CheckSASToken( this._configuration.GetRequiredSection( "Settings:SAS_TOKEN_URL" ).Value );

        return settings;
    }

    /// <summary>
    ///  Grabs the SAS Token URL from the environment.  Separates the URL from the Signature parameters.
    ///  Then manually checks the start_date and end_date, so we don't have to wait for a failure from Azure.
    /// </summary>
    /// <param name="sasTokenUrl">
    ///  Container name from <see cref="Settings"/>.  Used to match to Url in SAS Token.
    /// </param>
    /// <returns>
    ///  Two strings subdivided from the complete SAS Token URL.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///  If no SAS Token URL was set in the Environment.
    /// </exception>
    private static (string, string) CheckSASToken( string sasTokenUrl )
    {
        if( string.IsNullOrEmpty( sasTokenUrl ) )
        {
            throw new ArgumentException( "No SAS Token Url provided!" );
        }

        //  Parse the SAS Token Url into several actionable items for validation.
        string[] splitSasTokenUrl = sasTokenUrl.Split( '?' );
        string fullUrl = splitSasTokenUrl[0];
        int lastSlash = fullUrl.LastIndexOf( '/' );
        string blobServiceUrl = fullUrl[..lastSlash];
        string sasToken = splitSasTokenUrl[1];

        //  Check the dates of the SAS token to see if they are good.
        DateTime now = DateTime.UtcNow;
        //  Don't care about '&' in sig token, just "st" and "se" tokens are needed.
        string[] sasTokens = sasToken.Split( '&' );
        string st = sasTokens.Where( y => y.StartsWith( "st", StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
        string se = sasTokens.Where( y => y.StartsWith( "se", StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();

        bool success = DateTime.TryParse( st.Split( '=' )[1], out DateTime startTime );
        if( success )
        {
            if( startTime.ToUniversalTime() > now )
            {
                throw new ArgumentException( "Start time on SAS Token Url is after today.  Please update SAS Token Url." );
            }
        }
        success = DateTime.TryParse( se.Split( '=' )[1], out DateTime endTime );
        if( success )
        {
            if( endTime.ToUniversalTime() < now )
            {
                throw new ArgumentException( "End time on SAS Token Url is before today.  Please update SAS Token Url." );
            }
        }

        return (blobServiceUrl, sasToken);
    }

    /// <summary>
    ///  Errors (or failures) get written to Azure Application Insights.
    /// </summary>
    /// <param name="message">
    ///  The message to log and email.
    /// </param>
    /// <param name="ex">
    ///  Associated exception.
    /// </param>
    private async Task ProcessMessageAsync( string message, Exception? ex = null )
    {
        if( ex is null )
        {
            LoggingService.LogInfo( this._logger, message );
            await this.ApplicationInsights.TrackTrace( message ).ConfigureAwait( false );
        }
        else
        {
            LoggingService.LogError( this._logger, message, ex );
            BlazorApplicationInsights.Error error = new BlazorApplicationInsights.Error()
            {
                Name = ex.GetType().ToString(),
                Message = message,
                Stack = ex.StackTrace
            };
            await this.ApplicationInsights.TrackException( error ).ConfigureAwait( false );
        }
    }
}
