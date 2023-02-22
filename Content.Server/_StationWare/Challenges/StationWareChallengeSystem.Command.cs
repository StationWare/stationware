using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._StationWare.Challenges;

public sealed partial class StationWareChallengeSystem
{
    [AdminCommand(AdminFlags.Debug)]
    private void StartChallengeCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Argument length must be 1");
            return;
        }

        if (!_prototype.TryIndex<ChallengePrototype>(args[0], out var wareEvent))
            return;

        StartChallenge(wareEvent);
    }

    private CompletionResult StartChallengeCommandCompletions(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _prototype.EnumeratePrototypes<ChallengePrototype>().Select(p => p.ID), "<prototype ID>");
        }

        return CompletionResult.Empty;
    }
}
