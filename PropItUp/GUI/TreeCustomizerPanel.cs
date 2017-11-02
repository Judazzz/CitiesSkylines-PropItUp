using ColossalFramework.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PropItUp.GUI
{
    public class TreeCustomizerPanel : UIPanel
    {
        private UILabel _selectedBuildingLabel;
        private UILabel _originalTreeLabel;
        private UIButton _resetReplacementButton;
        private UIFastList _originalTreeFastList;
        private UILabel _replacementTreeLabel;
        private UITextField _replacementTreeFastListSearchBox;
        private UIFastList _replacementTreeFastList;
        private UIButton _saveTreeReplacementButton;

        private string searchboxPlaceholder = "Find a replacement tree";

        public UILabel selectedBuildingLabel
        {
            get { return _selectedBuildingLabel; }
            set { _selectedBuildingLabel = value; }
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

        private int _selectedTreeOriginalIndex = 0;
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
        
        private BuildingInfo _selectedBuilding;
        public BuildingInfo selectedBuilding
        {
            get { return _selectedBuilding; }
            set { _selectedBuilding = value; }
        }

        private static TreeCustomizerPanel _instance;
        public static TreeCustomizerPanel instance
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
            PopulateAvailableTreesFastList();
        }

        private void SetupControls()
        {
            var originalContainer = UIUtils.CreateFormElement(this, "top");
            originalContainer.name = "originalContainer";
            originalContainer.size = new Vector2(UIUtils.c_tabPanelWidth, UIUtils.c_tabPanelHeight);
            //  Source Container:
            _selectedBuildingLabel = originalContainer.AddUIComponent<UILabel>();
            _selectedBuildingLabel.name = "selectedBuildingLabel";
            _selectedBuildingLabel.text = "No building selected";
            _selectedBuildingLabel.textColor = new Color(187, 187, 187, 255);
            _selectedBuildingLabel.textScale = 0.9f;
            //  Label:
            _originalTreeLabel = originalContainer.AddUIComponent<UILabel>();
            _originalTreeLabel.name = "originalTreeLabel";
            _originalTreeLabel.text = "Included trees";
            _originalTreeLabel.textScale = 0.9f;
            _originalTreeLabel.padding = new RectOffset(0, 0, 15, 5);
            //  'Reset replacement' Button
            _resetReplacementButton = UIUtils.CreateButton(_originalTreeLabel);
            _resetReplacementButton.text = "(reset selected)";
            _resetReplacementButton.relativePosition = new Vector3(UIUtils.c_resetButtonPosX, UIUtils.c_resetButtonPosY);
            _resetReplacementButton.width = UIUtils.c_resetButtonWidth;
            _resetReplacementButton.height = UIUtils.c_resetButtonHeight;
            _resetReplacementButton.textScale = 0.75f;
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
                    DebugUtils.Log($"TreeCustomizerPanel: 'Reset replacement' clicked.");
                }
                //  Get original tree:
                Configuration.Building building = PropItUpTool.config.GetBuilding(_selectedBuilding.name);
                //  TODO => TEST: Get tree by name instead of index (property obsolete due to alphabetized list)
                //Configuration.PrefabReplacement replacement = PropItUpTool.config.GetBuildingPrefabReplacementByIndex(building, "tree", _originalTreeFastList.selectedIndex);
                Configuration.PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByOriginalPrefabName(building, _selectedTreeOriginal.name);
                //  Restore replacement:
                //PropItUpTool.RestoreReplacementBuilding(_selectedTreeOriginalIndex, "tree", _selectedBuilding);
                PropItUpTool.RestoreReplacementBuilding(_selectedTreeOriginalIndex, "tree", _selectedBuilding, _selectedTreeOriginal);
                //  Repopulate/reset OriginalTreeFastList:
                _selectedTreeReplacement = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                PopulateIncludedTreesFastList();
                _selectedTreeOriginal = _originalTreeFastList.rowsData[_selectedTreeOriginalIndex] as TreeInfo;
                _resetReplacementButton.isEnabled = false;
            };
            _resetReplacementButton.isEnabled = false;
            // FastList
            _originalTreeFastList = UIFastList.Create<UIBuildingPrefabItem>(originalContainer);
            _originalTreeFastList.backgroundSprite = "UnlockingPanel";
            _originalTreeFastList.relativePosition = new Vector3(0, 15);
            _originalTreeFastList.width = UIUtils.c_fastListWidth;
            _originalTreeFastList.height = UIUtils.c_fastListHeight;
            _originalTreeFastList.canSelect = true;
            _originalTreeFastList.eventSelectedIndexChanged += OnSelectedOriginalChanged;

            // Replacement Label Container:
            var replacementContainer = UIUtils.CreateFormElement(this, "center");
            replacementContainer.name = "replacementContainer";
            replacementContainer.relativePosition = new Vector3(0, 275);
            //  Label:
            _replacementTreeLabel = originalContainer.AddUIComponent<UILabel>();
            _replacementTreeLabel.name = "replacementTreeLabel";
            _replacementTreeLabel.text = "Select replacement tree";
            _replacementTreeLabel.textScale = 0.9f;
            _replacementTreeLabel.padding = new RectOffset(0, 0, 20, 5);
            // Search Box Container:
            var searchboxContainer = UIUtils.CreateFormElement(this, "center");
            searchboxContainer.name = "searchboxContainer";
            searchboxContainer.relativePosition = new Vector3(0, 292);
            //  Search Box:
            _replacementTreeFastListSearchBox = UIUtils.CreateTextField(searchboxContainer);
            _replacementTreeFastListSearchBox.position = new Vector3(_selectedBuildingLabel.relativePosition.x, 315);
            _replacementTreeFastListSearchBox.width = UIUtils.c_searchBoxWidth;
            _replacementTreeFastListSearchBox.height = UIUtils.c_searchBoxHeight;
            _replacementTreeFastListSearchBox.padding = new RectOffset(6, 6, 8, 6);
            _replacementTreeFastListSearchBox.normalBgSprite = "TextFieldUnderline";
            _replacementTreeFastListSearchBox.hoveredBgSprite = "TextFieldUnderline";
            _replacementTreeFastListSearchBox.disabledBgSprite = "TextFieldUnderline";
            _replacementTreeFastListSearchBox.focusedBgSprite = "LevelBarBackground";
            _replacementTreeFastListSearchBox.horizontalAlignment = UIHorizontalAlignment.Left;
            _replacementTreeFastListSearchBox.text = searchboxPlaceholder;
            _replacementTreeFastListSearchBox.textColor = new Color32(187, 187, 187, 255);
            _replacementTreeFastListSearchBox.textScale = 0.85f;
            //  Search Box Events:
            _replacementTreeFastListSearchBox.eventTextChanged += (c, p) =>
            {
                searchQuery = p;
                Search();
            };
            _replacementTreeFastListSearchBox.eventGotFocus += (c, p) =>
            {
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
            fastlistContainer.relativePosition = new Vector3(0, 320);
            //  FastList:
            _replacementTreeFastList = UIFastList.Create<UITreeItem>(fastlistContainer);
            _replacementTreeFastList.position = new Vector3(_selectedBuildingLabel.relativePosition.x, 350);
            _replacementTreeFastList.width = UIUtils.c_fastListWidth;
            _replacementTreeFastList.height = UIUtils.c_fastListHeight;
            _replacementTreeFastList.backgroundSprite = "UnlockingPanel";
            _replacementTreeFastList.canSelect = true;
            _replacementTreeFastList.eventSelectedIndexChanged += OnSelectedReplacementChanged;

            //  Button Container:
            var buttonContainer = UIUtils.CreateFormElement(this, "bottom");
            //  Buttons:
            _saveTreeReplacementButton = UIUtils.CreateButton(buttonContainer);
            _saveTreeReplacementButton.relativePosition = new Vector3(5, 10);
            _saveTreeReplacementButton.width = 110;
            _saveTreeReplacementButton.name = "saveTreeReplacementButton";
            _saveTreeReplacementButton.text = "Replace tree";
            _saveTreeReplacementButton.tooltip = "Replace selected tree with selected replacement tree.";
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
                    DebugUtils.Log($"TreeCustomizerPanel: 'Replace tree' clicked'.");
                }
                //  Replace tree:
                PropItUpTool.SaveReplacementBuilding(_selectedTreeOriginalIndex, "tree", _selectedTreeOriginal, _selectedTreeReplacement, _selectedBuilding);
                //  Repopulate/reset OriginalTreeFastList:
                PopulateIncludedTreesFastList();
                _selectedTreeOriginal = _selectedTreeReplacement;
                _resetReplacementButton.isEnabled = true;
            };
        }

        public void PopulateIncludedTreesFastList()
        {
            //  Set selected building label:
            selectedBuildingLabel.text =
                $"{UIUtils.GenerateBeautifiedPrefabName(_selectedBuilding)}";
            UIUtils.TruncateLabel(selectedBuildingLabel, _replacementTreeFastListSearchBox.width);
            //  Null/empty check:
            if (_selectedBuilding.m_props == null || _selectedBuilding.m_props.Length == 0)
            {
                return;
            }
            //  Clear FastList:
            if (_originalTreeFastList.rowsData.m_size > 0)
            {
                _originalTreeFastList.Clear();
            }
            //  List all trees in selected building:
            listIsUpdating = true;
            List<BuildingInfo.Prop> allBuildingTrees = _selectedBuilding.m_props.Where(x => x.m_tree != null).ToList();
            allBuildingTrees = allBuildingTrees.OrderBy(x => UIUtils.GenerateBeautifiedPrefabName(x.m_tree)).ToList();
            List<TreeInfo> availableBuildingTreeList = new List<TreeInfo>();
            //  TODO: POPULATE PROP LIST BASED ON ACTUAL, CURRENT PROPS
            foreach (var prop in allBuildingTrees)
            {
                if (prop.m_tree != null)
                {
                    //  'Extreme Mode':
                    //  TODO: verify if this is still an issue: Exclude props with double quotes in name (causes infinite 'Array index is out of range' error loops):
                    if (PropItUpTool.config.enable_extrememode == false && prop.m_tree.name.Contains("\""))
                    {
                        continue;
                    }
                    //  TODO? list each tree (instance) individually (with index), so they can be replaced separately (if possible):
                    if (prop.m_tree == _selectedTreeOriginal)
                    {
                        prop.m_tree = _selectedTreeReplacement;
                    }
                    if (!availableBuildingTreeList.Contains(prop.m_tree))
                    {
                        availableBuildingTreeList.Add(prop.m_tree);
                        _originalTreeFastList.rowsData.Add(prop.m_tree);
                    }
                }
            }
            _originalTreeFastList.rowHeight = UIUtils.c_fastListRowHeight;
            listIsUpdating = false;
            //  Preset FastList:
            _originalTreeFastList.selectedIndex = _selectedTreeOriginalIndex;
            _originalTreeFastList.DisplayAt(_selectedTreeOriginalIndex);
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeCustomizerPanel: OriginalFastList populated with {availableBuildingTreeList.Count} trees for building '{_selectedBuilding.name}'.");
            }
        }
        protected void OnSelectedOriginalChanged(UIComponent component, int i)
        {
            if (listIsUpdating)
            {
                return;
            }
            //  
            _selectedTreeOriginal = _originalTreeFastList.rowsData[i] as TreeInfo;
            _selectedTreeOriginalIndex = i;
            //  Enable Reset Button if selected building has tree replacements and tree replacement is set for selected building:
            Configuration.Building selectedBuilding = PropItUpTool.config.GetBuilding(_selectedBuilding.name);
            if (selectedBuilding == null)
            {
                _resetReplacementButton.isEnabled = false;
            }
            else
            {
                //Configuration.PrefabReplacement selectedReplacement = PropItUpTool.config.GetBuildingPrefabReplacementByIndex(selectedBuilding, "tree", _originalTreeFastList.selectedIndex);
                Configuration.PrefabReplacement selectedReplacement = PropItUpTool.config.GetBuildingReplacementByOriginalPrefabName(selectedBuilding, _selectedTreeOriginal.name);
                if (selectedReplacement != null)
                {
                    _resetReplacementButton.isEnabled = true;
                }
                else
                {
                    _resetReplacementButton.isEnabled = false;
                }
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeCustomizerPanel: OriginalFastList selected: tree '{UIUtils.GenerateBeautifiedPrefabName(_selectedTreeOriginal)}'.");
            }
        }

        public void PopulateAvailableTreesFastList()
        {
            if (PropItUpTool.allAvailableTrees.Count > 0)
            {
                //  Search Query set?
                if (!string.IsNullOrEmpty(searchQuery) && searchQuery != searchboxPlaceholder)
                {
                    Search();
                    return;
                }
                //  TODO: Add 'No replacement' option:

                //  Add all available trees:

                foreach (var tree in PropItUpTool.allAvailableTrees)
                {
                    _replacementTreeFastList.rowsData.Add(tree);
                }
                _replacementTreeFastList.rowHeight = UIUtils.c_fastListRowHeight;
                _replacementTreeFastList.DisplayAt(0);
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"TreeCustomizerPanel: ReplacementFastList populated with {PropItUpTool.allAvailableTrees.Count} trees.");
                }
            }
        }
        protected void OnSelectedReplacementChanged(UIComponent component, int i)
        {
            if (i < 0)
            {
                return;
            }
            _selectedTreeReplacement = _replacementTreeFastList.rowsData[i] as TreeInfo;
            //  TODO: visually highlight selected tree instance:
            //TreeInstance t = TreeManager.instance.m_trees.m_buffer[_selectedTreeOriginal.GetInstanceID()];
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"TreeCustomizerPanel: ReplacementFastList selected: tree '{UIUtils.GenerateBeautifiedPrefabName(_selectedTreeReplacement)}' ('{_selectedTreeReplacement.name}').");
            }
        }

        public void Search()
        {
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
                tmpItemList = PropItUpTool.allAvailableTrees;
            }
            else
            {
                foreach (TreeInfo result in PropItUpTool.allAvailableTrees)
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
                DebugUtils.Log($"TreeCustomizerPanel: search query '{searchQuery}' returned {tmpItemList.Count} results.");
            }
        }

        public void ResetPanel()
        {
            listIsUpdating = true;
            _selectedBuildingLabel.text = "No building selected";
            _resetReplacementButton.isEnabled = false;
            if (_originalTreeFastList.rowsData.m_size > 0)
            {
                _originalTreeFastList.Clear();
                _selectedTreeOriginalIndex = 0;
                _originalTreeFastList.selectedIndex = -1;
            }
            searchQuery = string.Empty;
            _replacementTreeFastListSearchBox.text = string.Empty;
            if (_replacementTreeFastList.rowsData.m_size > 0)
            {
                _replacementTreeFastList.DisplayAt(0);
                _replacementTreeFastList.selectedIndex = -1;
            }
            _selectedBuilding = null;
            _selectedTreeOriginal = null;
            _selectedTreeReplacement = null;
            listIsUpdating = false;
        }
    }
}