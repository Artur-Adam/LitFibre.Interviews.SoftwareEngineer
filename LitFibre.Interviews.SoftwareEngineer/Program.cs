using LitFibre.Interviews.SoftwareEngineer.Models.Orders;
using LitFibre.Interviews.SoftwareEngineer.Services.Interfaces;
using LitFibre.Interviews.SoftwareEngineer.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IMemoryDatabase<Order>, InMemoryDatabaseService<Order>>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Seed the database with initial data
var db = app.Services.GetRequiredService<IMemoryDatabase<Order>>();

// Using anonymous type for simplicity.
var result = JsonConvert.DeserializeAnonymousType(
    File.ReadAllText("../orders.json"),
    new { orders = new List<Order>() });

foreach(var order in result.orders)
{
    db.Push(order);
}

// Read all orders
app.Map("/orders", ([FromServices] IMemoryDatabase<Order> db) => 
{
    // Pass a predicate that always return true to get all the entries.
    return db.Query(o => true);
});

// Read an order by id
app.MapGet("/orders/{id}", ([FromServices] IMemoryDatabase<Order> db, string id) =>
{
    var order = db.Read(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

// Add an order 
app.MapPost("/orders", ([FromServices] IMemoryDatabase<Order> db, Order order) =>
{
    db.Push(order);
    return Results.Created($"/orders/{order.Id}", order);
});

// Delete an order
app.MapDelete("/orders/{id}", ([FromServices] IMemoryDatabase<Order> db, string id) => 
{
    if(db.Read(id) != null)
    {
        db.Delete(id);
        return Results.Ok();
    }

    return Results.NotFound();


});

// Update an order
app.MapPut("/orders/{id}", ([FromServices] IMemoryDatabase<Order> db, string id, Order updatedOrder) => 
{
    var order = db.Read(id);
    if(order != null)
    {
        db.Delete(id);
        db.Push(updatedOrder);
        return Results.Ok();
    }
    return Results.NotFound();
});

// Read all orders that contain a specified product code.
app.MapGet("/orders/product/{productCode}", ([FromServices] IMemoryDatabase<Order> db, string productCode) =>
{
    var orders = db.Query(o => o.ProductCodes.Contains(productCode)).ToArray();
    if(orders.Length > 0)
    {
        return Results.Ok(orders);
    }

    return Results.NotFound();
    
});

// Read all orders that were placed after a specified date.
// This will work with both ISO Strings and just the date. 
app.MapGet("/orders/date/{date}", ([FromServices] IMemoryDatabase<Order> db, string dateString) =>
{
    if (DateTime.TryParse(dateString, out var dateTime))
    {
        var orders = db.Query(o => o.OrderDate.Date == dateTime.Date);
        return Results.Ok(orders);
    }
    return Results.NotFound();
});

app.Run();
