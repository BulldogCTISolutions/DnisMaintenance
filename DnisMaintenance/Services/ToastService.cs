using System.Timers;

using DnisMaintenance.Models;

namespace DnisMaintenance.Services;

/// <summary>
///  From: https://chrissainty.com/blazor-toast-notifications-using-only-csharp-html-css/
///  TODO: doesn't work.  no dialog is displayed even though eventing works.
/// </summary>
public sealed class ToastService : IDisposable
{
    public event EventHandler<ToastEventArgs> OnShow;
    public event EventHandler<EventArgs> OnHide;

    private System.Timers.Timer? _countdown;

    public void ShowToast( ToastEventArgs toastEventArgs )
    {
        OnShow?.Invoke( this, toastEventArgs );
        this.StartCountdown();
    }

    private void StartCountdown()
    {
        this.SetCountdown();

        if( this._countdown!.Enabled )
        {
            this._countdown.Stop();
            this._countdown.Start();
        }
        else
        {
            this._countdown!.Start();
        }
    }

    private void SetCountdown()
    {
        if( this._countdown != null )
        {
            return;
        }

        this._countdown = new System.Timers.Timer( 5000 );
        this._countdown.Elapsed += this.HideToast;
        this._countdown.AutoReset = false;
    }

    private void HideToast( object? source, ElapsedEventArgs args )
        => OnHide?.Invoke( source, args );

    public void Dispose()
        => this._countdown?.Dispose();
}
