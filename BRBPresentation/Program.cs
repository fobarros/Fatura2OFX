using System.Globalization;

var cultureInfo = new CultureInfo("pt-BR");
cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

// Adiciona logging ao console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Parse DataInicioFatura from appsettings.json
var dataInicioConfig = builder.Configuration["DataInicioFatura"];
DateTime dataInicioFatura;
if (!DateTime.TryParseExact(dataInicioConfig, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dataInicioFatura))
{
    throw new InvalidOperationException("DataInicioFatura no appsettings.json está em um formato inválido ou ausente.");
}

// Add services to the container.
builder.Services.AddControllersWithViews();


// Configure ProcessadorFatura to include DataInicioFatura
builder.Services.AddScoped<Core.ProcessadorFatura>(provider =>
    new Core.ProcessadorFatura(
        provider.GetRequiredService<Core.IExclusaoFaturaService>(),
        dataInicioFatura));

builder.Services.AddScoped<Infrastructure.PdfReaderService>(); 
builder.Services.AddSingleton<Core.IExclusaoFaturaService, BRBPresentation.Services.ExclusaoFaturaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pdf}/{action=Index}/{id?}");

app.Run();