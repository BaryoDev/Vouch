using System.Runtime.CompilerServices;
using PublicApiGenerator;
using Xunit;

namespace Vouch.ApiApproval.Tests;

public class PublicApiTests
{
    [Fact]
    public void Public_api_has_not_changed()
    {
        var actual = GenerateVouchApi();
        var approvedPath = ApprovedFile("Vouch.approved.txt");

        ApiApproval.Check(actual, approvedPath);
    }

    [Fact]
    public void Vouch_Time_public_api_has_not_changed()
    {
        var actual = GeneratePublicApi(typeof(Vouch.FluentTimeSpanExtensions).Assembly);
        var approvedPath = ApprovedFile("Vouch.Time.approved.txt");

        ApiApproval.Check(actual, approvedPath);
    }

    internal static string GenerateVouchApi() => GeneratePublicApi(typeof(Vouch.VouchException).Assembly);

    private static string GeneratePublicApi(System.Reflection.Assembly assembly)
    {
        return assembly.GeneratePublicApi(new ApiGeneratorOptions
        {
            ExcludeAttributes = new[]
            {
                "System.Diagnostics.DebuggerNonUserCodeAttribute",
                "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
                "System.Runtime.CompilerServices.InternalsVisibleToAttribute",
                "System.Runtime.CompilerServices.RefSafetyRulesAttribute",
                "System.Runtime.Versioning.TargetFrameworkAttribute",
                "System.Reflection.AssemblyMetadataAttribute",
                "System.Reflection.AssemblyCompanyAttribute",
                "System.Reflection.AssemblyConfigurationAttribute",
                "System.Reflection.AssemblyFileVersionAttribute",
                "System.Reflection.AssemblyInformationalVersionAttribute",
                "System.Reflection.AssemblyProductAttribute",
                "System.Reflection.AssemblyTitleAttribute",
                "System.Reflection.AssemblyVersionAttribute",
            },
        });
    }

    private static string ApprovedFile(string fileName, [CallerFilePath] string sourceFile = "")
    {
        var directory = Path.GetDirectoryName(sourceFile)!;
        return Path.Combine(directory, fileName);
    }
}

internal static class ApiApproval
{
    public static void Check(string actual, string approvedPath)
    {
        var normalizedActual = Normalize(actual);
        var receivedPath = approvedPath.Replace(".approved.txt", ".received.txt");
        var approved = File.Exists(approvedPath) ? Normalize(File.ReadAllText(approvedPath)) : "";

        if (approved == normalizedActual)
        {
            if (File.Exists(receivedPath))
            {
                File.Delete(receivedPath);
            }

            return;
        }

        File.WriteAllText(receivedPath, normalizedActual + "\n");

        Assert.Fail(
            "The public API of Vouch changed.\n\n"
            + Describe(approved, normalizedActual)
            + $"\nIf the change is intended, review\n  {receivedPath}\nand copy it over\n  {approvedPath}");
    }

    private static string Normalize(string text) => text.Trim().ReplaceLineEndings("\n");

    private static string Describe(string approved, string actual)
    {
        var before = approved.Split('\n');
        var after = actual.Split('\n');
        var removed = before.Except(after).ToArray();
        var added = after.Except(before).ToArray();
        var lines = new List<string>();

        if (removed.Length > 0)
        {
            lines.Add($"Removed or changed ({removed.Length}):");
            lines.AddRange(removed.Take(25).Select(l => $"  - {l.Trim()}"));
        }

        if (added.Length > 0)
        {
            lines.Add($"Added ({added.Length}):");
            lines.AddRange(added.Take(25).Select(l => $"  + {l.Trim()}"));
        }

        if (lines.Count == 0)
        {
            return "No lines were added or removed (order, duplicates or whitespace changed). Compare against the received file.\n";
        }

        return string.Join("\n", lines) + "\n";
    }
}
