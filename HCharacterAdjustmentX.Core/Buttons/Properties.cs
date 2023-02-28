﻿//
// Buttons Properties
//
using System.Collections.Generic;

using UnityEngine;


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
            { new(     "Up", MoveType.UP, ActionType.POSITION) },
            { new(   "Down", MoveType.DOWN, ActionType.POSITION) },
            { new(   "Left", MoveType.LEFT, ActionType.POSITION) },
            { new(  "Right", MoveType.RIGHT, ActionType.POSITION) },
            { new("Forward", MoveType.FORWARD, ActionType.POSITION) },
            { new(   "Back", MoveType.BACK, ActionType.POSITION) },
            { new( "Rot. +", MoveType.POSITIVEROTATION, ActionType.ROTATION) },
            { new( "Rot. -", MoveType.NEGATIVEROTATION, ActionType.ROTATION) },
            { new(   "Axis", MoveType.AXIS, ActionType.AXIS, true) },
            { new(   "Save", MoveType.SAVE, ActionType.POSITION) },
            { new(   "Load", MoveType.LOAD, ActionType.POSITION) },
            { new("R. Move", MoveType.RESETPOSITION, ActionType.POSITION) },
            { new("R. Rot.", MoveType.RESETROTATION, ActionType.ROTATION) }
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