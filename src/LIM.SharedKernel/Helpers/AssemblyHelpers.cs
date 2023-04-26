namespace LIM.SharedKernel.Helpers;

public static class AssemblyHelpers
{
    public static IEnumerable<Type> GetImplementations(Type baseType)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && p != baseType)
            .ToList();

        return types;
    }
}