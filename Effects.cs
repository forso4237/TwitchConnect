using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace TwitchConnect;

public sealed class EffectDef
{
    public readonly ModSettingBool Enabled;
    public readonly ModSettingString Word;
    public readonly string Label;
    public readonly Action<InGame> Apply;

    public EffectDef(ModSettingBool enabled, ModSettingString word, string label, Action<InGame> apply)
    {
        Enabled = enabled;
        Word = word;
        Label = label;
        Apply = apply;
    }
}

public static class Effects
{
    private static readonly Random Rng = new();

    public static readonly List<EffectDef> Registry = new()
    {
        new EffectDef(TwitchConnectMod.HalveCashEnabled,  TwitchConnectMod.HalveCashWord,  "halved your cash",          HalveCash),
        new EffectDef(TwitchConnectMod.HalveLivesEnabled, TwitchConnectMod.HalveLivesWord, "halved your lives",         HalveLives),
        new EffectDef(TwitchConnectMod.SellEnabled,       TwitchConnectMod.SellWord,       "sold a random tower",       SellRandomTower),
        new EffectDef(TwitchConnectMod.RushEnabled,       TwitchConnectMod.RushWord,       "re-sent the current round", RushRound),
        new EffectDef(TwitchConnectMod.GiftEnabled,       TwitchConnectMod.GiftWord,       "gave you some cash",        GiftCash),
    };

    public static void HalveCash(InGame inGame) =>
        inGame.SetCash(Math.Ceiling(inGame.GetCash() / 2));

    public static void HalveLives(InGame inGame) =>
        inGame.SetHealth(Math.Max(1, Math.Ceiling(inGame.GetHealth() / 2)));

    public static void GiftCash(InGame inGame) =>
        inGame.AddCash((double)TwitchConnectMod.GiftAmount);

    public static void SellRandomTower(InGame inGame)
    {
        var towers = inGame.GetTowers();
        if (towers == null || towers.Count == 0) return;
        inGame.SellTower(towers[Rng.Next(towers.Count)]);
    }

    public static void RushRound(InGame inGame)
    {
        var round = inGame.bridge.GetCurrentRound() + 1;
        inGame.SpawnBloons(round);
    }
}