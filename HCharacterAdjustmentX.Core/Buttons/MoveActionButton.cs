//
// Move action button definition
//
// Ignore Spelling: Cha

using UnityEngine;

using IDHIUtils;


namespace IDHIPlugins
{
    public struct MoveActionButton : IColorButton
    {
        #region Interface properties
        public string Text { get; set;  }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Rect Position { get; }
        #endregion

        #region Properties
        public MoveType Move { get; set; }
        public CharacterType ChaType { get; set; }
        #endregion

        #region constructor
        public MoveActionButton(
            Rect position,
            ButtonProperties b,
            CharacterType chaType)
        {
            Position = position;
            Text = b.Label;
            Move = b.MoveType;
            ChaType = chaType;
            ForegroundColor = Color.white;
            BackgroundColor = Color.gray;
        }
        #endregion

        #region Interface methods
        /// <summary>
        /// Trigger a move event
        /// </summary>
        public readonly void Process()
        {
#if DEBUG
            //_Log.Info($"[MoveActionButton.Process] {Text} Button pressed for {ChaType} ");
#endif
            MoveEvent.InvokeOnPositionMoveEvent(
                null,
                new MoveEvent.MoveEventArgs(ChaType, Move));
        }
        #endregion
    }
}
