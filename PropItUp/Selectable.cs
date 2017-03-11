using ColossalFramework;
using PropItUp.GUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PropItUp
{
    public class Selectable
    {
        public InstanceID id;
        public HashSet<Selectable> subInstances;
        private HashSet<ushort> m_segmentsHashSet = new HashSet<ushort>();

        private Vector3 m_startPosition;
        private float m_terrainHeight;
        private float m_startAngle;

        public object data
        {
            get
            {
                switch (id.Type)
                {
                    case InstanceType.Building:
                        {
                            return BuildingManager.instance.m_buildings.m_buffer[id.Building];
                        }
                }

                return null;
            }
        }

        public Vector3 position
        {
            get
            {
                switch (id.Type)
                {
                    case InstanceType.Building:
                        {
                            return BuildingManager.instance.m_buildings.m_buffer[id.Building].m_position;
                        }
                }

                return Vector3.zero;
            }
        }

        public bool isValid
        {
            get
            {
                switch (id.Type)
                {
                    case InstanceType.Building:
                        {
                            return (BuildingManager.instance.m_buildings.m_buffer[id.Building].m_flags & Building.Flags.Created) != Building.Flags.None;
                        }
                }

                return false;
            }
        }

        public override bool Equals(object obj)
        {
            Selectable instance = obj as Selectable;
            if (instance != null)
            {
                return instance.id == id;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public Selectable(InstanceID instance)
        {
            id = instance;

            switch (id.Type)
            {
                case InstanceType.Building:
                    {
                        Building[] buildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
                        NetNode[] nodeBuffer = NetManager.instance.m_nodes.m_buffer;

                        m_startPosition = buildingBuffer[id.Building].m_position;
                        m_startAngle = buildingBuffer[id.Building].m_angle;

                        subInstances = new HashSet<Selectable>();

                        ushort node = buildingBuffer[id.Building].m_netNode;
                        while (node != 0)
                        {
                            ItemClass.Layer layer = nodeBuffer[node].Info.m_class.m_layer;
                            if (layer != ItemClass.Layer.PublicTransport)
                            {
                                InstanceID nodeID = default(InstanceID);
                                nodeID.NetNode = node;
                                Selectable subInstance = new Selectable(nodeID);
                                subInstances.Add(subInstance);

                                m_segmentsHashSet.UnionWith(subInstance.m_segmentsHashSet);
                            }

                            node = nodeBuffer[node].m_nextBuildingNode;
                        }

                        ushort building = buildingBuffer[id.Building].m_subBuilding;
                        while (building != 0)
                        {
                            Selectable subBuilding = new Selectable(InstanceID.Empty);
                            subBuilding.id.Building = building;
                            subBuilding.m_startPosition = buildingBuffer[building].m_position;
                            subBuilding.m_startAngle = buildingBuffer[building].m_angle;
                            subInstances.Add(subBuilding);

                            node = buildingBuffer[building].m_netNode;
                            while (node != 0)
                            {
                                ItemClass.Layer layer = nodeBuffer[node].Info.m_class.m_layer;

                                if (layer != ItemClass.Layer.PublicTransport)
                                {
                                    InstanceID nodeID = default(InstanceID);
                                    nodeID.NetNode = node;
                                    Selectable subInstance = new Selectable(nodeID);
                                    subInstances.Add(subInstance);

                                    m_segmentsHashSet.UnionWith(subInstance.m_segmentsHashSet);
                                }

                                node = nodeBuffer[node].m_nextBuildingNode;
                            }

                            building = buildingBuffer[building].m_subBuilding;
                        }

                        if (subInstances.Count == 0)
                        {
                            subInstances = null;
                        }
                        break;
                    }
            }
            if (!id.IsEmpty)
            {
                m_terrainHeight = TerrainManager.instance.SampleOriginalRawHeightSmooth(position);
            }
        }

        public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor)
        {
            switch (id.Type)
            {
                case InstanceType.Building:
                    {

                        ushort building = id.Building;
                        NetManager netManager = NetManager.instance;
                        BuildingManager buildingManager = BuildingManager.instance;
                        BuildingInfo buildingInfo = buildingManager.m_buildings.m_buffer[building].Info;
                        float alpha = 1f;
                        BuildingTool.CheckOverlayAlpha(buildingInfo, ref alpha);
                        ushort node = buildingManager.m_buildings.m_buffer[building].m_netNode;
                        int count = 0;
                        while (node != 0)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                ushort segment = netManager.m_nodes.m_buffer[node].GetSegment(j);
                                if (segment != 0 && netManager.m_segments.m_buffer[segment].m_startNode == node && (netManager.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                                {
                                    NetTool.CheckOverlayAlpha(ref netManager.m_segments.m_buffer[segment], ref alpha);
                                }
                            }
                            node = netManager.m_nodes.m_buffer[node].m_nextBuildingNode;
                            if (++count > 32768)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                        ushort subBuilding = buildingManager.m_buildings.m_buffer[building].m_subBuilding;
                        count = 0;
                        while (subBuilding != 0)
                        {
                            BuildingTool.CheckOverlayAlpha(buildingManager.m_buildings.m_buffer[subBuilding].Info, ref alpha);
                            subBuilding = buildingManager.m_buildings.m_buffer[subBuilding].m_subBuilding;
                            if (++count > 49152)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                        toolColor.a *= alpha;
                        int length = buildingManager.m_buildings.m_buffer[building].Length;
                        Vector3 position = buildingManager.m_buildings.m_buffer[building].m_position;
                        float angle = buildingManager.m_buildings.m_buffer[building].m_angle;
                        BuildingTool.RenderOverlay(cameraInfo, buildingInfo, length, position, angle, toolColor, false);

                        node = buildingManager.m_buildings.m_buffer[building].m_netNode;
                        count = 0;
                        while (node != 0)
                        {
                            for (int k = 0; k < 8; k++)
                            {
                                ushort segment2 = netManager.m_nodes.m_buffer[node].GetSegment(k);
                                if (segment2 != 0 && netManager.m_segments.m_buffer[segment2].m_startNode == node && (netManager.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                                {
                                    NetTool.RenderOverlay(cameraInfo, ref netManager.m_segments.m_buffer[segment2], toolColor, toolColor);
                                }
                            }
                            node = netManager.m_nodes.m_buffer[node].m_nextBuildingNode;
                            if (++count > 32768)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                        subBuilding = buildingManager.m_buildings.m_buffer[building].m_subBuilding;
                        count = 0;
                        while (subBuilding != 0)
                        {
                            BuildingInfo subBuildingInfo = buildingManager.m_buildings.m_buffer[subBuilding].Info;
                            int subLength = buildingManager.m_buildings.m_buffer[subBuilding].Length;
                            Vector3 subPosition = buildingManager.m_buildings.m_buffer[subBuilding].m_position;
                            float subAngle = buildingManager.m_buildings.m_buffer[subBuilding].m_angle;
                            BuildingTool.RenderOverlay(cameraInfo, subBuildingInfo, subLength, subPosition, subAngle, toolColor, false);
                            subBuilding = buildingManager.m_buildings.m_buffer[subBuilding].m_subBuilding;
                            if (++count > 49152)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                        //  
                        TreeCustomizerPanel.instance.selectedBuilding = buildingInfo;
                        PropCustomizerPanel.instance.selectedBuilding = buildingInfo;

                        break;
                    }
            }
        }
    }
}