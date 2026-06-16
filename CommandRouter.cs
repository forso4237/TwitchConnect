using System;
using System.Collections.Concurrent;
using BTD_Mod_Helper;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace TwitchConnect;

public readonly struct QueuedEffect
{
    public readonly string User;
    public readonly string Label;
    public readonly Action<InGame> Apply;

    public QueuedEffect(string user, string label, Action<InGame> apply)
    {
        User = user;
        Label = label;
        Apply = apply;
    }
}

public static class CommandRouter
{
    public static readonly ConcurrentQueue<QueuedEffect> Pending = new();

    private static readonly object CooldownLock = new();
    private static DateTime _lastAccepted = DateTime.MinValue;

    public static void Handle(string user, string text)
    {
        if (!(bool)TwitchConnectMod.Enabled) return;
        if (string.IsNullOrWhiteSpace(text) || text[0] != '!') return;

        var word = text.Split(' ')[0].Substring(1).ToLowerInvariant();
        if (word.Length == 0) return;

        EffectDef? match = null;
        foreach (var def in Effects.Registry)
        {
            if (!(bool)def.Enabled) continue;
            var trigger = ((string)def.Word)?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(trigger)) continue;
            if (trigger == word) { match = def; break; }
        }
        if (match == null) return;

        lock (CooldownLock)
        {
            var cd = TimeSpan.FromSeconds((double)TwitchConnectMod.CooldownSeconds);
            if (DateTime.UtcNow - _lastAccepted < cd) return;
            _lastAccepted = DateTime.UtcNow;
        }

        Pending.Enqueue(new QueuedEffect(user, match.Label, match.Apply));
        ModHelper.Msg<TwitchConnectMod>($"{user} -> !{word}");
    }
}