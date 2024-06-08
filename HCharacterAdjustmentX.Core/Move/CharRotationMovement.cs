// Ignore Spelling: cha

using System;

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
        internal class CharRotationMovement
        {
            #region private fields
            internal static ChaControl _chaControl;
            internal static HCharaAdjustmentXController _controller;
            internal static bool _doAngleMove;
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
                _doAngleMove = false;

                _controller = GetControllerByType(chaType);
                _chaControl = _controller.ChaControl;

                // Normal button press
                var originalRotation = _controller.OriginalRotation;
                var eulerRotations = _controller.Rotation;
                int index;
                float currentAngle;
                float newAngle;
#if DEBUG
                _Log.Warning($"[CharRotationMovement.Move] Called for {chaType} to {moveType} eulerAngles={eulerRotations}");
#endif
                switch (moveType)
                {
                    case MoveType.RESETROTATION:
                        _controller.ResetRotation();
                        _doAngleMove = false;
                        break;
                    case MoveType.POSITIVEROTATION:
                        index = (int)_controller.CurrentAxis;
                        currentAngle = eulerRotations[index];
                        newAngle = Modulo(currentAngle + _fRotationStep, 360);
                        //newAngle = currentAngle + _fRotationStep;
                        eulerRotations[index] = newAngle;
                        _doAngleMove = true;
                        break;
                    case MoveType.NEGATIVEROTATION:
                        index = (int)_controller.CurrentAxis;
                        currentAngle = eulerRotations[index];
                        newAngle = Modulo(currentAngle - _fRotationStep, 360);
                        //newAngle = currentAngle - _fRotationStep;
                        eulerRotations[index] = newAngle;
                        _doAngleMove = true;
                        break;
                }

                if (_doAngleMove)
                {
                    var newRotation = RecalcRotation(
                        _chaControl, originalRotation, eulerRotations);
                    _controller.Rotation = eulerRotations;
                    if (newRotation != new Quaternion(0, 0, 0, 0))
                    {
                        _chaControl.transform.rotation = originalRotation;
                        _chaControl.transform.rotation *= newRotation;
                    }
                    return true;
                }
                return false;
            }

            internal static Quaternion RecalcRotation(
                ChaControl chaControl,
                Quaternion original,
                Vector3 eulerRotations)
            {
                try
                {
                    var currentRotation = chaControl.transform.rotation;

                    var newRotation = Quaternion.Euler(eulerRotations);
                    

                    if (DebugInfo.Value)
                    {
                        _Log.Debug($"[RecalcPosition] Move {chaControl.name}\n" +
                            $"     rotation vector {eulerRotations.Format()}\n" +
                            $"   original rotation {original.Format()}\n" +
                            $"    current position {currentRotation.Format()}\n" +
                            $"        new position {newRotation.Format()}");
                    }
                    return newRotation;
                }
                catch (Exception e)
                {
                    _Log.Level(LogLevel.Error, $"[RecalcRotation] Cannot adjust " +
                        $"position {chaControl.name} - {e}.");
                }
                return new Quaternion(0, 0, 0, 0);
            }

            //private static float mod(float a, float n)
            //{
            //    return (float)(a - n * Math.Floor(a / n));
            //}

            internal static readonly Func<float, float, float> Modulo =
                (a, n) =>
            {
                return (float)(a - n * Math.Floor(a / n));
            };
        }
    }
}
