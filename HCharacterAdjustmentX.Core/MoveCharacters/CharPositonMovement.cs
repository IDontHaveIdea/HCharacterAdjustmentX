﻿using System;
using System.Collections.Generic;

using UnityEngine;

using BepInEx.Logging;

using KKAPI.Utilities;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class HCharaAdjustmentX
    {
        /// <summary>
        /// Take care of movement requests
        /// </summary>
        internal class CharPositionMovement
        {
            #region private fields
            internal static ChaControl _chaControl;
            internal static HCharaAdjusmentXController _controller;
            internal static HCharaAdjusmentXController _controllerPlayer;
            internal static HCharaAdjusmentXController _controllerHeroine;
            internal static bool _doShortcutMove;
            #endregion

            #region Properties
            internal static Axis CurrentAxis { get; set; }
            #endregion
            /// <summary>
            /// Check for configured key shortcuts and execute the type of movement
            /// desired
            ///
            /// TODO:
            ///     Classify positions for correct relative left/right
            ///     and forward/backwards
            /// </summary>
            /// <param name="chaType">Character type</param>
            /// <param name="moveType">Move triggered</param>
            /// <returns></returns>
            public static bool Move(CharacterType chaType, MoveType moveType)
            {
                _doShortcutMove = false;

                _controller = GetControllerByType(chaType);
                _chaControl = _controller.ChaControl;

                if (_chaControl == null)
                {
                    throw new NullReferenceException($"HCAX0040: HProc instance " +
                        $"invalid cannot get ChaControl for {chaType}.");
                }

                //_pathFemaleBase = _hprocInstance.flags.nowAnimationInfo
                //    .pathFemaleBase.assetpath;

                // Normal button press
                var originalPosition = _controller.OriginalPosition;
                var fullMovement = _controller.Movement;
                if (!_controller.Moved)
                {
                    if ((_controller.Movement == Vector3.zero)
                        && (_controller.ALMovement != Vector3.zero))
                    {
                        fullMovement = _controller.ALMovement;
                    }
                }
                
                switch (moveType)
                {
                    case MoveType.RESETMOVE:
                        _controller.ResetPosition();
                        break;

                    case MoveType.UP:
                        fullMovement += (Vector3.up * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.DOWN:
                        fullMovement += (Vector3.down * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.RIGHT:
                        fullMovement += (Vector3.right * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.LEFT:
                        fullMovement += (Vector3.left * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.FORWARD:
                        fullMovement += (Vector3.forward * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.BACK:
                        fullMovement += (Vector3.back * _fAdjustStep);
                        _doShortcutMove = true;
                        break;
                    case MoveType.SAVE:
                        if (!_animationKey.IsNullOrEmpty())
                        {
                            var controllerPlayer =
                                GetControllerByType(CharacterType.Player);
                            var controllerHeroine =
                                GetControllerByType(CharacterType.Heroine);

                            var positions = new Dictionary<CharacterType, PositionData>
                            {
                                [CharacterType.Heroine] =
                                    new(controllerHeroine.Movement, Vector3.zero),
                                [CharacterType.Player] =
                                    new(controllerPlayer.Movement, Vector3.zero)
                            };

                            controllerHeroine.MoveData[_animationKey] = positions;
                        }
                        _doShortcutMove = false;
                        _controller.SaveData();
                        break;
                    case MoveType.LOAD:
                        if (!_animationKey.IsNullOrEmpty())
                        {
                            var controllerHeroine =
                                GetControllerByType(CharacterType.Heroine);
                            if (controllerHeroine.MoveData.Data
                                .TryGetValue(_animationKey, out var positions))
                            {
                                fullMovement = positions[chaType].Position;
                            }
                        }
                        _doShortcutMove = true;
                        break;
                    case MoveType.ROTP:
                        _doShortcutMove = false;
                        break;
                    case MoveType.ROTN:
                        _doShortcutMove = false;
                        break;
                    // Execute a move event with current parameters used
                    // for automatic position adjustment
                    case MoveType.MOVE:
                        _doShortcutMove = true;
                        break;
                    default:
                        _doShortcutMove = false;
                        break;
                }
                if (_doShortcutMove)
                {
                    if (!_controller.Moved)
                    {
                        _controller.Moved = true;
                    }
                    var newPosition = RecalcPosition(
                        _chaControl, originalPosition, fullMovement);
                    _doShortcutMove = false;
                    _chaControl.transform.position = newPosition;
                    _controller.Movement = fullMovement;
                    _controller.LastMovePosition = newPosition;
                }
                return _doShortcutMove;
            }

            internal static Vector3 RecalcPosition(
                ChaControl chaControl,
                Vector3 original,
                Vector3 fullMove)
            {
                try
                {
                    var currentPosition = chaControl.transform.position;

                    var fullNewPosition = original + fullMove
                        .MovementTransform(chaControl.transform);

                    if (DebugInfo.Value)
                    {
                        _Log.Debug($"[RecalcPosition] Move {chaControl.name}\n" +
                            $" original position {original.Format()}\n" +
                            $"  current position {currentPosition.Format()}\n" +
                            $"    move by vector {fullMove.Format()}\n" +
                            $"position by vector {fullNewPosition.Format()}");
                    }
                    return fullNewPosition;
                }
                catch (Exception e)
                {
                    _Log.Level(LogLevel.Error, $"HCAX0048: Cannot adjust position " +
                        $"{chaControl.name} - {e}.");
                }
                return Vector3.zero;
            }
        }

        #region Backup
        internal class CharPositionMovementReference
        {
            #region private fields
            internal static ChaControl _chaControl;
            internal static HCharaAdjusmentXController _controller;
            internal static HCharaAdjusmentXController _controllerPlayer;
            internal static HCharaAdjusmentXController _controllerHeroine;
            internal static bool _doShortcutMove;
            #endregion

            #region Properties
            internal static Axis CurrentAxis { get; set; }
            #endregion
            /// <summary>
            /// Check for configured key shortcuts and execute the type of movement
            /// desired
            ///
            /// TODO:
            ///     Classify positions for correct relative left/right
            ///     and forward/backwards
            /// </summary>
            /// <param name="chaType">Character type</param>
            /// <param name="moveType">Move triggered</param>
            /// <returns></returns>
            public static bool Move(CharacterType chaType, MoveType moveType)
            {
                _doShortcutMove = false;

                if (chaType == CharacterType.Player)
                {
                    _chaControl = _hprocInstance.flags.player?.chaCtrl;
                    //_positiveMove = false;
                }
                else
                {
                    _chaControl = _hprocInstance.flags.lstHeroine[(int)chaType]?.chaCtrl;
                    //_positiveMove = true;
                }
                if (_chaControl == null)
                {
                    throw new NullReferenceException($"HCAX0040: HProc instance " +
                        $"invalid.");
                }

                _controller = GetControllerByType(chaType);

                // Normal button press
                var originalPosition = _controller.OriginalPosition;
                var movement = _controller.Movement;
                var fullMovement = _controller.Movement;
                if (!_controller.Moved)
                {
                    if ((_controller.Movement == Vector3.zero)
                        && (_controller.ALMovement != Vector3.zero))
                    {
                        fullMovement = _controller.ALMovement;
                        movement = _controller.ALMovement;
                    }
                }

                Vector3 newPosition = new(0, 0, 0);
                switch (moveType)
                {
                    case MoveType.RESETMOVE:
                        _controller.ResetPosition();
                        break;

                    case MoveType.UP:
                        movement.y += _fAdjustStep;
                        fullMovement += (Vector3.up * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.DOWN:
                        movement.y -= _fAdjustStep;
                        fullMovement += (Vector3.down * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.RIGHT:
                        if (chaType == CharacterType.Player)
                        {
                            movement.x -= _fAdjustStep;
                        }
                        else
                        {
                            movement.x += _fAdjustStep;
                        }
                        fullMovement += (Vector3.right * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.LEFT:
                        if (chaType == CharacterType.Player)
                        {
                            movement.x += _fAdjustStep;
                        }
                        else
                        {
                            movement.x -= _fAdjustStep;

                        }
                        fullMovement += (Vector3.left * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.FORWARD:
                        if (chaType == CharacterType.Player)
                        {
                            movement.z -= _fAdjustStep;
                        }
                        else
                        {
                            movement.z += _fAdjustStep;
                        }
                        fullMovement += (Vector3.forward * _fAdjustStep);
                        _doShortcutMove = true;
                        break;

                    case MoveType.BACK:
                        if (chaType == CharacterType.Player)
                        {
                            movement.z += _fAdjustStep;
                        }
                        else
                        {
                            movement.z -= _fAdjustStep;
                        }
                        fullMovement += (Vector3.back * _fAdjustStep);
                        _doShortcutMove = true;
                        break;
                    case MoveType.SAVE:
                        if (!_animationKey.IsNullOrEmpty())
                        {
                            _controllerPlayer =
                                GetControllerByType(CharacterType.Player);
                            _controllerHeroine =
                                GetControllerByType(CharacterType.Heroine);

                            var positions = new Dictionary<CharacterType, PositionData> {
                                [chaType] = new(_controllerHeroine.Movement,
                                    Vector3.zero)
                            };

                            positions[CharacterType.Player] =
                                new(_controllerPlayer.Movement, Vector3.zero);
                            positions[CharacterType.Heroine] =
                                new(_controllerHeroine.Movement, Vector3.zero);
                            _controller.MoveData[_animationKey] = positions;
                        }
                        _doShortcutMove = false;
                        _controller.SaveData();
                        break;
                    case MoveType.LOAD:
                        if (!_animationKey.IsNullOrEmpty())
                        {
                            _controller.MoveData.Data.TryGetValue(_animationKey,
                                out var position);
                            if (position != null)
                            {
                                // Use TryGetValue
                                movement = position[chaType]
                                   .Position;
                            }
                        }
                        _doShortcutMove = true;
                        break;
                    case MoveType.ROTP:
                        _doShortcutMove = false;
                        break;
                    case MoveType.ROTN:
                        _doShortcutMove = false;
                        break;
                    // Execute a move event with current parameters used
                    // for automatic position adjustment
                    case MoveType.MOVE:
                        _doShortcutMove = true;
                        break;
                    default:
                        _doShortcutMove = false;
                        break;
                }
                if (_doShortcutMove)
                {
                    //var tmp = _chaControl.transform.position;
                    if (!_controller.Moved)
                    {
                        _controller.Moved = true;
                    }
                    newPosition = RecalcPosition(
                        _chaControl, originalPosition, movement, fullMovement);
                    _doShortcutMove = false;
                    _chaControl.transform.position = newPosition;
                    _controller.Movement = fullMovement;
                    _controller.LastMovePosition = newPosition;
                    return true;
                }
                return false;
            }

            internal static Vector3 RecalcPosition(
                ChaControl chaControl,
                Vector3 original,
                Vector3 move,
                Vector3 fullMove)
            {
                try
                {
                    var right = chaControl.transform.right * move.x;
                    var up = chaControl.transform.up * move.y;
                    var forward = chaControl.transform.forward * move.z;

                    var currentPosition = chaControl.transform.position;

                    var newPosition = original;
                    var fullNewPosition = original + fullMove
                        .MovementTransform(chaControl.transform);

                    newPosition += up;
                    newPosition += right;
                    newPosition += forward;

                    if (DebugInfo.Value)
                    {
                        _Log.Debug($"[RecalcPosition] Move {chaControl.name}\n" +
                            $" original position {original.Format()}\n" +
                            $"  current position {currentPosition.Format()}\n" +
                            $"      move by axis {move.Format()}\n" +
                            $"    move by vector {fullMove.Format()}\n" +
                            $"           right x {right.Format()}\n" +
                            $"              up y {up.Format()}\n" +
                            $"         forward z {forward.Format()}\n" +
                            $"  position by axis {newPosition.Format()}\n" +
                            $"position by vector {fullNewPosition.Format()}");
                    }
                    return newPosition;
                }
                catch (Exception e)
                {
                    _Log.Level(LogLevel.Error, $"HCAX0048: Cannot adjust position " +
                        $"{chaControl.name} - {e}.");
                }
                return Vector3.zero;
            }

            internal static string Translate(string name)
            {
                if (!TranslationHelper.TryTranslate(name, out var tmp))
                {
                    return name;
                }

                return tmp;
            }

            internal static readonly Func<bool, Vector3, Vector3, Vector3> setDirection
                = (sign, trans, adjustment) =>
                {
                    var tmp = new Vector3(0, 0, 0);
                    if (sign)
                    {
                        tmp = (trans + adjustment);
                        return tmp;
                    }
                    tmp = trans - adjustment;
                    return tmp;
                };
        }
        #endregion
    }
}
