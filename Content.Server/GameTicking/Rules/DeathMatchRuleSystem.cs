using System.Linq;
using Content.Server._StationWare.WareEvents;
using Content.Server.Chat.Managers;
using Content.Shared.CCVar;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Server.GameTicking.Rules;

/// <summary>
///     Simple GameRule that will do a free-for-all death match.
///     Kill everybody else to win.
/// </summary>
public sealed class DeathMatchRuleSystem : GameRuleSystem
{
    public override string Prototype => "DeathMatch";

    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    private const float RestartDelay = 10f;
    private const float DeadCheckDelay = 0;

    private float? _deadCheckTimer;
    private float? _restartTimer;

    private string? _winnerName;
    private string? _winnerUsername;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WareEventRanEvent>(OnWareEvent);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnWareEvent(ref WareEventRanEvent ev)
    {
        RunDelayedCheck();
    }

    public override void Started()
    {
        _winnerName = null;
        _winnerUsername = null;
    }

    public override void Ended()
    {
        _deadCheckTimer = null;
        _restartTimer = null;
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        if (!RuleAdded)
            return;

        var line = _winnerName != null && _winnerUsername != null
            ? Loc.GetString("rule-death-match-check-winner", ("name", _winnerName), ("username", _winnerUsername))
            : Loc.GetString("rule-death-match-check-winner-stalemate");
        ev.AddLine(line);
    }

    private void RunDelayedCheck()
    {
        if (!RuleAdded || _deadCheckTimer != null)
            return;

        _deadCheckTimer = DeadCheckDelay;
    }

    public override void Update(float frameTime)
    {
        if (!RuleAdded)
            return;

        // If the restart timer is active, that means the round is ending soon, no need to check for winners.
        // TODO: We probably want a sane, centralized round end thingie in GameTicker, RoundEndSystem is no good...
        if (_restartTimer != null)
        {
            _restartTimer -= frameTime;

            if (_restartTimer > 0f)
                return;

            //GameTicker.EndRound();
            GameTicker.RestartRound();
            return;
        }

        if (!_cfg.GetCVar(CCVars.GameLobbyEnableWin) || _deadCheckTimer == null)
            return;

        _deadCheckTimer -= frameTime;

        if (_deadCheckTimer > 0)
            return;

        _deadCheckTimer = null;

        List<IPlayerSession> winners = new();
        foreach (var (actor, mobState) in EntityQuery<ActorComponent, MobStateComponent>())
        {
            if (actor.PlayerSession.AttachedEntity is not {Valid: true} playerEntity)
                continue;

            if (_mobStateSystem.IsDead(playerEntity, mobState))
                continue;

            winners.Add(actor.PlayerSession);
        }

        if (winners.Count > 1)
            return;

        var winner = winners.FirstOrDefault();
        _winnerUsername = winner?.Name;
        if (winner?.AttachedEntity != null)
            _winnerName = MetaData(winner.AttachedEntity.Value).EntityName;

        _chatManager.DispatchServerAnnouncement(Loc.GetString("rule-restarting-in-seconds", ("seconds", RestartDelay)));
        GameTicker.ShowRoundEndScoreboard();
        _restartTimer = RestartDelay;
    }
}
