namespace OptRes;

/// <summary>
/// Option type which can be either of the two variants: Some(value-of-<typeparamref name="T"/>) or None.
/// </summary>
/// <typeparam name="T">Any T.</typeparam>
public readonly struct Opt<T> : IEquatable<Opt<T>>
{
    // data
    readonly T? Val;
    /// <summary>
    /// Returns whether the option has Some value or not.
    /// <code>
    /// var someInt = Some(12);
    /// Assert(noneInt.IsNone);
    /// </code>
    /// </summary>
    public readonly bool IsSome;
    /// <summary>
    /// Returns whether the option is None or not.
    /// <code>
    /// var noneInt = None&lt;int>();
    /// Assert(noneInt.IsNone);
    /// </code>
    /// </summary>
    public bool IsNone => !IsSome;


    // ctor
    /// <summary>
    /// Option type of <typeparamref name="T"/>: either None or Some value.
    /// Parameterless ctor returns None; better use <see cref="Some{T}(T)"/> or <see cref="None{T}"/> to construct options by adding `using static OptRes.Ext`.
    /// </summary>
    public Opt()
    {
        Val = default;
        IsSome = false;
    }
    internal Opt(T? value)
    {
        Val = value;
        if (typeof(T).IsClass)
            IsSome = value != null;
        else
            IsSome = true;
    }
    /// <summary>
    /// Implicitly converts to <paramref name="value"/> into <see cref="Opt{T}"/>.Some(<paramref name="value"/>).
    /// Note that it is safe to convert a non-null value to option as Some(value).
    /// <code>
    /// Opt&lt;int> number = 42;
    /// // is equivalent to:
    /// Opt&lt;int> number = Some(42);
    /// </code>
    /// </summary>
    /// <param name="value">A non-null value of T.</param>
    public static implicit operator Opt<T>(T value)
        => new(value);


    // throw
    /// <summary>
    /// Returns the option back when <see cref="IsSome"/>; throws when <see cref="IsNone"/>.
    /// Appends the <paramref name="errorMessage"/> to the exception if the message <see cref="IsSome"/>.
    /// Can be called without breaking the fluent api.
    /// <code>
    /// var interestRate = GetOptionalUser(input)
    ///     .ThrowIfNone("failed to get the user")
    ///     .Map(user => ComputeInterestRate(user))
    ///     .Unwrap();
    /// </code>
    /// </summary>
    /// <param name="errorMessage">Optional message to append to the exception message.</param>
    public Opt<T> ThrowIfNone(Opt<string> errorMessage = default)
    {
        var _ = errorMessage.IsSome ? Unwrap(errorMessage.Unwrap()) : Unwrap();
        return this;
    }


    // unwrap
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; or throws when <see cref="IsNone"/>.
    /// Must be called shyly, as it is not necessary to unwrap until the final result is achieved due to Map, FlatMap and TryMap methods.
    /// <code>
    /// Opt&lt;int> optAge = "42".ParseIntOrNone();
    /// if (optAge.IsSome) {
    ///     int age = optAge.Unwrap(); // use the uwrapped age
    /// } else { // handle the None case
    /// }
    /// </code>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap()
        => (IsSome && Val != null) ? Val : throw new NullReferenceException("Cannot Unwrap None.");
    /// <summary>
    /// Similar to <see cref="Unwrap()"/> method except that the <paramref name="errorMessageIfNone"/> is appended to the error message if <see cref="IsNone"/>.
    /// </summary>
    /// <param name="errorMessageIfNone">Error message to append to the exception message that will be thrown if None.</param>
    public T Unwrap(string errorMessageIfNone)
        => (IsSome && Val != null) ? Val : throw new NullReferenceException(string.Format("Cannot Unwrap None. {0}", errorMessageIfNone));
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsNone"/>.
    /// This is a safe way to unwrap the optional, by explicitly handling the None variant.
    /// Use the lazy <see cref="UnwrapOr(Func{T})"/> variant if the computation of the fallback value is expensive.
    /// <code>
    /// Assert(Some(42).UnwrapOr(7) == 42);
    /// Assert(None&lt;int>().UnwrapOr(7) == 7);
    /// </code>
    /// </summary>
    /// <param name="fallbackValue">Fallback value that will be returned if the option is None.</param>
    public T UnwrapOr(T fallbackValue)
        => (IsSome && Val != null) ? Val : fallbackValue;
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; or returns <paramref name="lazyFallbackValue"/>() when <see cref="IsNone"/>.
    /// This is a safe way to unwrap the optional, by explicitly handling the None variant.
    /// Use the eager <see cref="UnwrapOr(T)"/> variant if the fallback value is cheap or readily available.
    /// <code>
    /// static int GetCapacity(IEnumerable&lt;T> collection, Opt&lt;int> givenCapacity) {
    ///     // capacity will be either the givenCapacity, or the number of elements in the collection.
    ///     // note that, collection.Count() might be expensive requiring linear search.
    ///     // lazy call avoids this call when givenCapacity.IsSome.
    ///     return givenCapacity.UnwrapOr(() => collection.Count());
    /// }
    /// </code>
    /// </summary>
    /// <param name="lazyFallbackValue">Function to be called lazily to create the return value if the option is None.</param>
    public T UnwrapOr(Func<T> lazyFallbackValue)
        => (IsSome && Val != null) ? Val : lazyFallbackValue();


    // match
    /// <summary>
    /// Maps into <paramref name="whenSome"/>(Unwrap()) whenever IsSome; and into <paramref name="whenNone"/> otherwise.
    /// <code>
    /// Opt&lt;User> user = GetOptionalUser(..);
    /// string greeting = user.Match(u => $"Welcome back {u.Name}", "Hello");
    /// greeting = user.Match(whenSome: u => $"Welcome back {u.Name}", whenNone: "Hello");
    /// </code>
    /// </summary>
    /// <param name="whenSome">Mapping function (T -> TOut) that will be called with Unwrapped value to get the return value when Some.</param>
    /// <param name="whenNone">Return value when None.</param>
    public TOut Match<TOut>(Func<T, TOut> whenSome, TOut whenNone)
        => (IsSome && Val != null) ? whenSome(Val) : whenNone;
    /// <summary>
    /// Maps into <paramref name="whenSome"/>(Unwrap()) whenever IsSome; and into lazy <paramref name="whenNone"/>() otherwise.
    /// Similar to <see cref="Match{TOut}(Func{T, TOut}, TOut)"/> except that None variant is evaluated only when IsNone.
    /// <code>
    /// // assuming QueryAnonymousGreeting() is expensive.
    /// Opt&lt;User> user = GetOptionalUser(..);
    /// string greeting = user.Match(u => $"Welcome back {u.Name}", () => QueryAnonymousGreeting());
    /// </code>
    /// </summary>
    /// <param name="whenSome">Mapping function (T -> TOut) that will be called with Unwrapped value to get the return value when Some.</param>
    /// <param name="whenNone">Function to be called lazily to get the return value when None.</param>
    public TOut Match<TOut>(Func<T, TOut> whenSome, Func<TOut> whenNone)
        => (IsSome && Val != null) ? whenSome(Val) : whenNone();
    /// <summary>
    /// Executes <paramref name="whenSome"/>(Unwrap()) if IsSome; <paramref name="whenNone"/>() otherwise.
    /// <code>
    /// static Greet(Opt&lt;User> user) {
    ///     user.MatchDo(
    ///         whenSome: u => Console.WriteLine($"Welcome back {u.Name}"),
    ///         whenNone: Console.WriteLine("Hello")
    ///     );
    /// }
    /// </code>
    /// </summary>
    /// <param name="whenSome">Action to be called lazily when Some.</param>
    /// <param name="whenNone">Action to be called lazily when None.</param>
    public void MatchDo(Action<T> whenSome, Action whenNone)
    {
        if (IsSome && Val != null)
            whenSome(Val);
        else
            whenNone();
    }


    // to res
    /// <summary>
    /// Converts Opt&lt;T> to Res&lt;T>, by mapping:
    /// <list type="bullet">
    /// <item>Some(val) to Ok(val),</item>
    /// <item>None to Err.</item>
    /// </list>
    /// </summary>
    public Res<T> IntoRes()
    {
        if (Val == null)
            return Err<T>("Required value is None.");
        else
            return Ok(Val);
    }
    /// <summary>
    /// Converts Opt&lt;T> to Res&lt;T>, by mapping:
    /// <list type="bullet">
    /// <item>Some(val) to Ok(val),</item>
    /// <item>None to Err invluding <paramref name="requiredValueName"/> in the message..</item>
    /// </list>
    /// </summary>
    /// <param name="requiredValueName">Name of the required value that will be appended to message of the Err when the option is None.</param>
    public Res<T> IntoRes(string requiredValueName)
    {
        if (Val == null)
            return Err<T>(string.Format("Required value '{0}' is None.", requiredValueName));
        else
            return Ok(Val);
    }


    // someif
    /// <summary>
    /// Returns back None if IsNone.
    /// Otherwise, returns Some(value) if <paramref name="condition"/>(value) holds; None if it does not hold.
    /// Especially useful in fluent input validation.
    /// <code>
    /// static Opt&lt;Account> MaybeParseAccount(..) { }
    /// static bool IsAccountNumberValid(int number) { }
    /// static bool DoesAccountExist(string code) { }
    /// 
    /// var account = MaybeParseAccount(..)
    ///                 .SomeIf(acc => IsAccountNumberValid(acc.Number))
    ///                 .SomeIf(acc => DoesAccountExist(acc.Code));
    /// // account will be Some(account) only if:
    /// // - MaybeParseAccount returns Some(account), and further,
    /// // - both IsAccountNumberValid and DoesAccountExist validation checks return true.
    /// </code>
    /// </summary>
    /// <param name="condition">Condition on the underlying value that should hold to get a Some, rather than None.</param>
    public Opt<T> SomeIf(Func<T, bool> condition)
        => IsNone || Val == null ? this : (condition(Val) ? this : new Opt<T>());


    // do
    /// <summary>
    /// Runs <paramref name="action"/>(Unwrap()) only if IsSome; and returns itself back.
    /// <code>
    /// // the logging call will only be made if the result of GetOptionalUser is Some of a user.
    /// // Since Do returns back the option, it can still be assigned to var 'user'.
    /// Opt&lt;User> user = GetOptionalUser().Do(u => Log.Info($"User '{u.Name}' grabbed"));
    /// </code>
    /// </summary>
    /// <param name="action">Action that will be called with the underlying value when Some.</param>
    public Opt<T> Do(Action<T> action)
    {
        if (IsSome && Val != null)
            action(Val);
        return this;
    }


    // do-if-none
    /// <summary>
    /// Runs <paramref name="actionOnNone"/>() only if IsNone; and returns itself back.
    /// Counterpart of <see cref="Do(Action{T})"/> for the None variant.
    /// <code>
    /// // the logging call will only be made if the result of GetOptionalUser is None.
    /// // Since DoIfNone returns back the option, it can still be assigned to var 'user'.
    /// Opt&lt;User> user = GetOptionalUser().DoIfNone(() => Log.Warning("User could not be read"));
    /// </code>
    /// </summary>
    /// <param name="actionOnNone">Action that will be called when None.</param>
    public Opt<T> DoIfNone(Action actionOnNone)
    {
        if (IsNone)
            actionOnNone();
        return this;
    }


    // map
    /// <summary>
    /// Returns None when IsNone; Some(<paramref name="map"/>(Unwrap())) when IsSome.
    /// <code>
    /// // session will be None if the user is None; Some of a session for the user when Some.
    /// Opt&lt;Session> session = GetOptionalUser.Map(user => NewSession(user.Secrets));
    /// </code>
    /// </summary>
    /// <param name="map">Mapper function (T -> TOut) to be called with the underlying value when Some.</param>
    public Opt<TOut> Map<TOut>(Func<T, TOut> map)
        => (IsSome && Val != null) ? Some(map(Val)) : default;


    // flat-map
    /// <summary>
    /// Returns None when IsNone; <paramref name="map"/>(val) when IsSome flattening the result.
    /// Shorthand combining Map and Flatten calls.
    /// <code>
    /// static Opt&lt;User> GetOptionalUser() {
    ///     // method that tries to get the user, which can be omitted.
    ///     ...
    /// }
    /// static Opt&lt;string> GetNickname(User user) {
    ///     // method that tries to get the nickname of the passed-in user; which is optional
    ///     ...
    /// }
    /// Opt&lt;string> nickname = GetOptionalUser().FlatMap(GetNickname);
    /// // equivalent to both below:
    /// nickname = GetOptionalUser().FlatMap(user => GetNickname(user));
    /// nickname = GetOptionalUser().Map(user => GetNickname(user) /*Opt&lt;Opt&lt;string>>*/).Flatten();
    /// </code>
    /// </summary>
    /// <param name="map">Function (T -> Opt&lt;TOut>) mapping the underlying value to option of TOut if IsSome.</param>
    public Opt<TOut> FlatMap<TOut>(Func<T, Opt<TOut>> map)
        => (IsSome && Val != null) ? map(Val) : default;


    // try
    /// <summary>
    /// When IsSome executes <paramref name="action"/>(val) in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns Err when None.
    /// <code>
    /// static Opt&lt;User> GetUser() { .. }
    /// static void PutUserToDb(User user) {
    ///     // method that writes the user to a database table
    ///     // might fail and throw!
    /// }
    /// 
    /// Res&lt;User> user = GetUser().Try(PutUserToDb);
    /// // equivalently:
    /// Res&lt;User> user = GetUser().Try(() => PutUserToDb());
    /// 
    /// // user will be:
    /// // - Err(called on None) if GetUser returns None.
    /// // - Err(relevant error message) if GetUser returns Some(user) but database action throws an exception.
    /// // - Ok(user) if GetUser returns Some(user), further the action is operated successfully;
    ///
    /// // it provides a shorthand for the following verbose/unpleasant version:
    /// Opt&lt;User> maybeUser = GetUser();
    /// Res&lt;User> user;
    /// if (maybeUser.IsNone)
    ///     user = Err&lt;User>("no user");
    /// else
    /// {
    ///     try
    ///     {
    ///         PutUserToDb(maybeUser.Unwrap());
    ///         user = Ok(maybeUser.Unwrap());
    ///     }
    ///     catch (Exception e)
    ///     {
    ///         user = Err&lt;User>("db-operation failed, check the exception message: " + e.Message);
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="action">Action to be called with the underlying value in try-catch block when Some.</param>
    /// <param name="name">Name of the operation/action; to be appended to the error messages if the action throws. Omitting the argument will automatically be filled with the action's expression in the caller side.</param>
    public Res<T> Try(Action<T> action, [CallerArgumentExpression("action")] string name = "")
    {
        if (IsSome && Val != null)
        {
            try
            {
                action(Val);
                return new(Val);
            }
            catch (Exception e)
            {
                return new(string.Empty, name, e);
            }
        }
        else
            return Err<T>(Config.GetErrorString(("Try called on None.", name, null)));
    }


    // try-map
    /// <summary>
    /// When IsOk tries to map to Ok(<paramref name="map"/>(value)) in a try-catch block: returns Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// <code>
    /// static Opt&lt;User> GetUser() { .. }
    /// static long PutUserToDbGetId(User user) {
    ///     // method that writes the user to a database table and returns back the auto-generated id/primary-key
    ///     // might fail and throw!
    /// }
    /// 
    /// Res&lt;long> id = GetUser().TryMap(PutUserToDbGetId);
    /// // equivalently:
    /// Res&lt;long> id = GetUser().TryMap(user => PutUserToDbGetId(user));
    /// 
    /// // Res&lt;long> id will be:
    /// // - Err(due to none) when GetUser returns None,
    /// // - Err(relevant error message) when GetUser returns Some(user) but the database transaction throws an exception,
    /// // - Ok(id) when GetUser returns Some(user), the database transaction succeeds and returns the auto-generated id.
    /// 
    /// // it provides a shorthand for the following verbose/unpleasant version:
    /// Opt&lt;User> maybeUser = GetUser();
    /// Res&lt;long> id;
    /// if (maybeUser.IsNone)
    ///     id = Err&lt;long>("no user");
    /// else
    /// {
    ///     try
    ///     {
    ///         id = Ok(PutUserToDb(maybeUser.Unwrap()));
    ///     }
    ///     catch (Exception e)
    ///     {
    ///         id = Err&lt;long>("db-operation failed, check the exception message: " + e.Message);
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <param name="map">Function (T -> TOut) to be called in try-catch block to get the result when Some; will not be called when None.</param>
    /// <param name="name">Name of the map function/operation; to be appended to the error messages if the function throws. Omitting the argument will automatically be filled with the function's expression in the caller side.</param>
    public Res<TOut> TryMap<TOut>(Func<T, TOut> map, [CallerArgumentExpression("map")] string name = "")
    {
        if (IsSome && Val != null)
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
            return Err<TOut>(Config.GetErrorString(("TryMap called on None.", name, null)));
    }


    // common
    /// <summary>
    /// Returns the text representation of the option.
    /// </summary>
    public override string ToString()
        => (!IsSome || Val == null) ? "None" : string.Format("Some({0})", Val);
    /// <summary>
    /// Returns whether this option is equal to the <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other optional to compare to.</param>
    public override bool Equals(object? other)
        => other != null && (other is Opt<T>) && (Equals(other));
    /// <summary>
    /// Returns true if both values are <see cref="IsSome"/> and their unwrapped values are equal; false otherwise.
    /// </summary>
    /// <param name="other">Other optional to compare to.</param>
    public bool Equals(Opt<T> other)
        => IsSome && other.IsSome && Val != null && other.Val != null && Val.Equals(other.Val);
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
        => Val == null ? int.MinValue : Val.GetHashCode();
    /// <summary>
    /// Returns true if both values are <see cref="IsSome"/> and their unwrapped values are equal; false otherwise.
    /// <code>
    /// AssertEqual(None&lt;int>() == None&lt;int>(), false);
    /// AssertEqual(None&lt;int>() == Some(42), false);
    /// AssertEqual(Some(42) == None&lt;int>(), false);
    /// AssertEqual(Some(42) == Some(7), false);
    /// AssertEqual(Some(42) == Some(42), true);
    /// </code>
    /// </summary>
    /// <param name="left">Lhs of the equality operator.</param>
    /// <param name="right">Rhs of the equality operator.</param>
    public static bool operator ==(Opt<T> left, Opt<T> right)
        => left.IsSome && right.IsSome && left.Val != null && right.Val != null && left.Val.Equals(right.Val);
    //=> left.Equals(right);
    /// <summary>
    /// Returns true if either value is <see cref="IsNone"/> or their unwrapped values are not equal; false otherwise.
    /// <code>
    /// AssertEqual(None&lt;int>() != None&lt;int>(), true);
    /// AssertEqual(None&lt;int>() != Some(42), true);
    /// AssertEqual(Some(42) != None&lt;int>(), true);
    /// AssertEqual(Some(42) != Some(7), true);
    /// AssertEqual(Some(42) != Some(42), false);
    /// </code>
    /// </summary>
    /// <param name="left">Lhs of the inequality operator.</param>
    /// <param name="right">Rhs of the inequality operator.</param>
    public static bool operator !=(Opt<T> left, Opt<T> right)
        => !(left == right);
}
