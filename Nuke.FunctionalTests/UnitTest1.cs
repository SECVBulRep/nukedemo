using System.Net.Http.Json;
using Nuke.WebApplication.Entities;

namespace Nuke.FunctionalTests;

public class Tests
{
    private static readonly HttpClient _httpClient = new HttpClient() {BaseAddress = new("http://localhost:5131")};

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task ListCar()
    {
        var cars = await _httpClient.GetFromJsonAsync<List<Car>>("/cars");
        Assert.NotNull(cars);
        Assert.Equals(3, cars.Count);
    }

    [Test]
    public async Task GetAUDI()
    {
        var car = await _httpClient.GetFromJsonAsync<Car>("/cars/AUDI");
        Assert.NotNull(car);
        Assert.Equals(car.Name,"AUDI");
    }
}