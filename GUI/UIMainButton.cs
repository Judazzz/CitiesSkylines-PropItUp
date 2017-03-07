using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace PropItUp.GUI
{
    public class UIMainButton : UIButton
    {
        public static UIMainButton instance;
        private bool dragging = false;

        public override void Start()
        {
            base.Start();
            instance = this;

            const int buttonSize = 36;
            UITextureAtlas toggleButtonAtlas = null;
            string PIU = "PropItUp";

            //  Positioned relative to Freecamera Button:
            var freeCameraButton = UIView.GetAView().FindUIComponent<UIButton>("Freecamera");
            verticalAlignment = UIVerticalAlignment.Middle;
            //  
            if (PropItUpTool.config.buttonposition.y == -9999)
            {
                absolutePosition = new Vector2(freeCameraButton.absolutePosition.x - (4 * buttonSize) - 5, freeCameraButton.absolutePosition.y);
            }
            else
            {
                absolutePosition = PropItUpTool.config.buttonposition;
            }
            //  
            size = new Vector2(36f, 36f);
            playAudioEvents = true;
            tooltip = $"Prop it Up! " + Mod.version;
            //  Create custom atlas:
            if (toggleButtonAtlas == null)
            {
                toggleButtonAtlas = UIUtils.CreateAtlas(PIU, buttonSize, buttonSize, "ToolbarAtlas.png", new[]
                                            {
                                                "PropItUpNormalBg",
                                                "PropItUpHoveredBg",
                                                "PropItUpPressedBg",
                                                "PropItUpNormalFg",
                                                "PropItUpHoveredFg",
                                                "PropItUpPressedFg",
                                                "PropItUpButtonNormal",
                                                "PropItUpButtonHover",
                                                "PropItUpInfoTextBg",
                                            });
            }
            //  Apply custom sprite:
            atlas = toggleButtonAtlas;
            normalFgSprite = "PropItUpNormalBg";
            normalBgSprite = null;
            hoveredFgSprite = "PropItUpHoveredBg";
            hoveredBgSprite = "PropItUpHoveredFg";
            pressedFgSprite = "PropItUpPressedBg";
            pressedBgSprite = "PropItUpPressedFg";
            focusedFgSprite = "PropItUpPressedBg";
            focusedBgSprite = "PropItUpPressedFg";
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                UIMainPanel.instance.Toggle();
            }

            base.OnClick(p);
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                dragging = true;
            }
            base.OnMouseDown(p);
        }

        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                dragging = false;
            }
            base.OnMouseUp(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Right))
            {
                var ratio = UIView.GetAView().ratio;
                position = new Vector3(position.x + (p.moveDelta.x * ratio), position.y + (p.moveDelta.y * ratio), position.z);
                //  
                PropItUpTool.config.buttonposition = absolutePosition;
                PropItUpTool.SaveConfig();
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"Button position changed to {absolutePosition}.");
                }
            }
            base.OnMouseMove(p);
        }
    }
}