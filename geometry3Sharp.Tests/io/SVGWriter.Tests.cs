using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace geometry3Sharp.Tests.io
{
    [TestClass]
    public class SVGWriterTests
    {
        [TestMethod]
        public void TestBasic()
        {
            var writer = new SVGWriter();
            writer.AddLine(new Segment2d(Vector2d.Zero, Vector2d.AxisX));
            writer.Write("basic.svg");
        }

        [TestMethod]
        public void TestLayers()
        {
            var writer = new SVGWriter();
            writer.StartNewLayer("layer-0");
            writer.AddLine(new Segment2d(Vector2d.Zero, Vector2d.AxisX));

            writer.StartNewLayer("layer-1");
            writer.AddLine(new Segment2d(Vector2d.Zero, Vector2d.AxisY));
            writer.Write("layers.svg");
        }
    }
}