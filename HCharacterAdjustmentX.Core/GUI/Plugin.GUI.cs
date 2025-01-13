// 
// Buttons interface handling
//
using System.Collections.Generic;
using System.Linq;

using KKAPI;

using UnityEngine;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class HCharaAdjustmentX
    {
        #region private fields
        // TODO: Use a properties class to have one dictionary
        internal static Dictionary<CharacterType, ButtonsInterface> _buttonsInterface =
            new()
            {
                { CharacterType.Heroine, new ButtonsInterface(CharacterType.Heroine) },
                { CharacterType.Player, new ButtonsInterface(CharacterType.Player) },
                { CharacterType.Heroine3P, new ButtonsInterface(CharacterType.Heroine3P) }
            };
        internal static Dictionary<CharacterType, List<IColorButton>> _buttons =
            new()
            {
                { CharacterType.Heroine, null },
                { CharacterType.Heroine3P, null },
                { CharacterType.Player, null }
            };
        #endregion

        #region Properties
        internal static bool ShowGroupGuide
        {
            get
            {
                return _showGroupGuide;
            }

            set
            {
                _showGroupGuide = value;
                if (value)
                {
                    _showGroupGuide = CanShow();
                }
                ToggleGroupGuideObject(_showGroupGuide);
            }
        }
        #endregion

        #region Unity methods
        private void OnGUI()
        {
            //
            // Rect(x, y, xMax, yMax)
            //
            if (_buttonsInterface[CharacterType.Heroine].ShowInterface)
            {
                // Have _buttons to show
                // TODO: check how not to execute every frame
                if (_buttons[CharacterType.Heroine].Count > 0)
                {
                    foreach (var moveButton in _buttons[CharacterType.Heroine])
                    {
                        GUI.contentColor = moveButton.ForegroundColor;
                        GUI.backgroundColor = _SvgColor.mediumorchid;
                        GUI.backgroundColor.AlphaMultiplied(1f);
                        if (GUI.Button(moveButton.Position, moveButton.Text))
                        {
                            moveButton.Process();
                        }
                    }
                }
            }

            if (_buttonsInterface[CharacterType.Player].ShowInterface)
            {
                if (_buttons[CharacterType.Player]?.Count > 0)
                {
                    foreach (var moveButton in _buttons[CharacterType.Player])
                    {
                        GUI.contentColor = moveButton.ForegroundColor;
                        GUI.backgroundColor = _SvgColor.blue;
                        GUI.backgroundColor.AlphaMultiplied(1f);
                        if (GUI.Button(moveButton.Position, moveButton.Text))
                        {
                            moveButton.Process();
                        }
                    }
                }
            }
        }

#if DEBUG
        private void Update()
        {
            if (GroupGuide.Value.IsDown())
            {
                ShowGroupGuide = !ShowGroupGuide;
            }
        }
#endif
        #endregion

        #region private methods
        private static void SetupInterface(CharacterType chaType)
        {
            var _chaControl = chaType switch
            {
                CharacterType.Heroine => Heroines[0],
                CharacterType.Player => Player,
                CharacterType.Heroine3P => Heroines[1],
                _ => null,
            };

            if (_chaControl != null)
            {
                if (_buttons[chaType]?.Count > 0)
                {
                    return;
                }
            }

            _buttons[chaType] = GetControllerByType(chaType).ControllerButtons.ToList();
        }

        /// <summary>
        /// Toogle the group guide object on/off
        /// </summary>
        public static void ToggleGroupGuideObject(bool state)
        {
#if DEBUG
            //_Log.Warning($"[ToggleGroupGuideObject] Group Guide set to={state}");
#endif
            HProcTraverse.sprite.axis.tglDraw.isOn = state;
            HProcTraverse.sprite.MoveAxisDraw(HProcTraverse.sprite.axis.tglDraw.isOn);
            HProcTraverse.guideObject.gameObject.SetActive(state);
        }

        /// <summary>
        /// Conditions when the group guide should be hidden
        /// </summary>
        /// <returns></returns>
        private static bool CanShow()
        {
            // character is not showing
            if (!Player.visibleAll)
            {
                return false;
            }
            // Some pop up dialog on
            if (SceneApi.GetIsOverlap())
            {
                return false;
            }
            // scene still loading or start to exit
            if (SceneApi.GetIsNowLoadingFade())
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
