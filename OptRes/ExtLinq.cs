using System.Text;

namespace OptRes;

/// <summary>
/// Extension methods for the Opt and Res types.
/// </summary>
public static partial class Ext
{
    // first/last or none
    /// <summary>
    /// Returns Some of the first element of the <paramref name="collection"/> if it is non-empty; None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> collection)
    {
        foreach (var item in collection)
            return Some(item);
        return None<T>();
    }
    /// <summary>
    /// Returns Some of the first element of the <paramref name="collection"/> satisfying the <paramref name="filter"/> if any; None otherwise.
    /// </summary>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> collection, Func<T, bool> filter)
    {
        foreach (var item in collection)
            if (filter(item))
                return Some(item);
        return None<T>();
    }
    /// <summary>
    /// Returns Some of the last element of the <paramref name="collection"/> if it is non-empty; None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> collection)
    {
        bool hasAny = collection.GetEnumerator().MoveNext();
        return hasAny ? Some(collection.Last()) : None<T>();
    }
    /// <summary>
    /// Returns Some of the last element of the <paramref name="collection"/> satisfying the <paramref name="filter"/> if any; None otherwise.
    /// </summary>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> collection, Func<T, bool> filter)
        => LastOrNone(collection.Where(filter));


    // first-or
    /// <summary>
    /// Returns the first Err from the <paramref name="collection"/>; or Ok if all elements are okay.
    /// </summary>
    public static Res FirstErrOrOk(this IEnumerable<Res> collection)
        => collection.FirstOrNone(r => r.IsErr).UnwrapOr(Ok());
    /// <summary>
    /// Returns the first Err from the <paramref name="collection"/>; or Ok if all elements are okay.
    /// </summary>
    public static Res FirstErrOrOk<T>(this IEnumerable<Res<T>> collection)
        => collection.FirstOrNone(r => r.IsErr).Match(err => Err(err.ToString()), Ok());
    /// <summary>
    /// Returns the first Ok(value) from the <paramref name="collection"/>; or Err if all elements are error.
    /// </summary>
    public static Res<T> FirstOkOrErr<T>(this IEnumerable<Res<T>> collection)
        => collection.FirstOrNone(r => r.IsOk).UnwrapOr(Err<T>("None of the items in the collection returned Ok(value)."));
    /// <summary>
    /// Returns the first Some(value) from the <paramref name="collection"/>; or None if all elements are None.
    /// </summary>
    public static Opt<T> FirstSomeOrNone<T>(this IEnumerable<Opt<T>> collection)
        => collection.FirstOrNone(r => r.IsSome).Flatten();


    // unwrap values
    /// <summary>
    /// Returns a collection of underlying values from the optionals in the <paramref name="collection"/>, skipping None elements.
    /// </summary>
    public static IEnumerable<T> UnwrapSomes<T>(this IEnumerable<Opt<T>> collection)
        => collection.Where(x => x.IsSome).Select(x => x.Unwrap());
    /// <summary>
    /// Returns a collection of underlying values from the results in the <paramref name="collection"/>, skipping Err elements.
    /// </summary>
    public static IEnumerable<T> UnwrapOkays<T>(this IEnumerable<Res<T>> collection)
        => collection.Where(x => x.IsOk).Select(x => x.Unwrap());
    /// <summary>
    /// Converts <paramref name="collection"/> of results into Res of collection of the underlying values.
    /// Result will be Ok if all elements are Ok; the first Err otherwise.
    /// </summary>
    public static Res<IEnumerable<T>> TryUnwrap<T>(this IEnumerable<Opt<T>> collection)
    {
        if (collection.Any(x => x.IsNone))
            return Err<IEnumerable<T>>("Collection contains None elements, cannot unwrap all.");
        else
            return Ok(collection.Select(x => x.Unwrap()));
    }
    /// <summary>
    /// Converts <paramref name="collection"/> of results into Res of collection of the underlying values.
    /// Result will be Ok if all elements are Ok; the first Err otherwise.
    /// </summary>
    public static Res<IEnumerable<T>> TryUnwrap<T>(this IEnumerable<Res<T>> collection)
    {
        var firstError = collection.FirstOrNone(x => x.IsErr);
        if (firstError.IsSome)
            return Err<IEnumerable<T>>(firstError.Unwrap().ToString());
        else
            return Ok(collection.Select(x => x.Unwrap()));
    }


    // ctor
    /// <summary>
    /// Returns the optional collection where each item of <paramref name="collection"/> is mapped into Some(item).
    /// </summary>
    public static IEnumerable<Opt<T>> AsSome<T>(this IEnumerable<T> collection)
        => collection.Select(x => Some(x));
    /// <summary>
    /// Returns the result collection where each item of <paramref name="collection"/> is mapped into Ok(item).
    /// </summary>
    public static IEnumerable<Res<T>> AsOk<T>(this IEnumerable<T> collection)
        => collection.Select(x => Ok(x));


    // col-res
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.Do(Action)"/>
    /// </summary>
    public static IEnumerable<Res> Do(this IEnumerable<Res> collection, Action action)
    {
        foreach (var item in collection)
            item.Do(action);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.DoIfErr(Action)"/>
    /// </summary>
    public static IEnumerable<Res> DoIfErr(this IEnumerable<Res> collection, Action actionOnErr)
    {
        foreach (var item in collection)
            item.DoIfErr(actionOnErr);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.DoIfErr(Action{string})"/>
    /// </summary>
    public static IEnumerable<Res> DoIfErr(this IEnumerable<Res> collection, Action<string> actionOnErr)
    {
        foreach (var item in collection)
            item.DoIfErr(actionOnErr);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.Map{TOut}(TOut)"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> Map<T, TOut>(this IEnumerable<Res> collection, TOut map)
        => collection.Select(item => item.Map(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.Map{TOut}(Func{TOut})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> Map<T, TOut>(this IEnumerable<Res> collection, Func<TOut> map)
        => collection.Select(item => item.Map(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.FlatMap{TOut}(Res{TOut})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> FlatMap<T, TOut>(this IEnumerable<Res> collection, Res<TOut> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.FlatMap{TOut}(Res{TOut})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> FlatMap<T, TOut>(this IEnumerable<Res> collection, Func<Res<TOut>> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.FlatMap(Res)"/>
    /// </summary>
    public static IEnumerable<Res> FlatMap(this IEnumerable<Res> collection, Res map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.FlatMap(Func{Res})"/>
    /// </summary>
    public static IEnumerable<Res> FlatMap(this IEnumerable<Res> collection, Func<Res> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.Try(Action, string)"/>
    /// </summary>
    public static IEnumerable<Res> Try(this IEnumerable<Res> collection, Action action, [CallerArgumentExpression("action")] string name = "")
        => collection.Select(item => item.Try(action, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res.TryMap{TOut}(Func{TOut}, string)"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Res> collection, Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
        => collection.Select(item => item.TryMap(map, name));
    
    
    // reduce-res
    /// <summary>
    /// Reduces the collection of results into a single result; returns
    /// (i) Ok if all items are Ok;
    /// (ii) Err with the error message of the first (all) error(s) if <paramref name="stopAtFirstError"/> is true (false) if there exist at least one error.
    /// </summary>
    public static Res Reduce(this IEnumerable<Res> collection, bool stopAtFirstError = true)
    {
        var errors = collection.Where(r => r.IsErr);
        var first = errors.FirstOrNone();

        if (first.IsNone)
            return Ok();

        if (stopAtFirstError)
            return first.Unwrap();

        StringBuilder sb = new();
        foreach (var err in errors)
            sb.AppendLine(err.ToString());
        return Err(sb.ToString());
    }
    /// <summary>
    /// Reduces the collection of results into a single result; returns
    /// (i) Ok if all items are Ok;
    /// (ii) Err with the error message of the first error otherwise.
    /// </summary>
    public static Res ReduceResults(params Res[] results)
        => results.AsEnumerable().Reduce();

    // reduce-res<T>
    /// <summary>
    /// Reduces the underlying values of the <paramref name="collection"/> by transformation defined by <paramref name="funReduce"/> starting from <paramref name="initialValue"/>.
    /// Result is Ok of the reduced value only if all elements of the collection are Ok; Err otherwise.
    /// </summary>
    /// <returns></returns>
    public static Res<T> Reduce<T>(this IEnumerable<Res<T>> collection, Func<T, T, T> funReduce)
        => collection.TryUnwrap().Map(x => x.Aggregate(funReduce));
    /// <summary>
    /// Reduces the underlying values of the <paramref name="collection"/> by transformation defined by <paramref name="funReduce"/> starting from <paramref name="initialValue"/>.
    /// Result is Ok of the reduced value only if all elements of the collection are Ok; Err otherwise.
    /// </summary>
    /// <returns></returns>
    public static Res<U> Reduce<T, U>(this IEnumerable<Res<T>> collection, Func<U, T, U> funReduce, U initialValue)
        => collection.TryUnwrap().Map(x => x.Aggregate(initialValue, funReduce));


    // col-res<T>
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Do(Action)"/>
    /// </summary>
    public static IEnumerable<Res<T>> Do<T>(this IEnumerable<Res<T>> collection, Action action)
    {
        foreach (var item in collection)
            item.Do(action);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Do(Action{T})"/>
    /// </summary>
    public static IEnumerable<Res<T>> Do<T>(this IEnumerable<Res<T>> collection, Action<T> action)
    {
        foreach (var item in collection)
            item.Do(action);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.DoIfErr(Action)"/>
    /// </summary>
    public static IEnumerable<Res<T>> DoIfErr<T>(this IEnumerable<Res<T>> collection, Action actionOnErr)
    {
        foreach (var item in collection)
            item.DoIfErr(actionOnErr);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.DoIfErr(Action{string})"/>
    /// </summary>
    public static IEnumerable<Res<T>> DoIfErr<T>(this IEnumerable<Res<T>> collection, Action<string> actionOnErr)
    {
        foreach (var item in collection)
            item.DoIfErr(actionOnErr);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Map{TOut}(Func{TOut})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> Map<T, TOut>(this IEnumerable<Res<T>> collection, Func<TOut> map)
        => collection.Select(item => item.Map(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Map{TOut}(Func{T, TOut})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> Map<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> map)
        => collection.Select(item => item.Map(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.FlatMap{TOut}(Func{Res{TOut}})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> FlatMap<T, TOut>(this IEnumerable<Res<T>> collection, Func<Res<TOut>> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.FlatMap{TOut}(Func{T, Res{TOut}})"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> FlatMap<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, Res<TOut>> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.FlatMap(Func{Res})"/>
    /// </summary>
    public static IEnumerable<Res> FlatMap<T>(this IEnumerable<Res<T>> collection, Func<Res> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.FlatMap(Func{T, Res})"/>
    /// </summary>
    public static IEnumerable<Res> FlatMap<T>(this IEnumerable<Res<T>> collection, Func<T, Res> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Try(Action, string)"/>
    /// </summary>
    public static IEnumerable<Res<T>> Try<T>(this IEnumerable<Res<T>> collection, Action action, [CallerArgumentExpression("action")] string name = "")
        => collection.Select(item => item.Try(action, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Try(Action{T}, string)"/>
    /// </summary>
    public static IEnumerable<Res<T>> Try<T>(this IEnumerable<Res<T>> collection, Action<T> action, [CallerArgumentExpression("action")] string name = "")
        => collection.Select(item => item.Try(action, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.TryMap{TOut}(Func{TOut}, string)"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Res<T>> collection, Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
        => collection.Select(item => item.TryMap(map, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.TryMap{TOut}(Func{T, TOut}, string)"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> map, [CallerArgumentExpression("map")] string name = "")
        => collection.Select(item => item.TryMap(map, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(TOut, TOut)"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, TOut whenOk, TOut whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(TOut, Func{TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, TOut whenOk, Func<TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(Func{TOut}, TOut)"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<TOut> whenOk, TOut whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(Func{TOut}, Func{TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<TOut> whenOk, Func<TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(Func{T, TOut}, TOut)"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> whenOk, TOut whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(Func{T, TOut}, Func{TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> whenOk, Func<TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.MatchDo(Action{T}, Action)"/>
    /// </summary>
    public static void MatchDo<T>(this IEnumerable<Res<T>> collection, Action<T> whenOk, Action whenErr)
    {
        foreach (var item in collection)
            item.MatchDo(whenOk, whenErr);
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(TOut, Func{string, TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, TOut whenOk, Func<string, TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(Func{TOut}, Func{string, TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<TOut> whenOk, Func<string, TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.Match{TOut}(Func{T, TOut}, Func{string, TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> whenOk, Func<string, TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Res{T}.MatchDo(Action{T}, Action{string})"/>
    /// </summary>
    public static void MatchDo<T>(this IEnumerable<Res<T>> collection, Action<T> whenOk, Action<string> whenErr)
    {
        foreach (var item in collection)
            item.MatchDo(whenOk, whenErr);
    }


    // col-opt
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Do(Action)"/>
    /// </summary>
    public static IEnumerable<Opt<T>> Do<T>(this IEnumerable<Opt<T>> collection, Action action)
    {
        foreach (var item in collection)
            item.Do(action);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Do(Action{T})"/>
    /// </summary>
    public static IEnumerable<Opt<T>> Do<T>(this IEnumerable<Opt<T>> collection, Action<T> action)
    {
        foreach (var item in collection)
            item.Do(action);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.DoIfNone(Action)"/>
    /// </summary>
    public static IEnumerable<Opt<T>> DoIfNone<T>(this IEnumerable<Opt<T>> collection, Action actionOnNone)
    {
        foreach (var item in collection)
            item.DoIfNone(actionOnNone);
        return collection;
    }
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Map{TOut}(Func{TOut})"/>
    /// </summary>
    public static IEnumerable<Opt<TOut>> Map<T, TOut>(this IEnumerable<Opt<T>> collection, Func<TOut> map)
        => collection.Select(item => item.Map(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Map{TOut}(Func{T, TOut})"/>
    /// </summary>
    public static IEnumerable<Opt<TOut>> Map<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> map)
        => collection.Select(item => item.Map(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.FlatMap{TOut}(Func{Opt{TOut}})"/>
    /// </summary>
    public static IEnumerable<Opt<TOut>> FlatMap<T, TOut>(this IEnumerable<Opt<T>> collection, Func<Opt<TOut>> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.FlatMap{TOut}(Func{T, Opt{TOut}})"/>
    /// </summary>
    public static IEnumerable<Opt<TOut>> FlatMap<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, Opt<TOut>> map)
        => collection.Select(item => item.FlatMap(map));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Try(Action, string)"/>
    /// </summary>
    public static IEnumerable<Res<T>> Try<T>(this IEnumerable<Opt<T>> collection, Action action, [CallerArgumentExpression("action")] string name = "")
        => collection.Select(item => item.Try(action, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Try(Action{T}, string)"/>
    /// </summary>
    public static IEnumerable<Res<T>> Try<T>(this IEnumerable<Opt<T>> collection, Action<T> action, [CallerArgumentExpression("action")] string name = "")
        => collection.Select(item => item.Try(action, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.TryMap{TOut}(Func{TOut}, string)"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Opt<T>> collection, Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
        => collection.Select(item => item.TryMap(map, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.TryMap{TOut}(Func{T, TOut}, string)"/>
    /// </summary>
    public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> map, [CallerArgumentExpression("map")] string name = "")
        => collection.Select(item => item.TryMap(map, name));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Match{TOut}(TOut, TOut)"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, TOut whenOk, TOut whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Match{TOut}(TOut, Func{TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, TOut whenOk, Func<TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Match{TOut}(Func{TOut}, TOut)"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, Func<TOut> whenOk, TOut whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Match{TOut}(Func{TOut}, Func{TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, Func<TOut> whenOk, Func<TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Match{TOut}(Func{T, TOut}, TOut)"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> whenOk, TOut whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.Match{TOut}(Func{T, TOut}, Func{TOut})"/>
    /// </summary>
    public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> whenOk, Func<TOut> whenErr)
        => collection.Select(item => item.Match(whenOk, whenErr));
    /// <summary>
    /// [Uplifted method for collection] 
    /// <inheritdoc cref="Opt{T}.MatchDo(Action{T}, Action)"/>
    /// </summary>
    public static void MatchDo<T>(this IEnumerable<Opt<T>> collection, Action<T> whenOk, Action whenErr)
    {
        foreach (var item in collection)
            item.MatchDo(whenOk, whenErr);
    }


    // dict - tryget
    /// <summary>
    /// Gets Some of the value of the key-value pair in the <paramref name="dictionary"/> with the given <paramref name="key"/>; None if it doesn't exist.
    /// </summary>
    public static Opt<V> GetOpt<K, V>(this Dictionary<K, V> dictionary, K key)
        where K : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return Some(value);
        else
            return None<V>();
    }
}
