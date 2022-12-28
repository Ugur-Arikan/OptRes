namespace OptRes;

public static partial class Ext
{
    // ctors
    /// <summary>
    /// Creates an option of <typeparamref name="T"/> as None variant.
    /// </summary>
    public static Opt<T> None<T>()
        => new();
    /// <summary>
    /// Creates an option of <typeparamref name="T"/> as Some variant with the given <paramref name="value"/>.
    /// However, if the <paramref name="value"/> is null, it will map into None.
    /// </summary>
    public static Opt<T> Some<T>(T value)
        => new(value);


    // some-if
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Some variant with value <paramref name="value"/> if the <paramref name="someCondition"/> holds.
    /// Otherwise, it will return the None variant.
    /// </summary>
    public static Opt<T> SomeIf<T>(bool someCondition, T value)
        => someCondition ? new(value) : None<T>();


    // flatten
    /// <summary>
    /// Flattens the option of option of <typeparamref name="T"/>.
    /// </summary>
    public static Opt<T> Flatten<T>(this Opt<Opt<T>> result)
    {
        if (result.IsNone)
            return default;
        else
            return result.Unwrap();
    }


    // parse
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> using the <paramref name="parser"/> if succeeds; None if fails.
    /// Parser is called within a try-catch block, where exceptions are mapped to None.
    /// </summary>
    public static Opt<T> TryParseOrNone<T>(this string text, Func<string, T> parser)
    {
        try
        {
            return parser(text);
        }
        catch
        {
            return default;
        }
    }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<int> ParseIntOrNone(this string text)
    { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<int> ParseIntOrNone(this ReadOnlySpan<char> text)
    { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<double> ParseDoubleOrNone(this string text)
    { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<double> ParseDoubleOrNone(this ReadOnlySpan<char> text)
    { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<float> ParseFloatOrNone(this string text)
    { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<float> ParseFloatOrNone(this ReadOnlySpan<char> text)
    { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<short> ParseShortOrNone(this string text)
    { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<short> ParseShortOrNone(this ReadOnlySpan<char> text)
    { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<long> ParseLongOrNone(this string text)
    { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<long> ParseLongOrNone(this ReadOnlySpan<char> text)
    { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<Half> ParseHalfOrNone(this string text)
    { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<Half> ParseHalfOrNone(this ReadOnlySpan<char> text)
    { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<bool> ParseBoolOrNone(this string text)
    { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<bool> ParseBoolOrNone(this ReadOnlySpan<char> text)
    { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateTime> ParseDateTimeOrNone(this string text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateTime> ParseDateTimeOrNone(this ReadOnlySpan<char> text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this string text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this ReadOnlySpan<char> text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this string text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this ReadOnlySpan<char> text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }
}
