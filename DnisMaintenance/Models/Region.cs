namespace DnisMaintenance.Models;

public class Region : IComparable
{
    public string Code { get; set; }
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

