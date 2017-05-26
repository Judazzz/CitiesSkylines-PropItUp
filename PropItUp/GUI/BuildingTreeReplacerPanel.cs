﻿using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace PropItUp.GUI
{
    public class BuildingTreeReplacerPanel : UIPanel
    {
        private UILabel _introLabel;
        private UILabel _originalTreeLabel;
        private UIButton _resetReplacementButton;
        private UIFastList _originalTreeFastList;
        private UILabel _replacementTreeLabel;
        private UITextField _replacementTreeFastListSearchBox;
        private UIFastList _replacementTreeFastList;
        private UIButton _saveTreeReplacementButton;

        private string searchboxPlaceholder = "Find a tree";

        public UILabel introLabel
        {
            get { return _introLabel; }
            set { _introLabel = value; }
        }

        public UIFastList originalTreeFastList
        {
            get { return _originalTreeFastList; }
        }
        public UITextField replacementTreeFastListSearchBox
        {
            get { return _replacementTreeFastListSearchBox; }
        }
        public UIFastList replacementTreeFastList
        {
            get { return _replacementTreeFastList; }
        }
        public UIButton saveTreeReplacementButton
        {
            get { return _saveTreeReplacementButton; }
        }

        private int _selectedTreeOriginalIndex = -1;
        public int selectedTreeOriginalIndex
        {
            get { return _selectedTreeOriginalIndex; }
        }

        private TreeInfo _selectedTreeOriginal;
        public TreeInfo selectedTreeOriginal
        {
            get { return _selectedTreeOriginal; }
            set { _selectedTreeOriginal = value; }
        }

        private TreeInfo _selectedTreeReplacement;
        public TreeInfo selectedTreeReplacement
        {
            get { return _selectedTreeReplacement; }
            set { _selectedTreeReplacement = value; }
        }

        private static BuildingTreeReplacerPanel _instance;
        public static BuildingTreeReplacerPanel instance
        {
            get { return _instance; }
        }

        private static bool listIsUpdating = false;
        private string searchQuery = string.Empty;

        public override void Start()
        {
            base.Start();
            _instance = this;
            canFocus = true;
            isInteractive = true;
            //  
            SetupControls();
            if (PropItUpTool.allCustomTrees.Count > 0)
            {
                PopulateVanillaTreesFastList();
                PopulateCustomTreesFastList();
            }
        }

        private void SetupControls()
        {
            // Vanilla Container:
            var originalContainer = UIUtils.CreateFormElement(this, "top");
            originalContainer.name = "originalContainer";
            //  Label:
            _introLabel = originalContainer.AddUIComponent<UILabel>();
            _introLabel.textColor = new Color(187, 187, 187, 255);
            _introLabel.textScale = 0.8f;
            _introLabel.padding = new RectOffset(0, 0, 0, 5);
            //  No custom trees: hide UI:
            if (PropItUpTool.allCustomTrees.Count == 0)
            {
                _introLabel.width = parent.width;
                _introLabel.text = "THIS FEATURE IS UNAVAILABLE!\n\nReason: no custom trees found";
                return;
            }
            _introLabel.text = "Global building replacement";
            //  Source:
            _originalTreeLabel = originalContainer.AddUIComponent<UILabel>();
            _originalTreeLabel.text = "Vanilla trees";
            _originalTreeLabel.textScale = 0.8f;
            _originalTreeLabel.padding = new RectOffset(0, 0, 0, 5);
            //  'Reset replacement' Button
            _resetReplacementButton = UIUtils.CreateButton(_originalTreeLabel);
            _resetReplacementButton.text = "(reset selected)";
            _resetReplacementButton.relativePosition = new Vector3(149f, -1.25f);
            _resetReplacementButton.textScale = 0.65f;
            _resetReplacementButton.width = 100;
            _resetReplacementButton.height = 16;
            _resetReplacementButton.normalBgSprite = null;
            _resetReplacementButton.hoveredBgSprite = null;
            _resetReplacementButton.pressedBgSprite = null;
            _resetReplacementButton.focusedBgSprite = null;
            _resetReplacementButton.disabledBgSprite = null;
            _resetReplacementButton.disabledColor = new Color32(83, 91, 95, 255);
            _resetReplacementButton.eventClicked += (c, e) =>
            {
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"BuildingTreeReplacerPanel: 'Reset replacement' clicked'.");
                }
                //  Get original tree:
                Configuration.PrefabReplacement replacement = PropItUpTool.config.GetGlobalBuildingReplacementByIndex(_selectedTreeOriginalIndex);
                //  Restore replacement:
                PropItUpTool.RestoreBuildingReplacementGlobal();
                //  Repopulate/reset OriginalTreeFastList:
                _selectedTreeReplacement = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                //  TODO: stay at selected index:
                PopulateVanillaTreesFastList();
                _selectedTreeOriginal = _originalTreeFastList.rowsData[_selectedTreeOriginalIndex] as TreeInfo;
                _resetReplacementButton.isEnabled = false;


                //PropItUpTool.RestoreBuildingReplacementGlobal();
                ////  Repopulate originalTreeFastList:
                ////  TODO: stay at selected index:
                //PopulateVanillaTreesFastList();
                //_selectedTreeOriginal = _originalTreeFastList.rowsData[_selectedTreeOriginalIndex] as TreeInfo;
                //_resetReplacementButton.isEnabled = false;
            };
            _resetReplacementButton.isEnabled = false;
            // FastList
            _originalTreeFastList = UIFastList.Create<UIVanillaTreeItem>(originalContainer);
            _originalTreeFastList.backgroundSprite = "UnlockingPanel";
            _originalTreeFastList.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _originalTreeFastList.height = 90;
            _originalTreeFastList.canSelect = true;
            _originalTreeFastList.eventSelectedIndexChanged += OnSelectedVanillaChanged;

            // Replacement Label Container:
            var replacementContainer = UIUtils.CreateFormElement(this, "center");
            replacementContainer.name = "replacementContainer";
            replacementContainer.relativePosition = new Vector3(0, 165);
            //  Label:
            _replacementTreeLabel = originalContainer.AddUIComponent<UILabel>();
            _replacementTreeLabel.text = "Select custom tree";
            _replacementTreeLabel.textScale = 0.8f;
            _replacementTreeLabel.padding = new RectOffset(0, 0, 15, 5);

            // Search Box Container:
            var searchboxContainer = UIUtils.CreateFormElement(this, "center");
            searchboxContainer.name = "searchboxContainer";
            searchboxContainer.relativePosition = new Vector3(0, 182);
            //  Search Box:
            _replacementTreeFastListSearchBox = UIUtils.CreateTextField(searchboxContainer);
            _replacementTreeFastListSearchBox.position = new Vector3(_introLabel.relativePosition.x, 205);
            _replacementTreeFastListSearchBox.width = parent.width - (3 * PropItUpTool.SPACING) - 10;
            _replacementTreeFastListSearchBox.height = 25;
            _replacementTreeFastListSearchBox.padding = new RectOffset(6, 6, 6, 6);
            _replacementTreeFastListSearchBox.normalBgSprite = "TextFieldUnderline";
            _replacementTreeFastListSearchBox.hoveredBgSprite = "TextFieldUnderline";
            _replacementTreeFastListSearchBox.disabledBgSprite = "TextFieldUnderline";
            _replacementTreeFastListSearchBox.focusedBgSprite = "LevelBarBackground";
            _replacementTreeFastListSearchBox.horizontalAlignment = UIHorizontalAlignment.Left;
            _replacementTreeFastListSearchBox.text = searchboxPlaceholder;
            _replacementTreeFastListSearchBox.textColor = new Color32(187, 187, 187, 255);
            _replacementTreeFastListSearchBox.textScale = 0.75f;
            //  Search Box Events:
            _replacementTreeFastListSearchBox.eventTextChanged += (c, p) =>
            {
                searchQuery = p;
                Search();
            };
            _replacementTreeFastListSearchBox.eventGotFocus += (c, p) =>
            {
                //_replacementTreeFastList.selectedIndex = -1;
                if (_replacementTreeFastListSearchBox.text == searchboxPlaceholder)
                {
                    _replacementTreeFastListSearchBox.text = string.Empty;
                }
            };
            _replacementTreeFastListSearchBox.eventLostFocus += (c, p) =>
            {
                if (_replacementTreeFastListSearchBox.text == string.Empty)
                {
                    _replacementTreeFastListSearchBox.text = searchboxPlaceholder;
                }
            };

            // FastList Container:
            var fastlistContainer = UIUtils.CreateFormElement(this, "center");
            fastlistContainer.name = "fastlistContainer";
            fastlistContainer.relativePosition = new Vector3(0, 205);
            // FastList:
            _replacementTreeFastList = UIFastList.Create<UITreeItem>(fastlistContainer);
            _replacementTreeFastList.position = new Vector3(_introLabel.relativePosition.x, 233);
            _replacementTreeFastList.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _replacementTreeFastList.height = 90;
            _replacementTreeFastList.backgroundSprite = "UnlockingPanel";
            _replacementTreeFastList.canSelect = true;
            _replacementTreeFastList.eventSelectedIndexChanged += OnSelectedCustomChanged;

            //  Button Container:
            var buttonContainer = UIUtils.CreateFormElement(this, "bottom");

            //  Buttons:
            _saveTreeReplacementButton = UIUtils.CreateButton(buttonContainer);
            _saveTreeReplacementButton.relativePosition = new Vector3(5, 10);
            _saveTreeReplacementButton.width = 110;
            _saveTreeReplacementButton.name = "saveReplacementButton";
            _saveTreeReplacementButton.text = "Replace tree";
            _saveTreeReplacementButton.tooltip = "Replace selected building vanilla tree with selected custom tree.";
            _saveTreeReplacementButton.eventClicked += (c, e) =>
            {
                //  Only save if original and replacement are selected:
                if (selectedTreeOriginal == null || selectedTreeReplacement == null)
                {
                    return;
                }
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"BuildingTreeReplacerPanel: 'Replace tree' clicked'.");
                }
                PropItUpTool.SaveBuildingReplacementGlobal();
                //  Repopulate originalTreeFastList:
                //  TODO: stay at selected index:
                PopulateVanillaTreesFastList();
                _selectedTreeOriginal = _selectedTreeReplacement;
                _resetReplacementButton.isEnabled = true;
            };
        }

        public void PopulateVanillaTreesFastList()
        {
            //  
            listIsUpdating = true;
            if (_originalTreeFastList.rowsData.m_size > 0)
            {
                _originalTreeFastList.Clear();
            }
            //  
            foreach (var tree in PropItUpTool.allVanillaTrees)
            {
                _originalTreeFastList.rowsData.Add(tree);
            }
            _originalTreeFastList.rowHeight = 26f;
            listIsUpdating = false;
            //  Preset FastList:
            if (selectedTreeOriginalIndex == -1)
            {
                _originalTreeFastList.DisplayAt(0);
            }
            else
            {
                _originalTreeFastList.selectedIndex = _selectedTreeOriginalIndex;
                _originalTreeFastList.DisplayAt(_selectedTreeOriginalIndex);
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"BuildingTreeReplacerPanel: originalTreeFastList populated with {PropItUpTool.allVanillaTrees.Count} trees.");
            }
        }
        protected void OnSelectedVanillaChanged(UIComponent component, int i)
        {
            if (listIsUpdating)
            {
                return;
            }
            _selectedTreeOriginal = _originalTreeFastList.rowsData[i] as TreeInfo;
            _selectedTreeOriginalIndex = i;
            //  Enable Reset Button if global replacement is set for selected tree:
            if (PropItUpTool.config.GetGlobalReplacementByVanillaTreeName(_selectedTreeOriginal.name) != null)
            {
                _resetReplacementButton.isEnabled = true;
            }
            else
            {
                _resetReplacementButton.isEnabled = false;
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"BuildingTreeReplacerPanel: originalTreeFastList selected: tree '{_selectedTreeOriginal.name}'.");
            }
        }

        public void PopulateCustomTreesFastList()
        {
            //  Search Query set?
            if (!string.IsNullOrEmpty(searchQuery) && searchQuery != searchboxPlaceholder)
            {
                Search();
                return;
            }
            //  TODO: Add 'No replacement' option:

            //  Add all available custom trees:
            foreach (var tree in PropItUpTool.allCustomTrees)
            {
                _replacementTreeFastList.rowsData.Add(tree);
            }
            _replacementTreeFastList.rowHeight = 26f;
            _replacementTreeFastList.DisplayAt(0);
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"BuildingTreeReplacerPanel: replacementTreeFastList populated with {PropItUpTool.allCustomTrees.Count} trees.");
            }
        }
        protected void OnSelectedCustomChanged(UIComponent component, int i)
        {
            if (i < 0)
            {
                return;
            }
            _selectedTreeReplacement = _replacementTreeFastList.rowsData[i] as TreeInfo;
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"BuildingTreeReplacerPanel: CustomFastList selected: tree '{_selectedTreeReplacement.name}'.");
            }
        }

        public void Search()
        {
            //  
            if (searchQuery == searchboxPlaceholder)
            {
                return;
            }
            //  Deselect and clear FastList:
            _replacementTreeFastList.selectedIndex = -1;
            _replacementTreeFastList.Clear();
            _selectedTreeReplacement = null;
            //  Create temporary list for search results:
            List<TreeInfo> tmpItemList = new List<TreeInfo>();
            if (string.IsNullOrEmpty(searchQuery))
            {
                tmpItemList = PropItUpTool.allCustomTrees;
            }
            else
            {
                foreach (TreeInfo result in PropItUpTool.allCustomTrees)
                {
                    if (result.name.ToLower().Contains(searchQuery.ToLower()))
                    {
                        tmpItemList.Add(result);
                    }
                }
            }
            //  Repopulate with search results, and show at first item if results are found:
            for (int i = 0; i < tmpItemList.Count; i++)
            {
                if (tmpItemList[i] != null)
                {
                    _replacementTreeFastList.rowsData.Add(tmpItemList[i]);
                }
            }
            if (tmpItemList.Count > 0)
            {
                _replacementTreeFastList.DisplayAt(0);
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"BuildingTreeReplacerPanel: search query '{searchQuery}' returned {tmpItemList.Count} results.");
            }
        }

        public void ResetPanel()
        {
            _resetReplacementButton.isEnabled = false;
            searchQuery = string.Empty;
            _replacementTreeFastListSearchBox.text = string.Empty;
            _replacementTreeFastList.DisplayAt(0);
            _replacementTreeFastList.selectedIndex = -1;
            _selectedTreeOriginal = null;
            _selectedTreeReplacement = null;
            listIsUpdating = false;
        }
    }
}