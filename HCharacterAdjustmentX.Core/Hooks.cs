//
// Hooks for HCharaAdjustmentX
//

using UnityEngine;

using H;

using HarmonyLib;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class HCharaAdjustmentX
    {
        // Hooks
        #region Fields
        internal static Harmony _hookInstance;
        internal static HFlag.EMode _mode;
        internal static bool _MovePerformed = false;
        internal static string _hPointName = string.Empty;
        internal static Vector3 _hPointPos = Vector3.zero;
        #endregion

        #region Properties
        internal static string AnimationKey { get; set; }
        internal static object HProcObject { get; set; }
        internal static HSceneProc HPprocInstance { get; set; }
        internal static HSceneProcTraverse HProcTraverse { get; set; }
        #endregion

        internal partial class Hooks
        {
            /// <summary>
            /// Patch system and save patch instance
            /// </summary>
            internal static void Init()
            {
                _hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            /// <summary>
            /// Set the new original position when changing positions via the
            /// H point picker scene
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeCategory))]
            private static void ChangeCategoryPostfix(HPointData _data, int _category)
            {
                if (!IsSupportedScene)
                {
                    return;
                }
#if DEBUG
                // Get calling method name
                var callingMethod = Utilities.CallingMethod();
                //_Log.Warning($"[{callingMethod}] Calling ChangeCategoryPostfix");
#endif
                PlugInUtils.SetOriginalPositionAll();
                PlugInUtils.RecalcAdjustmentAll();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void ChangeAnimatorPrefix(
                object __instance,
                HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (_nextAinmInfo == null)
                {
                    return;
                }
#if DEBUG
                // This hook is executed before HProcMonitor runs the first time
                // HSceneProcMonin finish loading event to get nowHpointData the
                // field needs to be initialized when null
                //HProcTraverse ??= new(__instance);

                // Get calling method name
                //try
                //{
                    //var nowHPointDataPos = HProcTraverse.nowHpointDataPos;
                    //var nowHPointData = HProcTraverse.nowHpointData;
                    //_hPointName = nowHPointData;
                    //_hPointPos = nowHPointDataPos;
                    //var callingMethod = Utilities.CallingMethod();
                    //_Log.Info($"[ChangeAnimatorPrefix] Calling [{callingMethod}] " +
                    //    $"Position Name={nowHPointData} Position " +
                    //    $"Point={nowHPointDataPos.Format()}");
                //}
                //catch
                //{
                //    var callingMethod = Utilities.CallingMethod();
                //    _Log.Error($"[ChangeAnimatorPrefix] Calling [{callingMethod}] " +
                //        $"ChangeAnimatorPrefix");
                //}
#endif
                AnimationKey = "";
                AnimationKey = PlugInUtils.GetAnimationKey(_nextAinmInfo);
                PlugInUtils.SetALMove(_nextAinmInfo);
                PlugInUtils.ResetPositionAll();
            }

            /// <summary>
            /// Set the new original position when changing positions not using
            /// the H point picker
            /// </summary>
            /// <param name="_nextAinmInfo"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void ChangeAnimatorPostfix(
                HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (_nextAinmInfo == null)
                {
                    return;
                }
#if DEBUG
                //try
                //{
                //    var nowHPointDataPos = HProcTraverse.nowHpointDataPos;
                //    var nowHPointData = HProcTraverse.nowHpointData;
                //    nowHPointData ??= "null";
                //    if (nowHPointDataPos == null)
                //    {
                //        nowHPointDataPos = new Vector3(-1, -1, -1);
                //    }

                //    var callingMethod = Utilities.CallingMethod();
                //    _Log.Info($"[ChangeAnimatorPostfix] Calling [{callingMethod}] " +
                //        $"Position Name={nowHPointData} Position " +
                //        $"Point={nowHPointDataPos}");
                //}
                //catch
                //{
#if KKS
                //    var nowHPointDataPos = HPointInfo.InitialPosition;
                //    var nowHPointData = HPointInfo.InitialPositionName;
                    // Get calling method name
                //    var callingMethod = Utilities.CallingMethod();
                //    _Log.Error($"[ChangeAnimatorPostfix] Calling [{callingMethod}] " +
                //        $"Error: Position Name={nowHPointData} Position " +
                //        $"Point={nowHPointDataPos}");
#else
                //    var callingMethod = Utilities.CallingMethod();
                //    _Log.Warning($"[{callingMethod}] Calling ChangeAnimatorPrefix");

#endif
                //}
#endif
                AnimationKey = PlugInUtils.GetAnimationKey(_nextAinmInfo);
                PlugInUtils.SetMode(_nextAinmInfo.mode);
                PlugInUtils.SetALMove(_nextAinmInfo);
                PlugInUtils.SetOriginalPositionAll();
                PlugInUtils.RecalcAdjustmentAll();
                PlugInUtils.InitialPosition();
            }
        }
    }
}
