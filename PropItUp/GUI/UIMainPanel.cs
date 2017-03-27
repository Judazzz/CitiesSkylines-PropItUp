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
            width = PropItUpTool.SPACING + PropItUpTool.WIDTH;
            height = PropItUpTool.TITLE_HEIGHT + PropItUpTool.TABS_HEIGHT + PropItUpTool.HEIGHT + PropItUpTool.SPACING;
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
            panelTabs.relativePosition = new Vector2(10, PropItUpTool.TITLE_HEIGHT + PropItUpTool.SPACING);
            panelTabs.size = new Vector2(PropItUpTool.WIDTH - (3 * PropItUpTool.SPACING), PropItUpTool.TABS_HEIGHT);

            //  Tab Buttons:
            //  Global Tree Replacer Button:
            treeReplacerButton = UIUtils.CreateTab(panelTabs, "Global");
            treeReplacerButton.name = "treeReplacerButton";
            treeReplacerButton.tooltip = "Replace free-standing trees globally";
            treeReplacerButton.textScale = 0.8f;
            //treeReplacerButton.width = 60f;
            treeReplacerButton.width = 64f;
            //  Global BuildingTree Replacer Button:
            buildingTreeReplacerButton = UIUtils.CreateTab(panelTabs, "Building");
            buildingTreeReplacerButton.name = "buildingTreeReplacerButton";
            buildingTreeReplacerButton.tooltip = "Replace building trees globally";
            buildingTreeReplacerButton.textScale = 0.8f;
            //buildingTreeReplacerButton.width = 60f;
            buildingTreeReplacerButton.width = 64f;
            //  Prop Customizer Button:
            propCustomizerButton = UIUtils.CreateTab(panelTabs, "Props");
            propCustomizerButton.name = "propCustomizerButton";
            propCustomizerButton.tooltip = "Replace props per building";
            propCustomizerButton.textScale = 0.8f;
            //propCustomizerButton.width = 100f;
            propCustomizerButton.width = 64f;
            //  Tree Customizer Button:
            treeCustomizerButton = UIUtils.CreateTab(panelTabs, "Trees");
            treeCustomizerButton.name = "treeCustomizerButton";
            treeCustomizerButton.tooltip = "Replace trees per building";
            treeCustomizerButton.textScale = 0.8f;
            //treeCustomizerButton.width = 100f;
            treeCustomizerButton.width = 64f;
            //  Tab Button Events:
            treeReplacerButton.eventClick += (c, e) => TabClicked(c, e);
            buildingTreeReplacerButton.eventClick += (c, e) => TabClicked(c, e);
            treeCustomizerButton.eventClick += (c, e) => TabClicked(c, e);
            propCustomizerButton.eventClick += (c, e) => TabClicked(c, e);

            //  Main Panel:
            UIPanel body = AddUIComponent<UIPanel>();
            body.width = PropItUpTool.WIDTH;
            body.height = PropItUpTool.HEIGHT;
            //  ScrollRect
            body.relativePosition = new Vector3(5, 36 + 28 + 5);

            //  Section Panels:
            //  Global Tree Replacer Panel:
            treeReplacerPanel = body.AddUIComponent<TreeReplacerPanel>();
            treeReplacerPanel.name = "treeReplacerPanel";
            treeReplacerPanel.width = PropItUpTool.WIDTH - (3 * PropItUpTool.SPACING);
            treeReplacerPanel.height = PropItUpTool.HEIGHT;
            treeReplacerPanel.relativePosition = new Vector3(5, 0);
            treeReplacerPanel.isVisible = true;
            //  Global Building Tree Replacer Panel:
            buildingTreeReplacerPanel = body.AddUIComponent<BuildingTreeReplacerPanel>();
            buildingTreeReplacerPanel.name = "buildingTreeReplacerPanel";
            buildingTreeReplacerPanel.width = PropItUpTool.WIDTH - (3 * PropItUpTool.SPACING);
            buildingTreeReplacerPanel.height = PropItUpTool.HEIGHT;
            buildingTreeReplacerPanel.relativePosition = new Vector3(5, 0);
            buildingTreeReplacerPanel.isVisible = false;
            //  Prefab Tree Customizer Panel:
            propCustomizerPanel = body.AddUIComponent<PropCustomizerPanel>();
            propCustomizerPanel.name = "propCustomizerPanel";
            propCustomizerPanel.width = PropItUpTool.WIDTH - (3 * PropItUpTool.SPACING);
            propCustomizerPanel.height = PropItUpTool.HEIGHT;
            propCustomizerPanel.relativePosition = new Vector3(5, 0);
            propCustomizerPanel.isVisible = false;
            //  Prefab Prop Customizer Panel:
            treeCustomizerPanel = body.AddUIComponent<TreeCustomizerPanel>();
            treeCustomizerPanel.name = "treeCustomizerPanel";
            treeCustomizerPanel.width = PropItUpTool.WIDTH - (3 * PropItUpTool.SPACING);
            treeCustomizerPanel.height = PropItUpTool.HEIGHT;
            treeCustomizerPanel.relativePosition = new Vector3(5, 0);
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
            //  TODO: canel BuildingSelectionTool's selected building + building highlight:
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