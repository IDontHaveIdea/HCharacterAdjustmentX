// Ignore Spelling: cha

using System;
using System.Collections.Generic;

using UnityEngine;

using BepInEx.Logging;

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
            internal static HCharaAdjustmentXController _controller;
            internal static bool _doPositionMove;
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
                _doPositionMove = false;

                _controller = GetControllerByType(chaType);
                _chaControl = _controller.ChaControl;

                if (_chaControl == null)
                {
                    throw new NullReferenceException($"HCAX0040: HProc instance " +
                        $"invalid cannot get ChaControl for {chaType}.");
                }

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
                    case MoveType.RESETPOSITION:
                        _controller.ResetPosition();
                        _doPositionMove = false;
                        break;

                    case MoveType.UP:
                        fullMovement += (Vector3.up * _fAdjustStep);
                        _doPositionMove = true;
                        break;

                    case MoveType.DOWN:
                        fullMovement += (Vector3.down * _fAdjustStep);
                        _doPositionMove = true;
                        break;

                    case MoveType.RIGHT:
                        fullMovement += (Vector3.right * _fAdjustStep);
                        _doPositionMove = true;
                        break;

                    case MoveType.LEFT:
                        fullMovement += (Vector3.left * _fAdjustStep);
                        _doPositionMove = true;
                        break;

                    case MoveType.FORWARD:
                        fullMovement += (Vector3.forward * _fAdjustStep);
                        _doPositionMove = true;
                        break;

                    case MoveType.BACK:
                        fullMovement += (Vector3.back * _fAdjustStep);
                        _doPositionMove = true;
                        break;
                    case MoveType.SAVE:
                        if (!AnimationKey.IsNullOrEmpty())
                        {
                            var controllerPlayer =
                                GetControllerByType(CharacterType.Player);
                            var controllerHeroine =
                                GetControllerByType(CharacterType.Heroine);

                            var positions = new Dictionary<CharacterType, PositionData>
                            {
                                [CharacterType.Heroine] =
                                    new(controllerHeroine.Movement,
                                        controllerHeroine.Rotation),
                                [CharacterType.Player] =
                                    new(controllerPlayer.Movement,
                                        controllerPlayer.Rotation)
                            };

                            if (!PositionsEmpty(positions))
                            {
                                controllerHeroine.MoveData[AnimationKey] = positions;
                            }
                            else
                            {
                                controllerHeroine.MoveData.Remove(AnimationKey);
                            }
                            controllerHeroine.SaveData();
                        }
                        _doPositionMove = false;
                        break;
                    case MoveType.LOAD:
                        if (!AnimationKey.IsNullOrEmpty())
                        {
                            var controllerHeroine =
                                GetControllerByType(CharacterType.Heroine);
                            if (controllerHeroine.MoveData
                                .TryGetValue(AnimationKey, out var positions))
                            {
                                fullMovement = positions[chaType].Position;
                            }
                        }
                        _doPositionMove = true;
                        break;
                    case MoveType.POSITIVEROTATION:
                        _doPositionMove = false;
                        break;
                    case MoveType.NEGATIVEROTATION:
                        _doPositionMove = false;
                        break;
                    // Execute a move event with current parameters used
                    // for automatic position adjustment
                    case MoveType.MOVE:
                        _doPositionMove = true;
                        break;
                    default:
                        _doPositionMove = false;
                        break;
                }
                if (_doPositionMove)
                {
                    if (!_controller.Moved)
                    {
                        _controller.Moved = true;
                    }
                    var newPosition = RecalcPosition(
                        _chaControl, originalPosition, fullMovement);
                    _chaControl.transform.position = originalPosition;
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
                Vector3 fullMove)
            {
                try
                {
                    var currentPosition = chaControl.transform.position;
                    var move = fullMove;
                    var transformMove = fullMove
                        .MovementTransform(chaControl.transform);
                    var calcPositon = original + move;
                    var newPosition = original + transformMove;
                    var diffVector = newPosition - original;
                        

                    if (DebugInfo.Value)
                    {
                        _Log.Debug($"[RecalcPosition] For {chaControl.name}\n" +
                            $" Movement={fullMove.Format()} " +
                            $"transform={transformMove.Format()}\n" +
                            $"     from={original.Format()}\n" +
                            $"       to={newPosition.Format()}\n" +
                            $" original={original.Format()}");
                        //_Log.Debug($"[RecalcPosition] Move {chaControl.name}\n" +
                        //    $"          forward is {Vector3.forward}\n" +
                        //    $"transform forward is {chaControl.transform.forward}\n" +
                        //    $"   original position {original.Format()}\n" +
                        //    $"    current position {currentPosition.Format()}\n" +
                        //    $"         move vector {fullMove.Format()}\n" +
                        //    $"   trans. move vector{transformMove.Format()}\n" +
                        //    $"    diff. move vector{diffVector.Format()}\n" +
                        //    $"  normal calc vector {calcPositon.Format()}\n" +
                        //    $"        new position {newPosition.Format()}\n" +
                        //    $"            rotation {chaControl.transform.rotation.Format()}");
                    }
                    return newPosition;
                }
                catch (Exception e)
                {
                    _Log.Level(LogLevel.Error, $"[RecalcPosition] Cannot adjust " +
                        $"position {chaControl.name} - {e}.");
                }
                return Vector3.zero;
            }

            private static bool PositionsEmpty(Dictionary<CharacterType, PositionData> positions)
            {
                foreach ( var position in positions )
                {
                    if (position.Value.Position != Vector3.zero)
                    {
                        return false;
                    }
                    if (position.Value.Rotation != Vector3.zero)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
