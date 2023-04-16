using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI;
using SimpleDI.Containers;
using Xunit.Sdk;

namespace Tests;

public class InjectionTechniqueTests
{
    private interface ITestClass
    {
        public string ToTestString();
    }
    private class A
    {
        public string? Text { get; set; }
    }
    
    private class B
    {
        public string? Text { get; set; }
    }

    private class PropertyInjectionClass : ITestClass
    {
        [Inject] public A ClassA { get; set; }
        [Inject] public B ClassB { get; set; }

        public PropertyInjectionClass()
        {
            ClassA = null!;
            ClassB = null!;
        }

        public string ToTestString() => ClassA.Text + ClassB.Text;
    }

    private class FieldInjectionClass : ITestClass
    {
        [Inject]
        private A _classAField;
        
        [Inject]
        private B _classBField;

        public FieldInjectionClass()
        {
            _classAField = null!;
            _classBField = null!;
        }

        public string ToTestString() => _classAField.Text + _classBField.Text;
    }

    private class ConstructorInjectionClass : ITestClass
    {
        private A _classAField;
        private B _classBField;

        public ConstructorInjectionClass(A a, B b)
        {
            _classAField = a;
            _classBField = b;
        }

        public string ToTestString() => _classAField.Text + _classBField.Text;
    }

    private class MultiConstructorInjectionClass : ITestClass
    {
        private A _classAField;
        private B _classBField;
        
        public MultiConstructorInjectionClass(A a, B b)
        {
            _classAField = a;
            _classBField = b;
        }

        public MultiConstructorInjectionClass(A a)
        {
            _classAField = a;
            _classBField = null!;
        }

        public MultiConstructorInjectionClass(A a, B b, int arbirtraryParameter)
        {
            _classAField = a;
            _classBField = b;
            _classBField.Text += arbirtraryParameter * 100;
        }

        public string ToTestString() => _classAField?.Text + _classBField?.Text;
    }

    public static IEnumerable<object[]> PropertyInjectionTestDataProvider()
    {
        var testCases = new (Type, string)[]
        {
            (typeof(PropertyInjectionClass), "Injection trough property"),
            (typeof(FieldInjectionClass), "Injection trough private field"),
        };

        var normalMode = testCases
            .Select(testCase => new object[] { testCase.Item2, false, testCase.Item1 });
        var compatibilityMode = testCases
            .Select(testCase => new object[] { testCase.Item2, true, testCase.Item1 });
        return normalMode.Concat(compatibilityMode);
    }
    
    public static IEnumerable<object[]> ConstructorInjectionTestDataProvider()
    {
        var testCases = new (Type, string)[]
        {
            (typeof(ConstructorInjectionClass), "Injection trough constructor"),
            (typeof(MultiConstructorInjectionClass), "Injection trough constructor with multiple constructors"),
        };

        var normalMode = testCases
            .Select(test => new object[] { test.Item2, false, test.Item1 });
        var compatibilityMode = testCases
            .Select(testCase => new object[] { testCase.Item2, true, testCase.Item1 });
        return normalMode.Concat(compatibilityMode);
    }

    [Theory]
    [MemberData(nameof(PropertyInjectionTestDataProvider))]
    public void PropertyInjection_ShouldSucceed(string technique, bool compatibilityMode, Type testClassType)
    {
        //Arrange
        TestUtils.SetCompatibilityMode(compatibilityMode);
        var a = new A()
        {
            Text = "This is class A"
        };
        var b = new B()
        {
            Text = "This is class B"
        };
        var expectedString = a.Text + b.Text;
        var c = Activator.CreateInstance(testClassType) ?? throw new NotNullException();
        var container = DependencyInjector.CreateContainer();
        container.RegisterSingleton(a);
        container.RegisterSingleton(b);
        
        //Act
        container.ResolveDependencies(c);
        
        //Assert
        var result = (c as ITestClass)?.ToTestString();
        result.Should().Be(expectedString);
    }

    [Theory]
    [MemberData(nameof(ConstructorInjectionTestDataProvider))]
    public void ConstructorInjection_ShouldSucceed(string technique, bool compatibilityMode, Type testClassType)
    {
        //Arrange
        TestUtils.SetCompatibilityMode(compatibilityMode);
        var a = new A()
        {
            Text = "This is class A"
        };
        var b = new B()
        {
            Text = "This is class B"
        };
        var expectedString = a.Text + b.Text;
        var container = DependencyInjector.CreateContainer();
        container.RegisterSingleton(a);
        container.RegisterSingleton(b);
        container.RegisterService(ServiceDescriptor.Singleton(testClassType));
        
        //Act
        var testClassInstance = container.GetRequiredService(testClassType);
        
        //Assert
        var result = (testClassInstance as ITestClass)?.ToTestString();
        result.Should().Be(expectedString);
    }
}