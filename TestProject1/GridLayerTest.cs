using Mars.Interfaces.Environments;
using NUnit.Framework;

namespace TestMARS;

using CitySim.Backend.Entity.Agents;
using CitySim.Backend.World;

public class GridLayerTest
{
    private WorldLayer _layer;
    private Person _person;

    [SetUp]
    public void Setup()
    {
        _layer = new();
        _person = new();
        _person.Init(_layer);
        _person.Position = new Position(5, 5);
    }

    [Test]
    public void TestMoveTo1RightDown()
    {
        Position center = new(_person.Position.X, _person.Position.Y);
        Position expected = new Position(center.X + 1, center.Y + 1);
        Position newPos = _layer.GridEnvironment.MoveTo(_person, expected, 1);
        Assert.That(newPos, Is.EqualTo(expected));
    }

    [Test]
    public void TestMoveTo2RightDown()
    {
        Position center = new(_person.Position.X, _person.Position.Y);
        Position expected = new Position(center.X + 2, center.Y + 2);
        Position newPos = _layer.GridEnvironment.MoveTo(_person, expected, 2);
        Assert.That(newPos, Is.EqualTo(expected));
    }

    [Test]
    public void TestMoveTo1RightDown2Apart()
    {
        Position center = new(_person.Position.X, _person.Position.Y);
        Position target = new Position(center.X + 2, center.Y + 2);
        Position expected = new Position(center.X + 1, center.Y + 1);
        Position newPos = _layer.GridEnvironment.MoveTo(_person, target, 1);
        Assert.That(newPos, Is.EqualTo(expected));
    }


    [Test]
    public void TestMoveTo1LeftUp()
    {
        Position center = new(_person.Position.X, _person.Position.Y);
        Position expected = new Position(center.X - 1, center.Y - 1);
        Position newPos = _layer.GridEnvironment.MoveTo(_person, expected, 1);
        Assert.That(newPos, Is.EqualTo(expected));
    }

    [Test]
    public void TestMoveTo2LeftUp()
    {
        Position center = new(_person.Position.X, _person.Position.Y);
        Position expected = new Position(center.X - 2, center.Y - 2);
        Position newPos = _layer.GridEnvironment.MoveTo(_person, expected, 2);
        Assert.That(newPos, Is.EqualTo(expected));
    }

    [Test]
    public void TestMoveTo1LeftUp2Apart()
    {
        Position center = new(_person.Position.X, _person.Position.Y);
        Position target = new Position(center.X - 2, center.Y - 2);
        Position expected = new Position(center.X - 1, center.Y - 1);
        Position newPos = _layer.GridEnvironment.MoveTo(_person, target, 1);
        Assert.That(newPos, Is.EqualTo(expected));
    }
}