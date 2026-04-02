namespace DeviceManagement.Validation;

/// <summary>Regular expressions used with <see cref="System.ComponentModel.DataAnnotations.RegularExpressionAttribute"/>.</summary>
public static class ValidationPatterns
{
    /// <summary>24-character hexadecimal MongoDB ObjectId.</summary>
    public const string MongoObjectId = "^[a-fA-F0-9]{24}$";

    /// <summary>Empty or a valid MongoDB ObjectId (for optional foreign keys).</summary>
    public const string OptionalMongoObjectId = "^(|[a-fA-F0-9]{24})$";
}
