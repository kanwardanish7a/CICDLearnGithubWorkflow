var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Simulated in-memory store
var forecasts = new List<WeatherForecast>();

// Sample summaries
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Seed some initial data
forecasts.AddRange(Enumerable.Range(1, 3).Select(index =>
    new WeatherForecast
    (
        Id: Guid.NewGuid(),
        Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC: Random.Shared.Next(-20, 55),
        Summary: summaries[Random.Shared.Next(summaries.Length)]
    ))
);

// GET all forecasts
app.MapGet("/weather", () => forecasts)
   .WithName("GetAllWeather")
   .WithOpenApi();

// GET by Id
app.MapGet("/weather/{id}", (Guid id) =>
{
    var forecast = forecasts.FirstOrDefault(f => f.Id == id);
    return forecast is not null ? Results.Ok(forecast) : Results.NotFound();
})
.WithName("GetWeatherById")
.WithOpenApi();

// POST new forecast
app.MapPost("/weather", (WeatherForecastInput input) =>
{
    var forecast = new WeatherForecast(
        Id: Guid.NewGuid(),
        Date: input.Date,
        TemperatureC: input.TemperatureC,
        Summary: input.Summary
    );
    forecasts.Add(forecast);
    return Results.Created($"/weather/{forecast.Id}", forecast);
})
.WithName("CreateWeather")
.WithOpenApi();

// PUT (update) existing forecast
app.MapPut("/weather/{id}", (Guid id, WeatherForecastInput input) =>
{
    var existing = forecasts.FirstOrDefault(f => f.Id == id);
    if (existing is null)
        return Results.NotFound();

    forecasts.Remove(existing);
    var updated = new WeatherForecast(id, input.Date, input.TemperatureC, input.Summary);
    forecasts.Add(updated);
    return Results.Ok(updated);
})
.WithName("UpdateWeather")
.WithOpenApi();

app.Run();

internal record WeatherForecast(Guid Id, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastInput(DateOnly Date, int TemperatureC, string? Summary);
