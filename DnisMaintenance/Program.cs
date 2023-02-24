using BlazorApplicationInsights;

using DnisMaintenance.Services;

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

        builder.Services.AddScoped( sp => new HttpClient { BaseAddress = new Uri( builder.HostEnvironment.BaseAddress ) } );
        builder.Services.AddSingleton<AzureBlobService>();
        builder.Services.AddScoped<ToastService>();
        builder.Services.AddFluentUIComponents();

        builder.Services.AddMsalAuthentication( options =>
        {
            builder.Configuration.Bind( "AzureAd", options.ProviderOptions.Authentication );
        } );

        builder.Services.AddBlazorApplicationInsights( async applicationInsights =>
        {
            TelemetryItem? telemetryItem = new TelemetryItem()
            {
                Tags = new Dictionary<string, object>()
                {
                    { "ai.cloud.role", "SPA" },
                    { "ai.cloud.roleInstance", "Blazor Wasm" },
                }
            };

            string instrumentKey = builder.Configuration.GetSection( "ApplicationInsights" ).GetValue<string>( "InstrumentKey" );
            string connectionString = builder.Configuration.GetSection( "ApplicationInsights" ).GetValue<string>( "ConnectionString" );
            //await applicationInsights.SetInstrumentationKey( instrumentKey ).ConfigureAwait( false );
            //await applicationInsights.SetConnectionString( connectionString ).ConfigureAwait( false );
            await applicationInsights.AddTelemetryInitializer( telemetryItem ).ConfigureAwait( false );
        } );

        builder.Logging.AddConfiguration( builder.Configuration.GetSection( "Logging" ) );

        await builder.Build().RunAsync().ConfigureAwait( false );
    }
}
