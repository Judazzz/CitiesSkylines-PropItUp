using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace PropItUp.GUI
{
    public class PropCustomizerPanel : UIPanel
    {
        private UILabel _selectedBuildingLabel;
        private UILabel _originalPropLabel;
        private UIButton _resetReplacementButton;
        private UIFastList _originalPropFastList;
        private UILabel _replacementPropLabel;
        private UITextField _replacementPropFastListSearchBox;
        private UIFastList _replacementPropFastList;
        private UIButton _savePropReplacementButton;

        public UILabel selectedBuildingLabel
        {
            get { return _selectedBuildingLabel; }
            set { _selectedBuildingLabel = value; }
        }

        public UIFastList originalPropFastList
        {
            get { return _originalPropFastList; }
        }
        public UITextField replacementPropFastListSearchBox
        {
            get { return _replacementPropFastListSearchBox; }
        }
        public UIFastList replacementPropFastList
        {
            get { return _replacementPropFastList; }
        }
        public UIButton savePropReplacementButton
        {
            get { return _savePropReplacementButton; }
        }

        private int _selectedPropOriginalIndex = 0;
        public int selectedPropOriginalIndex
        {
            get { return _selectedPropOriginalIndex; }
        }

        private PropInfo _selectedPropOriginal;
        public PropInfo selectedPropOriginal
        {
            get { return _selectedPropOriginal; }
            set { _selectedPropOriginal = value; }
        }

        private PropInfo _selectedPropReplacement;
        public PropInfo selectedPropReplacement
        {
            get { return _selectedPropReplacement; }
            set { _selectedPropReplacement = value; }
        }

        private BuildingInfo _selectedBuilding;
        public BuildingInfo selectedBuilding
        {
            get { return _selectedBuilding; }
            set { _selectedBuilding = value; }
        }

        private static PropCustomizerPanel _instance;
        public static PropCustomizerPanel instance
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
            PopulateAvailablePropsFastList();
        }

        private void SetupControls()
        {
            var originalContainer = UIUtils.CreateFormElement(this, "top");
            originalContainer.name = "originalContainer";

            //  Source Container:
            _selectedBuildingLabel = originalContainer.AddUIComponent<UILabel>();
            _selectedBuildingLabel.text = "No building selected";
            _selectedBuildingLabel.textColor = new Color(187, 187, 187, 255);
            _selectedBuildingLabel.textScale = 0.8f;
            _selectedBuildingLabel.padding = new RectOffset(0, 0, 0, 5);
            //  Label:
            _originalPropLabel = originalContainer.AddUIComponent<UILabel>();
            _originalPropLabel.text = "Included props";
            _originalPropLabel.textScale = 0.8f;
            _originalPropLabel.padding = new RectOffset(0, 0, 0, 5);
            //  'Reset replacement' Button
            _resetReplacementButton = UIUtils.CreateButton(_originalPropLabel);
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
                    DebugUtils.Log($"PropCustomizerPanel: 'Reset replacement' clicked'.");
                }
                //  
                PropItUpTool.RestoreReplacementBuilding("prop");
                //  Repopulate/reset OriginalPropFastList:
                PopulateIncludedPropsFastList();
                _selectedPropOriginal = _originalPropFastList.rowsData[_selectedPropOriginalIndex] as PropInfo; ;
                _resetReplacementButton.isEnabled = false;
            };
            _resetReplacementButton.isEnabled = false;
            // FastList
            _originalPropFastList = UIFastList.Create<UIPropItem>(originalContainer);
            _originalPropFastList.backgroundSprite = "UnlockingPanel";
            _originalPropFastList.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _originalPropFastList.height = 90;
            _originalPropFastList.canSelect = true;
            _originalPropFastList.eventSelectedIndexChanged += OnSelectedOriginalChanged;

            // Replacement Label Container:
            var replacementContainer = UIUtils.CreateFormElement(this, "center");
            replacementContainer.name = "replacementContainer";
            replacementContainer.relativePosition = new Vector3(0, 165);
            //  Label:
            _replacementPropLabel = originalContainer.AddUIComponent<UILabel>();
            _replacementPropLabel.text = "Select replacement prop";
            _replacementPropLabel.textScale = 0.8f;
            _replacementPropLabel.padding = new RectOffset(0, 0, 15, 5);
            // Search Box Container:
            var searchboxContainer = UIUtils.CreateFormElement(this, "center");
            searchboxContainer.name = "searchboxContainer";
            searchboxContainer.relativePosition = new Vector3(0, 182);
            //  Search Box:
            _replacementPropFastListSearchBox = UIUtils.CreateTextField(searchboxContainer);
            _replacementPropFastListSearchBox.position = new Vector3(_selectedBuildingLabel.relativePosition.x, 205);
            _replacementPropFastListSearchBox.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _replacementPropFastListSearchBox.height = 25;
            _replacementPropFastListSearchBox.padding = new RectOffset(6, 6, 6, 6);
            _replacementPropFastListSearchBox.normalBgSprite = "TextFieldUnderline";
            _replacementPropFastListSearchBox.hoveredBgSprite = "TextFieldUnderline";
            _replacementPropFastListSearchBox.disabledBgSprite = "TextFieldUnderline";
            _replacementPropFastListSearchBox.focusedBgSprite = "LevelBarBackground";
            _replacementPropFastListSearchBox.horizontalAlignment = UIHorizontalAlignment.Left;
            _replacementPropFastListSearchBox.text = "Find a prop";
            _replacementPropFastListSearchBox.textColor = new Color32(187, 187, 187, 255);
            _replacementPropFastListSearchBox.textScale = 0.75f;
            //  Search Box Events:
            _replacementPropFastListSearchBox.eventTextChanged += (c, p) =>
            {
                searchQuery = p;
                Search();
            };
            _replacementPropFastListSearchBox.eventGotFocus += (c, p) =>
            {
                _replacementPropFastList.selectedIndex = -1;
                if (_replacementPropFastListSearchBox.text == "Find a prop")
                {
                    _replacementPropFastListSearchBox.text = string.Empty;
                }
            };
            _replacementPropFastListSearchBox.eventLostFocus += (c, p) =>
            {
                if (_replacementPropFastListSearchBox.text == string.Empty)
                {
                    _replacementPropFastListSearchBox.text = "Find a prop";
                }
            };

            // FastList Container:
            var fastlistContainer = UIUtils.CreateFormElement(this, "center");
            fastlistContainer.name = "fastlistContainer";
            fastlistContainer.relativePosition = new Vector3(0, 205);
            //  FastList:
            _replacementPropFastList = UIFastList.Create<UIPropItem>(fastlistContainer);
            _replacementPropFastList.position = new Vector3(_selectedBuildingLabel.relativePosition.x, 233);
            _replacementPropFastList.width = parent.width - (3 * PropItUpTool.SPACING) - 12;
            _replacementPropFastList.height = 90;
            _replacementPropFastList.backgroundSprite = "UnlockingPanel";
            _replacementPropFastList.canSelect = true;
            _replacementPropFastList.eventSelectedIndexChanged += OnSelectedReplacementChanged;

            //  Button Container:
            var buttonContainer = UIUtils.CreateFormElement(this, "bottom");
            //  Buttons:
            _savePropReplacementButton = UIUtils.CreateButton(buttonContainer);
            _savePropReplacementButton.relativePosition = new Vector3(5, 10);
            _savePropReplacementButton.width = 110;
            _savePropReplacementButton.name = "savePropReplacementButton";
            _savePropReplacementButton.text = "Replace prop";
            _savePropReplacementButton.tooltip = "Replace selected prop with selected replacement prop.";
            _savePropReplacementButton.eventClicked += (c, e) =>
            {
                //  Only save if original and replacement are selected:
                if (selectedPropOriginal == null || selectedPropReplacement == null)
                {
                    return;
                }
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"PropCustomizerPanel: 'Replace prop' clicked'.");
                }
                PropItUpTool.SaveReplacementBuilding("prop");
                //  Repopulate/reset OriginalPropFastList:
                PopulateIncludedPropsFastList();
                _selectedPropOriginal = _selectedPropReplacement;
                _resetReplacementButton.isEnabled = true;
            };
        }

        public void PopulateIncludedPropsFastList()
        {
            //  Set selected building label:
            selectedBuildingLabel.text =
                $"{UIUtils.GenerateBeautifiedPrefabName(BuildingSelectionTool.instance.m_selectedBuilding)}";
            UIUtils.TruncateLabel(selectedBuildingLabel, _replacementPropFastListSearchBox.width); // ({BuildingSelectionTool.instance.m_selectedBuildingInstanceId})
            //  Null/empty check:
            if (BuildingSelectionTool.instance.m_selectedBuilding.m_props == null || BuildingSelectionTool.instance.m_selectedBuilding.m_props.Length == 0)
            {
                return;
            }
            //  Clear FastList:
            if (_originalPropFastList.rowsData.m_size > 0)
            {
                _originalPropFastList.Clear();
            }
            //  List all props in selected building:
            listIsUpdating = true;
            List<PropInfo> selectedBuildingPropList = new List<PropInfo>();
            foreach (var prop in BuildingSelectionTool.instance.m_selectedBuilding.m_props)
            {
                if (prop.m_prop != null)
                {
                    //  Exclude props without LOD/with double quotes in name (causes infinite 'Array index is out of range' error loops):
                    if (prop.m_prop.name.Contains("\"") || prop.m_prop.m_lodMesh == null || prop.m_prop.m_lodObject == null)
                    {
                        continue;
                    }
                    //  TODO: list each prop (instance) individually (with index), so they can be replaced separately (if possible):
                    if (!selectedBuildingPropList.Contains(prop.m_prop))
                    {
                        selectedBuildingPropList.Add(prop.m_prop);
                        _originalPropFastList.rowsData.Add(prop.m_prop);
                    }
                }
            }
            _originalPropFastList.rowHeight = 26f;
            listIsUpdating = false;
            //  Preset FastList:
            _originalPropFastList.selectedIndex = _selectedPropOriginalIndex;
            _originalPropFastList.DisplayAt(_selectedPropOriginalIndex);
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"PropCustomizerPanel: OriginalFastList populated with {selectedBuildingPropList.Count} props for building '{BuildingSelectionTool.instance.m_selectedBuilding.name}'.");
            }
        }
        protected void OnSelectedOriginalChanged(UIComponent component, int i)
        {
            if (listIsUpdating)
            {
                return;
            }
            //  
            if (_originalPropFastList.rowsData.m_size > (_selectedPropOriginalIndex + 1))
            {
                _selectedPropOriginal = _originalPropFastList.rowsData[i] as PropInfo;
                _selectedPropOriginalIndex = i;
                //  Enable Reset Button if selected building has tree replacements and tree replacement is set for selected building:
                Configuration.Building selectedBuilding = PropItUpTool.config.GetBuilding(BuildingSelectionTool.instance.m_selectedBuilding.name);
                if (selectedBuilding == null)
                {
                    _resetReplacementButton.isEnabled = false;
                }
                else
                {
                    Configuration.PrefabReplacement selectedReplacement = PropItUpTool.config.GetBuildingPrefabReplacementByIndex(selectedBuilding, "prop", _selectedPropOriginalIndex);
                    if (selectedReplacement != null)
                    {
                        _resetReplacementButton.isEnabled = true;
                    }
                    else
                    {
                        _resetReplacementButton.isEnabled = false;
                    }
                }
                //  Update/filter availablePropsList:
                FilterAvailablePropsFastList();
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"PropCustomizerPanel: OriginalFastList selected: prop '{UIUtils.GenerateBeautifiedPrefabName(_selectedPropOriginal)}'.");
                }
            }
        }

        public void PopulateAvailablePropsFastList()
        {
            if (PropItUpTool.allAvailableProps.Count > 0)
            {
                //  TODO: Add 'No replacement' option:

                //  Add all available props:
                foreach (var prop in PropItUpTool.allAvailableProps)
                {
                    _replacementPropFastList.rowsData.Add(prop);
                }
                _replacementPropFastList.rowHeight = 26f;
                _replacementPropFastList.DisplayAt(0);
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"PropCustomizerPanel: ReplacementFastList populated with {PropItUpTool.allAvailableProps.Count} props.");
                }
                //}
            }
        }
        public void FilterAvailablePropsFastList()
        {
            if (PropItUpTool.allAvailableProps.Count > 0)
            {
                //  TODO: Add 'No replacement' option:

                //  Add all available props:
                _replacementPropFastList.Clear();

                //  Search Query set?
                if (searchQuery != string.Empty)
                {
                    Search();
                }
                else
                {
                    foreach (var prop in PropItUpTool.allAvailableProps)
                    {
                        if (prop.m_isDecal != _selectedPropOriginal.m_isDecal)
                        {
                            continue;
                        }
                        if (prop.m_specialPlaces.Length != _selectedPropOriginal.m_specialPlaces.Length)
                        {
                            continue;
                        }
                        _replacementPropFastList.rowsData.Add(prop);
                    }
                    _replacementPropFastList.rowHeight = 26f;
                    _replacementPropFastList.DisplayAt(0);
                }
                //  
                if (PropItUpTool.config.enable_debug)
                {
                    DebugUtils.Log($"PropCustomizerPanel: ReplacementFastList re-populated with {PropItUpTool.allAvailableProps.Count} props.");
                }
                //}
            }
        }
        protected void OnSelectedReplacementChanged(UIComponent component, int i)
        {
            if (i < 0)
            {
                return;
            }
            _selectedPropReplacement = _replacementPropFastList.rowsData[i] as PropInfo;

            //  TODO: visually highlight selected prop instance:
            //PropInstance p = PropManager.instance.m_props.m_buffer[_selectedPropOriginal.GetInstanceID()];

            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"PropCustomizerPanel: ReplacementFastList selected: prop '{UIUtils.GenerateBeautifiedPrefabName(_selectedPropReplacement)}' ('{_selectedPropReplacement.name}').");
            }
        }

        public void Search()
        {
            //  Deselect and clear FastList:
            _replacementPropFastList.selectedIndex = -1;
            _replacementPropFastList.Clear();
            _selectedPropReplacement = null;
            //  Create temporary list for search results:
            List<PropInfo> tmpItemList = new List<PropInfo>();
            foreach (PropInfo result in PropItUpTool.allAvailableProps)
            {
                if (result.name.ToLower().Contains(searchQuery.ToLower()))
                {
                    if (_selectedPropOriginal != null)
                    {
                        if (result.m_isDecal != _selectedPropOriginal.m_isDecal)
                        {
                            continue;
                        }
                        if (result.m_specialPlaces.Length != _selectedPropOriginal.m_specialPlaces.Length)
                        {
                            continue;
                        }
                    }
                    tmpItemList.Add(result);
                }
            }
            //  Repopulate with search results, and show at first item if results are found:
            for (int i = 0; i < tmpItemList.Count; i++)
            {
                if (tmpItemList[i] != null)
                {
                    _replacementPropFastList.rowsData.Add(tmpItemList[i]);
                }
            }
            if (tmpItemList.Count > 0)
            {
                _replacementPropFastList.DisplayAt(0);
            }
            //  
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"PropCustomizerPanel: search query '{searchQuery}' returned {tmpItemList.Count} results.");
            }
        }

        public void ResetPanel()
        {
            listIsUpdating = true;
            _selectedBuildingLabel.text = "No building selected";
            _resetReplacementButton.isEnabled = false;
            if (_originalPropFastList.rowsData.m_size > 0)
            {
                _selectedPropOriginalIndex = 0;
                _originalPropFastList.selectedIndex = -1;
            }
            _originalPropFastList.Clear();
            searchQuery = string.Empty;
            _replacementPropFastListSearchBox.text = string.Empty;
            if (_replacementPropFastList.rowsData.m_size > 0)
            {
                _replacementPropFastList.DisplayAt(0);
                _replacementPropFastList.selectedIndex = -1;
            }
            _selectedBuilding = null;
            _selectedPropOriginal = null;
            _selectedPropReplacement = null;
            listIsUpdating = false;
        }
    }
}