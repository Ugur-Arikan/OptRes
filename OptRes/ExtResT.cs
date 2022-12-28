namespace OptRes;

public static partial class Ext
{
    // ctors
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Ok variant with value <paramref name="value"/>.
    /// However, if the <paramref name="value"/> is null, it will map into Err.
    /// </summary>
    public static Res<T> Ok<T>(T value)
        => new(value);
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Err with the given <paramref name="errorMessage"/>.
    /// </summary>
    public static Res<T> Err<T>(string errorMessage)
        => new(errorMessage, string.Empty, null);
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Err with the given <paramref name="errorMessage"/> which is observed during <paramref name="when"/>.
    /// </summary>
    public static Res<T> Err<T>(string errorMessage, string when)
        => new(errorMessage, when, null);


    // okif
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Ok variant with value <paramref name="value"/> if the <paramref name="okCondition"/> holds.
    /// Otherwise, it will map into an Err.
    /// </summary>
    public static Res<T> OkIf<T>(bool okCondition, T value, [CallerArgumentExpression("okCondition")] string name = "")
        => okCondition ? new(value) : new("Condition doesn't hold.", name, null);
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Ok variant with value <paramref name="lazyGetValue"/>() if the <paramref name="okCondition"/> holds.
    /// Otherwise, it will map into an Err.
    /// Note that the <paramref name="lazyGetValue"/> is only evaluated if the <paramref name="okCondition"/> holds.
    /// </summary>
    public static Res<T> OkIf<T>(bool okCondition, Func<T> lazyGetValue, [CallerArgumentExpression("okCondition")] string name = "")
        => okCondition ? new(lazyGetValue()) : new("Condition doesn't hold.", name, null);


    // flatten
    /// <summary>
    /// Flattens the result of result.
    /// </summary>
    public static Res Flatten(this Res<Res> result)
    {
        if (result.IsErr)
            return new(result.ToString(), string.Empty, null);
        else
            return result.Unwrap();
    }
    /// <summary>
    /// Flattens the result of result of <typeparamref name="T"/>.
    /// </summary>
    public static Res<T> Flatten<T>(this Res<Res<T>> result)
    {
        if (result.IsErr)
            return new(result.ToString(), string.Empty, null);
        else
            return result.Unwrap();
    }


    // map-append - 2
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="nextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2)> MapAppend<T1, T2>(this Res<T1> result, T2 nextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2)>(result.ToString());
        return Ok((result.Unwrap(), nextResult));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2)> MapAppend<T1, T2>(this Res<T1> result, Func<T2> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2)>(result.ToString());
        return Ok((result.Unwrap(), getNextResult()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2)> MapAppend<T1, T2>(this Res<T1> result, Func<Res<T2>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2)>(result.ToString());

        var next = getNextResult();
        if (next.IsErr)
            return Err<(T1, T2)>(next.ToString());

        return Ok((result.Unwrap(), next.Unwrap()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2)> MapAppend<T1, T2>(this Res<T1> result, Func<T1, T2> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2)>(result.ToString());
        return Ok((result.Unwrap(), getNextResult(result.Unwrap())));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2)> MapAppend<T1, T2>(this Res<T1> result, Func<T1, Res<T2>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2)>(result.ToString());

        var next = getNextResult(result.Unwrap());
        if (next.IsErr)
            return Err<(T1, T2)>(next.ToString());

        return Ok((result.Unwrap(), next.Unwrap()));
    }
    // map-append - 3
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="nextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, T3 nextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3)>(result.ToString());
        (var v1, var v2) = result.Unwrap();
        return Ok((v1, v2, nextResult));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<T3> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3)>(result.ToString());
        (var v1, var v2) = result.Unwrap();
        return Ok((v1, v2, getNextResult()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<Res<T3>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3)>(result.ToString());

        var next = getNextResult();
        if (next.IsErr)
            return Err<(T1, T2, T3)>(next.ToString());

        (var v1, var v2) = result.Unwrap();
        return Ok((v1, v2, next.Unwrap()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<T1, T2, T3> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3)>(result.ToString());
        (var v1, var v2) = result.Unwrap();
        return Ok((v1, v2, getNextResult(v1, v2)));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3)> MapAppend<T1, T2, T3>(this Res<(T1, T2)> result, Func<T1, T2, Res<T3>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3)>(result.ToString());

        (var v1, var v2) = result.Unwrap();
        var next = getNextResult(v1, v2);
        if (next.IsErr)
            return Err<(T1, T2, T3)>(next.ToString());

        return Ok((v1, v2, next.Unwrap()));
    }
    // map-append - 4
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="nextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, T4 nextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4)>(result.ToString());
        (var v1, var v2, var v3) = result.Unwrap();
        return Ok((v1, v2, v3, nextResult));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<T4> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4)>(result.ToString());
        (var v1, var v2, var v3) = result.Unwrap();
        return Ok((v1, v2, v3, getNextResult()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<Res<T4>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4)>(result.ToString());

        var next = getNextResult();
        if (next.IsErr)
            return Err<(T1, T2, T3, T4)>(next.ToString());

        (var v1, var v2, var v3) = result.Unwrap();
        return Ok((v1, v2, v3, next.Unwrap()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<T1, T2, T3, T4> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4)>(result.ToString());
        (var v1, var v2, var v3) = result.Unwrap();
        return Ok((v1, v2, v3, getNextResult(v1, v2, v3)));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4)> MapAppend<T1, T2, T3, T4>(this Res<(T1, T2, T3)> result, Func<T1, T2, T3, Res<T4>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4)>(result.ToString());

        (var v1, var v2, var v3) = result.Unwrap();
        var next = getNextResult(v1, v2, v3);
        if (next.IsErr)
            return Err<(T1, T2, T3, T4)>(next.ToString());

        return Ok((v1, v2, v3, next.Unwrap()));
    }
    // map-append - 5
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="nextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, T5 nextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(result.ToString());
        (var v1, var v2, var v3, var v4) = result.Unwrap();
        return Ok((v1, v2, v3, v4, nextResult));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<T5> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(result.ToString());
        (var v1, var v2, var v3, var v4) = result.Unwrap();
        return Ok((v1, v2, v3, v4, getNextResult()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<Res<T5>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(result.ToString());

        var next = getNextResult();
        if (next.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(next.ToString());

        (var v1, var v2, var v3, var v4) = result.Unwrap();
        return Ok((v1, v2, v3, v4, next.Unwrap()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, T5> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(result.ToString());
        (var v1, var v2, var v3, var v4) = result.Unwrap();
        return Ok((v1, v2, v3, v4, getNextResult(v1, v2, v3, v4)));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5)> MapAppend<T1, T2, T3, T4, T5>(this Res<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Res<T5>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(result.ToString());

        (var v1, var v2, var v3, var v4) = result.Unwrap();
        var next = getNextResult(v1, v2, v3, v4);
        if (next.IsErr)
            return Err<(T1, T2, T3, T4, T5)>(next.ToString());

        return Ok((v1, v2, v3, v4, next.Unwrap()));
    }
    // map-append - 6
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="nextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, T6 nextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());
        (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
        return Ok((v1, v2, v3, v4, v5, nextResult));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<T6> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());
        (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
        return Ok((v1, v2, v3, v4, v5, getNextResult()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<Res<T6>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());

        var next = getNextResult();
        if (next.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(next.ToString());

        (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
        return Ok((v1, v2, v3, v4, v5, next.Unwrap()));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<T1, T2, T3, T4, T5, T6> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());
        (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
        return Ok((v1, v2, v3, v4, v5, getNextResult(v1, v2, v3, v4, v5)));
    }
    /// <summary>
    /// Just returns back the Err when IsErr.
    /// Extends the value with <paramref name="getNextResult"/> when IsOk.
    /// </summary>
    public static Res<(T1, T2, T3, T4, T5, T6)> MapAppend<T1, T2, T3, T4, T5, T6>(this Res<(T1, T2, T3, T4, T5)> result, Func<T1, T2, T3, T4, T5, Res<T6>> getNextResult)
    {
        if (result.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(result.ToString());

        (var v1, var v2, var v3, var v4, var v5) = result.Unwrap();
        var next = getNextResult(v1, v2, v3, v4, v5);
        if (next.IsErr)
            return Err<(T1, T2, T3, T4, T5, T6)>(next.ToString());

        return Ok((v1, v2, v3, v4, v5, next.Unwrap()));
    }


    // parse
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> using the <paramref name="parser"/> if succeeds; None if fails.
    /// Parser is called within a try-catch block, where exceptions are mapped to None.
    /// </summary>
    public static Res<T> TryParseOrErr<T>(this string text, Func<string, T> parser)
        => TryMap(() => parser(text));
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<int> ParseIntOrErr(this string text)
    { bool s = int.TryParse(text, out var val); return s ? Ok(val) : ParserErr<int>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<int> ParseIntOrErr(this ReadOnlySpan<char> text)
    { bool s = int.TryParse(text, out var val); return s ? Ok(val) : Err<int>(Config.ErrParserFailed); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<double> ParseDoubleOrErr(this string text)
    { bool s = double.TryParse(text, out var val); return s ? Ok(val) : ParserErr<double>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<double> ParseDoubleOrErr(this ReadOnlySpan<char> text)
    { bool s = double.TryParse(text, out var val); return s ? Ok(val) : ParserErr<double>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<float> ParseFloatOrErr(this string text)
    { bool s = float.TryParse(text, out var val); return s ? Ok(val) : ParserErr<float>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<float> ParseFloatOrErr(this ReadOnlySpan<char> text)
    { bool s = float.TryParse(text, out var val); return s ? Ok(val) : ParserErr<float>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<short> ParseShortOrErr(this string text)
    { bool s = short.TryParse(text, out var val); return s ? Ok(val) : ParserErr<short>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<short> ParseShortOrErr(this ReadOnlySpan<char> text)
    { bool s = short.TryParse(text, out var val); return s ? Ok(val) : ParserErr<short>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<long> ParseLongOrErr(this string text)
    { bool s = long.TryParse(text, out var val); return s ? Ok(val) : ParserErr<long>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<long> ParseLongOrErr(this ReadOnlySpan<char> text)
    { bool s = long.TryParse(text, out var val); return s ? Ok(val) : ParserErr<long>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<Half> ParseHalfOrErr(this string text)
    { bool s = Half.TryParse(text, out var val); return s ? Ok(val) : ParserErr<Half>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<Half> ParseHalfOrErr(this ReadOnlySpan<char> text)
    { bool s = Half.TryParse(text, out var val); return s ? Ok(val) : ParserErr<Half>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<bool> ParseBoolOrErr(this string text)
    { bool s = bool.TryParse(text, out var val); return s ? Ok(val) : ParserErr<bool>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<bool> ParseBoolOrErr(this ReadOnlySpan<char> text)
    { bool s = bool.TryParse(text, out var val); return s ? Ok(val) : ParserErr<bool>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<DateTime> ParseDateTimeOrErr(this string text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateTime>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<DateTime> ParseDateTimeOrErr(this ReadOnlySpan<char> text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateTime>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<DateOnly> ParseDateOnlyOrErr(this string text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateOnly>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<DateOnly> ParseDateOnlyOrErr(this ReadOnlySpan<char> text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<DateOnly>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Res<TimeOnly> ParseTimeOnlyOrErr(this string text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Ok(val) : ParserErr<TimeOnly>(text); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
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
