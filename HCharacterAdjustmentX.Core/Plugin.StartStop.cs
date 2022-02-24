﻿// 
// Buttons interface handling
//
using System;

using UnityEngine.SceneManagement;

using BepInEx.Logging;

using IDHIUtils;

using static IDHIPlugins.HProcScene;
using static IDHIPlugins.HCharaterAdjustX.HCharacterAdjustXController;


namespace IDHIPlugins
{
    public partial class HCharaterAdjustX
    {
        /// <summary>
        /// Wait for screen with name HProc this is a H scene loading.
        /// This will enable HProcScene and SHCAdjustController
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="loadSceneMode"></param>
        private void MonitorHProc(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "HProc")
            {
                Log.Warning($"XXXX: Start SHCA.");
                _sceneName = scene.name;
                Hooks.Init();
                HProcScene.HSHooks.Init();
                HProcScene.Nakadashi = true;
                HProcScene.OnHSceneExiting += OnHProcExit;
                HProcScene.OnHSceneFinishedLoading += OnHProcFinishedLoading;
                HProcScene.InvokeOnHSceneStartLoading(null, null);
            }
        }

        private void OnHProcExit(object s, EventArgs e)
        {
#if DEBUG
            Log.Info($"SHCA0024: Removing patches and disabling SHCA.");
#endif
            SetControllerEnabled(false);
            if (_hprocInstance != null)
            {
                _hprocInstance = null;
            }
            enabled = false;
            try
            {
                _hookInstance.UnpatchSelf();
            }
            catch (Exception ex) {
                Log.Level(LogLevel.Error, $"SHCA0034: {ex}");
            }
#if DEBUG
            Log.Info($"SHCA0025: Removing patches and disabling SHCA OK.");
#endif
            HProcScene.OnHSceneExiting -= OnHProcExit;
        }

        private void OnHProcFinishedLoading(object s, HSceneFinishedLoadingEventArgs e)
        {
#if DEBUG
            Log.Info($"SHCA0041: Enabling SHCA..");
#endif
            SetupController(e.Instance);
            enabled = true;
            SetControllerEnabled(true);
            HProcScene.OnHSceneFinishedLoading -= OnHProcFinishedLoading;
        }

        static private void SetupController(HSceneProc instance)
        {
            CharacterType chType;
            _hprocInstance = instance;  // HSceneProc instance will be used later

            // set various flags
            Utils.SetMode(_hprocInstance.flags.mode);

            // verify if is a scene we support
            if (!IsSupportedScene)
            {
                Log.Warning($"SHCA0006: No Way José!! The _mode {_mode}" +
                    $" is not supported.");
                return;
            }

            // Creates guides and disables the controllers
            for (var i = 0; i < _hprocInstance.flags.lstHeroine.Count; i++)
            {
                chType = (CharacterType)i;

                GetController(_hprocInstance.flags.lstHeroine[i].chaCtrl).Init(
                    _hprocInstance, chType);
            }

            GetController(_hprocInstance.flags.player.chaCtrl).Init(
                _hprocInstance, CharacterType.Player);
            _hprocInstance.sprite.axis.tglDraw.isOn = false;
            _hprocInstance.guideObject.gameObject.SetActive(false);
        }

        /// <summary>
        /// Method to enable/disabled the characters controllers
        /// </summary>
        /// <param name="setState"></param>
        static internal void SetControllerEnabled(bool setState = true)
        {
            try
            {
                for (var i = 0; i < Heroines.Count; i++)
                {
                    try
                    {
                        GetController(Heroines[i]).enabled = setState;
#if DEBUG
                        Log.Info($"SHCA0014: Controller {setEnabled(setState)} for {(CharacterType)i}");
#endif
                    }
                    catch (Exception e)
                    {
                        Log.Level(LogLevel.Warning, $"SHCA0016: Error trying to " +
                            $"{setEnabled(setState)} the Controller for {(CharacterType)i} - {e}");
                    }
                }

                try
                {
                    GetController(HProcScene.Player).enabled = setState;
#if DEBUG
                    Log.Info($"SHCA0015: Controller {setEnabled(setState)} for Player");
#endif
                }
                catch (Exception e)
                {
                    Log.Level(LogLevel.Warning, $"SHCA0017: Error trying to " +
                        $"{setEnabled(setState)} the Controller for Player - \n{e}");
                }
            }
            catch { Log.Level(LogLevel.Error, $"No Heroines found."); }
        }

        static readonly internal Func<bool, string> setEnabled = state =>
            state ? "set to enabled" : "set to disabled";
    }
}
