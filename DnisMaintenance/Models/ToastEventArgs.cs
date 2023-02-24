namespace DnisMaintenance.Models;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastEventArgs : EventArgs
{
    internal string Message { get; }
    internal ToastLevel Level { get; }

    internal ToastEventArgs()
    {
    }

    internal ToastEventArgs( string message, ToastLevel level ) : this()
    {
        this.Message = message;
        this.Level = level;
    }
}
