namespace OptRes;

/// <summary>
/// Result type which can be either of the two variants: Ok or Err(error-message).
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
    /// Parameterless ctor returns Ok; better use 'Ok' or `Err` to construct results by adding `using static OptRes.Ext`.
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
    /// Maps into either Ok (Err) if the <paramref name="condition"/> holds (fails) when IsOk; returns itself back otherwise.
    /// </summary>
    public Res OkIf(bool condition, [CallerArgumentExpression("condition")] string name = "")
        => IsErr ? this : (condition ? default : new("Condition doesn't hold.", name, null));


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
    /// Executes <paramref name="whenOk"/>() whenever IsOk; and <paramref name="whenErr"/>() otherwise.
    /// </summary>
    public void Match<TOut>(Action whenOk, Action whenErr)
    {
        if (IsOk)
            whenOk();
        else
            whenErr();
    }
    // match - err
    /// <summary>
    /// Maps into <paramref name="whenOk"/> whenever IsOk; and into <paramref name="whenErr"/>(errorMessage) otherwise.
    /// </summary>
    public TOut Match<TOut>(TOut whenOk, Func<string, TOut> whenErr)
        => IsOk ? whenOk : whenErr(ToString());
    /// <summary>
    /// Maps into <paramref name="whenOk"/>() whenever IsOk; and into <paramref name="whenErr"/>(errorMessage) otherwise.
    /// </summary>
    public TOut Match<TOut>(Func<TOut> whenOk, Func<string, TOut> whenErr)
        => IsOk ? whenOk() : whenErr(ToString());
    /// <summary>
    /// Executes <paramref name="whenOk"/>() whenever IsOk; and <paramref name="whenErr"/>(errorMessage) otherwise.
    /// </summary>
    public void Match<TOut>(Action whenOk, Action<string> whenErr)
    {
        if (IsOk)
            whenOk();
        else
            whenErr(ToString());
    }


    // do
    /// <summary>
    /// Runs <paramref name="action"/>() only if IsOk; and returns itself back.
    /// </summary>
    public Res Do(Action action)
    {
        if (IsOk)
            action();
        return this;
    }
    
    
    // do-if-err
    /// <summary>
    /// Runs <paramref name="action"/>() only if IsErr; and returns itself back.
    /// </summary>
    public Res DoIfErr(Action action)
    {
        if (Err != null)
            action();
        return this;
    }
    /// <summary>
    /// Runs <paramref name="action"/>(ErrorMessage) only if IsErr; and returns itself back.
    /// </summary>
    public Res DoIfErr(Action<string> action)
    {
        if (Err != null)
            action(ToString());
        return this;
    }


    // map
    /// <summary>
    /// Returns the error when IsErr; Ok(<paramref name="map"/>) when IsOk.
    /// </summary>
    public Res<TOut> Map<TOut>(TOut map)
        => Err == null ? new(map) : new(Err, string.Empty, null);
    /// <summary>
    /// Returns the error when IsErr; Ok(<paramref name="map"/>()) when IsOk.
    /// </summary>
    public Res<TOut> Map<TOut>(Func<TOut> map)
        => Err == null ? new(map()) : new(Err, string.Empty, null);


    // flatmap
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/> when IsOk, flattenning the result.
    /// </summary>
    public Res FlatMap(Res map)
        => Err == null ? map : new(Err, string.Empty, null);
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>() when IsOk, flattenning the result.
    /// </summary>
    public Res FlatMap(Func<Res> map)
        => Err == null ? map() : new(Err, string.Empty, null);
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/> when IsOk, flattenning the result.
    /// </summary>
    public Res<TOut> FlatMap<TOut>(Res<TOut> map)
        => Err == null ? map : new(Err, string.Empty, null);
    /// <summary>
    /// Returns the error when IsErr; <paramref name="map"/>() when IsOk, flattenning the result.
    /// </summary>
    public Res<TOut> FlatMap<TOut>(Func<Res<TOut>> map)
        => Err == null ? map() : new(Err, string.Empty, null);


    // try
    /// <summary>
    /// When IsOk executes <paramref name="action"/>() in a try-catch block: returns back itself if the process succeeds; Err if it throws.
    /// Does not do anything and returns back itself when IsErr.
    /// </summary>
    public Res Try(Action action, [CallerArgumentExpression("action")] string name = "")
        => Err == null ? Ext.Try(action, name) : this;


    // try-map
    /// <summary>
    /// Returns the error when IsErr.
    /// Otherwise, tries to map into Ok(<paramref name="map"/>()) in a try-catch block and returns the Err if it throws.
    /// </summary>
    public Res<TOut> TryMap<TOut>(Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
    {
        if (Err == null)
            return Ext.TryMap(map, name);
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
