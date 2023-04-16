using SimpleDI;

namespace Tests;

public static class TestUtils
{
    public static void SetCompatibilityMode(bool value)
    {
        DependencyInjectionSettings.UseCompatibilityMethodForConstructors = value;
        DependencyInjectionSettings.UseCompatibilityMethodForProperties = value;
    }
}