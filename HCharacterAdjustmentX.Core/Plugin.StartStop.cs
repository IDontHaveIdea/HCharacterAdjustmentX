// 
// Buttons interface handling
//
using System;
using System.Collections.Generic;

using BepInEx.Logging;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class HCharaAdjustmentX
    {
        public static HSceneProc Instance { get; internal set; }
        public static List<ChaControl> Heroines { get; internal set; }
        public static ChaControl Player { get; internal set; }

        private void OnHInit(object s, EventArgs e)
        {
            Hooks.Init();
            HProcMonitor.OnExit += OnHProcExit;
            HProcMonitor.OnFinishedLoading += OnHProcFinishedLoading;

        }

        private void OnHProcExit(object s, EventArgs e)
        {
            SetControllerEnabled(false);
            if (HProcTraverse != null)
            {
                HProcTraverse = null;
            }
            enabled = false;
            try
            {
                _hookInstance.UnpatchSelf();
                _hookInstance = null;
            }
            catch (Exception ex) {
                _Log.Level(LogLevel.Error, $"HCAX0005: {ex}");
            }
            HProcMonitor.OnExit -= OnHProcExit;
        }

        private void OnHProcFinishedLoading(
            object s, HProcMonitor.HSceneLoadingEventArgs e)
        {
#if DEBUG
            //_Log.Info($"[OnHProcFinishedLoading] Enabling HCAX.");
#endif
            SetupController(e.ObjectInstance);
            Heroines = e.Females;
            Player = e.Male;
            enabled = true;
            SetControllerEnabled(true);
            HProcMonitor.OnFinishedLoading -= OnHProcFinishedLoading;
        }

        private static void SetupController(object instance)
        {
            CharacterType chType;
            // HSceneProc instance will be used later
            HProcObject = instance;
            HProcTraverse = new HSceneProcTraverse(instance);

            // set various flags
            PlugInUtils.SetMode(HProcTraverse.flags.mode);

            // verify if is a scene we support
            if (!IsSupportedScene)
            {
                _Log.Debug($"[SetupController] The _mode {_mode}" +
                    $" is not supported.");
                return;
            }

            // Initialize character controllers
            for (var i = 0; i < HProcTraverse.flags.lstHeroine.Count; i++)
            {
                chType = (CharacterType)i;

                GetController(HProcTraverse.flags.lstHeroine[i].chaCtrl).Init(
                    HProcTraverse, chType);
            }

            GetController(HProcTraverse.flags.player.chaCtrl).Init(
                HProcTraverse, CharacterType.Player);

            // Group move guide off
            HProcTraverse.sprite.axis.tglDraw.isOn = false;
            HProcTraverse.guideObject.gameObject.SetActive(false);
        }

        /// <summary>
        /// Method to enable/disabled the characters controllers
        /// </summary>
        /// <param name="setState"></param>
        internal static void SetControllerEnabled(bool setState = true)
        {
            try
            {
                for (var i = 0; i < Heroines.Count; i++)
                {
                    try
                    {
                        GetController(Heroines[i]).enabled = setState;
                    }
                    catch (Exception e)
                    {
                        _Log.Level(LogLevel.Error, $"HCAX0010: Error trying to " +
                            $"{setEnabled(setState)} the Controller for " +
                            $"{(CharacterType)i} - {e}");
                    }
                }

                try
                {
                    GetController(Player).enabled = setState;
                }
                catch (Exception e)
                {
                    _Log.Level(LogLevel.Error, $"HCAX0012: Error trying to " +
                        $"{setEnabled(setState)} the Controller for Player - \n{e}");
                }
            }
            catch { _Log.Level(LogLevel.Error, $"HCAX0013: No Heroines found."); }
        }

        internal static readonly Func<bool, string> setEnabled = state =>
            state ? "set to enabled" : "set to disabled";
    }
}
