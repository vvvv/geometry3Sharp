using System.Collections.Generic;

namespace g3
{
    /// <summary>
    /// Holds both closed curves (as Polygon2d instances) and open curves (as PolyLine2d instances).
    /// </summary>
    public class CurveCollection
    {
        public List<Polygon2d> Loops { get; } = new List<Polygon2d>();

        public List<PolyLine2d> Paths { get; } = new List<PolyLine2d>();

        public void Add(CurveCollection other)
        {
            Loops.AddRange(other.Loops);
            Paths.AddRange(other.Paths);
        }
    }
}
