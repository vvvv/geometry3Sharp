using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace g3.math.Tests
{
    [TestClass]
    public class Vector2dTests
    {
        [TestMethod]
        public void AngleD_SameDirection()
        {
            // Arrange
            var vec1 = new Vector2d(1, 0);
            var vec2 = new Vector2d(1, 0);

            // Act
            var angle = vec1.AngleD(vec2);

            // Assert
            Assert.AreEqual(0, angle, MathUtil.Epsilon);
        }

        [TestMethod]
        public void AngleD_Right90Degrees()
        {
            // Arrange
            var vec1 = new Vector2d(1, 0);
            var vec2 = new Vector2d(0, -0.2);

            // Act
            var angle = vec1.AngleD(vec2);

            // Assert
            Assert.AreEqual(90, angle, MathUtil.Epsilon);
        }

        [TestMethod]
        public void AngleD_Left90Degrees()
        {
            // Arrange
            var vec1 = new Vector2d(1, 0);
            var vec2 = new Vector2d(0, 0.4);

            // Act
            var angle = vec1.AngleD(vec2);

            // Assert
            Assert.AreEqual(90, angle, MathUtil.Epsilon);
        }

        [TestMethod]
        public void AngleD_180Degrees()
        {
            // Arrange
            var vec1 = new Vector2d(1, 0);
            var vec2 = new Vector2d(-10, 0);

            // Act
            var angle = vec1.AngleD(vec2);

            // Assert
            Assert.AreEqual(180, angle, MathUtil.Epsilon);
        }
    }
}