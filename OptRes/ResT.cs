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
    /// <inheritdoc cref="Res{T}"/>
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
    /// Implicitly converts to <paramref name="value"/> into <see cref="Opt{T}"/>.Some(<paramref name="value"/>).
    /// </summary>
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
    /// Converts into Some(val) if the result is Ok(val); None if the result is Err.
    /// </summary>
    public Opt<T> IntoOpt()
        => IsOk ? new Opt<T>(Val) : new Opt<T>();


    // okif
    /// <summary>
    /// Maps into either this (Err) if the <paramref name="condition"/> holds (fails) when IsOk; returns itself back otherwise.
    /// </summary>
    public Res<T> OkIf(bool condition, [CallerArgumentExpression("condition")] string name = "")
        => IsErr ? this : (condition ? this : new("Condition doesn't hold.", name, null));
    /// <summary>
    /// Maps into either this (Err) if the <paramref name="condition"/>(Unwrap()) holds (fails) when IsOk; returns itself back otherwise.
    /// </summary>
    public Res<T> OkIf(Func<T, bool> condition, [CallerArgumentExpression("condition")] string name = "")
        => IsErr || Val == null ? this : (condition(Val) ? this : new("Condition doesn't hold.", name, null));


    // unwrap
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; or throws when <see cref="IsErr"/>.
    /// </summary>
    public T Unwrap()
        => (Err == null && Val != null) ? Val : throw new NullReferenceException(string.Format("Cannot Unwrap Err.\n{0}", Err));
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsErr"/>.
    /// </summary>
    /// <param name="fallbackValue"></param>
    public T UnwrapOr(T fallbackValue)
        => (Err == null && Val != null) ? Val : fallbackValue;
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; or returns <paramref name="lazyFallbackValue"/>() when <see cref="IsErr"/>.
    /// </summary>
    /// <param name="lazyFallbackValue"></param>
    public T UnwrapOr(Func<T> lazyFallbackValue)
        => (Err == null && Val != null) ? Val : lazyFallbackValue();
    /// <summary>
    /// Returns the underlying value when <see cref="IsOk"/>; throws when <see cref="IsErr"/>.
    /// </summary>
    public T UnwrapOrThrow()
        => (Err == null && Val != null) ? Val : throw new NullReferenceException(string.Format("Cannot Unwrap Err.\n{0}", Err));


    // match
    /// <summary>
    /// Maps into <paramref name="whenOk"/> whenever IsOk; and into <paramref name="whenErr"/> otherwise.
    /// </summary>
    public TOut Match<TOut>(TOut whenOk, TOut whenErr)
        => IsOk ? whenOk : whenErr;
    /// <summary>
    /// Maps into <paramref name="whenOk"/> whenever IsOk; and into <paramref name="whenErr"/>() otherwise.
    /// </summary>
    public TOut Match<TOut>(TOut whenOk, Func<TOut> whenErr)
        => IsOk ? whenOk : whenErr();
    /// <summary>
    /// Maps into <paramref name="whenOk"/>() whenever IsOk; and into <paramref name="whenErr"/> otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> whenOk, TOut whenErr)
        => IsOk ? whenOk() : whenErr;
    /// <summary>
    /// Maps into <paramref name="whenOk"/>() whenever IsOk; and into <paramref name="whenErr"/>() otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> whenOk, Func<TOut> whenErr)
        => IsOk ? whenOk() : whenErr();
    /// <summary>
    /// Maps into <paramref name="whenOk"/>(Unwrap()) whenever IsOk; and into <paramref name="whenErr"/> otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> whenOk, TOut whenErr)
        => (Err == null && Val != null) ? whenOk(Val) : whenErr;
    /// <summary>
    /// Maps into <paramref name="whenOk"/>(Unwrap()) whenever IsOk; and into <paramref name="whenErr"/>() otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> whenOk, Func<TOut> whenErr)
        => (Err == null && Val != null) ? whenOk(Val) : whenErr();
    /// <summary>
    /// Executes <paramref name="whenOk"/>(Unwrap()) if IsOk; <paramref name="whenErr"/>() otherwise.
    /// </summary>
    public void MatchDo(Action<T> whenOk, Action whenErr)
    {
        if (Err == null && Val != null)
            whenOk(Val);
        else
            whenErr();
    }
    // match-with-err
    /// <summary>
    /// Maps into <paramref name="whenOk"/> whenever IsOk; and into <paramref name="whenErr"/>(errMsg) otherwise.
    /// </summary>
    public TOut Match<TOut>(TOut whenOk, Func<string, TOut> whenErr)
        => Err == null ? whenOk : whenErr(Err);
    /// <summary>
    /// Maps into <paramref name="whenOk"/>() whenever IsOk; and into <paramref name="whenErr"/>(errMsg) otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> whenOk, Func<string, TOut> whenErr)
        => Err == null ? whenOk() : whenErr(Err);
    /// <summary>
    /// Maps into <paramref name="whenOk"/>(Unwrap()) whenever IsOk; and into <paramref name="whenErr"/>(errMsg) otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> whenOk, Func<string, TOut> whenErr)
    {
        if (Err != null)
            return whenErr(Err);
        if (Val != null)
            return whenOk(Val);
        throw Exc.MustNotReach;
    }
    /// <summary>
    /// Executes <paramref name="whenOk"/>(Unwrap()) if IsOk; <paramref name="whenErr"/>(errMsg) otherwise.
    /// </summary>
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
    /// Runs <paramref name="action"/>() only if IsOk; and returns itself back.
    /// </summary>
    public Res<T> Do(Action action)
    {
        if (Err == null)
            action();
        return this;
    }
    /// <summary>
    /// Runs <paramref name="action"/>(Unwrap()) only if IsOk; and returns itself back.
    /// </summary>
    public Res<T> Do(Action<T> action)
    {
        if (Err == null && Val != null)
            action(Val);
        return this;
    }
    // do-if-err
    /// <summary>
    /// Runs <paramref name="actionOnErr"/>() only if IsErr; and returns itself back.
    /// </summary>
    public Res<T> DoIfErr(Action actionOnErr)
    {
        if (Err != null)
            actionOnErr();
        return this;
    }
    /// <summary>
    /// Runs <paramref name="actionOnErr"/>(ErrorMessage) only if IsErr; and returns itself back.
    /// </summary>
    public Res<T> DoIfErr(Action<string> actionOnErr)
    {
        if (Err != null)
            actionOnErr(ToString());
        return this;
    }


    // map
    /// <summary>
    /// Returns the error when IsErr; Ok(<paramref name="map"/>()) when IsOk.
    /// </summary>
    public Res<TOut> Map<TOut>(Func<TOut> map)
    {
        if (Err == null)
            return Ok(map());
        else if (Err != null)
            return Err<TOut>(Err);
        else
            throw Exc.MustNotReach;
    }
    /// <summary>
    /// Returns the error when IsErr; Ok(<paramref name="map"/>(Unwrap())) when IsOk.
    /// </summary>
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
    /// Returns the error when IsErr; <paramref name="map"/>() when IsOk flattenning the result.
    /// </summary>
    public Res FlatMap(Func<Res> map)
    {
        if (Err == null)
            return map();
        else if (Err != null)
            return Err(Err);
        else
            throw Exc.MustNotReach;
    }
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>(Unwrap()) when IsOk flattenning the result.
    /// </summary>
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
    /// Returns the error when IsErr; <paramref name="map"/>() when IsOk flattenning the result.
    /// </summary>
    public Res<TOut> FlatMap<TOut>(Func<Res<TOut>> map)
    {
        if (Err == null)
            return map();
        else if (Err != null)
            return Err<TOut>(Err);
        else
            throw Exc.MustNotReach;
    }
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>(Unwrap()) when IsOk flattenning the result.
    /// </summary>
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
    /// When IsOk executes <paramref name="action"/>() in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// </summary>
    public Res<T> Try(Action action, [CallerArgumentExpression("action")] string name = "")
    {
        if (Err == null)
        {
            try
            {
                action();
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
    /// <summary>
    /// When IsOk executes <paramref name="action"/>(val) in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// </summary>
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
    /// Otherwise, tries to map into Ok(<paramref name="map"/>()) in a try-catch block and returns the Err if it throws.
    /// </summary>
    public Res<TOut> TryMap<TOut>(Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
    {
        if (Err == null)
        {
            try
            {
                return new(map());
            }
            catch (Exception e)
            {
                return new(string.Empty, name, e);
            }
        }
        else
            return new(Err, string.Empty, null);
    }
    /// <summary>
    /// Returns the error when IsErr.
    /// Otherwise, tries to map into Ok(<paramref name="map"/>(val)) in a try-catch block and returns the Err if it throws.
    /// </summary>
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


    // common
    /// <summary>
    /// String representation.
    /// </summary>
    public override string ToString()
        => Err ?? string.Format("Ok({0})", Val);
}
