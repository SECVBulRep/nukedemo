using System.Runtime.InteropServices.ComTypes;
using Nuke.WebApplication.Entities;

List<Car> cars = new()
{
    new Car( "Red", "Volvo",  2001),
    new Car( "Blue", "Volvo",  2021),
    new Car( "Green", "BMW",  2011),
    new Car( "Yellow", "AUDI",  2003),
    new Car( "Gray", "HAVAL",  2014),
    new Car( "Blue", "Volvo",  2010)
};


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseRouting();

app.MapGet("/cars", _ =>_.Response.WriteAsJsonAsync(cars));
app.MapGet("/cars/{name}", _ =>
    _.Response.WriteAsJsonAsync(cars.Where(x=>x.Name==_.GetRouteValue("name")?.ToString())));

app.Run();