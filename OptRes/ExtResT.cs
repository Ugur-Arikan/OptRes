namespace OptRes;

public static partial class Extensions
{
    // ctors
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Ok variant with value <paramref name="value"/>.
    /// However, if the <paramref name="value"/> is null, it will map into Err.
    /// <code>
    /// Res&lt;double> number = Ok(42.5);
    /// Assert(number.IsOk and number.Unwrap() == 42.5);
    /// 
    /// // on the other hand:
    /// string name = null;
    /// Res&lt;string> optName = Ok(name);
    /// Assert(optName.IsErr);
    /// </code>
    /// </summary>
    /// <param name="value">Expectedly non-null value of T.</param>
    public static Res<T> Ok<T>(T value)
        => new(value);
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Err with the given <paramref name="errorMessage"/>.
    /// <code>
    /// static Res&lt;double> Divide(double number, double divider)
    /// {
    ///     if (divider == 0)
    ///         return Err&lt;double>("Cannot divide to zero");
    ///     else
    ///         return Ok(number / divider);
    /// }
    /// </code>
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    public static Res<T> Err<T>(string errorMessage)
        => new(errorMessage, string.Empty, null);
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Err with the given <paramref name="errorMessage"/> which is observed during <paramref name="when"/>.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="when">Operation when the error is observed.</param>
    public static Res<T> Err<T>(string errorMessage, string when)
        => new(errorMessage, when, null);


    // okif
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Ok variant with value <paramref name="value"/> if the <paramref name="okCondition"/> holds.
    /// Otherwise, it will map into an Err.
    /// <code>
    /// Shape shape = GetShape(); // valid only if shape has a positive base area.
    /// Res&lt;Shape> resultShape = OkIf(shape.GetBaseArea() > 0, shape);
    /// </code>
    /// </summary>
    /// <param name="okCondition">Condition that must hold for the return value to be Ok(value).</param>
    /// <param name="value">Underlying value of the Ok variant to be returned if okCondition holds.</param>
    /// <param name="name">Name of the condition; to be appended to the error message if it does not hold. Omitting the argument will automatically be filled with the condition's expression in the caller side.</param>
    public static Res<T> OkIf<T>(bool okCondition, T value, [CallerArgumentExpression("okCondition")] string name = "")
        => okCondition ? new(value) : new("Condition doesn't hold.", name, null);
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Ok variant with value <paramref name="lazyGetValue"/>() if the <paramref name="okCondition"/> holds.
    /// Otherwise, it will map into an Err.
    /// Note that the <paramref name="lazyGetValue"/> is only evaluated if the <paramref name="okCondition"/> holds.
    /// <code>
    /// Res&lt;User> user = TryGetUser();
    /// // create a database connection (expensive) only if the user IsOk.
    /// Res&lt;Conn> conn = OkIf(user.IsOk, () => CreateDatabaseConnection());
    /// </code>
    /// </summary>
    /// <param name="okCondition">Condition that must hold for the return value to be Ok(value).</param>
    /// <param name="lazyGetValue">Function to create the underlying value of the Ok variant to be returned if okCondition holds.</param>
    /// <param name="name">Name of the condition; to be appended to the error message if it does not hold. Omitting the argument will automatically be filled with the condition's expression in the caller side.</param>
    public static Res<T> OkIf<T>(bool okCondition, Func<T> lazyGetValue, [CallerArgumentExpression("okCondition")] string name = "")
        => okCondition ? new(lazyGetValue()) : new("Condition doesn't hold.", name, null);


    // flatten
    /// <summary>
    /// Flattens the result of result; i.e., Res&lt;Res> -> Res, by mapping:
    /// <list type="bullet">
    /// <item>Err => Err,</item>
    /// <item>Ok(Err) => Err,</item>
    /// <item>Ok(Ok) => Ok.</item>
    /// </list>
    /// <code>
    /// Res&lt;Res> nestedResult = Err&lt;Res>("msg");
    /// Res result = nestedResult.Flatten();
    /// Assert(result.IsErr and result.ErrorMessage() == Some("msg"));
    /// 
    /// Res&lt;Res> nestedResult = Ok(Err("msg"));
    /// Res result = nestedResult.Flatten();
    /// Assert(result.IsErr and result.ErrorMessage() == Some("msg"));
    /// 
    /// Res&lt;Res> nestedResult = Ok(Ok());
    /// Res result = nestedResult.Flatten();
    /// Assert(result.IsOk);
    /// </code>
    /// </summary>
    /// <param name="result">Res of Res to be flattened.</param>
    public static Res Flatten(this Res<Res> result)
    {
        if (result.IsErr)
            return new(result.ToString(), string.Empty, null);
        else
            return result.Unwrap();
    }
    /// <summary>
    /// Flattens the result of result of <typeparamref name="T"/>; i.e., Res&lt;Res&lt;T>> -> Res&lt;T>, by mapping:
    /// <list type="bullet">
    /// <item>Err => Err,</item>
    /// <item>Ok(Err) => Err,</item>
    /// <item>Ok(Ok(value)) => Ok(value).</item>
    /// </list>
    /// <code>
    /// Res&lt;Res&lt;int>> nestedResult = Err&lt;Res&lt;int>>("msg");
    /// Res&lt;int> result = nestedResult.Flatten();
    /// Assert(result.IsErr and result.ErrorMessage() == Some("msg"));
    /// 
    /// Res&lt;Res&lt;int>> nestedResult = Ok(Err&lt;int>("msg"));
    /// Res&lt;int> result = nestedResult.Flatten();
    /// Assert(result.IsErr and result.ErrorMessage() == Some("msg"));
    /// 
    /// Res&lt;Res&lt;int>> nestedResult = Ok(Ok(42));
    /// Res&lt;int> result = nestedResult.Flatten();
    /// Assert(result.IsOk and result.Unwrap() == 42);
    /// </code>
    /// </summary>
    public static Res<T> Flatten<T>(this Res<Res<T>> result)
    {
        if (result.IsErr)
            return new(result.ToString(), string.Empty, null);
        else
            return result.Unwrap();
    }


   
    //// map-append - 3
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="nextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, T3 nextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3)>(result.ToString());
    //    (var v1, var v2) = result.Unwrap();
    //    return Ok((v1, v2, nextResult));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<T3> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3)>(result.ToString());
    //    (var v1, var v2) = result.Unwrap();
    //    return Ok((v1, v2, getNextResult()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<Res<T3>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3)>(result.ToString());

    //    var next = getNextResult();
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3)>(next.ToString());

    //    (var v1, var v2) = result.Unwrap();
    //    return Ok((v1, v2, next.Unwrap()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<T1, T2, T3> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3)>(result.ToString());
    //    (var v1, var v2) = result.Unwrap();
    //    return Ok((v1, v2, getNextResult(v1, v2)));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<T1, T2, Res<T3>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3)>(result.ToString());

    //    (var v1, var v2) = result.Unwrap();
    //    var next = getNextResult(v1, v2);
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3)>(next.ToString());

    //    return Ok((v1, v2, next.Unwrap()));
    //}
    //// map-append - 4
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="nextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, T4 nextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4)>(result.ToString());
    //    (var v1, var v2, var v3) = result.Unwrap();
    //    return Ok((v1, v2, v3, nextResult));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<T4> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4)>(result.ToString());
    //    (var v1, var v2, var v3) = result.Unwrap();
    //    return Ok((v1, v2, v3, getNextResult()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<Res<T4>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4)>(result.ToString());

    //    var next = getNextResult();
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3, T4)>(next.ToString());

    //    (var v1, var v2, var v3) = result.Unwrap();
    //    return Ok((v1, v2, v3, next.Unwrap()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<T1, T2, T3, T4> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4)>(result.ToString());
    //    (var v1, var v2, var v3) = result.Unwrap();
    //    return Ok((v1, v2, v3, getNextResult(v1, v2, v3)));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<T1, T2, T3, Res<T4>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4)>(result.ToString());

    //    (var v1, var v2, var v3) = result.Unwrap();
    //    var next = getNextResult(v1, v2, v3);
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3, T4)>(next.ToString());

    //    return Ok((v1, v2, v3, next.Unwrap()));
    //}
    //// map-append - 5
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="nextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, T5 nextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(result.ToString());
    //    (var v1, var v2, var v3, var v4) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, nextResult));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<T5> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(result.ToString());
    //    (var v1, var v2, var v3, var v4) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, getNextResult()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<Res<T5>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(result.ToString());

    //    var next = getNextResult();
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(next.ToString());

    //    (var v1, var v2, var v3, var v4) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, next.Unwrap()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, T5> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(result.ToString());
    //    (var v1, var v2, var v3, var v4) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, getNextResult(v1, v2, v3, v4)));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Res<T5>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(result.ToString());

    //    (var v1, var v2, var v3, var v4) = result.Unwrap();
    //    var next = getNextResult(v1, v2, v3, v4);
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3, T4, T5)>(next.ToString());

    //    return Ok((v1, v2, v3, v4, next.Unwrap()));
    //}
    //// map-append - 6
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="nextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, T6 nextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());
    //    (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, v5, nextResult));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<T6> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());
    //    (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, v5, getNextResult()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<Res<T6>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());

    //    var next = getNextResult();
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(next.ToString());

    //    (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, v5, next.Unwrap()));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<T1, T2, T3, T4, T5, T6> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());
    //    (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
    //    return Ok((v1, v2, v3, v4, v5, getNextResult(v1, v2, v3, v4, v5)));
    //}
    ///// <summary>
    ///// Just returns back the Err when IsErr.
    ///// Extends the value with <paramref name="getNextResult"/> when IsOk.
    ///// </summary>
    //public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<T1, T2, T3, T4, T5, Res<T6>> getNextResult)
    //{
    //    if (result.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());

    //    (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
    //    var next = getNextResult(v1, v2, v3, v4, v5);
    //    if (next.IsErr)
    //        return Err<(T1, T2, T3, T4, T5, T6)>(next.ToString());

    //    return Ok((v1, v2, v3, v4, v5, next.Unwrap()));
    //}


    // parse
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> using the <paramref name="parser"/> if succeeds; Err if fails.
    /// Parser is called within a try-catch block, where exceptions are mapped to Err.
    /// <code>
    /// static Wizard ParseWizard(text)
    /// {
    ///     string[] columns = text.Split(',');
    ///     string name = columns[0];               // this line might throw due to bad input
    ///     int nbSpells = int.Parse(columns[1]);   // this line might throw as well.
    ///     return new Wizard(name, nbSpells);
    /// }
    /// var solmyr = "solmyr,42".TryParseOrErr(ParseWizard);        // valid input
    /// Assert(solmyr.IsOk and solmyr.Unwrap().NbSpells == 42);
    /// 
    /// var errWizard = "wronginput".TryParseOrErr(ParseWizard);    // would've thrown, but TryParseOrErr handles the exception
    /// Assert(errWizard.IsErr);
    /// </code>
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    /// <param name="parser">Parser that converts text into a nonnull instance of T.</param>
    public static Res<T> TryParseOrErr<T>(this string text, Func<string, T> parser)
        => TryMap(() => parser(text));
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<int> ParseIntOrErr(this string text)
    { bool s = int.TryParse(text, out var val); return s ? Ok(val) : ParserErr<int>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<int> ParseIntOrErr(this ReadOnlySpan<char> text)
    { bool s = int.TryParse(text, out var val); return s ? Ok(val) : Err<int>(Config.ErrParserFailed); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<double> ParseDoubleOrErr(this string text)
    { bool s = double.TryParse(text, out var val); return s ? Ok(val) : ParserErr<double>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<double> ParseDoubleOrErr(this ReadOnlySpan<char> text)
    { bool s = double.TryParse(text, out var val); return s ? Ok(val) : ParserErr<double>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<float> ParseFloatOrErr(this string text)
    { bool s = float.TryParse(text, out var val); return s ? Ok(val) : ParserErr<float>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<float> ParseFloatOrErr(this ReadOnlySpan<char> text)
    { bool s = float.TryParse(text, out var val); return s ? Ok(val) : ParserErr<float>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<short> ParseShortOrErr(this string text)
    { bool s = short.TryParse(text, out var val); return s ? Ok(val) : ParserErr<short>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<short> ParseShortOrErr(this ReadOnlySpan<char> text)
    { bool s = short.TryParse(text, out var val); return s ? Ok(val) : ParserErr<short>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<long> ParseLongOrErr(this string text)
    { bool s = long.TryParse(text, out var val); return s ? Ok(val) : ParserErr<long>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<long> ParseLongOrErr(this ReadOnlySpan<char> text)
    { bool s = long.TryParse(text, out var val); return s ? Ok(val) : ParserErr<long>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<Half> ParseHalfOrErr(this string text)
    { bool s = Half.TryParse(text, out var val); return s ? Ok(val) : ParserErr<Half>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<Half> ParseHalfOrErr(this ReadOnlySpan<char> text)
    { bool s = Half.TryParse(text, out var val); return s ? Ok(val) : ParserErr<Half>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<bool> ParseBoolOrErr(this string text)
    { bool s = bool.TryParse(text, out var val); return s ? Ok(val) : ParserErr<bool>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<bool> ParseBoolOrErr(this ReadOnlySpan<char> text)
    { bool s = bool.TryParse(text, out var val); return s ? Ok(val) : ParserErr<bool>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<DateTime> ParseDateTimeOrErr(this string text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateTime>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<DateTime> ParseDateTimeOrErr(this ReadOnlySpan<char> text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateTime>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<DateOnly> ParseDateOnlyOrErr(this string text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateOnly>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<DateOnly> ParseDateOnlyOrErr(this ReadOnlySpan<char> text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateOnly>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<TimeOnly> ParseTimeOnlyOrErr(this string text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<TimeOnly>(text); }
    /// <summary>
    /// Returns Ok of parsed value from <paramref name="text"/> if succeeds; Err if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Res<TimeOnly> ParseTimeOnlyOrErr(this ReadOnlySpan<char> text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<TimeOnly>(text); }


    // helper - parser
    static Res<T> ParserErr<T>(string text)
        => Err<T>(string.Format("Failed to parse '{0}' as {1}.", text, typeof(T).Name));
    static Res<T> ParserErr<T>(ReadOnlySpan<char> text)
        => ParserErr<T>(text.ToString());
    // helper - map
    internal static Res<TOut> TryMap<TOut>(Func<TOut> map, [CallerArgumentExpression("map")] string name = "")
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
    // helper - flatmap
    internal static Res<TOut> TryFlatMap<TOut>(Func<Res<TOut>> map, [CallerArgumentExpression("map")] string name = "")
    {
        try
        {
            var res = map();
            return res.IsOk ? res : new(res.ToString(), name, null);
        }
        catch (Exception e)
        {
            return new(string.Empty, name, e);
        }
    }
    internal static Res TryFlatMap<TOut>(Func<Res> map, [CallerArgumentExpression("map")] string name = "")
    {
        try
        {
            var res = map();
            return res.IsOk ? res : new(res.ToString(), name, null);
        }
        catch (Exception e)
        {
            return new(string.Empty, name, e);
        }
    }
}
