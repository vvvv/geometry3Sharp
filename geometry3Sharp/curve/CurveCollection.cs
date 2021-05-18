using System.Collections.Generic;

namespace g3
{
    /// <summary>
    /// Holds both closed curves (as Polygon2d instances) and open curves (as PolyLine2d instances).
    /// </summary>
    public class CurveCollection
    {
        public List<Polygon2d> Loops { get; set; } = new List<Polygon2d>();

        public List<PolyLine2d> Paths { get; set; } = new List<PolyLine2d>();
    }
}
