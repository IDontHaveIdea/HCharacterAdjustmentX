//
// Buttons Properties
//
using System.Collections.Generic;

using UnityEngine;

using static IDHIPlugins.HCharaAdjustmentX;


namespace IDHIPlugins
{
    public enum Axis
    {
        UNKNOWN = -1,
        X = 0,
        Y = 1,
        Z = 2
    }

    public enum ActionType
    {
        POSITION,
        ROTATION,
        AXIS
    }

    public struct ButtonProperties
    {
        public string Label { get; set; }
        public MoveType MoveType { get; set; }
        public ActionType ActionType { get; set; }
        public bool DoubleWide { get; set; }
        public ButtonProperties(
            string label,
            MoveType moveType,
            ActionType actionType,
            bool doubleWide = false)
        {
            Label = label;
            MoveType = moveType;
            ActionType = actionType;
            DoubleWide = doubleWide;
        }
    }

    public readonly struct Move
    {
        public static readonly List<ButtonProperties> Buttons = new()
        {
            { new(            "Up", MoveType.UP, ActionType.POSITION) },
            { new(          "Down", MoveType.DOWN, ActionType.POSITION) },
            { new(          "Left", MoveType.LEFT, ActionType.POSITION) },
            { new(         "Right", MoveType.RIGHT, ActionType.POSITION) },
            { new(       "Forward", MoveType.FORWARD, ActionType.POSITION) },
            { new(          "Back", MoveType.BACK, ActionType.POSITION) },
            { new(    "Reset Move", MoveType.RESETPOSITION, ActionType.POSITION, doubleWide: true) },
#if DEBUG
            //{ new(        "Rot. +", MoveType.POSITIVEROTATION, ActionType.ROTATION) },
            //{ new(        "Rot. -", MoveType.NEGATIVEROTATION, ActionType.ROTATION) },
            //{ new(          "Axis", MoveType.AXIS, ActionType.AXIS, doubleWide: true) },
            //{ new("Reset Rotation", MoveType.RESETROTATION, ActionType.ROTATION, doubleWide: true) },
#endif
            { new(          "Save", MoveType.SAVE, ActionType.POSITION) },
            { new(          "Load", MoveType.LOAD, ActionType.POSITION) }
        };
    }

    public class AxisVector
    {
        private static readonly Dictionary<Axis, Vector3> _values = new()
        {
            {Axis.X, Vector3.right},
            {Axis.Y, Vector3.up},
            {Axis.Z, Vector3.forward}
        };

        public static readonly Dictionary<Axis, string> ColorName = new()
        {
            {Axis.X, "Red"},
            {Axis.Y, "Green"},
            {Axis.Z, "Blue"}
        };

        public static readonly Dictionary<Axis, Color>  Color = new()
        {
            {Axis.X, _SvgColor.magenta},
            {Axis.Y, _SvgColor.lime},
            {Axis.Z, _SvgColor.deepskyblue}
        };

        public static bool TryGetValue(Axis key, out Vector3 result)
        {
            return _values.TryGetValue(key, out result);
        }

        public Vector3 this[Axis key]
        {
            get { return _values[key]; }
            set { _values[key] = value; }
        }
    }
}
