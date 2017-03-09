using ICities;
using System;
using UnityEngine;

namespace PropItUp
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            try
            {
                // Create backup:
                PropItUpTool.SaveBackup();
            }
            catch (Exception e)
            {
                DebugUtils.LogException(e);
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            // Check if in-game:
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
            {
                DebugUtils.Log($"Mod not loaded: only available in-game, not in editors.");
                return;
            }
            // 
            PropItUpTool.Initialize();
            PropItUpTool.LoadConfig();

            if (BuildingSelectionTool.instance == null)
            {
                // Create instance:
                ToolController toolController = GameObject.FindObjectOfType<ToolController>();
                BuildingSelectionTool.instance = toolController.gameObject.AddComponent<BuildingSelectionTool>();
            }

            //  Building Prop/Tree Replacements:
            PropItUpTool.ListPropPrefabs();
            PropItUpTool.ListTreePrefabs();
            if (PropItUpTool.config.enable_applybuildingonload && PropItUpTool.config.buildings.Count > 0)
            {
                PropItUpTool.ReplacePrefabsBuilding();
            }

            //  Global Tree Replacements:
            if (PropItUpTool.config.enable_applyglobalonload && PropItUpTool.config.globalTreeReplacements.Count > 0)
            {
                PropItUpTool.ReplaceTreesGlobal();
            }

            base.OnLevelLoaded(mode);
        }

        public override void OnLevelUnloading()
        {
            if (BuildingSelectionTool.instance != null)
            {
                BuildingSelectionTool.instance.enabled = false;
            }
            PropItUpTool.isGameLoaded = false;
            PropItUpTool.Reset();
            //  
            base.OnLevelUnloading();
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}