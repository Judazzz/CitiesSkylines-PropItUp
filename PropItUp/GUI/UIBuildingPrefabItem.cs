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
        private UIButton _hidePrefabButton;

        private PropInfo _prop;
        public PropInfo prop;
        private TreeInfo _tree;
        public TreeInfo tree;
        public string _prefabType;

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

            _hidePrefabButton = AddUIComponent<UIButton>();
            _hidePrefabButton.relativePosition = new Vector3(UIUtils.c_fastListWidth - 30, 8);
            _hidePrefabButton.size = new Vector2(15, 15);
            _hidePrefabButton.normalBgSprite = "check-checked";
            _hidePrefabButton.hoveredBgSprite = "check-unchecked";
            _hidePrefabButton.pressedBgSprite = "check-unchecked";
            _hidePrefabButton.isVisible = true;

            _hidePrefabButton.eventClick += (component, param) =>
            {
                ConfirmPanel.ShowModal("Remove " + _prefabType, "Are you sure you want to remove " + _prefabType + " '" + _hidePrefabButton.name + "' from asset '" + _buildingInfo.name + "'?\nYou cannot undo this action!", (d, i) => {
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
                    _prefabType = "prop";
                }
                else if (PropItUpTool.allAvailableTrees.Contains(_prefab as TreeInfo))
                {
                    _isProp = false;
                    _prefabType = "tree";
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
                    _name.text = (_isProp) ? (_prop.m_isCustomContent) ? _prop.name : "[v] " + _prop.name : (_tree.m_isCustomContent) ? _tree.name : "[v] " + _tree.name;
                    _hidePrefabButton.name = (_isProp) ? _prop.name : _tree.name;
                }
                else
                {
                    if (_isProp)
                    {
                        PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(_existingBuilding, _prop.name);
                        if (replacement == null)
                        {
                            _name.text = (_prop.m_isCustomContent) ? UIUtils.GenerateBeautifiedPrefabName(_prop) : "[v] " + UIUtils.GenerateBeautifiedPrefabName(_prop);
                            _hidePrefabButton.name = _prop.name;
                        }
                        else
                        {
                            PropInfo replacementProp = PrefabCollection<PropInfo>.FindLoaded(replacement.original);
                            //_name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                            if (_prop.m_isCustomContent && replacementProp.m_isCustomContent)
                            {
                                _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                            }
                            else if (!_prop.m_isCustomContent && replacementProp.m_isCustomContent)
                            {
                                _name.text = $"[v] {UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                            }
                            else if (_prop.m_isCustomContent && !replacementProp.m_isCustomContent)
                            {
                                _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: [v] {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                            }
                            else
                            {
                                _name.text = $"[v] {UIUtils.GenerateBeautifiedPrefabName(_prop)}  [original: [v] {UIUtils.GenerateBeautifiedPrefabName(replacementProp)}]";
                            }
                            _hidePrefabButton.name = replacementProp.name;
                        }
                    }
                    else
                    {
                        PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(_existingBuilding, _tree.name);
                        if (replacement == null)
                        {
                            _name.text = (_tree.m_isCustomContent) ? UIUtils.GenerateBeautifiedPrefabName(_tree) : "[v] " + UIUtils.GenerateBeautifiedPrefabName(_tree);
                            _hidePrefabButton.name = _tree.name;
                        }
                        else
                        {
                            TreeInfo replacementTree = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                            //_name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                            if (_tree.m_isCustomContent && replacementTree.m_isCustomContent)
                            {
                                _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                            }
                            else if (!_tree.m_isCustomContent && replacementTree.m_isCustomContent)
                            {
                                _name.text = $"[v] {UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                            }
                            else if (_tree.m_isCustomContent && !replacementTree.m_isCustomContent)
                            {
                                _name.text = $"{UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: [v] {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                            }
                            else
                            {
                                _name.text = $"[v] {UIUtils.GenerateBeautifiedPrefabName(_tree)}  [original: [v] {UIUtils.GenerateBeautifiedPrefabName(replacementTree)}]";
                            }
                            _hidePrefabButton.name = replacementTree.name;
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
            backgroundSprite = "ListItemHighlight";
            color = new Color32(255, 255, 255, 255);
        }

        public void Deselect(bool isRowOdd)
        {
            backgroundSprite = null;
        }

        #endregion


        //  Apply/Store prefab hiding/showing:
        protected void initHidePrefabBuilding()
        {
            //  Gather all necessary info:
            if (PropItUpTool.allAvailableProps.Find(x => x.name == _hidePrefabButton.name) != null)
            {
                _isProp = true;
                PropCustomizerPanel.instance.originalPropFastList.Clear();

                BuildingInfo buildingInfo = PropCustomizerPanel.instance.selectedBuilding;
                PropInfo prop = PropItUpTool.allAvailableProps.Find(x => x.name == _hidePrefabButton.name);
                //  Save Prefab Removal:
                PropItUpTool.SaveRemovalBuilding(buildingInfo, prop, true);
            }
            else if (PropItUpTool.allAvailableTrees.Find(x => x.name == _hidePrefabButton.name) != null)
            {
                _isProp = false;
                TreeCustomizerPanel.instance.originalTreeFastList.Clear();

                BuildingInfo buildingInfo = TreeCustomizerPanel.instance.selectedBuilding;
                TreeInfo tree = PropItUpTool.allAvailableTrees.Find(x => x.name == _hidePrefabButton.name);
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