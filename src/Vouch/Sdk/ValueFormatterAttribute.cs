namespace Vouch.Sdk;

/// <summary>
/// Marks a public static method <c>string Format(T value)</c> as the failure-message formatter for values of type T.
/// The method and its declaring type must be public. Non-public methods and types are skipped.
/// Methods are found by <see cref="VouchConfiguration.AddFormattersFrom"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ValueFormatterAttribute : Attribute
{
}
