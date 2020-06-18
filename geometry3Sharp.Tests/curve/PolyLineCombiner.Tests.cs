using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace g3.curve.UnitTests
{
    [TestClass]
    public class PolyLineCombinerTests
    {
        private readonly double delta = 1e-6;

        [TestMethod]
        public void ConnectedSimple()
        {
            // Arrange
            var polylines = new List<PolyLine2d>();
            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, 1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, -1),
            }));

            // Act
            var result = PolyLineCombiner.CombineConnectedPolyLines(polylines, 1e-6);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].VertexCount);
            Assert.AreEqual(2, result[0].ArcLength, delta);
        }

        [TestMethod]
        public void UnconnectedSimple()
        {
            // Arrange
            var polylines = new List<PolyLine2d>();
            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0.1),
                new Vector2d(1, 1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, -1),
            }));

            // Act
            var result = PolyLineCombiner.CombineConnectedPolyLines(polylines, 1e-6);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ConnectedLoop()
        {
            // Arrange
            var polylines = new List<PolyLine2d>();
            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(0, 0),
                new Vector2d(1, 0),
                new Vector2d(1, 1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(0, 0),
                new Vector2d(0, 1),
                new Vector2d(1, 1),
            }));

            // Act
            var result = PolyLineCombiner.CombineConnectedPolyLines(polylines, 1e-6);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, result[0].VertexCount);
            Assert.AreEqual(4, result[0].ArcLength, delta);
        }

        [TestMethod]
        public void Mixed()
        {
            // Arrange
            var polylines = new List<PolyLine2d>();
            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, 1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, -1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(3, 0),
                new Vector2d(3, 1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(4, 0),
                new Vector2d(4, -1),
            }));

            // Act
            var result = PolyLineCombiner.CombineConnectedPolyLines(polylines, 1e-6);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void ContainsThreeWayJunction_DoesNotCombine()
        {
            // Arrange
            var polylines = new List<PolyLine2d>();
            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, 1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(1, 0),
                new Vector2d(1, -1),
            }));

            polylines.Add(new PolyLine2d(new Vector2d[] {
                new Vector2d(0, 0),
                new Vector2d(1, 0),
            }));

            // Act
            var result = PolyLineCombiner.CombineConnectedPolyLines(polylines, 1e-6);

            // Assert
            Assert.AreEqual(3, result.Count);
        }
    }
}