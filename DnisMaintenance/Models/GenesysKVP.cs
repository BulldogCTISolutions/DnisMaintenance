namespace DnisMaintenance.Models;

public class GenesysKVP : IComparable
{
    public bool Disabled { get; }
    public bool Selected { get; set; }
    public string Value { get; set; }
    public string Text { get; set; }

    public int CompareTo( object? obj )
    {
        return obj is not GenesysKVP other
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
        return $"Genesys KVP: [ Text = {this.Text}, Value = {this.Value}, Selected = {this.Selected}, Disabled = {this.Disabled} ]";
    }
}
