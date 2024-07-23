using Microsoft.EntityFrameworkCore;
using APIProductDOTNetCore.Data;
using APIProductDOTNetCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.ToListAsync())
    .WithName("GetProducts");

app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product product ? Results.Ok(product) : Results.NotFound())
    .WithName("GetProductById");

app.MapPost("/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct");

app.MapPut("/products/{id}", async (int id, Product inputProduct, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product is null) return Results.NotFound();

    product.Name = inputProduct.Name;
    product.Price = inputProduct.Price;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("UpdateProduct");

app.MapDelete("/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteProduct");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
