using ColossalFramework.UI;
using System;
using System.Timers;
using UnityEngine;
using static PropItUp.Configuration;

namespace PropItUp.GUI
{
    public class UIBuildingPrefabItem : UIPanel, IUIFastListRow
    {
        private UILabel _name;
        private UIButton _visible;

        private PropInfo _prop;
        public PropInfo prop;
        private TreeInfo _tree;
        public TreeInfo tree;

        private bool _isProp;

        private BuildingInfo _buildingInfo;
        private Configuration.Building _existingBuilding;

        private Timer timer;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (_name == null) return;
        }

        private void SetupControls()
        {
            if (_name != null) return;

            isVisible = true;
            isInteractive = true;
            width = parent.width;
            height = UIUtils.c_fastListRowHeight;

            _name = AddUIComponent<UILabel>();
            _name.name = "PrefabName";
            _name.relativePosition = new Vector3(5, 9);
            _name.textColor = new Color32(238, 238, 238, 255);
            _name.textScale = 0.85f;

            _visible = AddUIComponent<UIButton>();
            _visible.relativePosition = new Vector3(UIUtils.c_fastListWidth - 30, 8);
            _visible.size = new Vector2(15, 15);
            _visible.normalBgSprite = "check-checked";
            _visible.hoveredBgSprite = "check-unchecked";
            _visible.pressedBgSprite = "check-unchecked";
            _visible.isVisible = false;


            var prefab = (PropCustomizerPanel.instance.isVisible) ? "prop" : "tree";
            _visible.eventClick += (component, param) =>
            {
                ConfirmPanel.ShowModal("Remove " + prefab, "Are you sure you want to remove this " + prefab + "?\nYou cannot undo this action!", (d, i) => {
                    if (i == 1)
                    {
                        initHidePrefabBuilding();
                    }
                });
            };
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            p.Use();

            base.OnMouseDown(p);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
        }

        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
        }


        #region IUIFastListRow implementation

        public void Display(object data, bool isRowOdd)
        {
            SetupControls();

            try
            {
                _prop = null;
                _tree = null;
                _buildingInfo = null;
                PrefabInfo _prefab = data as PrefabInfo;
                if (PropItUpTool.allAvailableProps.Contains(_prefab as PropInfo))
                {
                    _isProp = true;
                }
                else if (PropItUpTool.allAvailableTrees.Contains(_prefab as TreeInfo))
                {
                    _isProp = false;
                }

                //  Find PrefabReplacement (if present):
                if (_isProp)
                {
                    _prop = data as PropInfo;
                }
                else
                {
                    _tree = data as TreeInfo;
                }

                //  Output prefab name and optionally its replacement's name:
                _buildingInfo = (_isProp) ? PropCustomizerPanel.instance.selectedBuilding : TreeCustomizerPanel.instance.selectedBuilding;
                _existingBuilding = PropItUpTool.config.GetBuilding(_buildingInfo.name);
                if (_existingBuilding == null)
                {
                    _name.text = (_isProp) ? UIUtils.GenerateBeautifiedPrefabName(_prop) : UIUtils.GenerateBeautifiedPrefabName(_tree);
                    _visible.name = (_isProp) ? _prop.name : _tree.name;
                }
                else
                {
                    if (_isProp)
                    {
                        PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(_existingBuilding, _prop.name);
                        if (replacement == null)
                        {
                            _name.text = UIUtils.GenerateBeautifiedPrefabName(_prop);
                            _visible.name = _prop.name;
                        }
                        else
                        {
                            PropInfo replacementProp = PrefabCollection<PropInfo>.FindLoaded(replacement.original);
                            _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                            _visible.name = replacementProp.name;
                        }
                    }
                    else
                    {
                        PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(_existingBuilding, _tree.name);
                        if (replacement == null)
                        {
                            _name.text = UIUtils.GenerateBeautifiedPrefabName(_tree);
                            _visible.name = _tree.name;
                        }
                        else
                        {
                            TreeInfo replacementTree = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                            _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                            _visible.name = replacementTree.name;
                        }
                    }
                }
                backgroundSprite = null;
            }
            catch (Exception ex)
            {
                DebugUtils.Log($"[ERROR] - Adding prefab to FastList failed ({data})\n\nMessage: {ex.Message}\nStacktrace: {ex.StackTrace}\nInnerException: {ex.InnerException}");
            }
        }

        public void Select(bool isRowOdd)
        {
            if (_visible)
            {
                _visible.isVisible = true;
            }
            backgroundSprite = "ListItemHighlight";
            color = new Color32(255, 255, 255, 255);
        }

        public void Deselect(bool isRowOdd)
        {
            if (_visible)
            {
                _visible.isVisible = false;
            }
            backgroundSprite = null;
        }

        #endregion


        //  Apply/Store prefab hiding/showing:
        protected void initHidePrefabBuilding()
        {
            //  Gather all necessary info:
            if (PropItUpTool.allAvailableProps.Find(x => x.name == _visible.name) != null)
            {
                _isProp = true;
                PropCustomizerPanel.instance.originalPropFastList.Clear();

                BuildingInfo buildingInfo = PropCustomizerPanel.instance.selectedBuilding;
                PropInfo prop = PropItUpTool.allAvailableProps.Find(x => x.name == _visible.name);
                //  Save Prefab Removal:
                PropItUpTool.SaveRemovalBuilding(buildingInfo, prop, true);
            }
            else if (PropItUpTool.allAvailableTrees.Find(x => x.name == _visible.name) != null)
            {
                _isProp = false;
                TreeCustomizerPanel.instance.originalTreeFastList.Clear();

                BuildingInfo buildingInfo = TreeCustomizerPanel.instance.selectedBuilding;
                TreeInfo tree = PropItUpTool.allAvailableTrees.Find(x => x.name == _visible.name);
                //  Save Prefab Removal:
                PropItUpTool.SaveRemovalBuilding(buildingInfo, tree, false);
            }
            //  Refresh FastList after x ms delay (to allow replacement code to be fully executed):
            timer = new Timer();
            timer.Interval = 750;
            timer.Elapsed += (s, e) =>
            {
                if (_isProp)
                {
                    PropCustomizerPanel.instance.PopulateOriginalPropsFastList();
                }
                else
                {
                    TreeCustomizerPanel.instance.PopulateOriginalTreesFastList();
                }
                timer.Stop();
            };
            timer.Start();
        }
    }
}