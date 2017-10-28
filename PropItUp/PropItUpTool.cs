using ColossalFramework.Plugins;
using ColossalFramework.UI;
using static PropItUp.Configuration;
using PropItUp.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PropItUp
{
    public class PropItUpTool : MonoBehaviour
    {
        private static UIMainButton m_mainbutton;
        private static UIMainPanel m_mainpanel;

        public static Configuration config;
        public static bool isGameLoaded;

        public static string ConfigFileName;
        public static readonly string ConfigFileNameOnline = "CSL_PropItUp.xml";
        public static readonly string ConfigFileNameLocal = "CSL_PropItUp_local.xml";
        public static readonly string CustomFileNameOnline = "CSL_PropItUp_Custom.xml";
        public static readonly string CustomFileNameLocal = "CSL_PropItUp_Custom_local.xml";

        //  Size Constants:
        public static float WIDTH = 270;
        public static float HEIGHT = 350;
        public static float SPACING = 5;
        public static float TITLE_HEIGHT = 36;
        public static float TABS_HEIGHT = 28;

        //  PrefabReplacer:
        //  List all vanilla trees:
        public static List<TreeInfo> allVanillaTrees = new List<TreeInfo>();
        //  List all custom trees:
        public static List<TreeInfo> allCustomTrees = new List<TreeInfo>();
        //  List all global prefabReplacements:
        public static List<PrefabReplacement> allGlobalReplacements = new List<PrefabReplacement>();

        //  List all props:
        public static List<PropInfo> allAvailableProps = new List<PropInfo>();
        //  List all trees:
        public static List<TreeInfo> allAvailableTrees = new List<TreeInfo>();
        //  List all prefabReplacements:
        public static List<PrefabReplacement> allPrefabReplacements = new List<PrefabReplacement>();

        //  METHODS:
        public static void Reset()
        {
            var go = FindObjectOfType<PropItUpTool>();
            if (go != null)
            {
                Destroy(go);
            }

            config = null; // do??
            isGameLoaded = false;
        }

        public static void Initialize()
        {
            var go = new GameObject("PropItUpTool");
            try
            {
                go.AddComponent<PropItUpTool>();
                //  Init. GUI Components:
                m_mainbutton = UIView.GetAView().AddUIComponent(typeof(UIMainButton)) as UIMainButton;
                DebugUtils.Log("MainButton created.");
                m_mainpanel = UIView.GetAView().AddUIComponent(typeof(UIMainPanel)) as UIMainPanel;
                DebugUtils.Log("MainPanel created.");
                //  Set vars:
                isGameLoaded = true;
                ConfigFileName = (PluginManager.noWorkshop) ? ConfigFileNameLocal : ConfigFileNameOnline;
                DebugUtils.Log($"Currently used config File: {ConfigFileName}.");
                //  Get treeReplacements:
                allPrefabReplacements = config.globalTreeReplacements;
            }
            catch (Exception e)
            {
                DebugUtils.LogException(e);
                if (go != null)
                {
                    Destroy(go);
                }
            }
        }

        //  CONFIG:
        public static void SaveBackup()
        {
            ConfigFileName = (PluginManager.noWorkshop) ? ConfigFileNameLocal : ConfigFileNameOnline;
            if (!File.Exists(ConfigFileName)) return;

            File.Copy(ConfigFileName, ConfigFileName + ".bak", true);
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log("Backup configuration file created.");
            }
        }

        public static void RestoreBackup()
        {
            ConfigFileName = (PluginManager.noWorkshop) ? ConfigFileNameLocal : ConfigFileNameOnline;
            if (!File.Exists(ConfigFileName + ".bak")) return;

            File.Copy(ConfigFileName + ".bak", ConfigFileName, true);
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log("Backup configuration file restored.");
            }
        }

        public static void LoadConfig()
        {
            if (!isGameLoaded)
            {
                var fileName = (PluginManager.noWorkshop) ? ConfigFileNameLocal : ConfigFileNameOnline;
                if (File.Exists(fileName))
                {
                    //  Load config:
                    config = Configuration.Load(fileName);
                    SaveConfig();
                    if (config.enable_debug)
                    {
                        DebugUtils.Log($"OnSettingsUI: configuration loaded (file name: {fileName}).");
                    }
                }
                else
                {
                    //  No config: create and save new config:
                    config = new Configuration();
                    SaveConfig();
                }
                return;
            }
            //  Load config:
            if (!File.Exists(ConfigFileName))
            {
                //  No config:
                if (config.enable_debug)
                {
                    DebugUtils.Log($"OnLevelLoaded: No configuration found, new configuration file created (file name: {ConfigFileName}).");
                }
                //  Create and save new config:
                config = new Configuration();
                SaveConfig();
                return;
            }
            //  
            config = Configuration.Load(ConfigFileName);
            if (config.enable_debug)
            {
                DebugUtils.Log($"OnLevelLoaded: Configuration loaded (file name: {ConfigFileName}).");
            }
            return;
        }

        public static void SaveConfig()
        {
            Configuration.Save();
        }

        //  PREFAB-RELATED:
        //  List all available props:
        public static void ListPropPrefabs(bool isRefresh = false)
        {
            //  Clear prop list for refresh:
            if (isRefresh)
            {
                allAvailableProps.Clear();
            }

            //// 'No prop' option:
            //allAvailableProps.Add(null);

            //  Loop all props in 'PropCollection':
            int skipped = 0;
            for (uint i = 0; i < PrefabCollection<PropInfo>.PrefabCount(); i++)
            {
                PropInfo prop = PrefabCollection<PropInfo>.GetPrefab(i);
                //  Null check:
                if (prop == null)
                {
                    skipped++;
                    continue;
                }
                //  Skip Markers:
                if (prop.name.Contains("Hang Around Marker") || prop.name.Contains("Helipad Marker") || prop.name.Contains("Nautical Marker") || prop.name.Contains("Door Marker"))
                {
                    skipped++;
                    continue;
                }
                //  Temporary 'Extreme Mode' feature:
                //  TODO: verify if this is still an issue (lots of props are now not listed in replacement fastlist)!
                //  Exclude props without LOD or with double quotes in name (causes infinite 'Array index is out of range' error loops):
                if (config.enable_extrememode == false && (prop.name.Contains("\"") || prop.m_lodMesh == null || prop.m_lodObject == null))
                {
                    skipped++;
                    continue;
                }
                //  Add to list:
                allAvailableProps.Add(prop);
            }
            //  Sort list by alphabet:
            allAvailableProps = allAvailableProps.OrderBy(x => GUI.UIUtils.GenerateBeautifiedPrefabName(x)).ToList();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"Finished listing all props: {allAvailableProps.Count} props found ({skipped} potentially incompatible props skipped - Extreme Mode enabled: {config.enable_extrememode}).");
            }
        }

        //  List all available, all vanilla, all Workshop trees:
        public static void ListTreePrefabs(bool isRefresh = false)
        {
            //  Clear tree list for refresh:
            if (isRefresh)
            {
                allAvailableTrees.Clear();
            }

            //// 'No tree' options:
            //allVanillaTrees.Add(null);
            //allCustomTrees.Add(null);
            //allAvailableTrees.Add(null);

            //  Loop all props in 'PropCollection':
            int skipped = 0;
            for (uint i = 0; i < PrefabCollection<TreeInfo>.PrefabCount(); i++)
            {
                TreeInfo tree = PrefabCollection<TreeInfo>.GetPrefab(i);
                //  Null check:
                if (tree == null)
                {
                    continue;
                }
                //  Temporary 'Extreme Mode' feature:
                //  TODO: verify if this is still an issue!
                //  Exclude props without LOD or with double quotes in name (causes infinite 'Array index is out of range' error loops):
                if (config.enable_extrememode == false && tree.name.Contains("\""))
                {
                    skipped++;
                    continue;
                }
                //  Add to list:
                allAvailableTrees.Add(tree);
                if (tree.m_isCustomContent)
                {
                    allCustomTrees.Add(tree);
                }
                else
                {
                    if (!isRefresh)
                    {
                        allVanillaTrees.Add(tree);
                    }
                }
            }
            //  Sort lists by alphabet:
            allVanillaTrees = allVanillaTrees.OrderBy(x => GUI.UIUtils.GenerateBeautifiedPrefabName(x)).ToList();
            allCustomTrees = allCustomTrees.OrderBy(x => GUI.UIUtils.GenerateBeautifiedPrefabName(x)).ToList();
            allAvailableTrees = allAvailableTrees.OrderBy(x => GUI.UIUtils.GenerateBeautifiedPrefabName(x)).ToList();

            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"Finished listing all trees: {allAvailableTrees.Count} trees found ({skipped} potentially incompatible trees skipped - Extreme Mode enabled: {config.enable_extrememode}).");
            }
        }


        //  Global Tree Replacements:
        #region Global Tree Replacements:

        //  Apply all global tree replacements (onLoad):
        public static void ReplaceTreesGlobal()
        {
            List<string> allBuildings = config.GetAllBuildings();
            //  Start timer:
            Stopwatch ReplaceTreesTimer = new Stopwatch();
            ReplaceTreesTimer.Start();
            //  
            foreach (PrefabReplacement treeReplacement in config.globalTreeReplacements)
            {
                try
                {
                    ReplaceTreeGlobal(treeReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Global tree replacement {treeReplacement.original} with {treeReplacement.replacement_name} failed: ReplaceTreesGlobal()");
                    DebugUtils.LogException(e);
                    continue;
                }
            }
            //  Output timer data:
            ReplaceTreesTimer.Stop();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"All global tree replacements applied (time elapsed: {ReplaceTreesTimer.Elapsed} seconds).");
            }
        }

        //  Apply selected global tree replacement (runtime):
        public static void ReplaceTreeGlobal(PrefabReplacement selectedTreeReplacement)
        {
            SimulationManager.instance.AddAction(() =>
            {
                TreeInfo newTree = PrefabCollection<TreeInfo>.FindLoaded(selectedTreeReplacement.replacement_name);
                //  Null check:
                if (!newTree)
                {
                    if (config.enable_debug)
                    {
                        DebugUtils.Log($"Global tree replacement for {selectedTreeReplacement.replacement_name} failed. Reason: replacement tree not found!");
                    }
                    //  TODO: remove all replacements featuring not found prefab from config
                    return;
                }
                TreeInfo oldTree = PrefabCollection<TreeInfo>.FindLoaded(selectedTreeReplacement.original);
                //  Null check:
                if (!oldTree)
                {
                    if (config.enable_debug)
                    {
                        DebugUtils.Log($"Global tree replacement for {selectedTreeReplacement.replacement_name} failed. Reason: original tree not found!");
                    }
                    //  TODO: remove all replacements featuring not found prefab from config
                    return;
                }
                //  Replace freestanding trees:
                var trees = TreeManager.instance.m_trees.m_buffer;
                for (uint index = 0; index < trees.Length; index++)
                {
                    var tree = trees[index];
                    if (tree.m_flags == (ushort)TreeInstance.Flags.None)
                    {
                        continue;
                    }
                    var treeInstance = tree.Info;
                    if (treeInstance == null)
                    {
                        continue;
                    }
                    if (treeInstance.name != oldTree.name)
                    {
                        continue;
                    }
                    tree.Info = newTree;
                    trees[index] = tree;
                    TreeManager.instance.UpdateTreeRenderer(index, true); //that updates LODs
                }
            });
        }

        //  Save/apply selected global tree replacement:
        public static void SaveReplacementGlobal()
        {
            int index = TreeReplacerPanel.instance.selectedTreeOriginalIndex;
            TreeInfo originalTree = TreeReplacerPanel.instance.selectedTreeOriginal;
            TreeInfo replacementTree = TreeReplacerPanel.instance.selectedTreeReplacement;

            //  New tree replacement object:
            PrefabReplacement newTreeReplacement = new PrefabReplacement()
            {
                index = index,
                type = "tree",
                original = originalTree.name,
                replacement_name = replacementTree.name
            };
            //  Temporary tree replacement object to send to replace method (to ensure non-vanilla tree is replaced):
            PrefabReplacement executableTreeReplacement = newTreeReplacement;

            //  Save tree replacement to config:
            //  Check if tree has been replaced before: if yes, update; if no, add:
            PrefabReplacement existingTreeReplacement = config.GetGlobalReplacementByTreeName(originalTree.name);
            if (existingTreeReplacement != null)
            {
                //  Yes => find previous tree replacement (now needs to be replaced) and update:
                string existingTree = existingTreeReplacement.replacement_name;
                newTreeReplacement = new PrefabReplacement()
                {
                    type = existingTreeReplacement.type,
                    original = existingTreeReplacement.original,
                    index = index,
                    replacement_name = replacementTree.name
                };
                config.globalTreeReplacements.Remove(existingTreeReplacement);
                executableTreeReplacement.original = existingTree;
            }
            config.globalTreeReplacements.Add(newTreeReplacement);
            SaveConfig();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"[Configuration] - Global tree replacement '{newTreeReplacement.original}' with '{newTreeReplacement.replacement_name}' saved.");
            }
            //  Replace tree at runtime (if configured as such):
            if (config.enable_runtimereload)
            {
                //  Start timer:
                Stopwatch ReplaceTreeTimer = new Stopwatch();
                ReplaceTreeTimer.Start();
                //  Replace tree:
                try
                {
                    ReplaceTreeGlobal(executableTreeReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Replacing global tree replacement '{executableTreeReplacement.original}' with '{executableTreeReplacement.replacement_name}' failed: SaveReplacementGlobal()");
                    DebugUtils.LogException(e);
                }
                //  Output timer data:
                ReplaceTreeTimer.Stop();
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Replacing global tree replacement '{executableTreeReplacement.original}' with '{executableTreeReplacement.replacement_name}' completed (time elapsed: {ReplaceTreeTimer.Elapsed} seconds).");
                }
            }
        }

        //  Restore selected global tree replacement:
        public static void RestoreReplacementGlobal()
        {
            TreeInfo originalTree = TreeReplacerPanel.instance.selectedTreeOriginal;
            PrefabReplacement selectedTreeReplacement = config.GetGlobalReplacementByTreeName(originalTree.name);
            PrefabReplacement executableTreeReplacement = new PrefabReplacement()
            {
                index = selectedTreeReplacement.index,
                type = "tree",
                original = selectedTreeReplacement.replacement_name,
                replacement_name = selectedTreeReplacement.original
            };
            config.globalTreeReplacements.Remove(selectedTreeReplacement);
            SaveConfig();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"[Configuration] - Global tree replacement of '{selectedTreeReplacement.original}' removed.");
            }
            //  Reset tree at runtime (if configured as such):
            if (config.enable_runtimereload)
            {
                //  Start timer:
                Stopwatch ResetTreeTimer = new Stopwatch();
                ResetTreeTimer.Start();
                //  Replace tree:
                try
                {
                    ReplaceTreeGlobal(executableTreeReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Resetting global tree replacement '{executableTreeReplacement.original}' to '{originalTree.name}' failed: ResetReplacementGlobal()");
                    DebugUtils.LogException(e);
                }
                //  Output timer data:
                ResetTreeTimer.Stop();
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Resetting global tree replacement '{executableTreeReplacement.original}' to '{originalTree.name}' completed (time elapsed: {ResetTreeTimer.Elapsed} seconds).");
                }
            }
        }


        #endregion
        

        //  Global Building Tree Replacements:
        #region Global Building Tree Replacements:

        //  Apply all global building tree replacements (onLoad):
        public static void ReplaceBuildingTreesGlobal()
        {
            List<string> allBuildings = config.GetAllBuildings();
            //  Start timer:
            Stopwatch ReplaceTreesTimer = new Stopwatch();
            ReplaceTreesTimer.Start();
            //  
            foreach (PrefabReplacement treeReplacement in config.globalBuildingTreeReplacements)
            {
                try
                {
                    ReplaceBuildingTreeGlobal(treeReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Replacement {treeReplacement.original} with {treeReplacement.replacement_name} failed: ReplaceTreesGlobal()");
                    DebugUtils.LogException(e);
                    continue;
                }
            }
            //  Output timer data:
            ReplaceTreesTimer.Stop();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"All global tree replacements applied (time elapsed: {ReplaceTreesTimer.Elapsed} seconds).");
            }
        }

        //  Apply selected global building tree replacement (runtime):
        public static void ReplaceBuildingTreeGlobal(PrefabReplacement selectedTreeReplacement)
        {
            SimulationManager.instance.AddAction(() =>
            {
                TreeInfo newTree = PrefabCollection<TreeInfo>.FindLoaded(selectedTreeReplacement.replacement_name);
                //  Null check:
                if (!newTree)
                {
                    if (config.enable_debug)
                    {
                        DebugUtils.Log($"Global building tree replacement for {selectedTreeReplacement.replacement_name} failed. Reason: replacement tree not found!");
                    }
                    //  TODO: remove all replacements featuring not found prefab from config
                    return;
                }
                TreeInfo oldTree = PrefabCollection<TreeInfo>.FindLoaded(selectedTreeReplacement.original);
                //  Null check:
                if (!oldTree)
                {
                    if (config.enable_debug)
                    {
                        DebugUtils.Log($"Global building tree replacement for {selectedTreeReplacement.replacement_name} failed. Reason: original tree not found!");
                    }
                    //  TODO: remove all replacements featuring not found prefab from config
                    return;
                }
                //  
                List<string> allBuildings = config.GetAllBuildings();
                //  Replace building trees:
                var buildings = Resources.FindObjectsOfTypeAll<BuildingInfo>();
                foreach (var building in buildings)
                {
                    //  Skip buildings that have no trees or have custom TreeCustomizations:
                    if (building.m_props == null || allBuildings.Contains(building.name))
                    {
                        continue;
                    }
                    foreach (var tree in building.m_props)
                    {
                        var treeInstance = tree.m_finalTree;
                        if (treeInstance == null)
                        {
                            continue;
                        }
                        if (treeInstance.name == oldTree.name)
                        {
                            tree.m_finalTree = newTree;
                            tree.m_tree = newTree;
                        }
                    }
                }
                UpdateBuildingRenderers(); //that should update LODs
            });
        }

        //  Save/apply selected global building tree replacement:
        public static void SaveBuildingReplacementGlobal()
        {
            int index = BuildingTreeReplacerPanel.instance.selectedTreeOriginalIndex;
            TreeInfo originalTree = BuildingTreeReplacerPanel.instance.selectedTreeOriginal;
            TreeInfo replacementTree = BuildingTreeReplacerPanel.instance.selectedTreeReplacement;

            //  New tree replacement object:
            PrefabReplacement newTreeReplacement = new PrefabReplacement()
            {
                index = index,
                type = "tree",
                original = originalTree.name,
                replacement_name = replacementTree.name
            };
            //  Temporary tree replacement object to send to replace method (to ensure non-vanilla tree is replaced):
            PrefabReplacement executableTreeReplacement = newTreeReplacement;

            //  Save tree replacement to config:
            //  Check if tree has been replaced before: if yes, update; if no, add:
            PrefabReplacement existingTreeReplacement = config.GetGlobalBuildingReplacementByTreeName(originalTree.name);
            if (existingTreeReplacement != null)
            {
                //  Yes => find previous tree replacement (now needs to be replaced) and update:
                string existingTree = existingTreeReplacement.replacement_name;
                newTreeReplacement = new PrefabReplacement()
                {
                    type = existingTreeReplacement.type,
                    original = existingTreeReplacement.original,
                    index = index,
                    replacement_name = replacementTree.name
                };
                config.globalBuildingTreeReplacements.Remove(existingTreeReplacement);
                executableTreeReplacement.original = existingTree;
            }
            config.globalBuildingTreeReplacements.Add(newTreeReplacement);
            SaveConfig();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"[Configuration] - Global building tree replacement '{newTreeReplacement.original}' with '{newTreeReplacement.replacement_name}' saved.");
            }
            //  Replace tree at runtime (if configured as such):
            if (config.enable_runtimereload)
            {
                //  Start timer:
                Stopwatch ReplaceTreeTimer = new Stopwatch();
                ReplaceTreeTimer.Start();
                //  Replace tree:
                try
                {
                    ReplaceBuildingTreeGlobal(executableTreeReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Replacing global building tree replacement '{executableTreeReplacement.original}' with '{executableTreeReplacement.replacement_name}' failed: SaveBuildingReplacementGlobal()");
                    DebugUtils.LogException(e);
                }
                //  Output timer data:
                ReplaceTreeTimer.Stop();
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Replacing global building tree replacement '{executableTreeReplacement.original}' with '{executableTreeReplacement.replacement_name}' completed (time elapsed: {ReplaceTreeTimer.Elapsed} seconds).");
                }
            }
        }

        //  Restore selected global building tree replacement:
        public static void RestoreBuildingReplacementGlobal()
        {
            TreeInfo originalTree = BuildingTreeReplacerPanel.instance.selectedTreeOriginal;
            PrefabReplacement selectedTreeReplacement = config.GetGlobalBuildingReplacementByTreeName(originalTree.name);
            PrefabReplacement executableTreeReplacement = new PrefabReplacement()
            {
                index = selectedTreeReplacement.index,
                type = "tree",
                original = selectedTreeReplacement.replacement_name,
                replacement_name = selectedTreeReplacement.original
            };
            config.globalBuildingTreeReplacements.Remove(selectedTreeReplacement);
            SaveConfig();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"[Configuration] - Global building tree replacement of '{selectedTreeReplacement.original}' removed.");
            }
            //  Reset tree at runtime (if configured as such):
            if (config.enable_runtimereload)
            {
                //  Start timer:
                Stopwatch ResetTreeTimer = new Stopwatch();
                ResetTreeTimer.Start();
                //  Replace tree:
                try
                {
                    ReplaceBuildingTreeGlobal(executableTreeReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Resetting global building tree replacement '{executableTreeReplacement.original}' to '{originalTree.name}' failed: ResetBuildingReplacementGlobal()");
                    DebugUtils.LogException(e);
                }
                //  Output timer data:
                ResetTreeTimer.Stop();
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Resetting global building tree replacement '{executableTreeReplacement.original}' to '{originalTree.name}' completed (time elapsed: {ResetTreeTimer.Elapsed} seconds).");
                }
            }
        }


        #endregion


        //  Asset-based replacements:
        #region Asset-based replacements:

        //  Replace selected tree/prop with replacement for building (onLoad):
        public static void ReplacePrefabsBuilding()
        {
            //  Start timer:
            Stopwatch ReplaceTreesTimer = new Stopwatch();
            ReplaceTreesTimer.Start();
            //  
            foreach (Configuration.Building building in config.buildings)
            {
                foreach (PrefabReplacement prefabReplacement in building.prefabReplacements)
                {
                    try
                    {
                        ReplacePrefabBuilding(PrefabCollection<BuildingInfo>.FindLoaded(building.name), prefabReplacement);
                    }
                    catch (Exception e)
                    {
                        DebugUtils.Log($"Replacement '{prefabReplacement.original}' with '{prefabReplacement.replacement_name}' for building '{building.name}' failed: ReplacePrefabsBuilding()");
                        DebugUtils.LogException(e);
                        continue;
                    }
                }
            }
            //  Output timer data:
            ReplaceTreesTimer.Stop();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"All prop/tree replacements per asset applied (time elapsed: {ReplaceTreesTimer.Elapsed} seconds).");
            }
        }

        //  Replace selected tree/prop with replacement for building (runtime):
        public static void ReplacePrefabBuilding(BuildingInfo buildingInfo, PrefabReplacement selectedPrefabReplacement)
        {
            //  Null check:
            if (buildingInfo == null)
            {
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Per-building prop/tree replacement of prop {selectedPrefabReplacement.replacement_name} failed. Reason: building not found!");
                }
                //  TODO: remove all replacements featuring not found buildingInfo from config
                return;
            }
            SimulationManager.instance.AddAction(() =>
            {
                //  Replacement = prop:
                if (selectedPrefabReplacement.type == "prop")
                {
                    //  TODO: fix issues with buildings with accented characters in name (causes error);
                    PropInfo newProp = PrefabCollection<PropInfo>.FindLoaded(selectedPrefabReplacement.replacement_name);
                    //  Null check:
                    if (!newProp)
                    {
                        if (config.enable_debug)
                        {
                            DebugUtils.Log($"Per-building prop replacement of prop {selectedPrefabReplacement.replacement_name} for building {buildingInfo.name} failed. Reason: replacement prop not found!");
                        }
                        //  TODO: remove all replacements featuring not found prefab from config
                        return;
                    }
                    foreach (var prop in buildingInfo.m_props)
                    {
                        if (prop.m_prop != null)
                        {
                            var propInstance = prop.m_finalProp;
                            if (propInstance == null)
                            {
                                continue;
                            }
                            if (propInstance.name == selectedPrefabReplacement.original)
                            {
                                prop.m_finalProp = newProp;
                                prop.m_prop = newProp;
                            }
                        }
                    }
                }
                else
                {
                    //  Replacement = tree:
                    TreeInfo newTree = PrefabCollection<TreeInfo>.FindLoaded(selectedPrefabReplacement.replacement_name);
                    //  Null check:
                    if (!newTree)
                    {
                        if (config.enable_debug)
                        {
                            DebugUtils.Log($"Per-building tree replacement of tree {selectedPrefabReplacement.replacement_name} for building {buildingInfo.name} failed. Reason: replacement tree not found!");
                        }
                        //  TODO: remove all replacements featuring not found prefab from config
                        return;
                    }
                    foreach (var tree in buildingInfo.m_props)
                    {
                        if (tree.m_tree != null)
                        {
                            var treeInstance = tree.m_finalTree;
                            if (treeInstance == null)
                            {
                                continue;
                            }
                            if (treeInstance.name == selectedPrefabReplacement.original)
                            {
                                tree.m_finalTree = newTree;
                                tree.m_tree = newTree;
                            }
                        }
                    }
                }
                UpdateBuildingRenderers(buildingInfo); //that should update LODs
            });
        }

        //  Save/apply selected tree/prop replacement for building:
        public static void SaveReplacementBuilding(int index, string type, PrefabInfo originalPrefab, PrefabInfo replacementPrefab, BuildingInfo affectedBuilding)
        {
            //  New tree/prop replacement object:
            PrefabReplacement newPrefabReplacement = new PrefabReplacement()
            {
                index = index,
                type = type,
                original = originalPrefab.name,
                replacement_name = replacementPrefab.name,
            };
            //  Temporary tree/prop replacement object to send to replace method (to ensure non-vanilla tree/prop is replaced):
            PrefabReplacement executablePrefabReplacement = newPrefabReplacement;

            //  Save replacement to config:
            //  Check if building already has replaced prefab:
            Configuration.Building existingBuilding = config.GetBuilding(affectedBuilding.name);
            if (existingBuilding == null)
            {
                //  No => add:
                Configuration.Building newBuilding = new Configuration.Building()
                {
                    name = affectedBuilding.name
                };
                newBuilding.prefabReplacements.Add(newPrefabReplacement);
                config.buildings.Add(newBuilding);
                //  
                existingBuilding = newBuilding;
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"[Configuration] - Building '{newBuilding.name}' added.");
                }
            }
            else
            {
                //  Yes => update:
                string existingPrefab = originalPrefab.name;
                //  Check if tree/prop replacement already exists:
                PrefabReplacement existingPrefabReplacement = config.GetBuildingPrefabReplacementByIndex(existingBuilding, type, index);
                if (existingPrefabReplacement != null)
                {
                    //  Yes => find previous tree/prop replacement (now needs to be replaced) and update:
                    existingPrefab = existingPrefabReplacement.replacement_name;
                    newPrefabReplacement = new PrefabReplacement()
                    {
                        index = existingPrefabReplacement.index,
                        type = existingPrefabReplacement.type,
                        original = existingPrefabReplacement.original,
                        replacement_name = replacementPrefab.name
                    };
                    existingBuilding.prefabReplacements.Remove(existingPrefabReplacement);
                }
                else
                {
                    //  Yes => ...:
                }
                existingBuilding.prefabReplacements.Add(newPrefabReplacement);
                executablePrefabReplacement.original = existingPrefab;
            }
            SaveConfig();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"[Configuration] - Replacement '{executablePrefabReplacement.original}' with '{executablePrefabReplacement.replacement_name}' for building '{affectedBuilding.name}' saved.");
            }
            //  Replace tree/prop at runtime (if configured as such):
            if (config.enable_runtimereload)
            {
                //  Start timer:
                Stopwatch ReplaceTreeTimer = new Stopwatch();
                ReplaceTreeTimer.Start();
                //  Replace tree/prop:
                try
                {
                    ReplacePrefabBuilding(PropCustomizerPanel.instance.selectedBuilding, executablePrefabReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Replacement '{executablePrefabReplacement.original}' with '{executablePrefabReplacement.replacement_name}' for building '{affectedBuilding.name}' failed: SaveReplacementBuilding()");
                    DebugUtils.LogException(e);
                }
                //  Output timer data:
                ReplaceTreeTimer.Stop();
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Replacement '{executablePrefabReplacement.original}' with '{executablePrefabReplacement.replacement_name}' for building '{affectedBuilding.name}' completed (time elapsed: {ReplaceTreeTimer.Elapsed} seconds).");
                }
            }
        }

        //  Restore selected global tree replacement:
        public static void RestoreReplacementBuilding(int index, string type, BuildingInfo affectedBuilding)
        {
            //  Remove tree/prop replacement from config:
            Configuration.Building selectedBuilding = config.GetBuilding(affectedBuilding.name);
            PrefabReplacement selectedPrefabReplacement = config.GetBuildingPrefabReplacementByIndex(selectedBuilding, type, index);
            PrefabReplacement executablePrefabReplacement = new PrefabReplacement()
            {
                index = index,
                type = type,
                original = selectedPrefabReplacement.replacement_name,
                replacement_name = selectedPrefabReplacement.original
            };
            selectedBuilding.prefabReplacements.Remove(selectedPrefabReplacement);
            //  Remove building from config if no prefab replacements remain:
            if (selectedBuilding.prefabReplacements.Count == 0)
            {
                config.buildings.Remove(selectedBuilding);
            }
            SaveConfig();
            //  
            if (config.enable_debug)
            {
                DebugUtils.Log($"[Configuration] - Replacement of '{executablePrefabReplacement.original}' for building '{affectedBuilding.name}' removed.");
            }
            //  Reset tree/prop at runtime (if configured as such):
            if (config.enable_runtimereload)
            {
                //  Start timer:
                Stopwatch ResetTreeTimer = new Stopwatch();
                ResetTreeTimer.Start();
                //  Reset tree/prop:
                try
                {
                    ReplacePrefabBuilding(affectedBuilding, executablePrefabReplacement);
                }
                catch (Exception e)
                {
                    DebugUtils.Log($"Resetting '{executablePrefabReplacement.original}' to '{executablePrefabReplacement.replacement_name}' for building '{affectedBuilding.name}' failed: ResetReplacementGlobal()");
                    DebugUtils.LogException(e);
                }
                //  Output timer data:
                ResetTreeTimer.Stop();
                //  
                if (config.enable_debug)
                {
                    DebugUtils.Log($"Resetting '{executablePrefabReplacement.original}' to '{executablePrefabReplacement.replacement_name}' for building '{affectedBuilding.name}' completed (time elapsed: {ResetTreeTimer.Elapsed} seconds).");
                }
            }
        }

        #endregion


        //  Shared methods:
        #region Shared methods:

        //  Update building renderer to refresh correct building tree LODs:
        private static void UpdateBuildingRenderers(BuildingInfo buildingInfo = null)
        {
            var buildings = BuildingManager.instance.m_buildings.m_buffer;
            for (ushort index = 0; index < buildings.Length; index++)
            {
                var building = buildings[index];
                if (building.m_flags == Building.Flags.None)
                {
                    continue;
                }
                if (buildingInfo != null && building.Info != buildingInfo)
                {
                    continue;
                }
                BuildingManager.instance.UpdateBuildingRenderer(index, true); //that should update LODs
            }
        }

        #endregion
    }
}