using Blazored.LocalStorage;
using Labirint.Web;
using Labirint.Web.Services.Dialogs;
using Labirint.Web.Services.Toasts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();

builder.Services.AddSingleton<SoundService>();
builder.Services.AddSingleton<AnimationService>();
builder.Services.AddSingleton<ControlSchemeService, ControlSchemeService>();
builder.Services.AddScoped<ClipboardService, ClipboardService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ThemeService>();

var host = builder.Build();
await host.RunAsync();
