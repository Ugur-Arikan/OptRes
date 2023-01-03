using static Examples.Helpers;

namespace Examples;

internal class ExampleResT
{
    internal static void Run()
    {
        Log("\nRunning Res<T> Examples");


        // Ok of T
        var okInt = Ok(42); // Res<int>
        Assert(okInt.IsOk && okInt.Unwrap() == 42);


        // Err of T
        var errFloat = Err<float>("something went wrong");  // T needs to be explicit
        Assert(errFloat.IsErr);
        Assert(errFloat.ToString().Contains("something went wrong"));


        // Err of T from exception
        int zero = 0;
        var resInt = Ok().TryMap(() => 1 / zero);
        Assert(resInt.IsErr);
        Assert(resInt.ToString().Contains("DivideByZeroExceptionmessage"));


        // Ok: returns Err when null; Ok<T> is guaranteed to be null-free
        var errString = Ok<string?>(null);
        Assert(errString.IsErr);


        // AsRes: from Opt to Res:
        // * None -> Err
        // * Some(x) -> Ok(x)
        static Wizard ParseWizard(string str)
        {
            var parts = str.Split('-');
            return new(Name: parts[0], NbSpells: int.Parse(parts[1]));
        }
        Opt<Wizard> someWizard = "Merlin-42".TryParseOrNone(ParseWizard);
        Assert(someWizard == Some(new Wizard("Merlin", 42)));

        Res<Wizard> okWizard = someWizard.IntoRes(); // Some(Merlin) -> Ok(Merlin)
        Assert(okWizard.IsOk && okWizard.Unwrap() == new Wizard("Merlin", 42));

        Opt<Wizard> noneWizard = "badwizardinput".TryParseOrNone(ParseWizard);
        Assert(noneWizard.IsNone);

        Res<Wizard> errWizard = noneWizard.IntoRes(); // None -> Err
        Assert(errWizard.IsErr);


        // Unwrap(): only when sure that it IsOk
        var resDuration = Ok(TimeSpan.FromSeconds(42));
        var duration = resDuration.Unwrap();    // would throw if it were IsErr, avoid Unwrap as much as possible
        Assert(duration.Seconds == 42);
        
        resDuration = Err<TimeSpan>("sth wrong");
        try
        {
            duration = resDuration.Unwrap();
            Assert(false, "must have thrown an exception while unwrapping Err");
        }
        catch { /*will end up here*/ }


        // UnwrapOr: with a fallback value
        resDuration = Ok(TimeSpan.FromSeconds(42));
        duration = resDuration.UnwrapOr(TimeSpan.FromSeconds(10));
        Assert(duration.Seconds == 42); // fallback value is ignored since resDuration.IsOk

        resDuration = Err<TimeSpan>("sth went wrong");
        duration = resDuration.UnwrapOr(() => TimeSpan.FromSeconds(10)); // lazy version, in case the fallback is expensive
        Assert(duration.Seconds == 10);


        // Match: both Ok(val) and Err to values
        var resultWizard = Ok(new Wizard("Merlin", 42));

        int nbSpells = resultWizard.Match(whenOk: w => w.NbSpells, whenErr: _ => 0);
        nbSpells = resultWizard.Match(w => w.NbSpells, _ => 0);
        nbSpells = resultWizard.Match(w => w.NbSpells, error => 0);
        Assert(nbSpells == 42);

        int nbSpellsOfErr = Err<Wizard>("db-conn-err").Match(w => w.NbSpells, _ => 0);
        Assert(nbSpellsOfErr == 0);


        // Res<T> return for functions that can fail
        static Res<Wizard> GetWizard(string databaseName, Guid guid, bool simulateException)
        {
            return OkIf(databaseName != "bad-db")               // input validation, trivial error handling
                .Try(() => ExternalDbCall(simulateException))   // external call that can throw, safely handled Try.
                .Map(() => new Wizard("Morgana", 42));          // map to result only if the state is Ok.
        }

        resultWizard = GetWizard("good-db", new Guid(), false);
        Assert(resultWizard.IsOk);

        resultWizard = GetWizard("bad-db", new(), false);
        Assert(resultWizard.IsErr);

        resultWizard = GetWizard("good-db", new(), true);
        Assert(resultWizard.IsErr);


        // Pipe where Err track is bypassed
        static Wizard UpdateWizard(Wizard wizard, int newNbSpells) // safe operation that updates wizard in memory
            => wizard with { NbSpells = newNbSpells };
        static Res SaveWizard(Wizard wizard, bool simulateException)
            => ErrIf(simulateException, "Failed to save wizard.");
        
        var res = GetWizard("good-db", new(), false)            // Ok of wizard
                    .Map(w => UpdateWizard(w, w.NbSpells * 2))  // Ok of updated wizard
                    .FlatMap(w => SaveWizard(w, false));        // Ok (use of FlatMap to flatten Res<Res>).
        Assert(res.IsOk);

        res = GetWizard("good-db", new(), false)                // Ok of wizard
                    .Map(w => UpdateWizard(w, w.NbSpells * 2))  // Ok of updated wizard
                    .Map(w => SaveWizard(w, true))              // Err
                    .Flatten();                                 // alternative to FlatMap call above.
        Assert(res.IsErr);

        res = GetWizard("good-db", new(), true)                 // Err
                    .Map(w => UpdateWizard(w, w.NbSpells * 2))  // Err: UpdateWizard is never called
                    .FlatMap(w => SaveWizard(w, false));        // Err: SaveWizard is never called
        Assert(res.IsErr);


        // Do, where Err track is bypassed
        errWizard.Do(w => Console.WriteLine(w.Name));          // nothing will be written

        // DoIfErr, that is executed only when IsErr
        errWizard.DoIfErr(error => Console.WriteLine(error));   // error will be written

        // throw when error
        Ok(new Wizard("Merlin", 42)).ThrowIfErr();  // no exceptions
        try
        {
            errWizard.ThrowIfErr(); // will throw!
            Assert(false, "must have thrown before reaching here.");
        }
        catch
        {
        }


        // default parsers on string & ReadOnlySpan<char>
        resInt = "42".ParseIntOrErr();
        Assert(resInt.IsOk && resInt.Unwrap() == 42);

        // custom parser with parser argument
        resultWizard = "badwizardinput".TryParseOrErr(ParseWizard);


        // linq
        IEnumerable<Res<Wizard>> wizards = new List<Res<Wizard>>()
        {
            Err<Wizard>("db error"),
            Ok(new Wizard("Solmyr", 42)),
            Err<Wizard>("parsing error"),
            Ok(new Wizard("Jeddite", 42)),
        };

        // gets underlying values of Ok variants, skips Err's
        var okays = wizards.UnwrapOkays();
        Assert(okays.Count() == 2 && okays.First().Name == "Merlin");

        // converts IEnumerable<Res<T>> to Res<IEnumerable<T>>; Ok only if all items are Ok.
        var unwrappedWizards = wizards.TryUnwrap();
        Assert(unwrappedWizards.IsErr);

        unwrappedWizards = (new List<Res<Wizard>>() { Ok(new Wizard("Deemer", 42)) }).TryUnwrap();
        Assert(unwrappedWizards.IsOk);

        // combine with Select
        var csvWizards = "Ayden-42,Adela-42,Dracon-12,Astral-7";
        unwrappedWizards = 
            csvWizards                                      // string
            .Split(',')                                     // string[]
            .Select(txt => txt.TryParseOrErr(ParseWizard))  // IEnumerable<Res<Wizard>>
            .TryUnwrap();                                   // Res<IEnumerable<Wizard>>
        Assert(unwrappedWizards.IsOk);

        // reduce
        string csvNumbers = "1,2,3,4";
        var resultSumParsedNumbers =
            csvNumbers                                      // string
            .Split(",")                                     // string[]
            .Select(x => x.ParseIntOrErr())                 // IEnumerable<Res<int>>
            .Reduce((a, b) => a + b);                       // Res<int>
        Assert(resultSumParsedNumbers.IsOk && resultSumParsedNumbers.Unwrap() == 10);

        resultSumParsedNumbers =
            "1,2,!,4"
            .Split(",")
            .Select(x => x.ParseIntOrErr())
            .Reduce((a, b) => a + b);
        Assert(resultSumParsedNumbers.IsErr);
    }
}
