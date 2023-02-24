using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DnisMaintenance.Models;

public partial class GenesysKey : IComparable
{
    [JsonConstructor]
    public GenesysKey( string value, string text ) =>
                     (this.Value, this.Text) = (value, text);

    public bool Disabled { get; }
    public bool Selected { get; set; }
    [JsonPropertyName( "Value" )]
    public string Value { get; set; }
    [JsonPropertyName( "Text" )]
    public string Text { get; set; }

    public int CompareTo( object? obj )
    {
        return obj is not GenesysKey other
            ? 1
            : string.IsNullOrEmpty( this.Text ) && string.IsNullOrEmpty( other.Text )
               ? 0
               : string.IsNullOrEmpty( this.Text )
                 ? -1
                 : string.IsNullOrEmpty( other.Text )
                   ? 1
                   : string.Compare( this.Text, other.Text, StringComparison.OrdinalIgnoreCase );
    }

    public override string ToString()
    {
        return $"Genesys Key: [ Text = {this.Text}, Value = {this.Value}, Selected = {this.Selected}, Disabled = {this.Disabled} ]";
    }
}

public partial class GenesysKey
{
    public static Collection<GenesysKey>? FromJson( string json ) => JsonSerializer.Deserialize<Collection<GenesysKey>>( json );
}
