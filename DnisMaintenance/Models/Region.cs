using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DnisMaintenance.Models;

public partial class Region : IComparable
{
    [JsonConstructor]
    public Region( string code, string description ) =>
                 (this.Code, this.Description) = (code, description);

    [JsonPropertyName( "code" )]
    public string Code { get; set; }
    [JsonPropertyName( "description" )]
    public string Description { get; set; }

    public int CompareTo( object? obj )
    {
        return obj is not Region other
            ? 1
            : string.IsNullOrEmpty( this.Description ) && string.IsNullOrEmpty( other.Description )
               ? 0
               : string.IsNullOrEmpty( this.Description )
                 ? -1
                 : string.IsNullOrEmpty( other.Description )
                   ? 1
                   : string.Compare( this.Description, other.Description, StringComparison.OrdinalIgnoreCase );
    }
}

public partial class Region
{
    public static Collection<Region>? FromJson( string json ) => JsonSerializer.Deserialize<Collection<Region>>( json );
}
