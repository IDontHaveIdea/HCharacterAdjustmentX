﻿//
// Configuration entries
//
using UnityEngine;

using BepInEx.Configuration;
using BepInEx.Logging;

using KKAPI.Utilities;

//using IDHIUtils;


namespace IDHIPlugins
{
    public partial class HCharaAdjustmentX
    {
        internal static KeyShortcuts KeyHeroine = new();
        internal static KeyShortcuts KeyHeroine3P = new();
        internal static KeyShortcuts KeyPlayer = new();

        internal static ConfigEntry<bool> DebugInfo;
        internal static ConfigEntry<KeyboardShortcut> GroupGuide { get; set; }
        internal static ConfigEntry<float> cfgAdjustmentStep;
        
        internal void ConfigEntries(bool bHCAInstalled, bool bheroine3P = false)
        {
            // Definition of configuration items
            var sectionKeys = "Keyboard Shortcuts for Guide";
#if DEBUG
            _Log.Info($"HCAX0016: Creating Shortcuts for Characters");
#endif
            #region Heroine
            KeyHeroine.Menu = Config.Bind(
                section: sectionKeys,
                key: "Toggle button interface for Heroine.",
                defaultValue: new KeyboardShortcut(KeyCode.L),
                configDescription: new ConfigDescription(
                    description: "Show movement buttons",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 27 }));
            #endregion Female

            #region Player
            KeyPlayer.Menu = Config.Bind(
                section: sectionKeys,
                key: "Toggle button interface for Player.",
                defaultValue: new KeyboardShortcut(
                    KeyCode.L, KeyCode.RightAlt, KeyCode.AltGr),
                configDescription: new ConfigDescription(
                    description: "Show movement buttons",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 26 }));
            #endregion Player

            #region Heroine3P
            if (bheroine3P)
            {
                KeyHeroine.Menu = Config.Bind(
                    section: sectionKeys,
                    key: "Toggle button interface Heroine 2.",
                    defaultValue: new KeyboardShortcut(KeyCode.L),
                    configDescription: new ConfigDescription(
                        description: "Show movements buttons",
                        acceptableValues: null,
                        tags: new ConfigurationManagerAttributes { Order = 25 }));
            }
            #endregion Female 2

            #region Steps
#if DEBUG
            _Log.Info($"HCAX0017: Creating Shortcuts for Steps");
#endif

            sectionKeys = "Movement Step";
            cfgAdjustmentStep = Config.Bind(
                section: sectionKeys,
                key: "Move step amount",
                defaultValue: 0.01f,
                configDescription: new ConfigDescription(
                    description: "Set the step by with to move",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 14 }));
            cfgAdjustmentStep.SettingChanged += (_sender, _args) =>
            {
                if (_fAdjustStep != cfgAdjustmentStep.Value)
                {
                    _fAdjustStep = cfgAdjustmentStep.Value;
#if DEBUG
                    _Log.Info($"HCAX0018: Movement step read in configuration - {cfgAdjustmentStep.Value}");
#endif
                }
            };
            #endregion Steps
        }

        internal void ConfigDebugEnntry()
        {
            DebugInfo = Config.Bind(
                section: "Debug",
                key: "Debug Information",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information in Console",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes {
                        Order = 1,
                        IsAdvanced = true }));
            DebugInfo.SettingChanged += (_sender, _args) =>
            {
                _Log.Enabled = DebugInfo.Value;
#if DEBUG
                _Log.Info($"HCAX0019: Log.Enabled set to {_Log.Enabled}");
#endif
            };
        }
    }
}
