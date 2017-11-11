using ColossalFramework.UI;
using System;
using UnityEngine;
using static PropItUp.Configuration;

namespace PropItUp.GUI
{
    public class UIBuildingPrefabItem : UIPanel, IUIFastListRow
    {
        private UILabel _name;
        private PropInfo _prop;
        public PropInfo prop;
        private TreeInfo _tree;
        public TreeInfo tree;

        private BuildingInfo _building;
        private Configuration.Building _buildingReplacement;

        private bool _isProp;

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
            _name.name = "TreeName";
            _name.relativePosition = new Vector3(5, 9);
            _name.textColor = new Color32(238, 238, 238, 255);
            _name.textScale = 0.85f;
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
                _building = null;
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
                _building = (_isProp) ? PropCustomizerPanel.instance.selectedBuilding : TreeCustomizerPanel.instance.selectedBuilding;
                _buildingReplacement = PropItUpTool.config.GetBuilding(_building.name);
                if (_buildingReplacement == null)
                {
                    _name.text = (_isProp) ? UIUtils.GenerateBeautifiedPrefabName(_prop) : UIUtils.GenerateBeautifiedPrefabName(_tree);
                }
                else
                {
                    if (_isProp)
                    {
                        PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(_buildingReplacement, _prop.name);
                        if (replacement == null)
                        {
                            _name.text = UIUtils.GenerateBeautifiedPrefabName(_prop);
                        }
                        else
                        {
                            PropInfo replacementProp = PrefabCollection<PropInfo>.FindLoaded(replacement.original);
                            //_name.text = $"{UIUtils.GenerateBeautifiedPrefabName(replacementProp)}  [replacement: {UIUtils.GenerateBeautifiedPrefabName(_prop)}]";
                            _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                        }
                    }
                    else
                    {
                        PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(_buildingReplacement, _tree.name);
                        if (replacement == null)
                        {
                            _name.text = UIUtils.GenerateBeautifiedPrefabName(_tree);
                        }
                        else
                        {
                            TreeInfo replacementTree = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                            //_name.text = $"{UIUtils.GenerateBeautifiedPrefabName(replacementTree)} [replacement: {UIUtils.GenerateBeautifiedPrefabName(_tree)}]";
                            _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                        }
                    }
                }
                backgroundSprite = null;
            }
            catch (Exception ex)
            {
                DebugUtils.Log($"[DEBUG] - adding prefab to FastList failed ({data})\n\nMessage: {ex.Message}\nStacktrace: {ex.StackTrace}\nInnerException: {ex.InnerException}");
            }
        }

        public void Select(bool isRowOdd)
        {
            backgroundSprite = "ListItemHighlight";
            color = new Color32(255, 255, 255, 255);
        }

        public void Deselect(bool isRowOdd)
        {
            backgroundSprite = null;
        }
        #endregion
    }
}