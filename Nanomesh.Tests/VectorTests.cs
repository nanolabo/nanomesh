using NUnit.Framework;

namespace Nanomesh.Tests
{
    public class VectorTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Vector2F_Equality()
        {
            // Same X / Y
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0f, 0f), new Vector2(0f, 0f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.1f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.01f, 0.01f), new Vector2(0.01f, 0.01f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.001f, 0.001f), new Vector2(0.001f, 0.001f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.0001f, 0.0001f), new Vector2(0.0001f, 0.0001f)));
            // Different X / Y
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0f, 1f), new Vector2(0f, 1f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.0001f, 0.0002f), new Vector2(0.0001f, 0.0002f)));

            // Tiny delta
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.000001f, 0f), new Vector2(0f, 0f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0f, 0.000001f), new Vector2(0f, 0f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.000001f, 0.000001f), new Vector2(0f, 0f)));
            Assert.IsTrue(Vector2FComparer.Default.Equals(new Vector2(0.000001f, 0f), new Vector2(0f, 0.000001f)));

            // Not equal
            Assert.IsFalse(Vector2FComparer.Default.Equals(new Vector2(1f, 0f), new Vector2(0f, 0f)));
            Assert.IsFalse(Vector2FComparer.Default.Equals(new Vector2(0.1f, 0f), new Vector2(0f, 0f)));
            Assert.IsFalse(Vector2FComparer.Default.Equals(new Vector2(0.01f, 0f), new Vector2(0f, 0f)));
            Assert.IsFalse(Vector2FComparer.Default.Equals(new Vector2(0.001f, 0f), new Vector2(0f, 0f)));
            Assert.IsFalse(Vector2FComparer.Default.Equals(new Vector2(0.0001f, 0f), new Vector2(0f, 0f)));
        }
    }
}