namespace OptRes;

public static partial class Ext
{
    // ctor
    /// <summary>
    /// Creates a result as the Ok variant.
    /// </summary>
    public static Res Ok()
        => default;
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="errorMessage"/>.
    /// </summary>
    public static Res Err(string errorMessage)
        => new(errorMessage, string.Empty, null);
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="errorMessage"/>, <paramref name="when"/>.
    /// </summary>
    public static Res Err(string errorMessage, string when)
        => new(errorMessage, when, null);
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="when"/>, <paramref name="exception"/>.
    /// </summary>
    public static Res Err(string when, Exception exception)
        => new(string.Empty, when, exception);
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="errorMessage"/>, <paramref name="when"/>, <paramref name="exception"/>.
    /// </summary>
    public static Res Err(string errorMessage, string when, Exception exception)
        => new(errorMessage, when, exception);


    // okif
    /// <summary>
    /// Creates a result as Ok variant if the <paramref name="okCondition"/> holds.
    /// Otherwise, it will map into an Err.
    /// </summary>
    public static Res OkIf(bool okCondition, [CallerArgumentExpression("okCondition")] string name = "")
        => okCondition ? default : new("Condition doesn't hold.", name, null);


    // helper - try
    internal static Res Try(Action fun, [CallerArgumentExpression("fun")] string name = "")
    {
        try
        {
            fun();
            return Ok();
        }
        catch (Exception e)
        {
            return new(string.Empty, name, e);
        }
    }
}
