using System.Collections.Generic;

namespace g3
{
    [FullCovered]
    public static class PolyLineCombiner
    {
        /// <summary>
        /// Stitches together any PolyLines in a collection that share endpoints
        /// </summary>
        /// <remarks>
        /// If more than two PolyLine instances share an endpoint, none of the instances will be combined.
        ///
        /// This method is currently implemented using DGraph2.ExtractCurves, so order and orientation of the original
        /// PolyLine instances may not be preserved.
        /// </remarks>
        /// <param name="polylines">The collection of PolyLines to combine</param>
        /// <param name="mergeThreshold">Maximum distance between endpoints that will be considered shared between PolyLines</param>
        /// <returns>Collection of new PolyLine instances</returns>

        public static List<PolyLine2d> CombineConnectedPolyLines(IEnumerable<PolyLine2d> polylines, double mergeThreshold)
        {
            var graph = new DGraph2();
            foreach (var polyline in polylines)
                MergeAppendPolyline(graph, polyline, mergeThreshold);
            var curves = DGraph2Util.ExtractCurves(graph);

            var result = new List<PolyLine2d>();
            result.AddRange(curves.Paths);
            result.AddRange(curves.Loops.ConvertAll(PolyLineToPolygonConverter));
            return result;
        }

        private static PolyLine2d PolyLineToPolygonConverter(Polygon2d polygon)
        {
            return new PolyLine2d(polygon.VerticesItr(true));
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