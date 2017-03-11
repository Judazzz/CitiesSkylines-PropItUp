using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace PropItUp.GUI
{
    public class TreeReplacerPanel : UIPanel
    {
        private UILabel _introLabel;
        private UILabel _vanillaTreeLabel;
        private UIButton _resetReplacementButton;
        private UIFastList _vanillaTreeFastList;
        private UILabel _customTreeLabel;
        private UITextField _customTreeFastListSearchBox;
        private UIFastList _customTreeFastList;
        private UIButton _saveTreeReplacementButton;

        public UILabel introLabel
        {
            get { return _introLabel; }
            set { _introLabel = value; }
        }

        public UIFastList vanillaTreeFastList
        {
            get { return _vanillaTreeFastList; }
        }
        public UITextField customTreeFastListSearchBox
        {
            get { return _customTreeFastListSearchBox; }
        }
        public UIFastList customTreeFastList
        {
            get { return _customTreeFastList; }
        }
        public UIButton saveTreeReplacementButton
        {
            get { return _saveTreeReplacementButton; }
        }

        private int _selectedTreeVanillaIndex;
        public int selectedTreeVanillaIndex
        {
            get { return _selectedTreeVanillaIndex; }
        }

        private TreeInfo _selectedTreeVanilla;
        public TreeInfo selectedTreeVanilla
        {
            get { return _selectedTreeVanilla; }
            set { _selectedTreeVanilla = value; }
        }

        private TreeInfo _selectedTreeCustom;
        public TreeInfo selectedTreeCustom
        {
            get { return _selectedTreeCustom; }
            set { _selectedTreeCustom = value; }
        }

        private static TreeReplacerPanel _instance;
        public static TreeReplacerPanel instance
        {
            get { return _instance; }
        }

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
            _introLabel.text = "Global replacement";
            //  Source:
            _vanillaTreeLabel = originalContainer.AddUIComponent<UILabel>();
            _vanillaTreeLabel.text = "Vanilla trees";
            _vanillaTreeLabel.textScale = 0.8f;
            _vanillaTreeLabel.padding = new RectOffset(0, 0, 0, 5);
            //  'Reset replacement' Button
            _resetReplacementButton = UIUtils.CreateButton(_vanillaTreeLabel);
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
                    DebugUtils.Log($"TreeReplacerPanel: 'Reset replacement' clicked'.");
                }
                //  
                PropItUpTool.RestoreReplacementGlobal();
                //  Repopulate VanillaTreeFastList:
                //  TODO: stay at selected index:
                PopulateVanillaTreesFastList();
            };
            _resetReplacementButton.isEnabled = false;
            // FastList
            _vanillaTreeFastList = UIFastList.Create<UIVanillaTreeItem>(originalContainer);
            _vanillaTreeFastList.backgroundSprite = "UnlockingPanel";
            _vanillaTreeFastList.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _vanillaTreeFastList.height = 90;
            _vanillaTreeFastList.canSelect = true;
            _vanillaTreeFastList.eventSelectedIndexChanged += OnSelectedVanillaChanged;

            // Replacement Label Container:
            var replacementContainer = UIUtils.CreateFormElement(this, "center");
            replacementContainer.name = "replacementContainer";
            replacementContainer.relativePosition = new Vector3(0, 165);
            //  Label:
            _customTreeLabel = originalContainer.AddUIComponent<UILabel>();
            _customTreeLabel.text = "Select custom tree";
            _customTreeLabel.textScale = 0.8f;
            _customTreeLabel.padding = new RectOffset(0, 0, 15, 5);

            // Search Box Container:
            var searchboxContainer = UIUtils.CreateFormElement(this, "center");
            searchboxContainer.name = "searchboxContainer";
            searchboxContainer.relativePosition = new Vector3(0, 182);
            //  Search Box:
            _customTreeFastListSearchBox = UIUtils.CreateTextField(searchboxContainer);
            _customTreeFastListSearchBox.position = new Vector3(_introLabel.relativePosition.x, 205);
            _customTreeFastListSearchBox.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _customTreeFastListSearchBox.height = 25;
            _customTreeFastListSearchBox.padding = new RectOffset(6, 6, 6, 6);
            _customTreeFastListSearchBox.normalBgSprite = "TextFieldUnderline";
            _customTreeFastListSearchBox.hoveredBgSprite = "TextFieldUnderline";
            _customTreeFastListSearchBox.disabledBgSprite = "TextFieldUnderline";
            _customTreeFastListSearchBox.focusedBgSprite = "LevelBarBackground";
            _customTreeFastListSearchBox.horizontalAlignment = UIHorizontalAlignment.Left;
            _customTreeFastListSearchBox.text = "Find a tree";
            _customTreeFastListSearchBox.textColor = new Color32(187, 187, 187, 255);
            _customTreeFastListSearchBox.textScale = 0.75f;
            //  Search Box Events:
            _customTreeFastListSearchBox.eventTextChanged += (c, p) =>
            {
                searchQuery = p;
                Search();
            };
            _customTreeFastListSearchBox.eventGotFocus += (c, p) =>
            {
                _customTreeFastList.selectedIndex = -1;
                if (_customTreeFastListSearchBox.text == "Find a tree")
                {
                    _customTreeFastListSearchBox.text = string.Empty;
                }
            };
            _customTreeFastListSearchBox.eventLostFocus += (c, p) =>
            {
                if (_customTreeFastListSearchBox.text == string.Empty)
                {
                    _customTreeFastListSearchBox.text = "Find a tree";
                }
            };

            // FastList Container:
            var fastlistContainer = UIUtils.CreateFormElement(this, "center");
            fastlistContainer.name = "fastlistContainer";
            fastlistContainer.relativePosition = new Vector3(0, 205);
            // FastList:
            _customTreeFastList = UIFastList.Create<UITreeItem>(fastlistContainer);
            _customTreeFastList.position = new Vector3(_introLabel.relativePosition.x, 233);
            _customTreeFastList.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _customTreeFastList.height = 90;
            _customTreeFastList.backgroundSprite = "UnlockingPanel";
            _customTreeFastList.canSelect = true;
            _customTreeFastList.eventSelectedIndexChanged += OnSelectedCustomChanged;

            //  Button Container:
            var buttonContainer = UIUtils.CreateFormElement(this, "bottom");

            //  Buttons:
            _saveTreeReplacementButton = UIUtils.CreateButton(buttonContainer);
            _saveTreeReplacementButton.relativePosition = new Vector3(5, 10);
            _saveTreeReplacementButton.width = 110;
            _saveTreeReplacementButton.name = "saveReplacementButton";
            _saveTreeReplacementButton.text = "Replace tree";
            _saveTreeReplacementButton.tooltip = "Replace selected vanilla tree with selected custom tree.";
            _saveTreeReplacementButton.eventClicked += (c, e) =>
            {
                //  Only save if original and replacement are selected:
                if (selectedTreeVanilla == null || selectedTreeCustom == null)
                {
                    return;
                }
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"TreeReplacerPanel: 'Replace tree' clicked'.");
                }
                PropItUpTool.SaveReplacementGlobal();
                //  Repopulate VanillaTreeFastList:
                //  TODO: stay at selected index:
                PopulateVanillaTreesFastList();
            };
        }

        public void PopulateVanillaTreesFastList()
        {
            //  
            if (_vanillaTreeFastList.rowsData.m_size > 0) {
                _vanillaTreeFastList.Clear();
            }
            //  
            foreach (var tree in PropItUpTool.allVanillaTrees)
            {
                _vanillaTreeFastList.rowsData.Add(tree);
            }
            _vanillaTreeFastList.rowHeight = 26f;
            _vanillaTreeFastList.DisplayAt(0);
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: VanillaTreeFastList populated with {PropItUpTool.allVanillaTrees.Count} trees.");
            }
        }
        protected void OnSelectedVanillaChanged(UIComponent component, int i)
        {
            _selectedTreeVanilla = _vanillaTreeFastList.rowsData[i] as TreeInfo;
            _selectedTreeVanillaIndex = i;
            //  Enable Reset Button if global replacement is set for selected tree:
            if (PropItUpTool.config.GetGlobalReplacementByVanillaTreeName(_selectedTreeVanilla.name) != null) {
                _resetReplacementButton.isEnabled = true;
            }
            else
            {
                _resetReplacementButton.isEnabled = false;
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: VanillaTreeFastList selected: tree '{_selectedTreeVanilla.name}'.");
            }
        }

        public void PopulateCustomTreesFastList()
        {
            //  TODO: Add 'No replacement' option:

            //  Add all available custom trees:
            foreach (var tree in PropItUpTool.allCustomTrees)
            {
                _customTreeFastList.rowsData.Add(tree);
            }
            _customTreeFastList.rowHeight = 26f;
            _customTreeFastList.DisplayAt(0);
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: CustomTreeFastList populated with {PropItUpTool.allCustomTrees.Count} trees.");
            }
        }
        protected void OnSelectedCustomChanged(UIComponent component, int i)
        {
            if (i < 0)
            {
                return;
            }
            _selectedTreeCustom = _customTreeFastList.rowsData[i] as TreeInfo;
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: CustomFastList selected: tree '{_selectedTreeCustom.name}'.");
            }
        }

        public void Search()
        {
            //  
            if (searchQuery == "Find a tree")
            {
                searchQuery = string.Empty;
            }
            //  Deselect and clear FastList:
            _customTreeFastList.selectedIndex = -1;
            _customTreeFastList.Clear();
            _selectedTreeCustom = null;
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
                    _customTreeFastList.rowsData.Add(tmpItemList[i]);
                }
            }
            if (tmpItemList.Count > 0)
            {
                _customTreeFastList.DisplayAt(0);
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: search query '{searchQuery}' returned {tmpItemList.Count} results.");
            }
        }

        public void ResetPanel()
        {
            _resetReplacementButton.isEnabled = false;
            customTreeFastList.DisplayAt(0);
            customTreeFastList.selectedIndex = -1;
            selectedTreeVanilla = null;
            selectedTreeCustom = null;
            //PropItUpTool.selectedPropInstanceId = 0;
        }
    }
}