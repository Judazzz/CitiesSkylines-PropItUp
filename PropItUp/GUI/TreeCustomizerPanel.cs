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
            _resetReplacementButton = UIUtils.CreateResetButton(_originalTreeLabel);
            _resetReplacementButton.eventClicked += (c, e) =>
            {
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"TreeCustomizerPanel: 'Reset replacement' clicked.");
                }
                //  Get original tree:
                Configuration.Building building = PropItUpTool.config.GetBuilding(_selectedBuilding.name);
                Configuration.PrefabReplacement replacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(building, _selectedTreeOriginal.name);
                //  Restore replacement:
                PropItUpTool.RestoreReplacementBuilding(_selectedBuilding, building, replacement);
                //  Repopulate/reset OriginalTreeFastList:
                _selectedTreeReplacement = PrefabCollection<TreeInfo>.FindLoaded(replacement.original);
                PopulateOriginalTreesFastList(c);
                _selectedTreeOriginal = _originalTreeFastList.rowsData[_selectedTreeOriginalIndex] as TreeInfo;
                _resetReplacementButton.isEnabled = false;
                _resetReplacementButton.isVisible = false;
            };
            _resetReplacementButton.isEnabled = false;
            _resetReplacementButton.isVisible = false;
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
            _replacementTreeFastListSearchBox.text = searchboxPlaceholder;
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
                if (_selectedTreeOriginal == null || _selectedTreeReplacement == null)
                {
                    return;
                }
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"TreeCustomizerPanel: 'Replace tree' clicked'.");
                }
                //  Replace tree:
                PropItUpTool.SaveReplacementBuilding("tree", _selectedTreeOriginal, _selectedTreeReplacement, _selectedBuilding);
                //  Repopulate/reset OriginalTreeFastList:
                PopulateOriginalTreesFastList(c);
                _selectedTreeOriginal = _selectedTreeReplacement;
                _resetReplacementButton.isEnabled = true;
                _resetReplacementButton.isVisible = true;
            };
            _saveTreeReplacementButton.isVisible = false;
        }

        public void PopulateOriginalTreesFastList(UIComponent trigger = null)
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
            //  Get distinct, alphabetized List of all trees for building:
            listIsUpdating = true;
            List<BuildingInfo.Prop> allBuildingTrees = new List<BuildingInfo.Prop>();
            foreach (var prop in _selectedBuilding.m_props.Where(x => x.m_tree != null))
            {
                //  'Extreme Mode':
                //  TODO: verify if this is still an issue: Exclude props with double quotes in name (causes infinite 'Array index is out of range' error loops):
                if (PropItUpTool.config.enable_extrememode == false && prop.m_tree.name.Contains("\""))
                {
                    continue;
                }
                if (allBuildingTrees.Where(x => x.m_tree.name == prop.m_tree.name).ToList().Count == 0)
                {
                    allBuildingTrees.Add(prop);
                }
            }
            allBuildingTrees = allBuildingTrees.OrderBy(x => UIUtils.GenerateBeautifiedPrefabName(x.m_tree)).ToList();

            //  
            List<TreeInfo> availableBuildingTreeList = new List<TreeInfo>();
            //  Populate OriginalFastList:
            foreach (var prop in allBuildingTrees)
            {
                if (prop.m_tree != null)
                {
                    //  Skip 'Blacklisted' props:
                    if (!PropItUpTool.allAvailableTrees.Contains(prop.m_tree))
                    {
                        continue;
                    }
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
            int selectedRow = -1;
            if (trigger)
            {
                TreeInfo target = new TreeInfo();
                if (trigger.name == "resetReplacementButton")
                {
                    target = _selectedTreeReplacement;
                }
                else
                {
                    target = _selectedTreeReplacement;
                }
                //  
                for (int i = 0; i < availableBuildingTreeList.Count; i++)
                {
                    TreeInfo tmp = _originalTreeFastList.rowsData[i] as TreeInfo;
                    if (tmp.name == target.name)
                    {
                        selectedRow = i;
                        break;
                    }
                }
            }
            if (selectedRow == -1 || _originalTreeFastList.rowsData.m_size == 1)
            {
                if (trigger == null) {
                    _originalTreeFastList.selectedIndex = -1;
                }
                _originalTreeFastList.DisplayAt(0);
                _originalTreeFastList.selectedIndex = 0;
            }
            else
            {
                _originalTreeFastList.selectedIndex = selectedRow;
                _originalTreeFastList.DisplayAt(selectedRow);
            }
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
                _resetReplacementButton.isVisible = false;
            }
            else
            {
                Configuration.PrefabReplacement selectedReplacement = PropItUpTool.config.GetBuildingReplacementByReplacementPrefabName(selectedBuilding, _selectedTreeOriginal.name);
                if (selectedReplacement != null)
                {
                    if (selectedReplacement.is_visible)
                    {
                        _resetReplacementButton.isEnabled = true;
                        _resetReplacementButton.isVisible = true;
                    }
                    else
                    {
                        _resetReplacementButton.isEnabled = false;
                        _resetReplacementButton.isVisible = false;
                    }
                }
                else
                {
                    _resetReplacementButton.isEnabled = false;
                    _resetReplacementButton.isVisible = false;
                }
            }
            //  
            if (_originalTreeFastList.selectedIndex >= 0 && _replacementTreeFastList.selectedIndex >= 0)
            {
                _saveTreeReplacementButton.isVisible = true;
            }
            else
            {
                _saveTreeReplacementButton.isVisible = false;
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
            if (_originalTreeFastList.selectedIndex >= 0 && _replacementTreeFastList.selectedIndex >= 0)
            {
                _saveTreeReplacementButton.isVisible = true;
            }
            else
            {
                _saveTreeReplacementButton.isVisible = false;
            }
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
            _saveTreeReplacementButton.isVisible = false;
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
            _resetReplacementButton.isVisible = false;
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