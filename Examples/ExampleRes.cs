using static Examples.Helpers;

namespace Examples;

internal static class ExampleRes
{
    internal static void Run()
    {
        Log("\nRunning Res Examples");


        // Ok
        var justOk = Ok();
        Assert(justOk.IsOk);
        Assert(!justOk.IsErr);


        // Err
        var justErr = Err("something went wrong");
        Assert(justErr.IsErr);
        justErr.DoIfErr(error => error.Contains("something went wrong"));


        // Res from Try method
        int oneOverFive, divider = 5;
        var res = Ok().Try(() => oneOverFive = 1 / divider);
        Assert(res.IsOk);

        divider = 0;
        res = Ok().Try(() => oneOverFive = 1 / divider);    // Try methods run within try-catch blocks;
        Assert(res.IsErr);                                  // error messages are created from the expression & exception.
        Assert(res.ToString().Contains("DivideByZeroException: Attempted to divide by zero."));


        // Res return for actions that can fail
        static Res PutWizard(string databaseName, Wizard wizard, bool simulateException)
        {
            // chain of validations and operations
            return ErrIf(databaseName == "bad-db")                  // input validation, trivial error handling
                .OkIf(databaseName != "bad-db")                     // just to demonstrate the OkIf, counterpart of ErrIf
                .Try(() => ExternalDbCall(simulateException));      // external call that can throw, safely handled Try.
        }

        Wizard morgana = new("Morgana", 42);
        var pushed = PutWizard("good-db", morgana, false);
        Assert(pushed.IsOk);

        pushed = PutWizard("bad-db", morgana, false);
        Assert(pushed.IsErr);
        Assert(pushed.ToString().Contains("wrong database"));

        pushed = PutWizard("good-db", morgana, true /*unlucky*/);
        Assert(pushed.IsErr);
        Assert(pushed.ToString().Contains("database connection error"));


        // Match: both Ok and Err to values
        res = Ok();
        bool isValid = res.Match(true, _err => false); // match to a value; true if IsOk, false if IsErr
        string resultMessage = res.Match("valid", err => "Err: " + err); // error variant can use the internal error message


        // method chain where Err track is bypassed
        static Res PutRecord(Wizard _record, bool simulateErr)
            => ErrIf(simulateErr, "Failed to put record.");
        static Res PushLog(bool simulateErr)
            => simulateErr ? Err("Failed to push log.") : Ok();

        res = PutRecord(new("Merlin", 42), false)
            .FlatMap(() => PushLog(false));         // will successfully put-record & push-log; FlatMap avoids Res<Res>
        Assert(res.IsOk);

        res = PutRecord(new("Merlin", 42), false)   // will successfully put-record,
                .FlatMap(() => PushLog(true));      // but fail to push-log
        Assert(res.IsErr);

        res = PutRecord(new("Merlin", 42), true)    // will fail to put-record,
                .FlatMap(() => PushLog(false));     // PushLog will never be called
        Assert(res.IsErr);


        // reduce results of several operations
        var wizards = new Wizard[]
        {
            new("Vidomina", 42000),
            new("Sandro", 42),
            new("Gundula", 2),
            new("Andra", 2),
        };

        // will put all wizards successfully
        var overallResult = wizards.Select(w => PutWizard("good-db", w, false)).Reduce();
        Assert(overallResult.IsOk);

        // second PutWizard operation fails; third & fourth operations are not executed
        overallResult = wizards.Select(w => PutWizard("good-db", w, w.Name == "Sandro")).Reduce();
        Assert(overallResult.IsErr);

        // second PutWizard operation fails; third & fourth operations are still executed
        overallResult = wizards.Select(w => PutWizard("good-db", w, w.Name == "Sandro")).Reduce(stopAtFirstError: false);
        Assert(overallResult.IsErr); // reduced result is still Err
    }
}
