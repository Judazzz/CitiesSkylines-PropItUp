using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System.Linq;
using UnityEngine;

namespace PropItUp.GUI
{
    public class UIMainPanel : UIPanel
    {
        public UIMainTitleBar m_title;

        public UITabstrip panelTabs;
        public UIButton treeReplacerButton;
        public UIButton buildingTreeReplacerButton;
        public UIButton treeCustomizerButton;
        public UIButton propCustomizerButton;

        public TreeReplacerPanel treeReplacerPanel;
        public BuildingTreeReplacerPanel buildingTreeReplacerPanel;
        public TreeCustomizerPanel treeCustomizerPanel;
        public PropCustomizerPanel propCustomizerPanel;

        private BuildingSelectionTool buildingSelectionTool = ToolsModifierControl.toolController.gameObject.AddComponent<BuildingSelectionTool>();
        
        public static UIMainPanel _instance;
        public static UIMainPanel instance
        {
            get { return _instance; }
        }

        public static void Initialize()
        {
        }

        public override void Start()
        {
            base.Start();
            _instance = this;
            //  
            backgroundSprite = "LevelBarBackground";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            name = "modMainPanel";
            padding = new RectOffset(10, 10, 4, 4);
            width = UIUtils.c_modPanelWidth;
            height = UIUtils.c_modPanelHeight;
            relativePosition = new Vector3(width + 25, 60);
            //  
            SetupControls();
            //  
            buildingSelectionTool = ToolsModifierControl.toolController.gameObject.AddComponent<BuildingSelectionTool>();
        }

        public void SetupControls()
        {
            //  Title Bar:
            m_title = AddUIComponent<UIMainTitleBar>();
            m_title.title = "Prop it Up! " + PropItUpTool.config.version;

            //  Tabs:
            panelTabs = AddUIComponent<UITabstrip>();
            panelTabs.size = new Vector2(UIUtils.c_modPanelInnerWidth, UIUtils.c_tabButtonHeight);
            panelTabs.relativePosition = new Vector2(UIUtils.c_spacing, UIUtils.c_titleBarHeight + UIUtils.c_spacing);

            //  Tab Buttons:
            //  Global Tree Replacer Button:
            treeReplacerButton = UIUtils.CreateTab(panelTabs, "Global");
            treeReplacerButton.name = "treeReplacerButton";
            treeReplacerButton.tooltip = "Replace free-standing trees globally";
            treeReplacerButton.textScale = 0.9f;
            treeReplacerButton.width = UIUtils.c_tabButtonWidth;
            treeReplacerButton.height = UIUtils.c_tabButtonHeight;
            //  Global BuildingTree Replacer Button:
            buildingTreeReplacerButton = UIUtils.CreateTab(panelTabs, "Building");
            buildingTreeReplacerButton.name = "buildingTreeReplacerButton";
            buildingTreeReplacerButton.tooltip = "Replace building trees globally";
            buildingTreeReplacerButton.textScale = 0.9f;
            buildingTreeReplacerButton.width = UIUtils.c_tabButtonWidth;
            buildingTreeReplacerButton.height = UIUtils.c_tabButtonHeight;
            //  Prop Customizer Button:
            propCustomizerButton = UIUtils.CreateTab(panelTabs, "Props");
            propCustomizerButton.name = "propCustomizerButton";
            propCustomizerButton.tooltip = "Replace props per building";
            propCustomizerButton.textScale = 0.9f;
            propCustomizerButton.width = UIUtils.c_tabButtonWidth;
            propCustomizerButton.height = UIUtils.c_tabButtonHeight;
            //  Tree Customizer Button:
            treeCustomizerButton = UIUtils.CreateTab(panelTabs, "Trees");
            treeCustomizerButton.name = "treeCustomizerButton";
            treeCustomizerButton.tooltip = "Replace trees per building";
            treeCustomizerButton.textScale = 0.9f;
            treeCustomizerButton.width = UIUtils.c_tabButtonWidth;
            treeCustomizerButton.height = UIUtils.c_tabButtonHeight;
            //  Tab Button Events:
            treeReplacerButton.eventClick += (c, e) => TabClicked(c, e);
            buildingTreeReplacerButton.eventClick += (c, e) => TabClicked(c, e);
            treeCustomizerButton.eventClick += (c, e) => TabClicked(c, e);
            propCustomizerButton.eventClick += (c, e) => TabClicked(c, e);

            //  Main Panel:
            UIPanel body = AddUIComponent<UIPanel>();
            body.name = "modPanelContainer";
            body.width = UIUtils.c_modPanelInnerWidth;
            body.height = UIUtils.c_modPanelInnerHeight;
            //  ScrollRect
            body.relativePosition = new Vector3(5, 36 + 28 + 5);

            //  Section Panels:
            //  Global Tree Replacer Panel:
            treeReplacerPanel = body.AddUIComponent<TreeReplacerPanel>();
            treeReplacerPanel.name = "treeReplacerPanel";
            treeReplacerPanel.width = UIUtils.c_modPanelInnerWidth;
            treeReplacerPanel.height = UIUtils.c_modPanelInnerHeight;
            treeReplacerPanel.relativePosition = Vector3.zero;
            treeReplacerPanel.isVisible = true;
            //  Global Building Tree Replacer Panel:
            buildingTreeReplacerPanel = body.AddUIComponent<BuildingTreeReplacerPanel>();
            buildingTreeReplacerPanel.name = "buildingTreeReplacerPanel";
            buildingTreeReplacerPanel.width = UIUtils.c_modPanelInnerWidth;
            buildingTreeReplacerPanel.height = UIUtils.c_modPanelInnerHeight;
            buildingTreeReplacerPanel.relativePosition = Vector3.zero;
            buildingTreeReplacerPanel.isVisible = false;
            //  Prefab Tree Customizer Panel:
            propCustomizerPanel = body.AddUIComponent<PropCustomizerPanel>();
            propCustomizerPanel.name = "propCustomizerPanel";
            propCustomizerPanel.width = UIUtils.c_modPanelInnerWidth;
            propCustomizerPanel.height = UIUtils.c_modPanelInnerHeight;
            propCustomizerPanel.relativePosition = Vector3.zero;
            propCustomizerPanel.isVisible = false;
            //  Prefab Prop Customizer Panel:
            treeCustomizerPanel = body.AddUIComponent<TreeCustomizerPanel>();
            treeCustomizerPanel.name = "treeCustomizerPanel";
            treeCustomizerPanel.width = UIUtils.c_modPanelInnerWidth;
            treeCustomizerPanel.height = UIUtils.c_modPanelInnerHeight;
            treeCustomizerPanel.relativePosition = Vector3.zero;
            treeCustomizerPanel.isVisible = false;
        }

        private void TabClicked(UIComponent trigger, UIMouseEventParameter e)
        {
            if (PropItUpTool.config.enable_debug)
            {
                DebugUtils.Log($"MainPanel: Tab '{trigger.name}' clicked");
            }
            //  
            treeReplacerPanel.isVisible = false;
            buildingTreeReplacerPanel.isVisible = false;
            propCustomizerPanel.isVisible = false;
            treeCustomizerPanel.isVisible = false;

            if (trigger == treeReplacerButton)
            {
                treeReplacerPanel.isVisible = true;
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            else if (trigger == buildingTreeReplacerButton)
            {
                buildingTreeReplacerPanel.isVisible = true;
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            else if (trigger == propCustomizerButton)
            {
                ToolsModifierControl.toolController.CurrentTool = buildingSelectionTool;
                ToolsModifierControl.SetTool<BuildingSelectionTool>();
                //  Refresh prop list if Aedificium mod is detected (to include newly hotloaded trees):
                if (PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == 793489846 && mod.isEnabled)))
                {
                    PropItUpTool.ListPropPrefabs(true);
                    propCustomizerPanel.PopulateAvailablePropsFastList();
                }
                propCustomizerPanel.isVisible = true;
            }
            else if (trigger == treeCustomizerButton)
            {
                ToolsModifierControl.toolController.CurrentTool = buildingSelectionTool;
                ToolsModifierControl.SetTool<BuildingSelectionTool>();
                //  Refresh prop list if Aedificium mod is detected (to include newly hotloaded props):
                if (PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == 793489846 && mod.isEnabled)))
                {
                    PropItUpTool.ListTreePrefabs(true);
                    treeCustomizerPanel.PopulateAvailableTreesFastList();
                }
                treeCustomizerPanel.isVisible = true;
            }
        }

        //  Toggle main panel and update button state:
        public void Toggle()
        {
            //  TODO: cancel BuildingSelectionTool's selected building + building highlight:
            //  Reset ModPanels:
            TreeReplacerPanel.instance.Reset();
            BuildingTreeReplacerPanel.instance.Reset();
            PropCustomizerPanel.instance.Reset();
            TreeCustomizerPanel.instance.Reset();
            if (_instance.isVisible)
            {
                //  Hide MainPanel:
                _instance.isVisible = false;
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            else
            {
                //  Show MainPanel:
                _instance.isVisible = true;
                //  Reset TabMenu:
                ResetTabMenu();
                //  Show ModPanel:
                treeReplacerPanel.isVisible = true;
                buildingTreeReplacerPanel.isVisible = false;
                propCustomizerPanel.isVisible = false;
                treeCustomizerPanel.isVisible = false;
            }
            //  Mod Main Button state:
            if (_instance.isVisible)
            {
                UIMainButton.instance.state = UIButton.ButtonState.Focused;
            }
            else
            {
                UIMainButton.instance.state = UIButton.ButtonState.Normal;
            }
        }

        //  Reset TabMenu:
        public void ResetTabMenu()
        {
            treeReplacerButton.SimulateClick();
            treeReplacerButton.Focus();
            buildingTreeReplacerButton.Unfocus();
            propCustomizerButton.Unfocus();
            treeCustomizerButton.Unfocus();
        }
    }
}