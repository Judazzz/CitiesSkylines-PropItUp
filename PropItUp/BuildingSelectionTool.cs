using PropItUp.GUI;
using UnityEngine;

namespace PropItUp
{
    class BuildingSelectionTool : ToolBase
    {
        private bool m_prevRenderZones;
        private ToolBase m_prevTool;

        private Selectable m_hoverInstance;

        public Selectable hoverInstance
        {
            get { return m_hoverInstance; }
            set { m_hoverInstance = value; }
        }

        public BuildingInfo m_selectedBuilding;

        private static Color m_hoverSelectableColor = new Color32(0, 181, 255, 255);
        private static Color m_hoverUnselectableColor = new Color32(204, 0, 0, 255);
        private static Color m_selectedColor = new Color32(95, 166, 0, 244);

        public static RenderManager.CameraInfo m_cameraInfo;

        public bool toolLocked = false;

        protected override void Awake()
        {
            m_toolController = GameObject.FindObjectOfType<ToolController>();
            enabled = false;
            DebugUtils.Log("Building selection tool awake.");

            base.Awake();
        }

        protected override void OnEnable()
        {
            InfoManager.InfoMode infoMode = InfoManager.instance.CurrentMode;
            InfoManager.SubInfoMode subInfoMode = InfoManager.instance.CurrentSubMode;

            m_prevRenderZones = TerrainManager.instance.RenderZones;
            m_prevTool = m_toolController.CurrentTool;

            m_toolController.CurrentTool = this;

            InfoManager.instance.SetCurrentMode(infoMode, subInfoMode);
            TerrainManager.instance.RenderZones = true;
            DebugUtils.Log("Building selection tool engaged.");
        }

        protected override void OnDisable()
        {
            TerrainManager.instance.RenderZones = m_prevRenderZones;
            if (m_toolController.NextTool == null && m_prevTool != null && m_prevTool != this)
            {
                m_prevTool.enabled = true;
            }
            m_prevTool = null;
            DebugUtils.Log("Building selection tool disengaged.");
        }

        protected override void OnToolUpdate()
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!m_toolController.IsInsideUI && Cursor.visible)
            {
                RaycastHoverInstance(mouseRay);
                //  TODO: Avoid repopulating fastlist after re-clicking on already selected building:
                if (m_hoverInstance != null)
                {
                    //  Select
                    if (Input.GetMouseButtonDown(0))
                    {
                        toolLocked = true;
                        //  
                        PropCustomizerPanel.instance.PopulateIncludedPropsFastList();
                        TreeCustomizerPanel.instance.PopulateIncludedTreesFastList();
                    }
                    //  Deselect
                    if (Input.GetMouseButtonDown(1))
                    {
                        toolLocked = false;
                        m_hoverInstance = null;
                        m_selectedBuilding = null;
                        PropCustomizerPanel.instance.ResetPanel();
                        TreeCustomizerPanel.instance.ResetPanel();
                    }
                }
            }
        }

        private void RaycastHoverInstance(Ray mouseRay)
        {
            if (!toolLocked)
            {
                RaycastInput input = new RaycastInput(mouseRay, Camera.main.farClipPlane);
                RaycastOutput output;

                input.m_netService.m_itemLayers = GetItemLayers();
                input.m_ignoreTerrain = true;

                input.m_ignoreSegmentFlags = NetSegment.Flags.All;
                input.m_ignoreBuildingFlags = Building.Flags.None;
                input.m_ignorePropFlags = PropInstance.Flags.All;
                input.m_ignoreTreeFlags = TreeInstance.Flags.All;

                m_hoverInstance = null;

                if (ToolBase.RayCast(input, out output))
                {
                    InstanceID id = default(InstanceID);

                    if (output.m_building != 0)
                    {
                        id.Building = Building.FindParentBuilding(output.m_building);
                        if (id.Building == 0) id.Building = output.m_building;
                        m_hoverInstance = new Selectable(id);
                    }
                }
            }
        }

        private ItemClass.Layer GetItemLayers()
        {
            ItemClass.Layer itemLayers = ItemClass.Layer.Default;

            if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.Water)
            {
                itemLayers = itemLayers | ItemClass.Layer.WaterPipes;
            }

            if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.Traffic || InfoManager.instance.CurrentMode == InfoManager.InfoMode.Transport)
            {
                itemLayers = itemLayers | ItemClass.Layer.MetroTunnels;
            }

            return itemLayers;
        }

        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderGeometry(cameraInfo);
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (m_hoverInstance != null && m_hoverInstance.isValid)
            {
                m_cameraInfo = cameraInfo;
                m_hoverInstance.RenderOverlay(cameraInfo, m_hoverSelectableColor);
            }
            base.RenderOverlay(cameraInfo);
        }
    }
}