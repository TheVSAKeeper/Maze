using Blazored.LocalStorage;
using Labirint.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddMudServices();

builder.Services.AddSingleton<SoundService>();
builder.Services.AddSingleton<AnimationService>();
builder.Services.AddSingleton<IControlSchemeService, ControlSchemeService>();
builder.Services.AddScoped<IClipboardService, ClipboardService>();

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();
