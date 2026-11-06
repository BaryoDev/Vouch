using System.Reflection;

namespace Vouch.Sdk;

internal sealed class MethodValueFormatter(MethodInfo method, Type valueType) : IValueFormatter
{
    public bool CanHandle(object? value) => value is not null && valueType.IsInstanceOfType(value);

    public string Format(object? value, FormattingContext context)
    {
        try
        {
            return (string?)method.Invoke(null, new[] { value }) ?? "null";
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    internal static List<IValueFormatter> Scan(Assembly assembly)
    {
        var found = new List<IValueFormatter>();
        Type[] types;
        try
        {
            types = assembly.GetExportedTypes();
        }
        catch (NotSupportedException)
        {
            return found;
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null && t.IsVisible).ToArray()!;
        }

        foreach (var type in types.OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            if (type.IsGenericTypeDefinition)
            {
                continue;
            }

            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            }
            catch (Exception ex) when (ex is TypeLoadException or IOException or NotSupportedException or ReflectionTypeLoadException)
            {
                continue;
            }

            foreach (var method in methods.OrderBy(m => m.Name, StringComparer.Ordinal))
            {
                if (!method.IsDefined(typeof(ValueFormatterAttribute), false)
                    || method.IsGenericMethodDefinition
                    || method.ReturnType != typeof(string))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 1 && !parameters[0].ParameterType.IsByRef && !parameters[0].ParameterType.IsPointer)
                {
                    var parameterType = parameters[0].ParameterType;
                    found.Add(new MethodValueFormatter(method, Nullable.GetUnderlyingType(parameterType) ?? parameterType));
                }
            }
        }

        return found;
    }
}
