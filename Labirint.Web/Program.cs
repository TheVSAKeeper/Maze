using Blazored.LocalStorage;
using Labirint.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddMudServices();

builder.Services.AddSingleton<SoundService>();
builder.Services.AddSingleton<AnimationService>();
builder.Services.AddSingleton<ControlSchemeService, ControlSchemeService>();
builder.Services.AddScoped<ClipboardService, ClipboardService>();

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new(builder.HostEnvironment.BaseAddress),
});

var host = builder.Build();
await host.RunAsync();
