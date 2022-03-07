﻿using System.Collections.Generic;

using UnityEngine;
using HSceneUtility;

using KKAPI;
using KKAPI.Chara;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class HCharaAdjustmentX
    {
        // Controller
        static internal Vector3 _newPosition = Vector3.zero;
        static internal Vector3 _clAdjustUnit = Vector3.zero;
        static internal Vector3 _lrAdjustUnit = Vector3.zero;
        static internal Vector3 _udAdjustUnit = new(0, 0.01f, 0);
        static internal float _fAdjustStep = 0.01f;

        public partial class HCharaAdjusmentXController : CharaCustomFunctionController
        {
            #region private fields
            internal CharacterType _chaType = CharacterType.Unknown;
            internal Vector3 _lastMovePosition = new(0, 0, 0);
            internal Vector3 _originalPosition = new(0, 0, 0);
            #endregion

            #region public fields
            public List<MoveActionButton> buttons;
            public enum CharacterType { Heroine, Heroine3P, Player, Janitor, Group, Unknown }
            #endregion

            #region properties
            public bool DoRecalc { get; set; } = true;
            #endregion

            #region private methods
            internal void Init(HSceneProc hSceneProc, CharacterType characterType)
            {
                _Log.Info($"SHCA0002: Initialization for {characterType}");
                _chaType = characterType;
                CreateGuideObject(hSceneProc, characterType);
                SetOriginalPosition();
                if (characterType == CharacterType.Heroine)
                {
                    buttons = new ButtonsGUI(characterType, xMargin: 0f, yMargin: 0.08f,
                        width: 57f, height: 25f, xOffset: (-124f)).Buttons;
                }
                else if (characterType == CharacterType.Player)
                {
                    buttons = new ButtonsGUI(characterType, xMargin: 0f, yMargin: 0.08f,
                        width: 57f, height: 25f, xOffset: (-240f)).Buttons;
                }
                // Start disabled
                enabled = false;
            }

            /// <summary>
            /// Save original position
            /// </summary>
            internal void SetOriginalPosition()
            {
                _originalPosition = ChaControl.transform.position;
            }
            #endregion

            #region public methods
            /// <summary>
            /// Restore original position
            /// </summary>
            public void ResetPosition()
            {
                if (_originalPosition != Vector3.zero)
                {
                    if (_guideObject.gameObject.activeInHierarchy)
                    {
                        _guideObject.amount.position = _originalPosition;
                    }
                    else
                    {
                        ChaControl.transform.position = _originalPosition;
                    }
#if DEBUG
                    _Log.Info($"SHCA0003: Reset position for {_chaType}");
#endif
                }
            }
            #endregion

            #region unity methods
            /// <summary>
            /// TODO: Save and read information about any movement done.  
            /// Need to identify 3P and Darkness scene.  
            /// For now it won't be supported. Message to remember.
            /// This must be defined.
            /// </summary>
            /// <param name="currentGameMode"></param>
            override protected void OnCardBeingSaved(GameMode currentGameMode)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            override protected void Update()
            {
                if (HProcScene.Nakadashi && IsSupportedScene && (_chaType != CharacterType.Unknown))
                {
                    if (_guideObject)
                    {
                        if (_guideObject.gameObject.activeInHierarchy)
                        {
                            ChaControl.transform.position = _guideObject.amount.position;
                        }
                        else
                        {
                            _guideObject.amount.position = ChaControl.transform.position;
                        }
                    }
                    if (DoRecalc)
                    {
                        _fAdjustStep = cfgAdjustmentStep.Value;
                        _clAdjustUnit = ChaControl.transform.forward * _fAdjustStep;
                        _lrAdjustUnit = ChaControl.transform.right * _fAdjustStep;
                        _udAdjustUnit.y = _fAdjustStep;
                        DoRecalc = false;
#if DEBUG
                        _Log.Info($"SHCA0036: Calculation for {_chaType} " +
                            $"with Step {_fAdjustStep} - " +
                            $"TF ({ChaControl.transform.forward.x }, " +
                            $"{ChaControl.transform.forward.y}, " +
                            $"{ChaControl.transform.forward.z}), " +
                            $"TF ({ChaControl.transform.position.x }, " +
                            $"{ChaControl.transform.position.y}, " +
                            $"{ChaControl.transform.position.z}), " +
                            $"CL ({_clAdjustUnit.x }, {_clAdjustUnit.y}, {_clAdjustUnit.z}), " +
                            $"LR ({_lrAdjustUnit.x }, {_lrAdjustUnit.y}, {_lrAdjustUnit.z})");
#endif
                    }
                    if (_chaType == CharacterType.Heroine)
                    {
                        if (KeyHeroine.GuideObject.Value.IsDown())
                        {
                            ToggleGuideObject();
                        }

                        if (KeyHeroine.Menu.Value.IsDown())
                        {
#if DEBUG
                            _Log.Info($"[SHCAdjustController] Toggle interface for {_chaType} " +
                                $"current {_buttonsInterface[_chaType].ShowInterface}");
#endif
                            _buttonsInterface[_chaType].ShowInterface =
                                !_buttonsInterface[_chaType].ShowInterface;
                        }
                    }
                    if (_chaType == CharacterType.Player)
                    {
                        if (KeyPlayer.Menu.Value.IsDown())
                        {
#if DEBUG
                            _Log.Info($"[SHCAdjustController] Toggle interface for {_chaType} " +
                                $"current {_buttonsInterface[_chaType].ShowInterface}");
#endif
                            _buttonsInterface[_chaType].ShowInterface =
                                !_buttonsInterface[_chaType].ShowInterface;
                        }
                    }
                }
                base.Update();
            }
            #endregion
        }
    }
}
