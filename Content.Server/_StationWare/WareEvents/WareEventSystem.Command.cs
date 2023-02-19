using System.Linq;
using Content.Server.Administration;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._StationWare.WareEvents;

public sealed partial class WareEventSystem
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    public void InitializeCommands()
    {
        _conHost.RegisterCommand("addwareevent", "Runs the specified Ware Event", "addwareevent <prototype ID>",
            AddWareEventCommand,
            AddWareEventCommandCompletions);
    }

    [AdminCommand(AdminFlags.Debug)]
    private void AddWareEventCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Argument length must be 1");
            return;
        }

        if (!_prototype.TryIndex<WareEventPrototype>(args[0], out var wareEvent))
            return;

        RunWareEvent(wareEvent);
    }

    private CompletionResult AddWareEventCommandCompletions(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(_prototype.EnumeratePrototypes<WareEventPrototype>().Select(p => p.ID), "<prototype ID>");
        }

        return CompletionResult.Empty;
    }
}
