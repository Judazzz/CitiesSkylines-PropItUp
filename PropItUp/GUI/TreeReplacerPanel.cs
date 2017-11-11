using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace PropItUp.GUI
{
    public class TreeReplacerPanel : UIPanel
    {
        private UILabel _titleLabel;
        private UILabel _originalTreeLabel;
        private UIButton _resetReplacementButton;
        private UITextField _originalTreeFastListSearchBox;
        private UIFastList _originalTreeFastList;
        private UILabel _replacementTreeLabel;
        private UITextField _replacementTreeFastListSearchBox;
        private UIFastList _replacementTreeFastList;
        private UIButton _saveTreeReplacementButton;

        private string originalSearchBoxPlaceholder = "Find a tree";
        private string replacementSearchBoxPlaceholder = "Find a replacement tree";

        public UILabel titleLabel
        {
            get { return _titleLabel; }
            set { _titleLabel = value; }
        }

        public UITextField originalTreeFastListSearchBox
        {
            get { return _originalTreeFastListSearchBox; }
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

        private static TreeReplacerPanel _instance;
        public static TreeReplacerPanel instance
        {
            get { return _instance; }
        }

        private static bool listIsUpdating = false;
        private string originalSearchQuery = string.Empty;
        private string replacementSearchQuery = string.Empty;

        public override void Start()
        {
            base.Start();
            _instance = this;
            canFocus = true;
            isInteractive = true;
            //  
            SetupControls();
            if (PropItUpTool.allAvailableTrees.Count > 0)
            {
                PopulateOriginalTreesFastList();
                PopulateCustomTreesFastList();
            }
        }

        private void SetupControls()
        {
            // TITLE:
            // Title Container:
            var titleContainer = UIUtils.CreateFormElement(this, "top");
            titleContainer.name = "titleContainer";
            titleContainer.size = new Vector2(UIUtils.c_tabPanelWidth, UIUtils.c_tabPanelHeight);
            // Title Label (selected asset name):
            _titleLabel = titleContainer.AddUIComponent<UILabel>();
            _titleLabel.name = "titleLabel";
            _titleLabel.textColor = new Color(187, 187, 187, 255);
            _titleLabel.textScale = 0.9f;
            // Feature disabled: hide UI:
            if (!PropItUpTool.config.enable_globalfreestanding)
            {
                _titleLabel.width = parent.width;
                _titleLabel.text = "THIS FEATURE IS UNAVAILABLE!\n\nReason: feature was disabled by player";
                return;
            }
            _titleLabel.text = "Global tree replacement";


            // ORIGINAL:
            var originalLabelContainer = UIUtils.CreateFormElement(this, "center");
            originalLabelContainer.name = "originalLabelContainer";
            originalLabelContainer.relativePosition = new Vector3(0, 36);
            // Original Label Container:
            _originalTreeLabel = originalLabelContainer.AddUIComponent<UILabel>();
            _originalTreeLabel.name = "originalTreeLabel";
            _originalTreeLabel.text = "Select original tree";
            _originalTreeLabel.textScale = 0.9f;
            _originalTreeLabel.padding = new RectOffset(0, 0, 15, 5);
            // 'Reset replacement' Button
            _resetReplacementButton = UIUtils.CreateResetButton(_originalTreeLabel);
            _resetReplacementButton.eventClicked += (c, e) =>
            {
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"TreeReplacerPanel: 'Reset replacement' clicked'.");
                }
                //  Get original tree:
                //  TODO => TEST: Get tree by name instead of index (property obsolete due to alphabetized list)
                Configuration.PrefabReplacement replacement = PropItUpTool.config.GetGlobalReplacementByTreeName(_selectedTreeOriginal.name);
                //  Restore replacement:
                PropItUpTool.RestoreReplacementGlobal();
                //  Repopulate/reset OriginalTreeFastList:
                _selectedTreeReplacement = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                //  TODO: update selected item (may have changed position in alphabetized list due to updated name)
                PopulateOriginalTreesFastList();
                _selectedTreeOriginal = _originalTreeFastList.rowsData[_selectedTreeOriginalIndex] as TreeInfo;
                _resetReplacementButton.isEnabled = false;
                _resetReplacementButton.isVisible = false;
            };
            _resetReplacementButton.isEnabled = false;
            _resetReplacementButton.isVisible = false;
            // Original Search Box Container:
            var originalTreeSearchBoxContainer = UIUtils.CreateFormElement(this, "center");
            originalTreeSearchBoxContainer.name = "originalTreeSearchBoxContainer";
            originalTreeSearchBoxContainer.relativePosition = new Vector3(0, 74);
            // Original Search Original :
            _originalTreeFastListSearchBox = UIUtils.CreateTextField(originalTreeSearchBoxContainer);
            _originalTreeFastListSearchBox.text = originalSearchBoxPlaceholder;
            // Original Search Box Events:
            _originalTreeFastListSearchBox.eventTextChanged += (c, p) =>
            {
                originalSearchQuery = p;
                SearchOriginal();
            };
            _originalTreeFastListSearchBox.eventGotFocus += (c, p) =>
            {
                if (_originalTreeFastListSearchBox.text == originalSearchBoxPlaceholder)
                {
                    _originalTreeFastListSearchBox.text = string.Empty;
                }
            };
            _originalTreeFastListSearchBox.eventLostFocus += (c, p) =>
            {
                if (_originalTreeFastListSearchBox.text == string.Empty)
                {
                    _originalTreeFastListSearchBox.text = originalSearchBoxPlaceholder;
                }
            };
            // Original FastList Container:
            var originalTreeFastListContainer = UIUtils.CreateFormElement(this, "center");
            originalTreeFastListContainer.name = "originalTreeFastListContainer";
            originalTreeFastListContainer.relativePosition = new Vector3(0, 102);
            // Original FastList
            _originalTreeFastList = UIFastList.Create<UIVanillaTreeItem>(originalTreeFastListContainer);
            _originalTreeFastList.backgroundSprite = "UnlockingPanel";
            _originalTreeFastList.relativePosition = new Vector3(0, 15);
            _originalTreeFastList.width = UIUtils.c_fastListWidth;
            _originalTreeFastList.height = UIUtils.c_fastListHeight - 20;
            _originalTreeFastList.canSelect = true;
            _originalTreeFastList.eventSelectedIndexChanged += OnSelectedOriginalChanged;


            // REPLACEMENT:
            // Replacement Label Container:
            var replacementLabelContainer = UIUtils.CreateFormElement(this, "center");
            replacementLabelContainer.name = "replacementLabelContainer";
            replacementLabelContainer.relativePosition = new Vector3(0, 270);
            // Replacement Label:
            _replacementTreeLabel = replacementLabelContainer.AddUIComponent<UILabel>();
            _replacementTreeLabel.name = "replacementTreeLabel";
            _replacementTreeLabel.text = "Select custom tree";
            _replacementTreeLabel.textScale = 0.9f;
            _replacementTreeLabel.padding = new RectOffset(0, 0, 20, 5);
            // Replacement Search Box Container:
            var replacementTreeSearchBoxContainer = UIUtils.CreateFormElement(this, "center");
            replacementTreeSearchBoxContainer.name = "replacementTreeSearchBoxContainer";
            replacementTreeSearchBoxContainer.relativePosition = new Vector3(0, 312);
            // Replacement Search Box:
            _replacementTreeFastListSearchBox = UIUtils.CreateTextField(replacementTreeSearchBoxContainer);
            _replacementTreeFastListSearchBox.text = replacementSearchBoxPlaceholder;
            // Replacement Search Box Events:
            _replacementTreeFastListSearchBox.eventTextChanged += (c, p) =>
            {
                replacementSearchQuery = p;
                SearchReplacement();
            };
            _replacementTreeFastListSearchBox.eventGotFocus += (c, p) =>
            {
                if (_replacementTreeFastListSearchBox.text == replacementSearchBoxPlaceholder)
                {
                    _replacementTreeFastListSearchBox.text = string.Empty;
                }
            };
            _replacementTreeFastListSearchBox.eventLostFocus += (c, p) =>
            {
                if (_replacementTreeFastListSearchBox.text == string.Empty)
                {
                    _replacementTreeFastListSearchBox.text = replacementSearchBoxPlaceholder;
                }
            };
            // Replacement FastList Container:
            var replacementTreeFastListContainer = UIUtils.CreateFormElement(this, "center");
            replacementTreeFastListContainer.name = "replacementTreeFastListContainer";
            replacementTreeFastListContainer.relativePosition = new Vector3(0, 340);
            // Replacement FastList:
            _replacementTreeFastList = UIFastList.Create<UITreeItem>(replacementTreeFastListContainer);
            _replacementTreeFastList.width = UIUtils.c_fastListWidth;
            _replacementTreeFastList.height = UIUtils.c_fastListHeight -20;
            _replacementTreeFastList.backgroundSprite = "UnlockingPanel";
            _replacementTreeFastList.canSelect = true;
            _replacementTreeFastList.eventSelectedIndexChanged += OnSelectedCustomChanged;


            // BUTTONS:
            // Button Container:
            var buttonContainer = UIUtils.CreateFormElement(this, "bottom");
            // Buttons:
            _saveTreeReplacementButton = UIUtils.CreateButton(buttonContainer);
            _saveTreeReplacementButton.relativePosition = new Vector3(5, 10);
            _saveTreeReplacementButton.width = 110;
            _saveTreeReplacementButton.name = "saveReplacementButton";
            _saveTreeReplacementButton.text = "Replace tree";
            _saveTreeReplacementButton.tooltip = "Replace selected original tree with selected custom tree.";
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
                    DebugUtils.Log($"TreeReplacerPanel: 'Replace tree' clicked'.");
                }
                PropItUpTool.SaveReplacementGlobal();
                //  Repopulate originalTreeFastList:
                //  TODO: update selected item (may have changed position in alphabetized list due to updated name)
                PopulateOriginalTreesFastList();
                _selectedTreeOriginal = _selectedTreeReplacement;
                _resetReplacementButton.isEnabled = true;
                _resetReplacementButton.isVisible = true;
            };
        }

        public void PopulateOriginalTreesFastList()
        {
            //  Search Query set?
            if (!string.IsNullOrEmpty(originalSearchQuery) && originalSearchQuery != originalSearchBoxPlaceholder)
            {
                SearchOriginal();
                return;
            }
            //  
            listIsUpdating = true;
            if (_originalTreeFastList.rowsData.m_size > 0)
            {
                _originalTreeFastList.Clear();
            }
            //  
            foreach (var tree in PropItUpTool.allAvailableTrees)
            {
                _originalTreeFastList.rowsData.Add(tree);
            }
            _originalTreeFastList.rowHeight = UIUtils.c_fastListRowHeight;
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
                DebugUtils.Log($"TreeReplacerPanel: originalTreeFastList populated with {PropItUpTool.allAvailableTrees.Count} trees.");
            }
        }
        protected void OnSelectedOriginalChanged(UIComponent component, int i)
        {
            if (listIsUpdating)
            {
                return;
            }
            _selectedTreeOriginal = _originalTreeFastList.rowsData[i] as TreeInfo;
            _selectedTreeOriginalIndex = i;
            //  Enable Reset Button if global replacement is set for selected tree:
            if (PropItUpTool.config.GetGlobalReplacementByTreeName(_selectedTreeOriginal.name) != null) {
                _resetReplacementButton.isEnabled = true;
                _resetReplacementButton.isVisible = true;
            }
            else
            {
                _resetReplacementButton.isEnabled = false;
                _resetReplacementButton.isVisible = false;
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: originalTreeFastList selected: tree '{_selectedTreeOriginal.name}'.");
            }
        }

        public void PopulateCustomTreesFastList()
        {   
            //  Search Query set?
            if (!string.IsNullOrEmpty(replacementSearchQuery) && replacementSearchQuery != replacementSearchBoxPlaceholder)
            {
                SearchReplacement();
                return;
            }
            //  TODO: Add 'No replacement' option:

            //  Add all available custom trees:
            foreach (var tree in PropItUpTool.allAvailableTrees)
            {
                _replacementTreeFastList.rowsData.Add(tree);
            }
            _replacementTreeFastList.rowHeight = UIUtils.c_fastListRowHeight;
            _replacementTreeFastList.DisplayAt(0);
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: replacementTreeFastList populated with {PropItUpTool.allAvailableTrees.Count} trees.");
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
                DebugUtils.Log($"TreeReplacerPanel: CustomFastList selected: tree '{_selectedTreeReplacement.name}'.");
            }
        }

        public void SearchOriginal()
        {
            //  
            if (originalSearchQuery == originalSearchBoxPlaceholder)
            {
                return;
            }
            //  Deselect and clear FastList:
            _originalTreeFastList.selectedIndex = -1;
            _originalTreeFastList.Clear();
            //  Create temporary list for search results:
            List<TreeInfo> tmpItemList = new List<TreeInfo>();
            if (string.IsNullOrEmpty(replacementSearchQuery))
            {
                tmpItemList = PropItUpTool.allAvailableTrees;
            }
            else
            {
                foreach (TreeInfo result in PropItUpTool.allAvailableTrees)
                {
                    if (result.name.ToLower().Contains(originalSearchQuery.ToLower()))
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
                    _originalTreeFastList.rowsData.Add(tmpItemList[i]);
                }
            }
            if (tmpItemList.Count > 0)
            {
                _originalTreeFastList.DisplayAt(0);
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeReplacerPanel: originalFastList search query '{originalSearchQuery}' returned {tmpItemList.Count} results.");
            }
        }

        public void SearchReplacement()
        {
            //  
            if (replacementSearchQuery == replacementSearchBoxPlaceholder)
            {
                return;
            }
            //  Deselect and clear FastList:
            _replacementTreeFastList.selectedIndex = -1;
            _replacementTreeFastList.Clear();
            _selectedTreeReplacement = null;
            //  Create temporary list for search results:
            List<TreeInfo> tmpItemList = new List<TreeInfo>();
            if (string.IsNullOrEmpty(replacementSearchQuery))
            {
                tmpItemList = PropItUpTool.allAvailableTrees;
            }
            else
            {
                foreach (TreeInfo result in PropItUpTool.allAvailableTrees)
                {
                    if (result.name.ToLower().Contains(replacementSearchQuery.ToLower()))
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
                DebugUtils.Log($"TreeReplacerPanel: replacementFastList search query '{replacementSearchQuery}' returned {tmpItemList.Count} results.");
            }
        }

        public void ResetPanel()
        {
            originalSearchQuery = string.Empty;
            _resetReplacementButton.isEnabled = false;
            _resetReplacementButton.isVisible = false;
            replacementSearchQuery = string.Empty;
            _replacementTreeFastListSearchBox.text = string.Empty;
            _replacementTreeFastList.DisplayAt(0);
            _replacementTreeFastList.selectedIndex = -1;
            _selectedTreeOriginal = null;
            _selectedTreeReplacement = null;
            listIsUpdating = false;
        }
    }
}