//
// Character Controller
//
// Ignore Spelling: Recalc

using System.Collections.Generic;

using UnityEngine;

using KKAPI;
using KKAPI.Chara;

using IDHIUtils;


namespace IDHIPlugins
{
    public enum CharacterType
    {
        Heroine,
        Heroine3P,
        Player,
        Janitor,
        Group,
        Unknown
    }

    public partial class HCharaAdjustmentX
    {
        // Controller
        #region  Fields
        internal const string MoveDataID = "MoveData";
        internal static float _fAdjustStep = 0.01f;
        internal static float _fRotationStep = 5f;
        #endregion

        public partial class HCharaAdjustmentXController : CharaCustomFunctionController
        {
            #region Fields
            internal MoveData MoveData;
            internal List<IColorButton> ControllerButtons;
            internal int _height;
            internal float _shapeHeight;
            #endregion

            #region Properties
            public bool DoRecalc { get; set; } = true;
            public bool Moved { get; set; } = false;
            public Vector3 OriginalPosition { get; set; } = new(0, 0, 0);
            public Quaternion OriginalRotation { get; set; } = new(0, 0, 0, 0);
            public Vector3 LastMovePosition { get; set; } = new(0, 0, 0);
            public Vector3 FoundPosition { get; set; } = new(0, 0, 0);
            public Vector3 Movement { get; set; } = new(0, 0, 0);
            public Vector3 Rotation { get; set; } = new(0, 0, 0);
            public Axis CurrentAxis { get; set; } = Axis.UNKNOWN;
            public Vector3 ALMovement { get; set; } = new(0, 0, 0);
            internal CharacterType ChaType { get; set; } = CharacterType.Unknown;
            #endregion

            #region public Methods
            public void ReadData()
            {
                // Information for Player is saved in Heroine card
                if (ChaControl.sex == (byte)Sex.Male)
                {
                    return;
                }

                // Initialize MoveData if null
                MoveData ??= new(ChaControl);

                var data = GetExtendedData();

                if (data != null)
                {
                    MoveData.Load(data);
                }
                else
                {
                    var name = ChaControl.chaFile.parameter.fullname.Trim()
                            ?? string.Empty;
#if DEBUG
                    _Log.Debug($"[ReadData] [{name}] Data is null.");
#endif
                }
            }

            public void SaveData(bool clear = false)
            {
                // Information for Player is save in Heroine card
                if (ChaControl.sex == (byte)Sex.Male)
                {
                    return;
                }
                if (MoveData != null)
                {
                    if (MoveData.Count == 0)
                    {
#if DEBUG
                        var name = ChaControl.chaFile?.parameter.fullname.Trim()
                            ?? string.Empty;
                        _Log.Info($"[SaveData] [{name}] MoveData total is 0 setting "
                            + $"ExtendedData to null(Not Really!).");
#endif
                        // TODO: Make sure this is correct
                        //SetExtendedData(null);
                    }
                    else
                    {
                        SetExtendedData(MoveData.Save());
                    }
                }
#if DEBUG
                else
                {
                    var name = ChaControl.chaFile?.parameter.fullname.Trim()
                        ?? string.Empty;
                    _Log.Info($"[SaveData] [{name}] MoveData is null.");
                }
#endif
            }
            #endregion

            #region protected override Methods
            /// <summary>
            /// TODO: Need to identify 3P and Darkness scene. For now it won't be
            /// supported. Message to remember. This must be defined.
            /// </summary>
            /// <param name="currentGameMode">MainGame, Maker, Studio</param>
            protected override void OnCardBeingSaved(GameMode currentGameMode)
            {
                if (currentGameMode == GameMode.Maker)
                {
                    if (MakerInfo.InMaker)
                    {
                        return;
                    }
                }
                SaveData();
            }

            protected override void OnReload(
                GameMode currentGameMode, bool maintainState)
            {
                if (currentGameMode == GameMode.Maker)
                {
                    if (MakerInfo.InRoomMaker)
                    {
                        _Log.Message($"[{PlugInName}] [OnReload] If you load a card " +
                            $"with Info selected you may lose move information.");
                    }
                }
                if (maintainState)
                {
                    return;
                }
                ReadData();
            }

            protected override void Update()
            {
                if (HProcMonitor.Nakadashi && IsSupportedScene
                    && (ChaType != CharacterType.Unknown))
                {
                    if (DoRecalc)
                    {
                        _fAdjustStep = AdjustmentStep.Value;
                        DoRecalc = false;
                    }
                    if (ChaType == CharacterType.Heroine)
                    {
                        if (KeyHeroine.Menu.Value.IsDown())
                        {
                            _buttonsInterface[ChaType].ShowInterface =
                                !_buttonsInterface[ChaType].ShowInterface;
                        }
                    }
                    if (ChaType == CharacterType.Player)
                    {
                        if (KeyPlayer.Menu.Value.IsDown())
                        {
                            _buttonsInterface[ChaType].ShowInterface =
                                !_buttonsInterface[ChaType].ShowInterface;
                        }
                    }
                }
                base.Update();
            }
#endregion

            #region private Methods
            internal void Init(HSceneProcTraverse hSceneProc, CharacterType characterType)
            {
                ChaType = characterType;
                MoveData ??= new(ChaControl);
                SetOriginalPosition();

                _height = ChaControl.Height();
                _shapeHeight = ChaControl.ShapeValueBody(
                    ChaFileDefine.BodyShapeIdx.Height);

                var xOffset = 0f;
                var width = 62f;
                var height = 25f;

                if (characterType == CharacterType.Heroine)
                {
                    xOffset = (-((width * 2) + 2));
                }
                else if (characterType == CharacterType.Player)
                {
                    xOffset = (-(width * 4 + 2));
                }
                // TODO: To many arguments use a struct
                ControllerButtons = new ButtonsGUI(characterType, xMargin: 0f,
                    yMargin: 0.075f, width: width, height: height,
                    xOffset: xOffset).Buttons;

                // Start disabled
                enabled = false;
            }

            /// <summary>
            /// Save original position
            /// </summary>
            internal void SetOriginalPosition()
            {
                var nowHPointDataPos = HProcTraverse.nowHpointDataPos;
                var nowHPointData = HProcTraverse.nowHpointData;
                
                OriginalPosition = nowHPointDataPos;
                OriginalRotation = ChaControl.transform.rotation;
                FoundPosition = ChaControl.transform.position;
                LastMovePosition = Vector3.zero;
                Movement = Vector3.zero;
                Moved = false;
                if (OriginalPosition != FoundPosition)
                {
                    _Log.Level(BepInEx.Logging.LogLevel.Warning, $"[SetOriginalPosition] " +
                        $"Original position and found position mismatched " +
                        $"{OriginalPosition.Format()}!={FoundPosition.Format()}");
                }
            }

            /// <summary>
            /// Restore original position
            /// </summary>
            internal void ResetPosition()
            {
                var currentPosition = ChaControl.transform.position;
                // Get calling method name
                var callingMethod = Utilities.CallingMethod();
                var movement = Movement;
                var moved = Moved;

                if (OriginalPosition != Vector3.zero)
                {
                    ChaControl.transform.position = OriginalPosition;
                    Movement = Vector3.zero;
                    LastMovePosition = Vector3.zero;
                    Moved = false;

                    // Move in case ALMovement is not zero
                    if (ALMovement != Vector3.zero)
                    {
                        MoveEvent.InvokeOnPositionMoveEvent(null,
                            new MoveEvent.MoveEventArgs(
                                ChaType, MoveType.MOVE));
                    }
#if DEBUG
                    var finalPosition = ChaControl.transform.position;
                    _Log.Info($"[ResetPosition] Calling [{callingMethod}] " +
                        $"For {ChaType}\n" +
                        $"    Movement={movement.Format()}\n" +
                        $"from current={currentPosition.Format()}\n" +
                        $"          to={finalPosition.Format()}\n" +
                        $"    original={OriginalPosition.Format()}");
                    //_Log.Info($"[ResetPosition] Calling [{callingMethod}] " +
                    //    $"Reset position for {ChaType}\n" +
                    //    $"      ALMovement={ALMovement.Format()} Moved={moved}\n" +
                    //    $"        Movement={movement.Format()}\n" +
                    //    $"    from current={currentPosition.Format()}\n" +
                    //    $"              to={OriginalPosition.Format()}\n" +
                    //    $"nowHpointDataPos={_hPointPos.Format()}\n" +
                    //    $"   finalPosiiton={finalPosition.Format()}");
#endif
                }
            }

            /// <summary>
            /// Restore original position
            /// </summary>
            internal void ResetRotation()
            {
                var currentRotation = ChaControl.transform.rotation;

                // Get calling method name
                var callingMethod = Utilities.CallingMethod();
                var rotation = Rotation;

                if (OriginalRotation != new Quaternion(0, 0, 0, 0))
                {
                    ChaControl.transform.rotation = OriginalRotation;
                    Rotation = Vector3.zero;
#if DEBUG
                    var finalPosition = ChaControl.transform.rotation;
                    _Log.Info($"[ResetRotation] Calling [{callingMethod}] " +
                        $"Reset rotation for {ChaType}\n" +
                        $"  Angle Movement={rotation.Format()}\n" +
                        $"    from current={currentRotation.Format()}\n" +
                        $"              to={OriginalRotation.Format()}\n" +
                        $"  final Rotation={finalPosition.Format()}");
#endif
                }
            }
            #endregion
        }
    }
}
