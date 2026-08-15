using Blazored.LocalStorage;
using Labirint.Web;
using Labirint.Web.Parameters;
using Labirint.Web.Services.Dialogs;
using Labirint.Web.Services.Toasts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();

builder.Services.AddSingleton<SoundService>();
builder.Services.AddSingleton<AnimationService>();
builder.Services.AddSingleton<ControlSchemeService, ControlSchemeService>();
builder.Services.AddScoped<ClipboardService, ClipboardService>();
builder.Services.AddScoped<PickupFlightService>();
builder.Services.AddScoped<MotionService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AppUpdateService>();

var host = builder.Build();

await LabyrinthParametersLoader.ApplyAsync(
    host.Services.GetRequiredService<ILocalStorageService>(),
    host.Services.GetRequiredService<ILogger<Program>>());

await host.RunAsync();
