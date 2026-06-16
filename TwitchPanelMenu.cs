using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.ChallengeEditor;
using System;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace TwitchConnect;

public class TwitchPanelMenu : ModGameMenu<ExtraSettingsScreen>
{
    public override bool OnMenuOpened(Object data)
    {
        try
        {
            var host = GameMenu.gameObject.GetComponentInChildrenByName<RectTransform>("Panel");
            var parent = host != null ? host.transform : GameMenu.transform;
            if (host != null) host.gameObject.DestroyAllChildren();
            ModHelper.Msg<TwitchConnectMod>($"[Panel] host found = {host != null}");

            var root = ModHelperPanel.Create(
                new Info("TwitchRoot", InfoPreset.FillParent),
                VanillaSprites.MainBGPanelBlue, RectTransform.Axis.Vertical, 20, 50);
            root.AddTo(parent);

            root.AddText(new Info("Title") { FlexWidth = 1, Height = 130 }, "TwitchConnect", 75f);

            var channel = ((string)TwitchConnectMod.TwitchChannel)?.Trim();
            var status = string.IsNullOrEmpty(channel)
                ? "No channel set  -  Settings > Mods > TwitchConnect"
                : $"Reading chat from  #{channel}";
            root.AddText(new Info("Status") { FlexWidth = 1, Height = 80 }, status, 45f);

            var list = root.AddScrollPanel(
                new Info("CmdList", InfoPreset.Flex),
                RectTransform.Axis.Vertical, null, 20, 20);

            foreach (var def in Effects.Registry)
                AddCommandRow(list.ScrollContent, def);

            ModHelper.Msg<TwitchConnectMod>($"[Panel] built {Effects.Registry.Count} rows");
        }
        catch (Exception e)
        {
            ModHelper.Error<TwitchConnectMod>($"[Panel] build failed: {e}");
        }

        return false;
    }

    private static void AddCommandRow(ModHelperPanel parent, EffectDef def)
    {
        var row = parent.AddPanel(
            new Info(def.Label) { FlexWidth = 1, Height = 140 },
            VanillaSprites.BlueInsertPanelRound, RectTransform.Axis.Horizontal, 40, 30);

        var startOn = (bool)def.Enabled;
        ModHelperButton? toggle = null;
        ModHelperText? toggleText = null;

        toggle = row.AddButton(
            new Info("Toggle", 260, 110),
            startOn ? VanillaSprites.GreenBtnLong : VanillaSprites.RedBtnLong,
            new Action(() =>
            {
                var on = !(bool)def.Enabled;
                def.Enabled.SetValueAndSave(on);
                toggle!.Image.SetSprite(on ? VanillaSprites.GreenBtnLong : VanillaSprites.RedBtnLong);
                toggleText!.SetText(on ? "ON" : "OFF");
            }));
        toggleText = toggle.AddText(new Info("State", 260, 110), startOn ? "ON" : "OFF", 44f);

        row.AddText(new Info("Word", 460, 100), "!" + (string)def.Word, 50f);
        row.AddText(new Info("Label") { FlexWidth = 1, Height = 100 }, def.Label, 36f);
    }
}