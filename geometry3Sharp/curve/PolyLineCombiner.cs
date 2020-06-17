using System;
using System.Collections.Generic;
using System.Text;

namespace g3
{
    public static class PolyLineCombiner
    {
        public static List<PolyLine2d> CombineConnectedPolyLines(IEnumerable<PolyLine2d> polylines, double mergeThreshold)
        {
            var graph = new DGraph2();
            foreach (var polyline in polylines)
                MergeAppendPolyline(graph, polyline, mergeThreshold);
            var curves = DGraph2Util.ExtractCurves(graph);
            return curves.Paths;
        }

        private static int MergeAppendVertex(DGraph2 graph, Vector2d vertex, double mergeThreshold)
        {
            for (int i = 0; i < graph.VertexCount; i++)
            {
                double d = vertex.Distance(graph.GetVertex(i));
                if (d < mergeThreshold)
                    return i;
            }

            return graph.AppendVertex(vertex);
        }

        private static void MergeAppendPolyline(DGraph2 graph, PolyLine2d polyline, double mergeThreshold, int gid = -1)
        {
            int previousVertexIndex = -1;
            foreach (var vertex in polyline)
            {
                int currentVertexIndex = MergeAppendVertex(graph, vertex, mergeThreshold);
                if (previousVertexIndex != -1)
                {
                    graph.AppendEdge(previousVertexIndex, currentVertexIndex, gid);
                }
                previousVertexIndex = currentVertexIndex;
            }
        }
    }
}
