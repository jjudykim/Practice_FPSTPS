using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGraphLayout
{
    public enum FlowDirection
    {
        LeftToRight,
        BottomToTop
    }

    public struct LayoutSettings
    {
        public float primarySpacing;        // 흐름 방향 간격 (depth 간격)
        public float secondarySpacing;      // depth 내부의 퍼짐 간격
        public Vector2 origin;
        public FlowDirection direction;

        public LayoutSettings(float primarySpacing, float secondarySpacing, Vector2 origin, FlowDirection direction)
        {
            this.primarySpacing = primarySpacing;
            this.secondarySpacing = secondarySpacing;
            this.origin = origin;
            this.direction = direction;
        }
    }
    
    // MapGraph를 받아서 nodeId -> anchoredPosition
    public static Dictionary<int, Vector2> BuildNodePositions(MapGraph graph, LayoutSettings settings)
    {
        var result = new Dictionary<int, Vector2>();
        if (graph == null || graph.Nodes == null || graph.Nodes.Count == 0)
            return result;

        var groups = graph.Nodes.GroupBy(n => n.Depth)
                                                   .OrderBy(g => g.Key)
                                                   .ToList();

        foreach (var depthGroup in groups)
        {
            int depth = depthGroup.Key;
            var nodesAtDepth = depthGroup.OrderBy(n => n.Id).ToList();

            int count = nodesAtDepth.Count;
            float centerOffset = (count - 1) * 0.5f;
            
            for (int i = 0; i < count; ++i)
            {
                float secondary = (i - centerOffset) * settings.secondarySpacing;
                Vector2 pos;
                if (settings.direction == FlowDirection.LeftToRight)
                {
                    float x = settings.origin.x + depth * settings.primarySpacing;
                    float y = settings.origin.y + secondary;
                    pos = new Vector2(x, y);
                }
                else // BottomToTop
                {
                    float y = settings.origin.y + depth * settings.primarySpacing;
                    float x = settings.origin.x + secondary;
                    pos = new Vector2(x, y);
                }

                result[nodesAtDepth[i].Id] = pos;
            }
        }

        return result;
    }
}