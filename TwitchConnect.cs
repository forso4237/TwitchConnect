global using BTD_Mod_Helper.Extensions;
using System;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using MelonLoader;
using TwitchConnect;
using UnityEngine;

[assembly: MelonInfo(typeof(TwitchConnect.TwitchConnectMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6-Epic")]

namespace TwitchConnect;

public class TwitchConnectMod : BloonsTD6Mod
{

    private static readonly ModSettingCategory ConnectionCat = new("Connection");

    public static readonly ModSettingString TwitchChannel = new("")
    {
        category = ConnectionCat,
        description = "Your Twitch channel name, all lowercase. Leave blank to disable."
    };

    public static readonly ModSettingBool Enabled = new(true)
    {
        category = ConnectionCat,
        description = "Master switch for all chat effects."
    };

    public static readonly ModSettingDouble CooldownSeconds = new(30)
    {
        category = ConnectionCat,
        description = "Minimum seconds between accepted chat effects (stops chat spam-nuking you).",
        min = 0,
        max = 300
    };

    private static readonly ModSettingCategory SabotageCat = new("Sabotage Commands");

    public static readonly ModSettingBool HalveCashEnabled = new(true)
    { category = SabotageCat, description = "Command: halve the streamer's cash." };
    public static readonly ModSettingString HalveCashWord = new("halfcash")
    { category = SabotageCat, description = "Trigger word for halve-cash (no '!')." };

    public static readonly ModSettingBool HalveLivesEnabled = new(true)
    { category = SabotageCat, description = "Command: halve the streamer's lives." };
    public static readonly ModSettingString HalveLivesWord = new("halflife")
    { category = SabotageCat, description = "Trigger word for halve-lives (no '!')." };

    public static readonly ModSettingBool SellEnabled = new(true)
    { category = SabotageCat, description = "Command: sell a random tower." };
    public static readonly ModSettingString SellWord = new("sell")
    { category = SabotageCat, description = "Trigger word for sell-random-tower (no '!')." };

    public static readonly ModSettingBool RushEnabled = new(true)
    { category = SabotageCat, description = "Command: re-send the current round's bloons." };
    public static readonly ModSettingString RushWord = new("rush")
    { category = SabotageCat, description = "Trigger word for rush (no '!')." };

    private static readonly ModSettingCategory HelpCat = new("Help Command");

    public static readonly ModSettingBool GiftEnabled = new(false)
    { category = HelpCat, description = "Command: give the streamer cash." };
    public static readonly ModSettingString GiftWord = new("help")
    { category = HelpCat, description = "Trigger word for the gift-cash command (no '!')." };
    public static readonly ModSettingDouble GiftAmount = new(500)
    { category = HelpCat, description = "How much cash the help command grants.", min = 0, max = 100000 };

    public override void OnApplicationStart()
    {
        ModHelper.Msg<TwitchConnectMod>("TwitchConnect loaded.");
        TwitchChatClient.Start();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F9))
            ModGameMenu.Open<TwitchPanelMenu>();

        var inGame = InGame.instance;
        if (inGame == null) return;

        if (inGame.IsCoop || Game.instance.IsInOdyssey()) return;

        if (!CommandRouter.Pending.TryDequeue(out var fx)) return;

        try
        {
            fx.Apply(inGame);
            ModHelper.Msg<TwitchConnectMod>($"{fx.User} {fx.Label}!");
        }
        catch (Exception e)
        {
            ModHelper.Warning<TwitchConnectMod>($"Effect '{fx.Label}' failed: {e.Message}");
        }
    }
}