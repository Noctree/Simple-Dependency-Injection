using FluentAssertions;
using SimpleDI;
using SimpleDI.Containers;

namespace Tests;

public class SingletonTests
{
    private class A
    {
        public string Text { get; set; }
    }
    
    private class B
    {
        public string Text { get; set; }
    }

    private class C
    {
        [Inject] public A ClassA { get; set; }
        [Inject] public B ClassB { get; set; }
    }
    [Fact]
    public void ResolveDependencies_WhenRequiredSingletonMissing_ShouldReturnFalse()
    {
        //Arrange
        var a = new A()
        {
            Text = "This is class A"
        };
        var c = new C();
        var container = new Container();
        container.RegisterDependency(a);
        
        //Act
        var result = container.ResolveDependencies(c);
        
        //Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void ResolveDependencies_WhenRequiredSingletonPresent_ShouldReturnTrue()
    {
        //Arrange
        var a = new A()
        {
            Text = "This is class A"
        };
        var b = new B()
        {
            Text = "This is class B"
        };
        var c = new C();
        var container = new Container();
        container.RegisterDependency(a);
        container.RegisterDependency(b);
        
        //Act
        var result = container.ResolveDependencies(c);
        
        //Assert
        result.Should().BeTrue();
    }
}