using static Examples.Helpers;

namespace Examples;

internal record Player(
    string Name,
    int Wins,
    Opt<string> Nickname = default,     // stating explicitly that not all players have a nickname
    Opt<string> EmailAddress = default  // default of Opt<T> is None<T>()
    )
{
    public void Greet()
    {
        // UnwrapOr returns the underlying value if IsSome; the fallback value otherwise.
        // When the fallback value is expensive (requires a database query, for instance), lazy version can be used:
        // Nickname.UnwrapOr(() => Name)
        string greeting = string.Format("Hey {0}", Nickname.UnwrapOr(Name));
        Console.WriteLine(greeting);

        // alternative uses of UnwrapOr, Match, IsSome & Unwrap
        Assert(Nickname.UnwrapOr(Name) == Nickname.Match(whenSome: nick => nick, whenNone: Name));
        Assert(Nickname.UnwrapOr(Name) == (Nickname.IsSome ? Nickname.Unwrap() : Name));
    }
    
    public bool HasNickname()
        => Nickname.IsSome; // = !IsNone

    public Opt<int> NicknameLength()
    {
        // Map into other types by chaining results, while the IsNone checks are internally handled:
        // * if Nickname.IsSome, the result will be the Some(Nickname.Unwrap().Length);
        // * None<int>() otherwise.
        return Nickname.Map(nick => nick.Length);
    }

    public void SendEmail(string message)
    {
        // send only if the player has an email address
        EmailAddress.Do(emailAddr => Console.WriteLine($"fake-sending {message} to {emailAddr}"));
    }

    public int Score()
    {
        // assume having a nickname provides +5 wins.
        return Wins + Nickname.Match(whenSome: _ => 5, whenNone: 0);
    }

    public void RemindToAddEmail()
    {
        // do only if IsNone
        EmailAddress.DoIfNone(() => Console.WriteLine("you may add your email address for ..."));
    }

    public Opt<Player> FindMatch(IEnumerable<Player> others)
    {
        // linq alternative to FirstOrDefault that would return null in absent case.

        // there also exist the following extension variants on IEnumerable<Opt<T>>:
        // * Do & DoIfNone
        // * Map & FlatMap
        // * Try & TryMap
        // * Match & MatchDo

        return others.FirstOrNone(x => x.Score() == Score());
    }
    public Opt<string> FindNicknameOfMatch(IEnumerable<Player> others)
    {
        // FindMatch(others).Map(x => x.Nickname) => would have returned Opt<Opt<string>>.
        // FlatMap allows to escape the nesting

        // Alternatively, one can use the Flatten method:
        // FindMatch(others).Map(x => x.Nickname).Flatten();
        
        return FindMatch(others).FlatMap(x => x.Nickname);
    }
    public Opt<char> FindInitialOfNicknameOfMatch(IEnumerable<Player> others)
    {
        // you may keep chaining:
        // * if any step returns Non, it will be carried on to the end bypassing succeeding methods
        // * the output will be IsSome only if all steps return Some.
        return FindMatch(others)
            .FlatMap(x => x.Nickname)
            .Map(nick => nick[0]);
    }


    // static methods
    public static Opt<string> ValidateEmail(string inputEmailAddress)
    {
        // the value will be Some only if the validation condition is true.
        return SomeIf(inputEmailAddress.Contains('@'), inputEmailAddress);
    }
    public static void Parsers()
    {
        // string & ReadOnlySpan<char> has ParseOrNone extensions for primitives
        Opt<int> nbGames = "12".ParseIntOrNone();
        Opt<bool> isWin = "false".ParseBoolOrNone();
        Opt<double> elapsedSeconds = "42.42".ParseDoubleOrNone();

        // and general parser for any T with lambda parser
        Opt<bool> isWinLambda = "Yes".TryParseOrNone(str => StringComparer.OrdinalIgnoreCase.Equals(str, "yes"));
    }
    public static List<string> Nicknames(IEnumerable<Player> players)
    {
        // will create a list of all nicknames, skipping None's.
        return players.Select(x => x.Nickname).UnwrapSomes().ToList();
    }
    public static Opt<Player> GetPlayerByNickname(Dictionary<string, Player> dictNickPlayer, string nickname)
    {
        // (similar to FirstOrNone or LastOrNone)
        // alternative to GetValueOrDefault method which would have returned null in absent case.

        return dictNickPlayer.GetOpt(nickname);
    }


    // static methods - constructors
    public static Player Dendi()
    {
        return new Player(
            Name: "Dendi",
            Wins: 0,
            Nickname: Some("NaVi"),         // static constructor for Some variant.
            EmailAddress: None<string>()    // static constructor for None variant; can also use default
            );
    }
}
