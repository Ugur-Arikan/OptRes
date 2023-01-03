namespace OptRes;

public static partial class Extensions
{
    // ctors
    /// <summary>
    /// Creates an option of <typeparamref name="T"/> as None variant.
    /// <code>
    /// var noneInt = None&lt;int>();
    /// Assert(noneInt.IsNone);
    /// 
    /// // also:
    /// Opt&lt;string> name = default;
    /// Assert(name.IsNone);
    /// </code>
    /// </summary>
    public static Opt<T> None<T>()
        => new();
    /// <summary>
    /// Creates an option of <typeparamref name="T"/> as Some variant with the given <paramref name="value"/>.
    /// However, if the <paramref name="value"/> is null, it will map into None.
    /// <code>
    /// Opt&lt;double> number = Some(42.5);
    /// Assert(number.IsSome and number.Unwrap() == 42.5);
    /// 
    /// // on the other hand:
    /// string name = null;
    /// Opt&lt;string> optName = Some(name);
    /// Assert(optName.IsNone);
    /// </code>
    /// </summary>
    /// <param name="value">Expectedly non-null value of T.</param>
    public static Opt<T> Some<T>(T value)
        => new(value);


    // some-if
    /// <summary>
    /// Creates a result of <typeparamref name="T"/> as Some variant with value <paramref name="value"/> if the <paramref name="someCondition"/> holds.
    /// Otherwise, it will return the None variant.
    /// <code>
    /// string team = "secret";
    /// int score = 42;
    /// 
    /// Opt&lt;string> winner = SomeIf(score > 30, team);
    /// Assert(winner == Some(team));
    /// 
    /// Opt&lt;string> loser = SomeIf(score &lt; 40, team);
    /// Assert(loser.IsNone);
    /// </code>
    /// </summary>
    /// <param name="someCondition">Condition that must hold for the return value to be Some(value).</param>
    /// <param name="value">Underlying value of the Some variant to be returned if someCondition holds.</param>
    public static Opt<T> SomeIf<T>(bool someCondition, T value)
        => someCondition ? new(value) : None<T>();


    // flatten
    /// <summary>
    /// Flattens the option of option of <typeparamref name="T"/>.
    /// Maps Opt&lt;Opt&lt;T>> to Opt&lt;T> as follows:
    /// <list type="bullet">
    /// <item>None => None&lt;T>(),</item>
    /// <item>Some(None&lt;T>()) => None&lt;T>(),</item>
    /// <item>Some(Some(T)) => Some(T).</item>
    /// </list>
    /// <code>
    /// Assert(None&lt;Opt&lt;char>>().Flatten() == None&lt;char>());
    /// Assert(Some(None&lt;char>()).Flatten() == None&lt;char>());
    /// Assert(Some(Some('c')).Flatten() == Some('c'));
    /// </code>
    /// </summary>
    /// <param name="option">Nested option to flatten.</param>
    public static Opt<T> Flatten<T>(this Opt<Opt<T>> option)
    {
        if (option.IsNone)
            return default;
        else
            return option.Unwrap();
    }


    // parse
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> using the <paramref name="parser"/> if succeeds; None if fails.
    /// Parser is called within a try-catch block, where exceptions are mapped to None.
    /// <code>
    /// static Wizard ParseWizard(text)
    /// {
    ///     string[] columns = text.Split(',');
    ///     string name = columns[0];               // this line might throw due to bad input
    ///     int nbSpells = int.Parse(columns[1]);   // this line might throw as well.
    ///     return new Wizard(name, nbSpells);
    /// }
    /// var solmyr = "solmyr,42".TryParseOrNone(ParseWizard);       // valid input
    /// Assert(solmyr.IsSome and solmyr.Unwrap().NbSpells == 42);
    /// 
    /// var errWizard = "wronginput".TryParseOrNone(ParseWizard);   // would've thrown, but TryParseOrNone handles the exception
    /// Assert(errWizard.IsNone);
    /// </code>
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    /// <param name="parser">Parser that converts text into a nonnull instance of T.</param>
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
    /// <param name="text">Text to be parsed.</param>
    public static Opt<int> ParseIntOrNone(this string text)
    { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<int> ParseIntOrNone(this ReadOnlySpan<char> text)
    { bool s = int.TryParse(text, out var val); return s ? Some(val) : None<int>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<double> ParseDoubleOrNone(this string text)
    { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<double> ParseDoubleOrNone(this ReadOnlySpan<char> text)
    { bool s = double.TryParse(text, out var val); return s ? Some(val) : None<double>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<float> ParseFloatOrNone(this string text)
    { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<float> ParseFloatOrNone(this ReadOnlySpan<char> text)
    { bool s = float.TryParse(text, out var val); return s ? Some(val) : None<float>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<short> ParseShortOrNone(this string text)
    { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<short> ParseShortOrNone(this ReadOnlySpan<char> text)
    { bool s = short.TryParse(text, out var val); return s ? Some(val) : None<short>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<long> ParseLongOrNone(this string text)
    { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<long> ParseLongOrNone(this ReadOnlySpan<char> text)
    { bool s = long.TryParse(text, out var val); return s ? Some(val) : None<long>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<Half> ParseHalfOrNone(this string text)
    { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<Half> ParseHalfOrNone(this ReadOnlySpan<char> text)
    { bool s = Half.TryParse(text, out var val); return s ? Some(val) : None<Half>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<bool> ParseBoolOrNone(this string text)
    { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<bool> ParseBoolOrNone(this ReadOnlySpan<char> text)
    { bool s = bool.TryParse(text, out var val); return s ? Some(val) : None<bool>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<DateTime> ParseDateTimeOrNone(this string text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<DateTime> ParseDateTimeOrNone(this ReadOnlySpan<char> text)
    { bool s = DateTime.TryParse(text, out var val); return s ? Some(val) : None<DateTime>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this string text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<DateOnly> ParseDateOnlyOrNone(this ReadOnlySpan<char> text)
    { bool s = DateOnly.TryParse(text, out var val); return s ? Some(val) : None<DateOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this string text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }
    /// <summary>
    /// Returns Some of parsed value from <paramref name="text"/> if succeeds; None if fails.
    /// </summary>
    /// <param name="text">Text to be parsed.</param>
    public static Opt<TimeOnly> ParseTimeOnlyOrNone(this ReadOnlySpan<char> text)
    { bool s = TimeOnly.TryParse(text, out var val); return s ? Some(val) : None<TimeOnly>(); }
}
