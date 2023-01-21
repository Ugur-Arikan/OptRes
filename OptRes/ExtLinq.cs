using System.Text;

namespace OptRes;

/// <summary>
/// Extension methods for the Opt and Res types.
/// </summary>
public static partial class Extensions
{
    // GetOrNone
    /// <summary>
    /// Safely gets and returns the <paramref name="index"/>-th element of the collection;
    /// or returns None if the index is invalid.
    /// 
    /// <code>
    /// Span&lt;int> collection = new[] { 0, 1, 2 };
    /// ReadOnlySpan&lt;int> collection = new[] { 0, 1, 2 };
    /// Memory&lt;int> collection = new[] { 0, 1, 2 };
    /// ReadOnlyMemory&lt;int> collection = new[] { 0, 1, 2 };
    /// int[] collection = new[] { 0, 1, 2 };
    /// List&lt;int> collection = new() { 0, 1, 2 };
    /// IList&lt;int> collection = new List&lt;int>() { 0, 1, 2 };
    /// IEnumerable&lt;int> collection = new int[] { 0, 1, 2 };
    /// 
    /// Assert(collection.GetOrNone(1) == Some(1));
    /// Assert(collection.GetOrNone(-1).IsNone);
    /// Assert(collection.GetOrNone(2).IsNone);
    /// </code>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this T[] collection, int index)
        => SomeIf(index > -1 && index < collection.Length, collection[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this Span<T> collection, int index)
        => SomeIf(index > -1 && index < collection.Length, collection[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this ReadOnlySpan<T> collection, int index)
        => SomeIf(index > -1 && index < collection.Length, collection[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this Memory<T> collection, int index)
        => SomeIf(index > -1 && index < collection.Length, collection.Span[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this ReadOnlyMemory<T> collection, int index)
        => SomeIf(index > -1 && index < collection.Length, collection.Span[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this List<T> collection, int index)
        => SomeIf(index > -1 && index < collection.Count, collection[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this IList<T> collection, int index)
        => SomeIf(index > -1 && index < collection.Count, collection[index]);
    /// <summary>
    /// <inheritdoc cref="GetOrNone{T}(T[], int)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to retrieve.</param>
    public static Opt<T> GetOrNone<T>(this IEnumerable<T> collection, int index)
    {
        if (index < 0)
            return default;
        if (collection.TryGetNonEnumeratedCount(out int count))
        {
            if (index < count)
                return Some(collection.ElementAt(index));
            else
                return default;
        }
        else
        {
            if (typeof(T).IsClass)
            {
                T? element = collection.ElementAtOrDefault(index);
                if (element == null)
                    return default;
                else
                    return Some(element);
            }
            else
            {
                try
                {
                    return Some(collection.ElementAt(index));
                }
                catch
                {
                    return default;
                }
            }
        }
    }


    // TrySet
    /// <summary>
    /// Tries to set the <paramref name="index"/>-th element of the collection with the given <paramref name="value"/>;
    /// returns Ok() if the <paramref name="index"/> is valid; the Err otherwise.
    /// 
    /// <code>
    /// Span&lt;int> collection = new[] { 0, 1, 2 };
    /// ReadOnlySpan&lt;int> collection = new[] { 0, 1, 2 };
    /// Memory&lt;int> collection = new[] { 0, 1, 2 };
    /// ReadOnlyMemory&lt;int> collection = new[] { 0, 1, 2 };
    /// int[] collection = new[] { 0, 1, 2 };
    /// List&lt;int> collection = new() { 0, 1, 2 };
    /// IList&lt;int> collection = new List&lt;int>() { 0, 1, 2 };
    /// IEnumerable&lt;int> collection = new int[] { 0, 1, 2 };
    /// 
    /// Assert(collection.GetOrNone(1) == Some(1));
    /// 
    /// Res res = collection.TrySet(1, 42);
    /// Assert(res.IsOk);
    /// Assert(collection.GetOrNone(1) == Some(42));
    /// 
    /// Assert(collection.TrySet(-1, 42).IsErr);
    /// Assert(collection.TrySet(2, 42).IsErr);
    /// </code>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to update.</param>
    /// <param name="value">Value to be set.</param>
    public static Res TrySet<T>(this T[] collection, int index, T value)
    {
        if (index > -1 && index < collection.Length)
        {
            collection[index] = value;
            return Ok();
        }
        else
            return Err(string.Format("Index out of bounds: index={0}, expected-range=[0,{1}).", index, collection.Length));
    }
    /// <summary>
    /// <inheritdoc cref="TrySet{T}(T[], int, T)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to update.</param>
    /// <param name="value">Value to be set.</param>
    public static Res TrySet<T>(this Span<T> collection, int index, T value)
    {
        if (index > -1 && index < collection.Length)
        {
            collection[index] = value;
            return Ok();
        }
        else
            return Err(string.Format("Index out of bounds: index={0}, expected-range=[0,{1}).", index, collection.Length));
    }
    /// <summary>
    /// <inheritdoc cref="TrySet{T}(T[], int, T)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to update.</param>
    /// <param name="value">Value to be set.</param>
    public static Res TrySet<T>(this Memory<T> collection, int index, T value)
    {
        if (index > -1 && index < collection.Length)
        {
            collection.Span[index] = value;
            return Ok();
        }
        else
            return Err(string.Format("Index out of bounds: index={0}, expected-range=[0,{1}).", index, collection.Length));
    }
    /// <summary>
    /// <inheritdoc cref="TrySet{T}(T[], int, T)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to update.</param>
    /// <param name="value">Value to be set.</param>
    public static Res TrySet<T>(this List<T> collection, int index, T value)
    {
        if (index > -1 && index < collection.Count)
        {
            collection[index] = value;
            return Ok();
        }
        else
            return Err(string.Format("Index out of bounds: index={0}, expected-range=[0,{1}).", index, collection.Count));
    }
    /// <summary>
    /// <inheritdoc cref="TrySet{T}(T[], int, T)"/>
    /// </summary>
    /// <typeparam name="T">Type of the collection.</typeparam>
    /// <param name="collection">Collection of elements of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the element of the collection to update.</param>
    /// <param name="value">Value to be set.</param>
    public static Res TrySet<T>(this IList<T> collection, int index, T value)
    {
        if (index > -1 && index < collection.Count)
        {
            collection[index] = value;
            return Ok();
        }
        else
            return Err(string.Format("Index out of bounds: index={0}, expected-range=[0,{1}).", index, collection.Count));
    }


    // first/last or none
    /// <summary>
    /// Returns Some of the first element of the <paramref name="collection"/> if it is non-empty; None otherwise.
    /// <code>
    /// Assert(Array.Empty&lt;Title>().FirstOrNone().IsNone);
    /// Assert((new int[2] { 1, 2 }).FirstOrNone() == Some(1));
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> collection)
    {
        Memory<int> span = new[] { 0, 1, 2 };
        IList<int> list = new List<int>() { 0, 1, 2 };
        IEnumerable<int> enumerable = new[] { 0, 1, 2 };

        foreach (var item in collection)
            return Some(item);
        return None<T>();
    }
    /// <summary>
    /// Returns Some of the first element of the <paramref name="collection"/> satisfying the <paramref name="filter"/> if any; None otherwise.
    /// <code>
    /// Assert(Array.Empty&lt;int>().FirstOrNone(x => x > 2).IsNone);
    /// Assert((new int[2] { 1, 2 }).FirstOrNone(x => x > 2).IsNone);
    /// Assert((new int[2] { 1, 2 }).FirstOrNone(x => x > 1) == Some(2));
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    /// <param name="filter">Predicate to filter the items of the collection.</param>
    public static Opt<T> FirstOrNone<T>(this IEnumerable<T> collection, Func<T, bool> filter)
    {
        foreach (var item in collection)
            if (filter(item))
                return Some(item);
        return None<T>();
    }
    /// <summary>
    /// Returns Some of the last element of the <paramref name="collection"/> if it is non-empty; None otherwise.
    /// <code>
    /// Assert(Array.Empty&lt;Title>().FirstOrNone().IsNone);
    /// Assert((new int[2] { 1, 2 }).LastOrNone() == Some(2));
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> collection)
    {
        bool hasAny = collection.GetEnumerator().MoveNext();
        return hasAny ? Some(collection.Last()) : None<T>();
    }
    /// <summary>
    /// Returns Some of the last element of the <paramref name="collection"/> satisfying the <paramref name="filter"/> if any; None otherwise.
    /// <code>
    /// Assert(Array.Empty&lt;int>().LastOrNone(x => x > 2).IsNone);
    /// Assert((new int[2] { 2, 1 }).LastOrNone(x => x > 2).IsNone);
    /// Assert((new int[2] { 2, 1 }).LastOrNone(x => x > 1) == Some(2));
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    /// <param name="filter">Predicate to filter the items of the collection.</param>
    public static Opt<T> LastOrNone<T>(this IEnumerable<T> collection, Func<T, bool> filter)
        => LastOrNone(collection.Where(filter));


    // first-or
    /// <summary>
    /// Returns the first Ok(value) from the <paramref name="collection"/>; or Err if all elements are Err.
    /// <code>
    /// static Res&lt;Ip> TryGetIp(Request request) { .. }
    /// 
    /// List&lt;Request> requests = GetRequests();      // List&lt;Request>
    /// var firstIp = requests.Select(r => TryGetIp(r)) // IEnumerable&lt;Res&lt;Ip>>
    ///                 .FirstOkOrErr();                // Res&lt;Ip>
    /// 
    /// // here, firstIp is:
    /// // * Err if none of the requests provide an Ok(ip);
    /// // * Ok(ip) of the first request for which TryGetIp returns Ok.
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static Res<T> FirstOkOrErr<T>(this IEnumerable<Res<T>> collection)
        => collection.FirstOrNone(r => r.IsOk).UnwrapOr(Err<T>("None of the items in the collection returned Ok(value)."));
    /// <summary>
    /// Returns the first Some(value) from the <paramref name="collection"/>; or None if all elements are None.
    /// <code>
    /// static Opt&lt;Ip> MaybeGetIp(Request request) { .. }
    /// 
    /// List&lt;Request> requests = GetRequests();          // List&lt;Request>
    /// var firstIp = requests.Select(r => MaybeGetIp(r))   // IEnumerable&lt;Opt&lt;Ip>>
    ///                 .FirstSomeOrNone();                 // Opt&lt;Ip>
    /// 
    /// // here, firstIp is:
    /// // * None if none of the requests provides Some(ip);
    /// // * Some(ip) of the first request for which MaybeGetIp returns Some.
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static Opt<T> FirstSomeOrNone<T>(this IEnumerable<Opt<T>> collection)
        => collection.FirstOrNone(r => r.IsSome).Flatten();


    // unwrap values
    /// <summary>
    /// Returns a collection of underlying values from the optionals in the <paramref name="collection"/>, skipping None elements.
    /// Shorthand for collection.Where(x => x.IsSome).Select(x => x.Unwrap()).
    /// <code>
    /// static Opt&lt;Ip> MaybeGetIp(Request request) { .. }
    /// 
    /// List&lt;Request> requests = GetRequests();
    /// Ip[] ips = requests                         // List&lt;Request>
    ///     .Select(r => MaybeGetIp(r))             // IEnumerable&lt;Opt&lt;Ip>>
    ///     .UnwrapSomes()                          // IEnumerable&lt;Ip>
    ///     .ToArray();                             // Ip[]
    ///     
    /// // Assume requests.Select(r => MaybeGetIp(r)) yields to [ Some(ip1), None, None, Some(ip2), None ].
    /// // Then, resulting ips is [ ip1, ip2 ].
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static IEnumerable<T> UnwrapSomes<T>(this IEnumerable<Opt<T>> collection)
        => collection.Where(x => x.IsSome).Select(x => x.Unwrap());
    /// <summary>
    /// Returns a collection of underlying values from the results in the <paramref name="collection"/>, skipping Err elements.
    /// Shorthand for collection.Where(x => x.IsOk).Select(x => x.Unwrap()).
    /// <code>
    /// static Res&lt;Ip> TryGetIp(Request request) { .. }
    /// 
    /// List&lt;Request> requests = GetRequests();
    /// Ip[] ips = requests                         // List&lt;Request>
    ///     .Select(r => TryGetIp(r))               // IEnumerable&lt;Res&lt;Ip>>
    ///     .UnwrapOkays()                          // IEnumerable&lt;Ip>
    ///     .ToArray();                             // Ip[]
    ///     
    /// // Assume requests.Select(r => TryGetIp(r)) yields to [ Ok(ip1), Err, Err, Ok(ip2), Err ].
    /// // Then, resulting ips is [ ip1, ip2 ].
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static IEnumerable<T> UnwrapOkays<T>(this IEnumerable<Res<T>> collection)
        => collection.Where(x => x.IsOk).Select(x => x.Unwrap());
    /// <summary>
    /// Converts <paramref name="collection"/> of results into Res of collection of the underlying values.
    /// Result will be Ok if all elements are Ok; the first Err otherwise.
    /// Maps IEnumerable&lt;Opt&lt;T>> => Opt&lt;IEnumerable&lt;T>>.
    /// <code>
    /// static Opt&lt;Ip> MaybeGetIp(Request request) { .. }
    /// 
    /// List&lt;Request> requests = GetRequests();
    /// var ips = requests                          // List&lt;Request>
    ///     .Select(r => MaybeGetIp(r))             // IEnumerable&lt;Opt&lt;Ip>>
    ///     .TryUnwrap();                           // Opt&lt;IEnumerable&lt;Ip>>
    ///     
    /// // If requests.Select(r => MaybeGetIp(r)) yields to [ Some(ip1), None, Some(ip2), None ].
    /// // Then, resulting ips is None.
    /// 
    /// // If requests.Select(r => MaybeGetIp(r)) yields to [ Some(ip1), Some(ip2), Some(ip3) ].
    /// // Then, resulting ips is Some([ ip1, ip2, ip3 ]).
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
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
    /// Maps IEnumerable&lt;Res&lt;T>> => Res&lt;IEnumerable&lt;T>>.
    /// <code>
    /// static Res&lt;Ip> TryGetIp(Request request) { .. }
    /// 
    /// List&lt;Request> requests = GetRequests();
    /// var ips = requests                          // List&lt;Request>
    ///     .Select(r => TryGetIp(r))               // IEnumerable&lt;Res&lt;Ip>>
    ///     .TryUnwrap();                           // Res&lt;IEnumerable&lt;Ip>>
    ///     
    /// // If requests.Select(r => MaybeGetIp(r)) yields to [ Ok(ip1), Err, Ok(ip2), Err ].
    /// // Then, resulting ips is the first Err observed in the collection.
    /// 
    /// // If requests.Select(r => MaybeGetIp(r)) yields to [ Ok(ip1), Ok(ip2), Ok(ip3) ].
    /// // Then, resulting ips is Ok([ ip1, ip2, ip3 ]).
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    public static Res<IEnumerable<T>> TryUnwrap<T>(this IEnumerable<Res<T>> collection)
    {
        var firstError = collection.FirstOrNone(x => x.IsErr);
        if (firstError.IsSome)
            return Err<IEnumerable<T>>(firstError.Unwrap().ToString());
        else
            return Ok(collection.Select(x => x.Unwrap()));
    }


    // reduce-res
    /// <summary>
    /// Reduces the collection of results into a single result; returns
    /// <list type="bullet">
    /// <item>Ok if all items are Ok;</item>
    /// <item>Err with the error message of the first (all) error(s) if <paramref name="stopAtFirstError"/> is true (false) if there exist at least one error.</item>
    /// </list>
    /// <code>
    /// static Res ValidateUser(User user) { .. }
    /// 
    /// List&lt;User> users = GetUsers();
    /// var validation = users.Select(u => ValidateUser(u)).Reduce();
    /// // will be Ok if all users are valid; first Err otherwise.
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    /// <param name="stopAtFirstError">When true, the iteration will stop at the first Err observed; will yield all results otherwise.</param>
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
    /// <list type="bullet">
    /// <item>Ok if all items are Ok;</item>
    /// <item>Err with the error message of the first error otherwise.</item>
    /// </list>
    /// <code>
    /// Res dbConn = CheckDbConnection();
    /// Res validUser = ValidateUser(user);
    /// Res result = ReduceResults(dbConn, validUser);
    /// </code>
    /// </summary>
    /// <param name="results">Results to be reduced.</param>
    public static Res ReduceResults(params Res[] results)
        => results.AsEnumerable().Reduce();


    // reduce-res<T>
    /// <summary>
    /// Reduces the underlying values of the <paramref name="collection"/> by transformation defined by <paramref name="funReduce"/>.
    /// Result is Ok of the reduced value only if all elements of the collection are Ok; Err otherwise.
    /// <code>
    /// static Res&lt;double> TryGetScore(Player player) { .. }
    /// 
    /// var players = GetPlayers();                         // Player[]
    /// var scores = players.Select(p => TryGetScore(p));   // IEnumerable&lt;Res&lt;double>>
    /// var totalScore = scores.Reduce((a, b) => a + b);    // Res&lt;double>
    /// 
    /// // here, totalScore is the sum of all unwrapped scores if all scores are Ok;
    /// // the first Err otherwise.
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    /// <param name="funReduce">Reduce (aggregate) function.</param>
    public static Res<T> Reduce<T>(this IEnumerable<Res<T>> collection, Func<T, T, T> funReduce)
        => collection.TryUnwrap().Map(x => x.Aggregate(funReduce));
    /// <summary>
    /// Reduces the underlying values of the <paramref name="collection"/> by transformation defined by <paramref name="funReduce"/> starting from <paramref name="initialValue"/>.
    /// Result is Ok of the reduced value only if all elements of the collection are Ok; Err otherwise.
    /// <code>
    /// static Res&lt;double> TryGetScore(Player player) { .. }
    /// 
    /// var players = GetPlayers();                         // Player[]
    /// var scores = players.Select(p => TryGetScore(p));   // IEnumerable&lt;Res&lt;double>>
    /// var totalScore = scores.Reduce((a, b) => a + b, 0); // Res&lt;double>
    /// 
    /// // here, totalScore is the sum of all unwrapped scores if all scores are Ok;
    /// // the first Err otherwise.
    /// </code>
    /// </summary>
    /// <param name="collection">Collection.</param>
    /// <param name="funReduce">Reduce (aggregate) function.</param>
    /// <param name="initialValue">Initial value for the reduction; i.e., seed.</param>
    public static Res<U> Reduce<T, U>(this IEnumerable<Res<T>> collection, Func<U, T, U> funReduce, U initialValue)
        => collection.TryUnwrap().Map(x => x.Aggregate(initialValue, funReduce));


    // dict - tryget
    /// <summary>
    /// Gets Some of the value of the key-value pair in the <paramref name="dictionary"/> with the given <paramref name="key"/>; None if it doesn't exist.
    /// <code>
    /// var dict = new Dictionary&lt;string, int>()
    /// {
    ///     ("Good", 42),
    ///     ("Okay", 12),
    /// };
    /// 
    /// Assert(dict.GetOpt("Good") == Some(42));
    /// Assert(dict.GetOpt("Bad").IsNone);
    /// </code>
    /// </summary>
    /// <param name="dictionary">Dictionary to check the key.</param>
    /// <param name="key">Key of the dictionary item to grab.</param>
    public static Opt<V> GetOpt<K, V>(this Dictionary<K, V> dictionary, K key)
        where K : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return Some(value);
        else
            return None<V>();
    }


    //// col-res
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.Do(Action)"/>
    ///// </summary>
    //public static IEnumerable<Res> Do(this IEnumerable<Res> collection, Action action)
    //{
    //    foreach (var item in collection)
    //        item.Do(action);
    //    return collection;
    //}
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.DoIfErr(Action{string})"/>
    ///// </summary>
    //public static IEnumerable<Res> DoIfErr(this IEnumerable<Res> collection, Action<string> actionOnErr)
    //{
    //    foreach (var item in collection)
    //        item.DoIfErr(actionOnErr);
    //    return collection;
    //}
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.Map{TOut}(Func{TOut})"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> Map<T, TOut>(this IEnumerable<Res> collection, Func<TOut> map)
    //    => collection.Select(item => item.Map(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.FlatMap{TOut}(Func{Res{TOut}})"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> FlatMap<T, TOut>(this IEnumerable<Res> collection, Func<Res<TOut>> map)
    //    => collection.Select(item => item.FlatMap(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.FlatMap(Func{Res})"/>
    ///// </summary>
    //public static IEnumerable<Res> FlatMap(this IEnumerable<Res> collection, Func<Res> map)
    //    => collection.Select(item => item.FlatMap(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.Try(Action, string)"/>
    ///// </summary>
    //public static IEnumerable<Res> Try(this IEnumerable<Res> collection, Action action, [CallerArgumentExpression("action")] string name = "")
    //    => collection.Select(item => item.Try(action, name));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res.TryMap{TOut}(Func{TOut}, string)"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Res> collection, Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
    //    => collection.Select(item => item.TryMap(map, name));
    
    
    //// col-res<T>
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.Do(Action{T})"/>
    ///// </summary>
    //public static IEnumerable<Res<T>> Do<T>(this IEnumerable<Res<T>> collection, Action<T> action)
    //{
    //    foreach (var item in collection)
    //        item.Do(action);
    //    return collection;
    //}
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.DoIfErr(Action{string})"/>
    ///// </summary>
    //public static IEnumerable<Res<T>> DoIfErr<T>(this IEnumerable<Res<T>> collection, Action<string> actionOnErr)
    //{
    //    foreach (var item in collection)
    //        item.DoIfErr(actionOnErr);
    //    return collection;
    //}
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.Map{TOut}(Func{T, TOut})"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> Map<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> map)
    //    => collection.Select(item => item.Map(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.FlatMap{TOut}(Func{T, Res{TOut}})"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> FlatMap<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, Res<TOut>> map)
    //    => collection.Select(item => item.FlatMap(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.FlatMap(Func{T, Res})"/>
    ///// </summary>
    //public static IEnumerable<Res> FlatMap<T>(this IEnumerable<Res<T>> collection, Func<T, Res> map)
    //    => collection.Select(item => item.FlatMap(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.Try(Action{T}, string)"/>
    ///// </summary>
    //public static IEnumerable<Res<T>> Try<T>(this IEnumerable<Res<T>> collection, Action<T> action, [CallerArgumentExpression("action")] string name = "")
    //    => collection.Select(item => item.Try(action, name));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.TryMap{TOut}(Func{T, TOut}, string)"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> map, [CallerArgumentExpression("map")] string name = "")
    //    => collection.Select(item => item.TryMap(map, name));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.Match{TOut}(Func{T, TOut}, Func{string, TOut})"/>
    ///// </summary>
    //public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Res<T>> collection, Func<T, TOut> whenOk, Func<string, TOut> whenErr)
    //    => collection.Select(item => item.Match(whenOk, whenErr));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Res{T}.MatchDo(Action{T}, Action{string})"/>
    ///// </summary>
    //public static void MatchDo<T>(this IEnumerable<Res<T>> collection, Action<T> whenOk, Action<string> whenErr)
    //{
    //    foreach (var item in collection)
    //        item.MatchDo(whenOk, whenErr);
    //}


    //// col-opt
    ///// <summary>
    ///// <para>[Uplifted method for collection.]</para>
    ///// <code>
    ///// collection.Do(action);
    ///// // is Shorthand for
    ///// collection.Select(x => x.Do(action));
    ///// </code>
    ///// <inheritdoc cref="Opt{T}.Do(Action{T})"/>
    ///// </summary>
    ///// <param name="collection">Collection.</param>
    ///// <param name="action">Action of T to execute each Some element of the collection.</param>
    //public static IEnumerable<Opt<T>> Do<T>(this IEnumerable<Opt<T>> collection, Action<T> action)
    //{
    //    foreach (var item in collection)
    //        item.Do(action);
    //    return collection;
    //}
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.DoIfNone(Action)"/>
    ///// </summary>
    //public static IEnumerable<Opt<T>> DoIfNone<T>(this IEnumerable<Opt<T>> collection, Action actionOnNone)
    //{
    //    foreach (var item in collection)
    //        item.DoIfNone(actionOnNone);
    //    return collection;
    //}
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.Map{TOut}(Func{T, TOut})"/>
    ///// </summary>
    //public static IEnumerable<Opt<TOut>> Map<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> map)
    //    => collection.Select(item => item.Map(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.FlatMap{TOut}(Func{T, Opt{TOut}})"/>
    ///// </summary>
    //public static IEnumerable<Opt<TOut>> FlatMap<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, Opt<TOut>> map)
    //    => collection.Select(item => item.FlatMap(map));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.Try(Action{T}, string)"/>
    ///// </summary>
    //public static IEnumerable<Res<T>> Try<T>(this IEnumerable<Opt<T>> collection, Action<T> action, [CallerArgumentExpression("action")] string name = "")
    //    => collection.Select(item => item.Try(action, name));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.TryMap{TOut}(Func{T, TOut}, string)"/>
    ///// </summary>
    //public static IEnumerable<Res<TOut>> TryMap<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> map, [CallerArgumentExpression("map")] string name = "")
    //    => collection.Select(item => item.TryMap(map, name));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.Match{TOut}(Func{T, TOut}, TOut)"/>
    ///// </summary>
    //public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> whenOk, TOut whenErr)
    //    => collection.Select(item => item.Match(whenOk, whenErr));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.Match{TOut}(Func{T, TOut}, Func{TOut})"/>
    ///// </summary>
    //public static IEnumerable<TOut> Match<T, TOut>(this IEnumerable<Opt<T>> collection, Func<T, TOut> whenOk, Func<TOut> whenErr)
    //    => collection.Select(item => item.Match(whenOk, whenErr));
    ///// <summary>
    ///// [Uplifted method for collection] 
    ///// <inheritdoc cref="Opt{T}.MatchDo(Action{T}, Action)"/>
    ///// </summary>
    //public static void MatchDo<T>(this IEnumerable<Opt<T>> collection, Action<T> whenOk, Action whenErr)
    //{
    //    foreach (var item in collection)
    //        item.MatchDo(whenOk, whenErr);
    //}
}
