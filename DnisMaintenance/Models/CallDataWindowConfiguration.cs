﻿// <auto-generated via https://app.quicktype.io/#l=cs&r=json2csharp with template provided in appConfig.json />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using KaiserPermanente.CCSTI.WDE.CallDataWindowExtension.Models;
//
//    var callDataWindowConfiguration = CallDataWindowConfiguration.FromJson(jsonString);

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace DnisMaintenance.Models;

public partial class CallDataWindowConfiguration
{
    [JsonPropertyName( "DNIS_List" )]
    public List<Dictionary<string, DnisList>> DnisList { get; set; }


    //
    //  UTILITIES
    //


    #region Utilities
    /// <summary>
    ///  String representation of this type.
    /// </summary>
    /// <remarks>
    ///  Item 5 from Effective C# by Bill Wagner
    /// </remarks>
    public override string ToString()
    {
        StringBuilder strSerialize = new StringBuilder();
        strSerialize.AppendLine( $"Number of nodes in DNISList ({this.DnisList[0].Count})." );
        foreach( KeyValuePair<string, DnisList> item in this.DnisList[0] )
        {
            strSerialize.AppendLine( $" Key = ({item.Key})" );
            strSerialize.AppendLine( $" Script = ({item.Value.Script})" );
            foreach( KvpList kvp in item.Value.KvpList )
            {
                strSerialize.AppendLine( $" Friendly Name = ({kvp.FriendlyName})" );
                strSerialize.AppendLine( $" Genesys Name = ({kvp.GenesysKey})" );
            }
        }
        return strSerialize.ToString();
    }
    #endregion Utilities
}

public partial class DnisList
{
    [JsonPropertyName( "script" )]
    public string Script { get; set; }

    [JsonPropertyName( "KVP_List" )]
    public List<KvpList> KvpList { get; set; }
}

public partial class KvpList : IComparable
{
    [JsonPropertyName( "viewOrder" )]
    public string ViewOrder { get; set; }

    [JsonPropertyName( "friendlyName" )]
    public string FriendlyName { get; set; } = string.Empty;

    [JsonPropertyName( "genesysKey" )]
    public string GenesysKey { get; set; } = string.Empty;

    [JsonPropertyName( "useAttribute" )]
    public bool UseAttribute { get; set; } = false;

    public int CompareTo( object obj )
    {
        return obj is not KvpList other
            ? 1
            : string.IsNullOrEmpty( this.ViewOrder ) && string.IsNullOrEmpty( other.ViewOrder )
               ? 0
               : string.IsNullOrEmpty( this.ViewOrder )
                 ? -1
                 : string.IsNullOrEmpty( other.ViewOrder )
                   ? 1
                   : string.Compare( this.ViewOrder, other.ViewOrder, StringComparison.OrdinalIgnoreCase );
    }
}

public partial class CallDataWindowConfiguration
{
    public static CallDataWindowConfiguration FromJson( string json ) => JsonSerializer.Deserialize<CallDataWindowConfiguration>( json );

    public static string ToJson( CallDataWindowConfiguration self ) => JsonSerializer.Serialize( self, new JsonSerializerOptions()
    {
        WriteIndented= true
    } );
}
