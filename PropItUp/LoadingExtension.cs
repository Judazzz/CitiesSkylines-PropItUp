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

            //  Add BuildingSelectionTool to ToolController:
            if (ToolsModifierControl.GetTool<BuildingSelectionTool>() == null)
            {
                ToolsModifierControl.toolController.gameObject.AddComponent<BuildingSelectionTool>();
            }

            //  Building Prop/Tree Replacements:
            PropItUpTool.ListPropPrefabs();
            PropItUpTool.ListTreePrefabs();
            if (PropItUpTool.config.enable_applybuildingonload && PropItUpTool.config.buildings.Count > 0)
            {
                PropItUpTool.ReplacePrefabsBuilding();
            }

            //  Global Free-standing Tree Replacements:
            if (PropItUpTool.config.enable_globalfreestanding && PropItUpTool.config.globalTreeReplacements.Count > 0)
            {
                PropItUpTool.ReplaceTreesGlobal();
            }

            //  Global Building Tree Replacements:
            if (PropItUpTool.config.enable_applyglobalonload && PropItUpTool.config.globalBuildingTreeReplacements.Count > 0)
            {
                PropItUpTool.ReplaceBuildingTreesGlobal();
            }

            base.OnLevelLoaded(mode);
        }

        public override void OnLevelUnloading()
        {
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