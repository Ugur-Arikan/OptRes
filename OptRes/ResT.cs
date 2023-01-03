namespace OptRes;

/// <summary>
/// Result type which can be either of the two variants: Ok(value-of-<typeparamref name="T"/>) or Err(error-message).
/// </summary>
public readonly struct Res<T>
{
    // data
    readonly T? Val;
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
    /// Result type which can either be Ok(value) or Err.
    /// Parameterless ctor returns Err; better use <see cref="Ok{T}(T)"/> or <see cref="Err{T}(string)"/> to construct results by adding `using static OptRes.Ext`.
    /// </summary>
    public Res()
    {
        Val = default;
        Err = "Invalid construction of Res<T>; use Ok(value) or Err(...) instead.";
    }
    internal Res(T? value)
    {
        Val = value;
        if (typeof(T).IsClass)
            Err = value != null ? null : "Null value";
        else
            Err = null;
    }
    internal Res(string msg, string when, Exception? e)
    {
        Val = default;
        Err = Config.GetErrorString((msg, when, e));
    }
    /// <summary>
    /// Implicitly converts to <paramref name="value"/> into Ok(<paramref name="value"/>).
    /// Note that it is safe to convert a non-null value to result as Ok(value).
    /// <code>
    /// Res&lt;int> number = 42;
    /// // is equivalent to:
    /// Res&lt;int> number = Ok(42);
    /// </code>
    /// </summary>
    /// <param name="value">A non-null value of T.</param>
    public static implicit operator Res<T>(T value)
        => new(value);


    /// <summary>
    /// Converts into <see cref="Res"/> dropping the value if it <see cref="IsOk"/>.
    /// </summary>
    /// <returns></returns>
    public Res WithoutVal()
        => Err == null ? new() : new(Err, string.Empty, null);


    // throw
    /// <summary>
    /// Returns the result back when <see cref="IsOk"/>; throws when <see cref="IsErr"/>.
    /// <code>
    /// static Res&lt;User> QueryUser(..) {
    ///     // might fail; hence, returns a Res&lt;User> rather than just User.
    /// }
    /// var result = QueryUser(..).ThrowIfErr();
    /// // result will be:
    /// // - Ok(user) if QueryUser succeeds and returns Ok of the user;
    /// // - the application will throw otherwise.
    /// </code>
    /// </summary>
    public Res<T> ThrowIfErr()
    {
        if (Err != null)
            throw new NullReferenceException(Err);
        else
            return this;
    }


    // conversions
    /// <summary>
    /// Converts Res&lt;T> to Opt&lt;T>, by mapping:
    /// <list type="bullet">
    /// <item>Ok(val) to Some(val),</item>
    /// <item>Err to None.</item>
    /// </list>
    /// </summary>
    public Opt<T> IntoOpt()
        => IsOk ? new Opt<T>(Val) : new Opt<T>();


    // okif
    /// <summary>
    /// Returns back the Err if this is Err.
    /// Otherwise, returns Ok(value) if <paramref name="condition"/>(value) holds; Err if it does not hold.
    /// Especially useful in fluent input validation.
    /// <code>
    /// static Res&lt;Account> TryParseAccount(..) { }
    /// static bool IsAccountNumberValid(int number) { }
    /// static bool DoesAccountExist(string code) { }
    /// 
    /// var account = TryParseAccount(..)
    ///                 .OkIf(acc => IsAccountNumberValid(acc.Number))
    ///                 .OkIf(acc => DoesAccountExist(acc.Code));
    /// // account will be Ok(account) only if:
    /// // - TryParseAccount returns Ok(account), and further,
    /// // - both IsAccountNumberValid and DoesAccountExist validation checks return true.
    /// </code>
    /// </summary>
    /// <param name="condition">Condition on the underlying value that should hold to get an Ok, rather than Err.</param>
    /// <param name="name">Name of the condition; to be appended to the error message if it does not hold. Omitting the argument will automatically be filled with the condition's expression in the caller side.</param>
    public Res<T> OkIf(Func<T, bool> condition, [CallerArgumentExpression("condition")] string name = "")
        => IsErr || Val == null ? this : (condition(Val) ? this : new("Condition doesn't hold.", name, null));


    // unwrap
    /// <summary>
    /// Returns Some(error-message) if IsErr; None&lt;string>() if IsOk.
    /// <code>
    /// var user = Err&lt;User>("failed to get user");
    /// Assert(user.ErrorMessage() == Some("failed to get user"));
    /// </code>
    /// </summary>
    public Opt<string> ErrorMessage()
        => Err == null ? None<string>() : Some(Err);
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; or throws when <see cref="IsErr"/>.
    /// Must be called shyly, as it is not necessary to unwrap until the final result is achieved due to Map, FlatMap and TryMap methods.
    /// <code>
    /// Res&lt;int> resultAge = "42".ParseIntOrErr();
    /// if (resultAge.IsSome) {
    ///     int age = resultAge.Unwrap(); // use the uwrapped age
    /// } else { // handle the Err case
    /// }
    /// </code>
    /// </summary>
    public T Unwrap()
        => (Err == null && Val != null) ? Val : throw new NullReferenceException(string.Format("Cannot Unwrap Err.\n{0}", Err));
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsErr"/>.
    /// This is a safe way to unwrap the result, by explicitly handling the Err variant.
    /// Use the lazy <see cref="UnwrapOr(Func{T})"/> variant if the computation of the fallback value is expensive.
    /// <code>
    /// Assert(Ok(42).UnwrapOr(7) == 42);
    /// Assert(Err&lt;int>("error-message").UnwrapOr(7) == 7);
    /// </code>
    /// </summary>
    /// <param name="fallbackValue">Fallback value that will be returned if the result is Err.</param>
    public T UnwrapOr(T fallbackValue)
        => (Err == null && Val != null) ? Val : fallbackValue;
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; or returns <paramref name="lazyFallbackValue"/>() when <see cref="IsErr"/>.
    /// This is a safe way to unwrap the result, by explicitly handling the Err variant.
    /// Use the eager <see cref="UnwrapOr(T)"/> variant if the fallback value is cheap or readily available.
    /// <code>
    /// static string ParseUserTablename(..) { /*parses the table name from command line input; might throw!*/ }
    /// static string QueryUserTablename(..) { /*makes an expensive db-call to find out the table name*/ }
    /// 
    /// string userTable = Ok()                                         // Res, certainly Ok
    ///                     .TryMap(() => ParseUserTablename(..))       // Res&lt;string>: might be Err if parser throws
    ///                     .UnwrapOr(() => QueryUserTablename(..));    // directly returns ParseUserTablename's result if it is Ok;
    ///                                                                 // calls QueryUserTablename otherwise and returns its result.
    /// </code>
    /// </summary>
    /// <param name="lazyFallbackValue">Function to be called lazily to create the return value if the option is None.</param>
    public T UnwrapOr(Func<T> lazyFallbackValue)
        => (Err == null && Val != null) ? Val : lazyFallbackValue();


    // match
    /// <summary>
    /// Maps into <paramref name="whenOk"/>(Unwrap()) whenever IsOk; and into <paramref name="whenErr"/>(error-message) otherwise.
    /// <code>
    /// Res&lt;User> user = TryGetUser(..);
    /// string greeting = user.Match(u => $"Welcome back {u.Name}", err => $"Failed to get user. {err}");
    /// // equivalently:
    /// greeting = user.Match(
    ///     whenOk: u => $"Welcome back {u.Name}",
    ///     whenErr: err => $"Failed to get user. {err}"
    /// );
    /// </code>
    /// </summary>
    /// <param name="whenOk">Mapping function (T -> TOut) that will be called with Unwrapped value to get the return value when Ok.</param>
    /// <param name="whenErr">Function of the error message to get the return value when Err.</param>
    public TOut Match<TOut>(Func<T, TOut> whenOk, Func<string, TOut> whenErr)
    {
        if (Err != null)
            return whenErr(Err);
        if (Val != null)
            return whenOk(Val);
        throw Exc.MustNotReach;
    }
    /// <summary>
    /// Executes <paramref name="whenOk"/>(Unwrap()) if IsOk; <paramref name="whenErr"/>(error-message) otherwise.
    /// <code>
    /// Res&lt;User> user = LoginUser(..);
    /// user.MatchDo(
    ///     whenOk: u => Log.Info($"Logged in user: {u.Name}"),
    ///     whenErr: err => Log.Error($"Failed login. ${err}")
    /// );
    /// </code>
    /// </summary>
    /// <param name="whenOk">Action of the underlying value to be called lazily when IsOk.</param>
    /// <param name="whenErr">Action of error message to be called lazily when IsErr.</param>
    public void MatchDo(Action<T> whenOk, Action<string> whenErr)
    {
        if (Err != null)
            whenErr(Err);
        if (Val != null)
            whenOk(Val);
        throw Exc.MustNotReach;
    }


    // do
    /// <summary>
    /// Runs <paramref name="action"/>(Unwrap()) only if IsOk; and returns itself back.
    /// <code>
    /// // the logging call will only be made if the result of TryGetUser is Ok of a user.
    /// // Since Do returns back the result, it can still be assigned to var 'user'.
    /// Res&lt;User> user = TryGetUser().Do(u => Log.Info($"User '{u.Name}' grabbed"));
    /// </code>
    /// </summary>
    /// <param name="action">Action that will be called with the underlying value when Ok.</param>
    public Res<T> Do(Action<T> action)
    {
        if (Err == null && Val != null)
            action(Val);
        return this;
    }
    // do-if-err
    /// <summary>
    /// Runs <paramref name="actionOnErr"/>() only if IsErr; and returns itself back.
    /// Counterpart of <see cref="Do(Action{T})"/> for the Err variant.
    /// <code>
    /// // the logging call will only be made if the result of TryGetUser is Err.
    /// // Since DoIfErr returns back the result, it can still be assigned to var 'user'.
    /// Res&lt;User> user = TryGetUser().DoIfErr(err => Log.Warning($"User could not be read. {err}"));
    /// </code>
    /// </summary>
    /// <param name="actionOnErr">Action that will be called when Err.</param>
    public Res<T> DoIfErr(Action<string> actionOnErr)
    {
        if (Err != null)
            actionOnErr(ToString());
        return this;
    }


    // map
    /// <summary>
    /// Returns the Err back when IsErr; Ok(<paramref name="map"/>(Unwrap())) when IsOk.
    /// <code>
    /// // session will be Err if the user is Err; Ok of a session for the user when Ok.
    /// Res&lt;Session> session = TryGetUser.Map(user => NewSession(user.Secrets));
    /// </code>
    /// </summary>
    /// <param name="map">Mapper function (T -> TOut) to be called with the underlying value when Ok.</param>
    public Res<TOut> Map<TOut>(Func<T, TOut> map)
    {
        if (Err == null && Val != null)
            return Ok(map(Val));
        else if (Err != null)
            return Err<TOut>(Err);
        else
            throw Exc.MustNotReach;
    }


    // flatmap
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>(Unwrap()) when IsOk flattenning the result.
    /// Shorthand combining Map and Flatten calls.
    /// <code>
    /// static Res&lt;Team> TryGetTeam() { .. } // tries to grab a team; might fail, hence, returns Res.
    /// static Res TryPutTeam(Team team) { .. } // tries to put the team; might fail, hence, returns Res.
    /// 
    /// var result = TryGetTeam().FlatMap(TryPutTeam);
    /// // equivalently:
    /// var result = TryGetTeam().FlatMap(team => TryPutTeam(team));
    /// 
    /// // this is a shorthand for:
    /// var result = TryGetTeam()   // Res&lt;Team>
    ///     .Map(TryPutTeam)        // Res&lt;Res>
    ///     .Flatten();             // Res
    /// </code>
    /// </summary>
    /// <param name="map">Function (T -> Res) that maps the underlying value to a Res when IsOk.</param>
    public Res FlatMap(Func<T, Res> map)
    {
        if (Err == null && Val != null)
            return map(Val);
        else if (Err != null)
            return Err(Err);
        else
            throw Exc.MustNotReach;
    }
    /// <summary>
    /// Returns None when IsNone; <paramref name="map"/>(val) when IsOk flattening the result.
    /// Shorthand combining Map and Flatten calls.
    /// <code>
    /// static Res&lt;User> TryGetUser() {
    ///     // method that tries to get the user, return Ok(user) or Err.
    /// }
    /// static Res&lt; double> TryGetBalance(User user) {
    ///     // method that tries to get usedr's balance; which might fail, returns:
    ///     // Ok(balance) or Err
    /// }
    /// Res&lt;double> balance = TryGetUser().FlatMap(TryGetBalance);
    /// // equivalent to both below:
    /// var balance = TryGetUser().FlatMap(user => TryGetBalance(user));
    /// var balance = TryGetUser()              // Res&lt;User>
    ///     .Map(user => TryGetBalance(user))   // Res&lt;Res>
    ///     .Flatten();                         // Res
    /// </code>
    /// </summary>
    /// <param name="map">Function (T -> Res&lt;TOut>) mapping the underlying value to result of TOut if this.IsOk.</param>
    public Res<TOut> FlatMap<TOut>(Func<T, Res<TOut>> map)
    {
        if (Err == null && Val != null)
            return map(Val);
        else if (Err != null)
            return Err<TOut>(Err);
        else
            throw Exc.MustNotReach;
    }


    // try
    /// <summary>
    /// When IsOk executes <paramref name="action"/>(val) in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// <code>
    /// static Res&lt;User> TryGetUser() { .. }
    /// static void PutUserToDb(User user) {
    ///     // method that writes the user to a database table
    ///     // might fail and throw!
    /// }
    /// 
    /// Res&lt;User> user = TryGetUser().Try(PutUserToDb);
    /// // equivalently:
    /// Res&lt;User> user = TryGetUser().Try(() => PutUserToDb());
    /// 
    /// // user will be:
    /// // - Err(called on Err) if () returns Err.
    /// // - Err(relevant error message) if () returns Ok(user) but database action throws an exception.
    /// // - Ok(user) if () returns Ok(user), further the action is operated successfully;
    ///
    /// // it provides a shorthand for the following verbose/unpleasant version:
    /// Res&lt;User> user = TryGetUser();
    /// if (user.IsOk)
    /// {
    ///     try
    ///     {
    ///         PutUserToDb(user.Unwrap());
    ///     }
    ///     catch (Exception e)
    ///     {
    ///         user = Err&lt;User>("db-operation failed, check the exception message: " + e.Message);
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="action">Action to be called with the underlying value in try-catch block when Ok.</param>
    /// <param name="name">Name of the operation/action; to be appended to the error messages if the action throws. Omitting the argument will automatically be filled with the action's expression in the caller side.</param>
    public Res<T> Try(Action<T> action, [CallerArgumentExpression("action")] string name = "")
    {
        if (Err == null && Val != null)
        {
            try
            {
                action(Val);
                return this;
            }
            catch (Exception e)
            {
                return new(string.Empty, name, e);
            }
        }
        else
            return this;
    }


    // try-map
    /// <summary>
    /// Returns the error when IsErr.
    /// Otherwise, tries to map into Ok(<paramref name="map"/>(val)) in a try-catch block and returns the Err if it throws.
    /// <code>
    /// static Res&lt;User> TryGetUser() { .. }
    /// static long PutUserToDbGetId(User user) {
    ///     // method that writes the user to a database table and returns back the auto-generated id/primary-key
    ///     // might fail and throw!
    /// }
    /// 
    /// Res&lt;long> id = TryGetUser().TryMap(PutUserToDbGetId);
    /// // equivalently:
    /// Res&lt;long> id = TryGetUser().TryMap(user => PutUserToDbGetId(user));
    /// // Res&lt;long> id will be:
    /// // - Err(called on Err) when TryGetUser returns Err,
    /// // - Err(relevant error message) when TryGetUser returns Ok(user) but the database transaction throws an exception,
    /// // - Ok(id) when TryGetUser returns Ok(user), the database transaction succeeds and returns the auto-generated id.
    /// 
    /// // it provides a shorthand for the following verbose/unpleasant version:
    /// Opt&lt;User> user = TryGetUser();
    /// Res&lt;long> id;
    /// if (user.IsNone)
    ///     id = Err&lt;long>("no user");
    /// else
    /// {
    ///     try
    ///     {
    ///         id = Ok(PutUserToDb(user.Unwrap()));
    ///     }
    ///     catch (Exception e)
    ///     {
    ///         id = Err&lt;long>("db-operation failed, check the exception message: " + e.Message);
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="map">Function (T -> TOut) to be called in try-catch block to get the result when Ok; will not be called when Err.</param>
    /// <param name="name">Name of the map function/operation; to be appended to the error messages if the function throws. Omitting the argument will automatically be filled with the function's expression in the caller side.</param>
    public Res<TOut> TryMap<TOut>(Func<T, TOut> map, [CallerArgumentExpression("map")] string name = "")
    {
        if (Err == null && Val != null)
        {
            try
            {
                return new(map(Val));
            }
            catch (Exception e)
            {
                return new(string.Empty, name, e);
            }
        }
        else
            return new(Err ?? string.Empty, string.Empty, null);
    }


    // map-append
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/>(Unwrap()) when IsOk.
    /// <code>
    /// static Res&lt;Person> TryGetPerson() { .. }
    /// static Session GetSession(Person person) { .. }
    /// 
    /// var personSession =
    ///     TryGetPerson()                      // Res&lt;Person>
    ///     .MapAppend(p => GetSession(p));     // Res&lt;(Person, Session)>
    /// 
    /// // here personSession is:
    /// // * Err if TryGetPerson returns Err, here GetSession will not be called,
    /// // * Ok((person, GetSession(person))) if TryGetPerson returns Ok(person).
    /// </code>
    /// </summary>
    /// <param name="getNextResult">Function of T to be called lazily to get the value to append to the current when IsOk.</param>
    public Res<(T, T2)> MapAppend<T2>(Func<T, T2> getNextResult)
    {
        if (IsErr)
            return Err<(T, T2)>(ToString());
        return Ok((Unwrap(), getNextResult(Unwrap())));
    }
    // flatmap-append
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/>(Unwrap()).Flatten() when IsOk.
    /// <code>
    /// static Res&lt;Person> TryGetPerson() { .. }
    /// static Res&lt;Session> TryGetSession(Person person) { .. }
    /// 
    /// var personSession =
    ///     TryGetPerson()                          // Res&lt;Person>
    ///     .FlatMapAppend(p => TryGetSession(p));  // Res&lt;(Person, Session)>
    ///     
    /// // equivalent to:
    /// var personSession =
    ///     TryGetPerson()
    ///     .MapAppend(p => TryGetSession(p).Flatten());
    /// 
    /// // here personSession is:
    /// // * Err if TryGetPerson returns Err, here TryGetSession will not be called,
    /// // * Err if TryGetSession returns Err,
    /// // * Ok((person, session)) if TryGetPerson returns Ok(person), and TryGetSession(person) returns Ok(session).
    /// </code>
    /// </summary>
    public Res<(T, T2)> FlatMapAppend<T2>(Func<T, Res<T2>> getNextResult)
    {
        if (IsErr)
            return Err<(T, T2)>(ToString());

        var next = getNextResult(Unwrap());
        if (next.IsErr)
            return Err<(T, T2)>(next.ToString());

        return Ok((Unwrap(), next.Unwrap()));
    }


    // common
    /// <summary>
    /// String representation.
    /// </summary>
    public override string ToString()
        => Err ?? string.Format("Ok({0})", Val);
}
