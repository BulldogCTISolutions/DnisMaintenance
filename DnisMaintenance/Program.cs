using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Fast.Components.FluentUI;

namespace DnisMaintenance;

public static class Program
{
    public static async Task Main( string[] args )
    {
        WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault( args );
        builder.RootComponents.Add<App>( "#app" );
        builder.RootComponents.Add<HeadOutlet>( "head::after" );

        builder.Services
               .AddScoped( sp => new HttpClient { BaseAddress = new Uri( builder.HostEnvironment.BaseAddress ) } );
        builder.Services.AddFluentUIComponents();

        builder.Services
               .AddMsalAuthentication( options =>
               {
                   builder.Configuration.Bind( "AzureAd", options.ProviderOptions.Authentication );
               } );

        await builder.Build().RunAsync().ConfigureAwait( false );
    }
}
