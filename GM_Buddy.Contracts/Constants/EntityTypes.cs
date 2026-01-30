namespace GM_Buddy.Contracts.Constants;

/// <summary>
/// Constants for entity types used in polymorphic relationships
/// </summary>
public static class EntityTypes
{
    public const string Npc = "npc";
    public const string Pc = "pc";
    public const string Organization = "organization";

    /// <summary>
    /// Validates if the provided entity type is one of the allowed values
    /// </summary>
    public static bool IsValid(string entityType)
    {
        return entityType is Npc or Pc or Organization;
    }

    /// <summary>
    /// Returns all valid entity types
    /// </summary>
    public static IReadOnlyList<string> All => new[] { Npc, Pc, Organization };
}
