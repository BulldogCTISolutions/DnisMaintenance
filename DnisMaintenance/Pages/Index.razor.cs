using System.Collections.ObjectModel;
using System.Globalization;

using BlazorApplicationInsights;

using DnisMaintenance.Models;
using DnisMaintenance.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.JSInterop;

namespace DnisMaintenance.Pages;

public partial class Index
{
    [Inject] private ILogger<Index> Logger { get; set; }
    [Inject] private IApplicationInsights ApplicationInsights { get; set; }
    [Inject] private AzureBlobService AzureBlobService { get; set; }
    [Inject] private IJSRuntime JsRuntime { get; set; }
    [Inject] private ToastService ToastService { get; set; }

    private const string RegionPlaceHolder = "Please select a Region";
    private bool _disableDNISSelect = true;
    private bool _disableButtons = true;
    private string _currentFileName = string.Empty;
    private string _currentRegion = string.Empty;
    private string _currentDNIS = string.Empty;
    private string _dnisScript = string.Empty;
    private string? _listBoxSelectedValue;
    private CallDataWindowConfiguration? _dnisConfigurations;
    private List<KvpList> _kvpList = new List<KvpList>();
    private List<GenesysKey>? _genesysKeys;
    private GenesysKey? _listboxSelectedItem;
    private readonly List<Option<string>> _regionOptions = new List<Option<string>>();
    private readonly List<Option<string>> _dnisOptions = new List<Option<string>>();

    private FluentDataGrid<KvpList>? GridOfKvps { get; set; }

    private GridItemsProvider<KvpList> _gridItemsProvider = default!;


    protected override async Task OnInitializedAsync()
    {
        string regionsJson = await this.AzureBlobService.GetDNISFileFromAzureBlobAsync( "regions.json", CancellationToken.None )
                                                        .ConfigureAwait( false );
        if( string.IsNullOrEmpty( regionsJson ) )
        {
            throw new InvalidOperationException( "Could not download Region List from Azure Blob Storage" );
        }
        ReadOnlyCollection<Region> regions = new ReadOnlyCollection<Region>( Region.FromJson( regionsJson ) );
        if( regions is null || regions.Count < 2 )
        {
            throw new InvalidOperationException( "Could not download Region List from Azure Blob Storage" );
        }

        // This may be a performance hit, but let's sort all the Regions on their description.
        // Uses Region.CompareTo().
        regions.ToList().Sort();
        this._regionOptions.Clear();
        this._regionOptions.Add( new Option<string> { Value = $"", Text = $"{RegionPlaceHolder}" } );
        foreach( Region region in regions )
        {
            // This goes to a combobox
            this._regionOptions.Add( new Option<string> { Value = $"{region.Code}", Text = $"{region.Description}" } );
        }

        string genesysJson = await this.AzureBlobService.GetDNISFileFromAzureBlobAsync( "genesys-keys.json", CancellationToken.None )
                                                        .ConfigureAwait( false );
        if( string.IsNullOrEmpty( genesysJson ) )
        {
            throw new InvalidOperationException( "Could not download Genesys Key List from Azure Blob Storage" );
        }
        ReadOnlyCollection<GenesysKey> genesysKeys = new ReadOnlyCollection<GenesysKey>( GenesysKey.FromJson( genesysJson ) );
        if( genesysKeys is null || genesysKeys.Count < 1 )
        {
            throw new InvalidOperationException( "Could not download Genesys Key List from Azure Blob Storage" );
        }

        // This may be a performance hit, but let's sort all the KVPs on their display text.
        // uses GenesysKey.CompareTo().
        this._genesysKeys = genesysKeys.ToList();
        this._genesysKeys.Sort();

        await base.OnInitializedAsync().ConfigureAwait( false );
    }

    protected async Task OnChangeSelectedRegionAsync( string selectedRegion )
    {
        if( string.IsNullOrEmpty( selectedRegion ) == true )
        {
            return;
        }
        this._currentRegion = selectedRegion;
        this._currentFileName = $"{this._currentRegion}_DNIS_Configuration.json";

        string json;
        try
        {
            json = await this.AzureBlobService.GetDNISFileFromAzureBlobAsync( $"{this._currentFileName}", CancellationToken.None )
                                              .ConfigureAwait( false );
            if( string.IsNullOrEmpty( json ) )
            {
                throw new InvalidOperationException( $"Could not download DNIS List for {this._currentRegion} from Azure Blob Storage" );
            }
        }
        catch( Exception ex )
        {
            _ = this.ProcessMessageAsync( $"No JSON retrieved for region = ({this._currentRegion}).", ex );
            return;
        }
        if( string.IsNullOrEmpty( json ) )
        {
            _ = this.ProcessMessageAsync( $"No JSON retrieved for region = ({this._currentRegion})." );
            return;
        }
        this._dnisConfigurations = CallDataWindowConfiguration.FromJson( json );

        this._dnisOptions.Clear();
        foreach( KeyValuePair<string, DnisList> item in this._dnisConfigurations.DnisList[0] )
        {
            // This goes to a combobox
            this._dnisOptions.Add( new Option<string> { Value = $"{item.Key}", Text = $"{item.Key}" } );
        }
        this._disableDNISSelect = false;
    }

    protected void OnChangeSelectedDnis( string selectedDNIS )
    {
        if( string.IsNullOrEmpty( selectedDNIS ) == true )
        {
            return;
        }
        this._currentDNIS = selectedDNIS;

        bool success = this._dnisConfigurations.DnisList[0].TryGetValue( this._currentDNIS, out DnisList item );
        if( success && item is not null )
        {
            this._dnisScript = item.Script;
            this._kvpList = item.KvpList;
        }
        else
        {
            // Added a DNIS, insert a blank row.
            KvpList blankKvpList = new KvpList() { ViewOrder = "1" };
            this._kvpList.Add( blankKvpList );
        }

        // Initialize the data provider.
        GridItemsProviderResult<KvpList> gridItemsProviderResult = GridItemsProviderResult.From( this._kvpList, this._kvpList.Count );
        this._gridItemsProvider = req => ValueTask.FromResult( gridItemsProviderResult );
    }

    protected void OnRowFocus()
    {
        this._disableButtons = false;
    }

    protected void OnClickDownArrow( KvpList? row )
    {
        if( row is null )
        {
            throw new ArgumentNullException( nameof( row ) );
        }

        _ = this.ProcessMessageAsync( $"Down Arrow Clicked: {row.ViewOrder}" );

        // Get the current row id.
        int viewOrderSelectedItem = int.Parse( row.ViewOrder, CultureInfo.CurrentCulture );
        KvpList selectedRow = this._kvpList.Single( c => c.ViewOrder.Equals( row.ViewOrder, StringComparison.Ordinal ) );

        // Increment to get the next item in KVPList.
        int nextViewOrderItem = viewOrderSelectedItem + 1;
        KvpList nextRow = this._kvpList.Single( c => c.ViewOrder.Equals( nextViewOrderItem.ToString( CultureInfo.CurrentCulture ), StringComparison.Ordinal ) );

        // Swap the view orders.
        selectedRow.ViewOrder = nextViewOrderItem.ToString( CultureInfo.CurrentCulture );
        nextRow.ViewOrder = viewOrderSelectedItem.ToString( CultureInfo.CurrentCulture );

        // Update the kvpList.  (subtract one because Lists are zero-based)
        this._kvpList[viewOrderSelectedItem - 1] = nextRow;
        this._kvpList[nextViewOrderItem - 1] = selectedRow;

        // Refresh the grid.
        _ = this.RefreshTableAsync();
    }

    protected void OnClickUpArrow( KvpList? row )
    {
        if( row is null )
        {
            throw new ArgumentNullException( nameof( row ) );
        }

        _ = this.ProcessMessageAsync( $"Up Arrow Clicked: {row.ViewOrder}" );

        //  Get the current row id.
        int viewOrderSelectedItem = int.Parse( row.ViewOrder, CultureInfo.CurrentCulture );
        KvpList selectedRow = this._kvpList.Single( c => c.ViewOrder.Equals( row.ViewOrder, StringComparison.Ordinal ) );

        //  Increment to get the previous item in KVPList.
        int previousViewOrderItem = viewOrderSelectedItem - 1;
        KvpList previousRow = this._kvpList.Single( c => c.ViewOrder.Equals( previousViewOrderItem.ToString( CultureInfo.CurrentCulture ), StringComparison.Ordinal ) );

        //  Swap the view orders.
        selectedRow.ViewOrder = previousViewOrderItem.ToString( CultureInfo.CurrentCulture );
        previousRow.ViewOrder = viewOrderSelectedItem.ToString( CultureInfo.CurrentCulture );

        // Update the kvpList.  (subtract one because Lists are zero-based)
        this._kvpList[viewOrderSelectedItem - 1] = previousRow;
        this._kvpList[previousViewOrderItem - 1] = selectedRow;

        // Refresh the grid.
        _ = this.RefreshTableAsync();
    }

    protected void OnClickAdd( KvpList? row )
    {
        if( row is null )
        {
            throw new ArgumentNullException( nameof( row ) );
        }

        _ = this.ProcessMessageAsync( $"Add below row: {row.ViewOrder}" );

        // Add a blank entry to the List.
        int currentRow = int.Parse( row.ViewOrder, CultureInfo.CurrentCulture );
        int currentCount = this._kvpList.Count;

        // Iterate through the List and increment the ViewOrder value.
        // Remember List is zero-based, and ViewOrder is one-based.
        for( int i = currentCount; i > currentRow; --i )
        {
            // Take the previous item from the List and update it's ViewOrder.
            KvpList previousItem = this._kvpList[i - 1];
            previousItem.ViewOrder = ( i + 1 ).ToString( CultureInfo.CurrentCulture );

            // This should leave a hole where the previous item was.
        }
        KvpList blankKvpList = new KvpList() { ViewOrder = ( currentRow + 1 ).ToString( CultureInfo.CurrentCulture ) };
        this._kvpList.Add( blankKvpList );

        // Re-sort the List.
        // This may be a performance hit, but let's sort all the rows on ViewOrder text.
        // uses KvpList.CompareTo().
        this._kvpList.Sort();

        // Refresh the grid.
        _ = this.RefreshTableAsync();
    }

    protected void OnClickDelete( KvpList? row )
    {
        if( row is null )
        {
            throw new ArgumentNullException( nameof( row ) );
        }

        _ = this.ProcessMessageAsync( $"Delete row: {row.ViewOrder}" );

        // Remove a row from the list.
        this._kvpList.Remove( row );
        int currentCount = this._kvpList.Count;
        int currentRow = int.Parse( row.ViewOrder, CultureInfo.CurrentCulture );

        // Iterate through the List and increment the ViewOrder value.
        // Remember List is zero-based, and ViewOrder is one-based.
        for( int i = currentRow; i <= currentCount; ++i )
        {
            // Take the previous item from the List and update it's ViewOrder.
            KvpList previousItem = this._kvpList[i - 1];
            previousItem.ViewOrder = i.ToString( CultureInfo.CurrentCulture );

            // This should leave a hole where the previous item was.
        }

        // Re-sort the List.
        // This may be a performance hit, but let's sort all the rows on ViewOrder text.
        // uses KvpList.CompareTo().
        this._kvpList.Sort();

        // Refresh the grid.
        _ = this.RefreshTableAsync();
    }

    private async Task RefreshTableAsync()
    {
        await ( this.GridOfKvps?.RefreshDataAsync() ).ConfigureAwait( false );
    }

    /// <summary>
    ///  Commit the grid to Azure Blob Storage.
    /// </summary>
    /// <returns>
    ///  Pops an alert on success / failure.
    /// </returns>
    protected async Task OnClickSaveAsync()
    {
        //  Commit grid to file.

        //  Changes in the grid are reflected immediately within the current KVPList.
        //  So update a new DnisList.
        DnisList newDnisList = new DnisList
        {
            KvpList = this._kvpList,
            Script = this._dnisScript
        };

        //  Update this DnisList into the DnisConfiguration

        //  Add() throws an exception if the Key exists.  Try to see if it does and Remove() it first.
        this._dnisConfigurations.DnisList[0].Remove( this._currentDNIS );
        bool success = this._dnisConfigurations.DnisList[0].TryAdd( this._currentDNIS, newDnisList );
        if( success == false )
        {
            InvalidDataException invalidDataException = new InvalidDataException( $"Unable to update DnisConfiguration for DNIS ({this._currentDNIS})." );
            await this.ProcessMessageAsync( invalidDataException.Message, invalidDataException ).ConfigureAwait( false );
            return;
        }
        await this.ProcessMessageAsync( $"Updated DNIS ({this._currentDNIS}) with new information." ).ConfigureAwait( false );

        //  Now upload the new DnisList to Azure.
        //  First convert the DNIS Configuration to a JSON string.
        string json;
        try
        {
            json = CallDataWindowConfiguration.ToJson( this._dnisConfigurations );
        }
        catch( NotSupportedException nse )
        {
            await this.ProcessMessageAsync( "Unable to convert DnisConfiguration to JSON string", nse ).ConfigureAwait( false );
            return;
        }
        catch( Exception ex )
        {
            await this.ProcessMessageAsync( "Unable to convert DnisConfiguration to JSON string", ex ).ConfigureAwait( false );
            return;
        }
        if( string.IsNullOrEmpty( json ) )
        {
            InvalidDataException invalidDataException = new InvalidDataException( "Unable to convert DnisConfiguration to JSON string" );
            await this.ProcessMessageAsync( invalidDataException.Message, invalidDataException ).ConfigureAwait( false );
            return;
        }

        //  Try to upload json string to Azure.
        success = await this.AzureBlobService.PutDNISFileToAzureBlobAsync( this._currentFileName, json, CancellationToken.None )
                                             .ConfigureAwait( false );
        if( success )
        {
            // Pop an alert for success.
            await this.AlertAsync( "Successfully updated new configuration." ).ConfigureAwait( false );
        }
        else
        {
            await this.AlertAsync( "Failed to update the new configuration." ).ConfigureAwait( false );
        }
    }

    protected async Task OnClickUndoAsync()
    {
        string message = "Are you sure you want to UNDO all changes?";
        bool confirm = await this.JsRuntime.InvokeAsync<bool>( "confirm", message ).ConfigureAwait( false );
        if( confirm == false )
        {
            return;
        }
        // Refresh grid from file.
        await this.OnChangeSelectedRegionAsync( this._currentRegion ).ConfigureAwait( false );
        this.OnChangeSelectedDnis( this._currentDNIS );
        await this.ProcessMessageAsync( "Reverted changes." ).ConfigureAwait( false );
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
            LoggingService.LogInfo( this.Logger, message );
            await this.ApplicationInsights.TrackTrace( message ).ConfigureAwait( false );
        }
        else
        {
            LoggingService.LogError( this.Logger, message, ex );
            Error error = new Error() { Name = ex.GetType().ToString(), Message = message, Stack = ex.StackTrace };
            await this.ApplicationInsights.TrackException( error ).ConfigureAwait( false );
            await this.AlertAsync( message ).ConfigureAwait( false );
        }
    }

    private async Task AlertAsync( string message )
    {
        await this.JsRuntime.InvokeVoidAsync( "AlertAsync", message ).ConfigureAwait( false );
        this.ToastService.ShowToast( new ToastEventArgs( message, ToastLevel.Warning ) );
    }
}
