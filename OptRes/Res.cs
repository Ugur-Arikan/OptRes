namespace OptRes;

/// <summary>
/// Valueless result type which can be either of the two variants: Ok or Err(error-message).
/// </summary>
public readonly struct Res
{
    // data
    readonly string? Err;


    // prop
    /// <summary>
    /// True if the result is Ok; false otherwise.
    /// </summary>
    public bool IsOk
        => Err == null;
    /// <summary>
    /// True if the result is Err; false otherwise.
    /// </summary>
    public bool IsErr
        => Err != null;


    // ctor
    /// <summary>
    /// Result type which can either be Ok or Err.
    /// Parameterless ctor returns Ok; better use <see cref="Ok"/> or <see cref="Extensions.Err(string)"/> to construct results by adding `using static OptRes.Ext`.
    /// </summary>
    public Res()
    {
        Err = default;
    }
    internal Res(string msg, string when, Exception? e)
        => Err = Config.GetErrorString((msg, when, e));


    // throw
    /// <summary>
    /// Returns the result back when <see cref="IsOk"/>; throws when <see cref="IsErr"/>.
    /// <code>
    /// static Res MakeApiCall() {
    ///     // method that makes an api call.
    ///     // might fail; hence, returns a Res rather than void.
    /// }
    /// var result = MakeApiCall().ThrowIfErr();
    /// // result will be:
    /// // - Ok() if MakeApiCall succeeds and returns Ok;
    /// // - the application will throw otherwise.
    /// </code>
    /// </summary>
    public Res ThrowIfErr()
    {
        if (Err != null)
            throw new NullReferenceException(Err);
        else
            return this;
    }


    // okif
    /// <summary>
    /// Returns back the Err if this is Err.
    /// Otherwise, returns Ok if <paramref name="condition"/> holds; Err if it does not hold.
    /// Especially useful in fluent input validation.
    /// <code>
    /// static Res&lt;User> Login(string username, string passwordHash)
    /// {
    ///     return OkIf(!string.IsNullOrEmpty(username))    // validate username
    ///         .OkIf(!string.IsNullOrEmpty(passwordHash))  // validate password-hash
    ///         .OkIf(userRepo.ContainsKey(username))       // further validate user
    ///         .Map(() => GetUser(username, password));    // finally map into actual result;
    ///                                                     // any Err in validation steps will directly be mapped to Err, avoiding GetUser call.
    /// }
    /// </code>
    /// </summary>
    /// <param name="condition">Condition that should hold to get an Ok, rather than Err.</param>
    /// <param name="name">Name of the condition; to be appended to the error message if it does not hold. Omitting the argument will automatically be filled with the condition's expression in the caller side.</param>
    public Res OkIf(bool condition, [CallerArgumentExpression("condition")] string name = "")
        => IsErr ? this : (condition ? default : new("Condition doesn't hold.", name, null));


    // unwrap
    /// <summary>
    /// Returns Some(error-message) if IsErr; None&lt;string>() if IsOk.
    /// <code>
    /// var result = Err("failed to connect");
    /// Assert(result.ErrorMessage() == Some("failed to connect"));
    /// </code>
    /// </summary>
    public Opt<string> ErrorMessage()
        => Err == null ? None<string>() : Some(Err);


    // match
    /// <summary>
    /// Maps into <paramref name="whenOk"/> whenever IsOk; and into <paramref name="whenErr"/>(errorMessage) otherwise.
    /// <code>
    /// Res&lt;User> user = TryGetUser(..);
    /// string greetingMessage = user.Match("Welcome", err => $"Error getting the user: {err}");
    /// </code>
    /// </summary>
    /// <param name="whenOk">Return value when IsOk.</param>
    /// <param name="whenErr">Map function (string -> TOut) to be called to get the return value when IsErr.</param>
    public TOut Match<TOut>(TOut whenOk, Func<string, TOut> whenErr)
        => IsOk ? whenOk : whenErr(ToString());
    /// <summary>
    /// Maps into <paramref name="whenOk"/>() lazily whenever IsOk; and into <paramref name="whenErr"/>(errorMessage) otherwise.
    /// Similar to <see cref="Match{TOut}(TOut, Func{string, TOut})"/> except that whenOk is lazy and evaluated only when IsOk.
    /// </summary>
    /// <param name="whenOk">Function (() -> TOut) to be called lazily to get the return value when IsOk.</param>
    /// <param name="whenErr">Map function (string -> TOut) to be called to get the return value when IsErr.</param>
    public TOut Match<TOut>(Func<TOut> whenOk, Func<string, TOut> whenErr)
        => IsOk ? whenOk() : whenErr(ToString());
    /// <summary>
    /// Executes <paramref name="whenOk"/>() whenever IsOk; and <paramref name="whenErr"/>(errorMessage) otherwise.
    /// <code>
    /// Res&lt;User> user = TryGetUser(..);
    /// user.MatchDo
    /// (
    ///     whenOk: () => Log.Info("New user login"),
    ///     whenErr: err => Log.Error($"Failed login. {err}")
    /// );
    /// </code>
    /// </summary>
    public void MatchDo<TOut>(Action whenOk, Action<string> whenErr)
    {
        if (IsOk)
            whenOk();
        else
            whenErr(ToString());
    }


    // do
    /// <summary>
    /// Runs <paramref name="action"/>() only if IsOk; and returns itself back.
    /// <code>
    /// User user = CreateUser(/*inputs*/);
    /// Res result = TryPutUserToDb(user).Do(Log.Success("user created"));
    /// // result will be:
    /// // - Ok if TryPutUserToDb succeeds and returns Ok; in this case the success message will be logged; or
    /// // - Err if TryPutUserToDb returns Err; and the success message log will not be called.
    /// </code>
    /// </summary>
    /// <param name="action">Action to be executed only if this IsOk.</param>
    public Res Do(Action action)
    {
        if (IsOk)
            action();
        return this;
    }
    
    
    // do-if-err
    /// <summary>
    /// Runs <paramref name="action"/>() only if IsErr; and returns itself back.
    /// This is the counterpart of the <see cref="Do(Action)"/> method.
    /// <code>
    /// Res result = RefreshIndices(/*inputs*/).DoIfErr(() => Alert("database connection failed"));
    /// // result will be:
    /// // - Ok if refreshing db indices succeeded;
    /// // - Err if it failed, in which case the Alert call will be made.
    /// </code>
    /// </summary>
    /// <param name="action">Action to be executed only if this IsErr.</param>
    public Res DoIfErr(Action action)
    {
        if (Err != null)
            action();
        return this;
    }
    /// <summary>
    /// Runs <paramref name="action"/>(error-message) only if IsErr; and returns itself back.
    /// This is the counterpart of the <see cref="Do(Action)"/> method.
    /// <code>
    /// Res result = RefreshIndices(/*inputs*/).DoIfErr(err =>  Alert($"database connection failed: {err}"));
    /// // result will be:
    /// // - Ok if refreshing db indices succeeded;
    /// // - Err if it failed, in which case the Alert call will be made.
    /// </code>
    /// </summary>
    /// <param name="action">Action to be executed only if this IsErr.</param>
    public Res DoIfErr(Action<string> action)
    {
        if (Err != null)
            action(ToString());
        return this;
    }


    // map
    /// <summary>
    /// Returns the error when IsErr; Ok(<paramref name="map"/>()) when IsOk.
    /// <code>
    /// Res ValidateInputs(Inputs inputs) { /*checks*/ }
    /// Output CalcOutput(Inputs inputs) { /*maps inputs to output*/ }
    /// 
    /// Inputs inputs = GetInputs();
    /// Res&lt;Output> output = ValidateInputs(inputs).Map(() => CalcOutput(inputs));
    /// // output will be:
    /// // - Err if ValidateInputs returns Err omitting the call to CalcOutput;
    /// // - Ok(CalcOutput(inputs)) if ValidateInputs returns Ok.
    /// </code>
    /// </summary>
    /// <param name="map">Map function that will be called lazily to get the return value only if this IsOk.</param>
    public Res<TOut> Map<TOut>(Func<TOut> map)
        => Err == null ? new(map()) : new(Err, string.Empty, null);


    // flatmap
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>() when IsOk, flattenning the result.
    /// This is a shorthand for sequential Map and Flatten calls.
    /// <code>
    /// // assume we have two methods that can fail; hence returns a Res:
    /// static Res TryRunRiskyOperation() { .. }
    /// static Res TryLogCompletion() { .. }
    /// 
    /// // we want to call both operations; but the second one only if the first one succeeds.
    /// Res result = TryRunRiskyOperation().FlatMap(TryLogCompletion);
    /// // alternatively:
    /// Res result = TryRunRiskyOperation().FlatMap(() => TryLogCompletion());
    /// 
    /// // this is equivalent to:
    /// Res result = TryRunRiskyOperation().Map(() => TryLogCompletion()/*Res&lt;Res>*/).Flatten()/*Res*/;
    /// </code>
    /// </summary>
    /// <param name="map">Map function to be called lazily to get the final result only if this IsOk.</param>
    public Res FlatMap(Func<Res> map)
        => Err == null ? map() : new(Err, string.Empty, null);
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>() when IsOk, flattenning the result.
    /// This is a shorthand for sequential Map and Flatten calls.
    /// <code>
    /// // assume we have two methods that can fail; hence returns a Res:
    /// static Res TryRunRiskyOperation() { .. }
    /// static Res&lt;int> TryGetCount() { .. }
    /// 
    /// // we want to call both operations; but the second one only if the first one succeeds.
    /// Res result = TryRunRiskyOperation().FlatMap(TryGetCount);
    /// // alternatively:
    /// Res result = TryRunRiskyOperation().FlatMap(() => TryGetCount());
    /// 
    /// // this is equivalent to:
    /// Res result = TryRunRiskyOperation().Map(() => TryGetCount()/*Res&lt;Res&lt;int>>*/).Flatten()/*Res&lt;int>*/;
    /// </code>
    /// </summary>
    /// <param name="map">Map function to be called lazily to get the final result only if this IsOk.</param>
    public Res<TOut> FlatMap<TOut>(Func<Res<TOut>> map)
        => Err == null ? map() : new(Err, string.Empty, null);


    // try
    /// <summary>
    /// When IsOk executes <paramref name="action"/>() in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// <code>
    /// static Res TryLogin() { .. }
    /// static void ClearSessionHistory() { /*risky function, might throw!*/ }
    /// 
    /// var result = TryLogin().Try(ClearSessionHistory);
    /// // the result will be:
    /// // - Err if TryLogin returns Err, in which case ClearSessionHistory is never called;
    /// // - Ok if TryLogin returns Ok; and ClearSessionHistory call succeeds without an exception;
    /// // - Err if TryLogin returns Ok, but ClearSessionHistory throws an exception.
    /// </code>
    /// </summary>
    /// <param name="action">Action to be executed in a try-catcy block only if this IsOk.</param>
    /// <param name="name">Name of the map action; to be appended to the error messages if the action throws. Omitting the argument will automatically be filled with the action's expression in the caller side.</param>
    public Res Try(Action action, [CallerArgumentExpression("action")] string name = "")
        => Err == null ? Extensions.Try(action, name) : this;


    // try-map
    /// <summary>
    /// Returns the error when IsErr.
    /// Otherwise, tries to map into Ok(<paramref name="map"/>()) in a try-catch block and returns the Err if it throws.
    /// <code>
    /// static Res ValidateUser(User user) { /*returns Ok if valid; Err o/w*/ }
    /// static Res&lt;Secret> TryGetSecrets(User user) { /*returns Ok(secrets) if succeds; Err if fails to get secrets*/ }
    /// 
    /// User user = GetUser(..);
    /// var secrets = ValidateUser(user).TryMap(() => TryGetSecrets(user));
    /// // TryGetSecrets will be called only if ValidateUser call returns Ok;
    /// // secrets will be Ok of the grabbed secrets if both ValidateUser and TryGetSecrets return Ok.
    /// </code>
    /// </summary>
    /// <param name="map">Map function to be called lazily to create the result only if this IsOk.</param>
    /// <param name="name">Name of the map function; to be appended to the error messages if the function throws. Omitting the argument will automatically be filled with the function's expression in the caller side.</param>
    public Res<TOut> TryMap<TOut>(Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
    {
        if (Err == null)
            return Extensions.TryMap(map, name);
        else
            return new(Err, string.Empty, null);
    }


    // common
    /// <summary>
    /// String representation.
    /// </summary>
    public override string ToString()
        => Err ?? "Ok";
}
