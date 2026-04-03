namespace DeviceManagement.Search;

/// <summary>Builds searchable RAM text for MongoDB text indexes (numeric field is not text-indexable).</summary>
public static class DeviceRamSearchText
{
    /// <summary>Tokens for common query variants: e.g. 16, 16gb, gb, ram.</summary>
    public static string Build(int ramGb)
    {
        if (ramGb <= 0)
            return "ram memory gb gigabyte";

        return $"{ramGb} {ramGb}gb gb ram memory gigabyte gigabytes";
    }
}
