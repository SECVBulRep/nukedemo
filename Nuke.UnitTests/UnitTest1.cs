using Nuke.WebApplication.Entities;

namespace Nuke.UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Car car = new Car("A","B",2001);
        var (a, b, c) = car;
        
        Assert.That(a, Is.EqualTo("A"));
        Assert.That(b, Is.EqualTo("B"));
        Assert.That(c, Is.EqualTo(2001));
        
    }
}