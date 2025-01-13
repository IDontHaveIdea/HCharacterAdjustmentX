//
// Utils.cs
//
// Ignore Spelling: Utils

using System;
using System.Collections.Generic;

using UnityEngine;

using BepInEx.Logging;

using IDHIUtils;

using CTRL = IDHIPlugins.HCharaAdjustmentX.HCharaAdjustmentXController;
using static IDHIPlugins.HCharaAdjustmentX;


namespace IDHIPlugins
{
    public class PlugInUtils

    {
        /// <summary>
        /// Return categories in the string form "{ cat 1, cat 2, ,,,}"
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="names"></param>
        /// <param name="quotes"></param>
        /// <returns></returns>
        internal static string CategoryList(
            List<HSceneProc.Category> categories, bool names = false, bool quotes = true)
        {
            var tmp = "";
            var first = true;

            foreach (var c in categories)
            {
                if (first)
                {
                    if (names)
                    {
                        tmp += (PositionCategory)c.category;
                    }
                    else
                    {
                        tmp += c.category.ToString();
                    }
                    first = false;
                }
                else
                {
                    if (names)
                    {
                        tmp += ", " + (PositionCategory)c.category;
                    }
                    else
                    {
                        tmp += ", " + c.category.ToString();
                    }
                }
            }
            return quotes ? "\" { " + tmp + " }\"" : "{ " + tmp + " }";
        }

        /// <summary>
        /// Ditto
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="names"></param>
        /// <param name="quotes"></param>
        /// <returns></returns>
        internal static string CategoryList(
            List<int> categories, bool names = false, bool quotes = true)
        {
            var tmp = "";
            var first = true;

            foreach (var c in categories)
            {
                if (first)
                {
                    if (names)
                    {
                        tmp += (PositionCategory)c;
                    }
                    else
                    {
                        tmp += c.ToString();
                    }
                    first = false;
                }
                else
                {
                    if (names)
                    {
                        tmp += ", " + (PositionCategory)c;
                    }
                    else
                    {
                        tmp += ", " + c.ToString();
                    }
                }
            }
            return quotes ? "\" { " + tmp + " }\"" : "{ " + tmp + " }";
        }

        internal static void CheckAnimationLoader()
        {
            if (_animationLoader.Installed)
            {
                if (_animationLoader.VersionAtLeast("1.1.2.2"))
                {
                    _animationKeyOk = true;
                }
                else
                {
                    _animationKeyOk = false;
                }
                if (_animationLoader.VersionAtLeast("1.1.3.0"))
                {
                    _animationLoaderMovementOk = true;
                }
                else
                {
                    _animationLoaderMovementOk = false;
                }
                _Log.Debug($"[CheckAnimationLoader] AnimationLoader " +
                    $"version={_animationLoader.Version} " +
                    $"ok={_animationLoaderMovementOk}");

            }
        }

        /// <summary>
        /// Get animation key
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        internal static string GetAnimationKey(HSceneProc.AnimationListInfo animation)
        {
            var result = "";
            if (_animationKeyOk)
            {
                try
                {
                    result = _animationLoader
                            .GetAnimationKey(animation);
                }
                catch (Exception e)
                {
                    _animationKeyOk = false;
                    _Log.Level(LogLevel.Error, $"HCAX0024A: Error - {e.Message}");
                }
            }
            //_Log.Debug($"[GetAnimationKey] Animation key={result} " +
            //    $"({Utilities.Translate(animation.nameAnimation)}).");
            return result;
        }


        internal static List<Vector3> GetAnimationMovement(
            HSceneProc.AnimationListInfo animation)
        {
            List<Vector3> result = [ new Vector3(0, 0, 0), new Vector3(0, 0, 0) ];

            if (animation != null)
            {
                if (_animationLoaderMovementOk)
                {
                    try
                    {
                        result = _animationLoader
                            .GetAnimationMovement(animation);
                    }
                    catch (Exception e)
                    {
                        _animationLoaderMovementOk = false;
                        _Log.Level(LogLevel.Error, $"HCAX0024B: Error - {e.Message}");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Determine if there is a change in original position
        /// </summary>
        /// <param name="chaControl"></param>
        /// <returns></returns>
        internal static bool IsNewPosition(ChaControl chaControl)
        {
            var controller = GetController(chaControl);
            var currentPosition = HProcTraverse.nowHpointDataPos;
            var originalPosition = controller.OriginalPosition;
            if (currentPosition != originalPosition)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if there is a change in original position
        /// </summary>
        /// <param name="chaControl"></param>
        /// <returns></returns>
        internal static bool IsSamePosition(ChaControl chaControl)
        {
            return !IsNewPosition(chaControl);
        }

        /// <summary>
        /// Show some information for Heroine 1
        /// </summary>
        /// <param name="instance"></param>
        internal static void InitialPosition()
        {
            if (HProcTraverse == null)
            {
                return;
            }
            if (AnimationKey.IsNullOrEmpty())
            {
                return;
            }

            var heroines = HProcTraverse.flags.lstHeroine;
            CTRL ctrl;

            for (var i = 0; i < heroines.Count; i++)
            {
                ctrl = GetController(heroines[i].chaCtrl);
                if (ctrl.MoveData.Data.Count > 0)
                {
                    ctrl.MoveData.Data.TryGetValue(AnimationKey,
                        out var position);
                    if (position != null)
                    {
                        // Use TryGetValue
                        position.TryGetValue(ctrl.ChaType, out var data);
                        if (data != null)
                        {
                            var movement = data.Position;
                            ctrl.Movement = movement;
                            MoveEvent.InvokeOnPositionMoveEvent(null,
                                new MoveEvent.MoveEventArgs(
                                    ctrl.ChaType, MoveType.MOVE));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Move characters to saved original position
        /// </summary>
        /// <param name="message"></param>
        internal static void ResetPositionAll()
        {
            if (HProcTraverse == null)
            {
                return;
            }

            var heroines = HProcTraverse.flags.lstHeroine;

            CTRL ctrl;
            for (var i = 0; i < heroines.Count; i++)
            {
                ctrl = GetController(heroines[i].chaCtrl);
                if (IsSamePosition(heroines[i].chaCtrl))
                {
                    ctrl.ResetPosition();
                }
            }
            ctrl = GetController(HProcTraverse.flags.player.chaCtrl);
            if (IsSamePosition(HProcTraverse.flags.player.chaCtrl))
            {
                ctrl.ResetPosition();
            }
        }

        /// <summary>
        /// Set new original position for characters if there is a move 
        /// from original position saved
        /// </summary>
        internal static void SetOriginalPositionAll()
        {
            if (HProcTraverse == null)
            {
                return;
            }

            var heroines = HProcTraverse.flags.lstHeroine;

            CTRL ctrl;
            for (var i = 0; i < heroines.Count; i++)
            {
                ctrl = GetController(heroines[i].chaCtrl);
                ctrl.SetOriginalPosition();
            }

            ctrl = GetController(HProcTraverse.flags.player.chaCtrl);
            ctrl.SetOriginalPosition();
        }

        internal static void SetALMove(
            HSceneProc.AnimationListInfo _nextAinmInfo)
        {
            if (HProcTraverse == null)
            {
                return;
            }

            if (_animationLoaderMovementOk)
            {
                var movement = PlugInUtils
                    .GetAnimationMovement(_nextAinmInfo);

                var heroines = HProcTraverse.flags.lstHeroine;
                CTRL ctrl;
                for (var i = 0; i < heroines.Count; i++)
                {
                    ctrl = GetController(heroines[i].chaCtrl);
                    if (i == 0)
                    {
                        ctrl.ALMovement = movement[(int)Sex.Female];
                    }
                }
                ctrl = GetController(HProcTraverse.flags.player.chaCtrl);
                ctrl.ALMovement = movement[(int)Sex.Male];
            }
        }

        /// <summary>
        /// Save H _mode flag
        /// </summary>
        /// <param name="emode"></param>
        internal static void SetMode(HFlag.EMode emode)
        {
            // set various flags
            _mode = emode;
            IsAibu = (emode == HFlag.EMode.aibu);
            IsHoushi = (emode == HFlag.EMode.houshi);
            IsSonyu = (emode == HFlag.EMode.sonyu);
            IsSupportedScene = (IsAibu || IsHoushi || IsSonyu);
        }

        /// <summary>
        /// Turn on recalculation flag
        /// </summary>
        /// <param name="message"></param>
        internal static void RecalcAdjustmentAll()
        {
            if (HProcTraverse == null)
            {
                return;
            }
            var heroines = HProcTraverse.flags.lstHeroine;
            for (var i = 0; i < heroines.Count; i++)
            {
                GetController(heroines[i].chaCtrl).DoRecalc = true;
            }
            GetController(HProcTraverse.flags.player.chaCtrl).DoRecalc = true;
        }
    }
}
