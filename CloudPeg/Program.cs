using System.Text.Json.Serialization.Metadata;
using CloudPeg.Application.Command;
using CloudPeg.Application.Hub;
using CloudPeg.Application.Service;
using CloudPeg.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // This resolver enables support for attributes like [JsonDerivedType]
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
});
builder.Services.AddScoped<IFsService, FsService>();
builder.Services.AddSingleton<IProcessingQueue, ProcessingQueue>();
builder.Services.AddSingleton<IProcessingService, ProcessingService>();
builder.Services.AddSingleton<IProcessingOptionsService, ProcessingOptionsService>();
builder.Services.AddSingleton<ISupportedCodecService, SupportedCodecService>();

builder.Services.AddSignalR();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(new[] { typeof(ProcessFileCommand).Assembly });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseStaticFiles();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHub<VideoProcessorHub>("/VideoProcessor");

await using var scope = app.Services.CreateAsyncScope();

var supportedCodecsService = scope.ServiceProvider.GetService<ISupportedCodecService>();
// supportedCodecsService?.ScanForSupportedCodecs();

var processingService = scope.ServiceProvider.GetRequiredService<IProcessingService>();
await  processingService.BeginProcessing();
app.Run();