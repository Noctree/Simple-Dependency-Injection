namespace SimpleDI;

public static class DependencyInjectionSettings
{
    /// <summary>
    /// If true, the properties will be set trough reflection and not by compiled delegates, useful if runtime compilation is not possible.
    /// </summary>
    public static bool UseCompatibilityMethodForProperties { get; set; }
    
    /// <summary>
    /// If true, the constructors will be invoked by reflection and not by compiled delegates, useful if runtime compilation is not possible.
    /// </summary>
    public static bool UseCompatibilityMethodForConstructors { get; set; }
}