namespace GM_Buddy.Contracts.Constants;

/// <summary>
/// Constants for the disposition scale used in entity relationships.
/// Disposition represents how an entity feels toward another, ranging from
/// extremely hostile (-5) to completely devoted (+5).
/// </summary>
public static class DispositionScale
{
    public const int Min = -5;
    public const int Max = 5;

    /// <summary>
    /// Returns a descriptive label for the given disposition score
    /// </summary>
    public static string GetLabel(int? disposition)
    {
        return disposition switch
        {
            -5 => "Hostile",
            -4 or -3 => "Antagonistic",
            -2 or -1 => "Unfriendly",
            0 => "Neutral",
            1 or 2 => "Friendly",
            3 or 4 => "Loyal",
            5 => "Devoted",
            null => "Unset",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Validates that a disposition score is within the allowed range
    /// </summary>
    public static bool IsValid(int? disposition)
    {
        return disposition is null or (>= Min and <= Max);
    }
}
