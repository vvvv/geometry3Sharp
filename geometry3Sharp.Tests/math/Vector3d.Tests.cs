using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace geometry3Sharp.Tests
{
    [TestClass]
    public class Vector3dTests
    {
        private static readonly double delta = 1e-6;
        private static readonly double d = 1 / Math.Sqrt(2);

        [TestMethod]
        public void AngleD()
        {
            var vector1 = new Vector3d(0, 0, 6);
            var vector2 = new Vector3d(0, 2, 2);
            double angle = Vector3d.AngleD(vector1, vector2, false);
            Assert.AreEqual(45, angle, delta);
        }

        [TestMethod]
        public void AngleD_Normalized()
        {
            var vector1 = new Vector3d(0, 0, 1);
            var vector2 = new Vector3d(0, d, d);
            double angle = Vector3d.AngleD(vector1, vector2, true);
            Assert.AreEqual(45, angle, delta);
        }

        [TestMethod]
        public void AngleR()
        {
            var vector1 = new Vector3d(0, 0, 6);
            var vector2 = new Vector3d(0, 2, 2);
            double angle = vector1.AngleR(vector2, false);
            Assert.AreEqual(Math.PI / 4, angle, delta);
        }

        [TestMethod]
        public void AngleR_Normalized()
        {
            var vector1 = new Vector3d(0, 0, 1);
            var vector2 = new Vector3d(0, d, d);
            double angle = vector1.AngleR(vector2, true);
            Assert.AreEqual(Math.PI / 4, angle, delta);
        }
    }
}
