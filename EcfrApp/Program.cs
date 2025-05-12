using EcfrApp.Data;
using Microsoft.EntityFrameworkCore; // Required for QuerySplittingBehavior

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddDbContext<EcfrContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ecfr.db");
    //options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
});
builder.Services.AddHttpClient("EcfrClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(300);
});
builder.Services.AddSwaggerGen();

// Serve static files
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles();

// Configure SPA fallback
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";
});

app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EcfrContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();