namespace Vouch;

internal static class TrimMessages
{
    public const string Equivalency =
        "Trimming leaves only members the app uses, so equal objects can fail and unequal ones pass. Not supported under Native AOT.";

    public const string Members =
        "Reads members found by reflection on the runtime type; members removed by trimming are not seen.";
}
