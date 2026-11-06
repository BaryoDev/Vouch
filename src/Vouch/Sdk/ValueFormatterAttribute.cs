namespace Vouch.Sdk;

/// <summary>
/// Marks a public static method <c>string Format(T value)</c> as the failure-message formatter for values of type T.
/// Methods are found by <see cref="VouchConfiguration.AddFormattersFrom"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ValueFormatterAttribute : Attribute
{
}
