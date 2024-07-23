//
// ActionButton
//
// Ignore Spelling: Cha

using UnityEngine;

using IDHIUtils;

using static IDHIPlugins.HCharaAdjustmentX;


namespace IDHIPlugins
{
    public struct AxisButton : IColorButton
    {
        private Axis _currentAxis;
        private readonly HCharaAdjustmentXController _controller;

        #region Interface Properties
        public readonly string Text => $"Axis: {_currentAxis} ({AxisVector.ColorName[_currentAxis]})";
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Rect Position { get; }
        #endregion

        #region Properties
        public Axis Axis
        {
            readonly get
            {
                return _currentAxis;
            }

            private set
            {
                _currentAxis = value;
                _controller.CurrentAxis = value;
                ForegroundColor = AxisVector.Color[value];
#if DEBUG
                //_Log.Warning($"[Axis] _currentAxis={_currentAxis} for ChaType={ChaType} " +
                //    $"set to={_controller.CurrentAxis}");
#endif
            }
        }
        public CharacterType ChaType { get; set; }
#endregion

        #region Constructors
        public AxisButton(
            Rect position,
            CharacterType chaType
            )
        {
            ChaType = chaType;
            // Used in Axis property
            _controller = GetControllerByType(chaType);
            Position = position;
            Axis = Axis.Y;
            BackgroundColor = Color.gray;

#if DEBUG
            //_Log.Warning($"[Axis] Constructor for {chaType} set ChaType={ChaType} " +
            //    $"set axis to={_controller.CurrentAxis} controller " +
            //    $"type={_controller.ChaType}");
#endif
        }
#endregion

        #region Interface public methods
        public void Process()
        {
#if DEBUG
            var axis = _controller.CurrentAxis;
#endif
            Axis = (Axis)(((int)_currentAxis + 1) % 3);
#if DEBUG
            //_Log.Info($"[Axis.Process] Change axis from {axis} to {Axis}.");
#endif
        }
        #endregion
    }
}
