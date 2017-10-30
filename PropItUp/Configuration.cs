using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace PropItUp
{
    public class Configuration
    {
        //  Misc.
        public string version;
        public int keyboardshortcut = 0;
        public Vector3 buttonposition = new Vector3(-9999, -9999, -9999);
        public bool enable_runtimereload = true;
        public bool enable_globalfreestanding = true;
        public bool enable_applyglobalonload = true;
        public bool enable_applybuildingonload = true;
        public bool enable_extrememode = false;
        public bool enable_debug = false;

        [XmlArray(ElementName = "GlobalTreeReplacements")]
        [XmlArrayItem(ElementName = "TreeReplacement")]
        public List<PrefabReplacement> globalTreeReplacements = new List<PrefabReplacement>();

        [XmlArray(ElementName = "GlobalBuildingTreeReplacements")]
        [XmlArrayItem(ElementName = "TreeReplacement")]
        public List<PrefabReplacement> globalBuildingTreeReplacements = new List<PrefabReplacement>();

        public PrefabReplacement GetGlobalReplacementByIndex(int index)
        {
            foreach (PrefabReplacement prefabReplacement in globalTreeReplacements)
            {
                if (prefabReplacement.index == index)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public PrefabReplacement GetGlobalReplacementByTreeName(string prefabName)
        {
            foreach (PrefabReplacement prefabReplacement in globalTreeReplacements)
            {
                if (prefabReplacement.original == prefabName)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public PrefabReplacement GetGlobalBuildingReplacementByIndex(int index)
        {
            foreach (PrefabReplacement prefabReplacement in globalBuildingTreeReplacements)
            {
                if (prefabReplacement.index == index)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public PrefabReplacement GetGlobalBuildingReplacementByTreeName(string prefabName)
        {
            foreach (PrefabReplacement prefabReplacement in globalBuildingTreeReplacements)
            {
                if (prefabReplacement.original == prefabName)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public static void Save()
        {
            var fileName = (PluginManager.noWorkshop) ? PropItUpTool.ConfigFileNameLocal : PropItUpTool.ConfigFileNameOnline;

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(Configuration));
                using (var streamWriter = new StreamWriter(fileName, false, Encoding.Default))
                {
                    PropItUpTool.config.version = Mod.version;

                    var configCopy = new Configuration()
                    {
                        version = PropItUpTool.config.version,
                        keyboardshortcut = PropItUpTool.config.keyboardshortcut,
                        buttonposition = PropItUpTool.config.buttonposition,
                        enable_runtimereload = PropItUpTool.config.enable_runtimereload,
                        enable_globalfreestanding = PropItUpTool.config.enable_globalfreestanding,
                        enable_applyglobalonload = PropItUpTool.config.enable_applyglobalonload,
                        enable_applybuildingonload = PropItUpTool.config.enable_applybuildingonload,
                        enable_extrememode = PropItUpTool.config.enable_extrememode,
                        enable_debug = PropItUpTool.config.enable_debug
                    };

                    //  Existing Global PrefabReplacements:
                    foreach (var treeReplacement in PropItUpTool.config.globalTreeReplacements)
                    {
                        var newPrefabReplacement = new PrefabReplacement
                        {
                            index = treeReplacement.index,
                            type = "tree",
                            original = treeReplacement.original,
                            replacement_name = treeReplacement.replacement_name
                        };
                        configCopy.globalTreeReplacements.Add(treeReplacement);
                    }

                    //  Existing Global Building PrefabReplacements:
                    foreach (var treeReplacement in PropItUpTool.config.globalBuildingTreeReplacements)
                    {
                        var newPrefabReplacement = new PrefabReplacement
                        {
                            index = treeReplacement.index,
                            type = "tree",
                            original = treeReplacement.original,
                            replacement_name = treeReplacement.replacement_name
                        };
                        configCopy.globalBuildingTreeReplacements.Add(treeReplacement);
                    }

                    //  Existing Building PrefabReplacements:
                    foreach (Building building in PropItUpTool.config.buildings)
                    {
                        Building newBuilding = new Building
                        {
                            name = building.name
                        };
                        foreach (var prefabReplacement in building.prefabReplacements.OrderBy(x => x.type).ThenBy(x => x.index).ToList())
                        {
                            var prefabTreeReplacement = new PrefabReplacement
                            {
                                index = prefabReplacement.index,
                                type = prefabReplacement.type,
                                original = prefabReplacement.original,
                                replacement_name = prefabReplacement.replacement_name
                            };
                            newBuilding.prefabReplacements.Add(prefabReplacement);
                        }
                        configCopy.buildings.Add(newBuilding);
                    }

                    xmlSerializer.Serialize(streamWriter, configCopy);

                    //  
                    if (PropItUpTool.config.enable_debug)
                    {
                        DebugUtils.Log("Configuration saved.");
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.LogException(e);
            }
        }

        public static Configuration Load(string filename)
        {
            if (!File.Exists(filename)) return null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
            try
            {
                using (StreamReader streamReader = new StreamReader(filename, Encoding.Default))
                {
                    return (Configuration)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load configuration (XML malformed?)");
                throw e;
            }
        }

        [XmlArray(ElementName = "Buildings")]
        [XmlArrayItem(ElementName = "Building")]
        public List<Building> buildings = new List<Building>();

        public Building GetBuilding(string name)
        {
            foreach (Building building in buildings)
            {
                if (building.name == name)
                {
                    return building;
                }
            }
            return null;
        }

        public PrefabReplacement GetBuildingPrefabReplacementByIndex(Building building, string type, int index)
        {
            foreach (PrefabReplacement prefabReplacement in building.prefabReplacements)
            {
                if (prefabReplacement.type == type && prefabReplacement.index == index)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public PrefabReplacement GetBuildingReplacementByOriginalPrefabName(Building building, string prefabName)
        {
            foreach (PrefabReplacement prefabReplacement in building.prefabReplacements)
            {
                if (prefabReplacement.original == prefabName)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public PrefabReplacement GetBuildingReplacementByReplacementPrefabName(Building building, string prefabName)
        {
            foreach (PrefabReplacement prefabReplacement in building.prefabReplacements)
            {
                if (prefabReplacement.replacement_name == prefabName)
                {
                    return prefabReplacement;
                }
            }
            return null;
        }

        public List<string> GetAllBuildings()
        {
            List<string> allBuildings = new List<string>();
            foreach (Building building in buildings)
            {
                if (!allBuildings.Contains(building.name))
                {
                    allBuildings.Add(building.name);
                }
            }
            return allBuildings;
        }

        public class PrefabReplacement
        {
            [XmlAttribute("index")]
            public int index;

            [XmlAttribute("type")]
            public string type;

            [XmlAttribute("original")]
            public string original = string.Empty;

            [XmlAttribute("replacement_name")]
            public string replacement_name = string.Empty;

            public PrefabReplacement(PrefabReplacement selectedPrefabReplacement)
            {
                original = selectedPrefabReplacement.original;
                type = selectedPrefabReplacement.type;
                index = selectedPrefabReplacement.index;
                replacement_name = selectedPrefabReplacement.replacement_name;
            }

            public PrefabReplacement()
            {
            }
        }

        public class Building
        {
            [XmlAttribute("name")]
            public string name;

            [XmlArray(ElementName = "PrefabReplacements")]
            [XmlArrayItem(ElementName = "PrefabReplacement")]
            public List<PrefabReplacement> prefabReplacements = new List<PrefabReplacement>();
        }
    }
}