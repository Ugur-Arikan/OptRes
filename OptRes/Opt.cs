namespace OptRes;

/// <summary>
/// Optiona type which can be either of the two variants: Some(value-of-<typeparamref name="T"/>) or None.
/// </summary>
public readonly struct Opt<T> : IEquatable<Opt<T>>
{
    // data
    readonly T? Val;
    /// <summary>
    /// Returns whether the option has Some value or not.
    /// </summary>
    public readonly bool IsSome;
    /// <summary>
    /// Returns whether the option is None or not.
    /// </summary>
    public bool IsNone => !IsSome;


    // ctor
    /// <summary>
    /// Option type of <typeparamref name="T"/>: either None or Some value.
    /// Parameterless ctor returns None; better use 'Some' or `None` to construct options by adding `using static OptRes.Ext`.
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
    /// </summary>
    public static implicit operator Opt<T>(T value)
        => new(value);


    // throw
    /// <summary>
    /// Returns the option back when <see cref="IsSome"/>; throws when <see cref="IsNone"/>.
    /// </summary>
    public Opt<T> ThrowIfNone()
    {
        var _ = Unwrap();
        return this;
    }
    /// <summary>
    /// Returns the option back when <see cref="IsSome"/>; throws with the given <paramref name="errorMessage"/> when <see cref="IsNone"/>.
    /// </summary>
    public Opt<T> ThrowIfNone(string errorMessage)
    {
        var _ = Unwrap(errorMessage);
        return this;
    }


    // unwrap
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; or throws when <see cref="IsNone"/>.
    /// </summary>
    public T Unwrap()
        => (IsSome && Val != null) ? Val : throw new NullReferenceException("Cannot Unwrap None.");
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; throws with the given <paramref name="errorMessageIfNone"/> when <see cref="IsNone"/>.
    /// </summary>
    public T Unwrap(string errorMessageIfNone)
        => (IsSome && Val != null) ? Val : throw new NullReferenceException(string.Format("Cannot Unwrap None. {0}", errorMessageIfNone));
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; or returns the <paramref name="fallbackValue"/> when <see cref="IsNone"/>.
    /// </summary>
    /// <param name="fallbackValue"></param>
    public T UnwrapOr(T fallbackValue)
        => (IsSome && Val != null) ? Val : fallbackValue;
    /// <summary>
    /// Returns the underlying value when <see cref="IsSome"/>; or returns <paramref name="lazyFallbackValue"/>() when <see cref="IsNone"/>.
    /// </summary>
    /// <param name="lazyFallbackValue"></param>
    public T UnwrapOr(Func<T> lazyFallbackValue)
        => (IsSome && Val != null) ? Val : lazyFallbackValue();


    // match
    /// <summary>
    /// Maps into <paramref name="whenSome"/> whenever IsSome; and into <paramref name="whenNone"/> otherwise.
    /// </summary>
    public TOut Match<TOut>(TOut whenSome, TOut whenNone)
        => IsSome ? whenSome : whenNone;
    /// <summary>
    /// Maps into <paramref name="whenSome"/> whenever IsSome; and into <paramref name="whenNone"/>() otherwise.
    /// </summary>
    public TOut Match<TOut>(TOut whenSome, Func<TOut> whenNone)
        => IsSome ? whenSome : whenNone();
    /// <summary>
    /// Maps into <paramref name="whenSome"/>() whenever IsSome; and into <paramref name="whenNone"/> otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> whenSome, TOut whenNone)
        => IsSome ? whenSome() : whenNone;
    /// <summary>
    /// Maps into <paramref name="whenSome"/>() whenever IsSome; and into <paramref name="whenNone"/>() otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> whenSome, Func<TOut> whenNone)
        => IsSome ? whenSome() : whenNone();
    /// <summary>
    /// Maps into <paramref name="whenSome"/>(Unwrap()) whenever IsSome; and into <paramref name="whenNone"/> otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> whenSome, TOut whenNone)
        => (IsSome && Val != null) ? whenSome(Val) : whenNone;
    /// <summary>
    /// Maps into <paramref name="whenSome"/>(Unwrap()) whenever IsSome; and into <paramref name="whenNone"/>() otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> whenSome, Func<TOut> whenNone)
        => (IsSome && Val != null) ? whenSome(Val) : whenNone();
    /// <summary>
    /// Executes <paramref name="whenSome"/>(Unwrap()) if IsSome; <paramref name="whenNone"/>() otherwise.
    /// </summary>
    public void MatchDo(Action<T> whenSome, Action whenNone)
    {
        if (IsSome && Val != null)
            whenSome(Val);
        else
            whenNone();
    }


    // to res
    /// <summary>
    /// Converts Some(val) to Ok(val); and None to Err.
    /// </summary>
    public Res<T> ToRes()
    {
        if (Val == null)
            return Err<T>("Required value is None.");
        else
            return Ok(Val);
    }
    /// <summary>
    /// Converts Some(val) to Ok(val); and None to Err with the error message including the <paramref name="requiredValueName"/>.
    /// </summary>
    public Res<T> ToRes(string requiredValueName)
    {
        if (Val == null)
            return Err<T>(string.Format("Required value '{0}' is None.", requiredValueName));
        else
            return Ok(Val);
    }


    // do
    /// <summary>
    /// Runs <paramref name="action"/>() only if IsSome; and returns itself back.
    /// </summary>
    public Opt<T> Do(Action action)
    {
        if (IsSome)
            action();
        return this;
    }
    /// <summary>
    /// Runs <paramref name="action"/>() only if IsSome; and returns itself back.
    /// </summary>
    public Opt<T> Do(Action<T> action)
    {
        if (IsSome && Val != null)
            action(Val);
        return this;
    }


    // do-if-none
    /// <summary>
    /// Runs <paramref name="actionOnNone"/>() only if IsNone; and returns itself back.
    /// </summary>
    public Opt<T> DoIfNone(Action actionOnNone)
    {
        if (IsNone)
            actionOnNone();
        return this;
    }


    // map
    /// <summary>
    /// Returns None when IsNone; Some(<paramref name="map"/>()) when IsSome.
    /// </summary>
    public Opt<TOut> Map<TOut>(Func<TOut> map)
        => IsSome ? Some(map()) : default;
    /// <summary>
    /// Returns None when IsNone; Some(<paramref name="map"/>(Unwrap())) when IsSome.
    /// </summary>
    public Opt<TOut> Map<TOut>(Func<T, TOut> map)
        => (IsSome && Val != null) ? Some(map(Val)) : default;


    // flat-map
    /// <summary>
    /// Returns None when IsNone; <paramref name="map"/>() when IsSome flattening the result.
    /// </summary>
    public Opt<TOut> FlatMap<TOut>(Func<Opt<TOut>> map)
        => IsSome ? map() : default;
    /// <summary>
    /// Returns None when IsNone; <paramref name="map"/>(val) when IsSome flattening the result.
    /// </summary>
    public Opt<TOut> FlatMap<TOut>(Func<T, Opt<TOut>> map)
        => (IsSome && Val != null) ? map(Val) : default;


    // try
    /// <summary>
    /// When IsSome executes <paramref name="action"/>() in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsNone.
    /// </summary>
    public Res<T> Try(Action action, [CallerArgumentExpression("action")] string name = "")
    {
        if (IsSome && Val != null)
        {
            try
            {
                action();
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
    /// <summary>
    /// When IsSome executes <paramref name="action"/>(val) in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsNone.
    /// </summary>
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
    /// When IsOk tries to map to Ok(<paramref name="map"/>()) in a try-catch block: returns Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// </summary>
    public Res<TOut> TryMap<TOut>(Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
    {
        if (IsSome)
            return Ext.TryMap(map, name);
        else
            return Err<TOut>(Config.GetErrorString(("TryMap called on None.", name, null)));
    }
    /// <summary>
    /// When IsOk tries to map to Ok(<paramref name="map"/>(value)) in a try-catch block: returns Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// </summary>
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
    /// Returns whether this option is equal to the <paramref name="obj"/>.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null && (obj is Opt<T>) && (Equals(obj));
    /// <summary>
    /// Returns true if both values are <see cref="IsSome"/> and their unwrapped values are equal; false otherwise.
    /// </summary>
    public bool Equals(Opt<T> other)
        => IsSome && other.IsSome && Val != null && other.Val != null && Val.Equals(other.Val);
    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
        => Val == null ? int.MinValue : Val.GetHashCode();
    /// <summary>
    /// Returns true if both values are <see cref="IsSome"/> and their unwrapped values are equal; false otherwise.
    /// </summary>
    public static bool operator ==(Opt<T> left, Opt<T> right)
        => left.Equals(right);
    /// <summary>
    /// Returns true if either value is <see cref="IsNone"/> or their unwrapped values are not equal; false otherwise.
    /// </summary>
    public static bool operator !=(Opt<T> left, Opt<T> right)
        => !(left == right);
}
