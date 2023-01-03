namespace OptRes;

public static partial class Extensions
{
    // ctor
    /// <summary>
    /// Creates a result as the Ok variant.
    /// <code>
    /// Res result = Ok();
    /// Assert(result.IsOk);
    /// </code>
    /// </summary>
    public static Res Ok()
        => default;
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="errorMessage"/>.
    /// <code>
    /// static Res AddUser(User user)
    /// {
    ///     if (AlreadyExists(user))
    ///         return Err($"user '{user.Id}' already exists.");
    ///     if (HasAvailableCapacity(session))
    ///         return Err("not enough capacity");
    ///     else
    ///     {
    ///         // add user
    ///         return Ok();
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    public static Res Err(string errorMessage)
        => new(errorMessage, string.Empty, null);
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="errorMessage"/>, <paramref name="when"/>.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="when">Operation when the error is observed.</param>
    public static Res Err(string errorMessage, string when)
        => new(errorMessage, when, null);
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="when"/>, <paramref name="exception"/>.
    /// <code>
    /// static Res PutItem(Item item)
    /// {
    ///     try
    ///     {
    ///         PutItemToDatabase(item);
    ///         return Ok();
    ///     }
    ///     catch (Exception e)
    ///     {
    ///         return Err(nameof(PutItem), e);
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="when">Operation when the error is observed.</param>
    /// <param name="exception">Exception causing the error.</param>
    public static Res Err(string when, Exception exception)
        => new(string.Empty, when, exception);
    /// <summary>
    /// Creates a result as the Err variant; with the given error information: <paramref name="errorMessage"/>, <paramref name="when"/>, <paramref name="exception"/>.
    /// <code>
    /// static Res PutItem(Item item)
    /// {
    ///     try
    ///     {
    ///         PutItemToDatabase(item);
    ///         return Ok();
    ///     }
    ///     catch (Exception e)
    ///     {
    ///         return Err("failed to execute sql command.", nameof(PutItem), e);
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="when">Operation when the error is observed.</param>
    /// <param name="exception">Exception causing the error.</param>
    public static Res Err(string errorMessage, string when, Exception exception)
        => new(errorMessage, when, exception);


    // okif
    /// <summary>
    /// Creates a result as Ok variant if the <paramref name="okCondition"/> holds.
    /// Otherwise, it will map into an Err.
    /// <code>
    /// static Res ValidateInput(Form form)
    /// {
    ///     return OkIf(!form.HasEmptyFields())
    ///         .OkIf(form.Date &lt;= DateTime.Now)
    ///         // chained validation calls
    ///         .OkIf(repo.AlreadyContains(form.Id));
    /// }
    /// </code>
    /// </summary>
    /// <param name="okCondition">Condition that must hold for the return value to be Ok().</param>
    /// <param name="name">Name of the condition; to be appended to the error message if it does not hold. Omitting the argument will automatically be filled with the condition's expression in the caller side.</param>
    public static Res OkIf(bool okCondition, [CallerArgumentExpression("okCondition")] string name = "")
        => okCondition ? default : new("Condition doesn't hold.", name, null);
    /// <summary>
    /// Creates a result as Err variant if the <paramref name="errorCondition"/> holds.
    /// Otherwise, it will return Ok.
    /// <code>
    /// static Res ValidateInput(Form form)
    /// {
    ///     return ErrIf(form.HasEmptyFields())
    ///         .ErrIf(form.Date > DateTime.Now)
    ///         // chained validation calls
    ///         .OkIf(repo.AlreadyContains(form.Id));
    /// }
    /// </code>
    /// </summary>
    /// <param name="errorCondition">Condition that must hold for the return value to be Err.</param>
    /// <param name="name">Name of the condition; to be appended to the error message if it does not hold. Omitting the argument will automatically be filled with the condition's expression in the caller side.</param>
    public static Res ErrIf(bool errorCondition, [CallerArgumentExpression("errorCondition")] string name = "")
        => errorCondition ? new("Error condition holds.", name, null) : default;


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
