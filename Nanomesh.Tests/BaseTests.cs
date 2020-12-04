using NUnit.Framework;

namespace Nanomesh.Tests
{
    public class BaseTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        [Category("Base")]
        public void DistancePointLine()
        {
            {
                var A = new Vector3(0, 0, 0);
                var B = new Vector3(0, 0, 0);
                var X = new Vector3(0, 0, 0);
                Assert.AreEqual(0, Vector3.DistancePointLine(X, A, B));
            }

            {
                var A = new Vector3(0, 0, 0);
                var B = new Vector3(0, 1, 0);
                var X = new Vector3(1, 0, 0);
                Assert.AreEqual(1, Vector3.DistancePointLine(X, A, B));
            }

            {
                var A = new Vector3(0, 0, 0);
                var B = new Vector3(0, 2, 3);
                var X = new Vector3(4, 0, 0);
                Assert.AreEqual(4, Vector3.DistancePointLine(X, A, B));
            }

            {
                var A = new Vector3(0, 0, 0);
                var B = new Vector3(0, 2, 3);
                var X = new Vector3(-4, 0, 0);
                Assert.AreEqual(4, Vector3.DistancePointLine(X, A, B));
            }

            {
                var A = new Vector3(0, 0, 0);
                var B = new Vector3(0, 0, 0.000001);
                var X = new Vector3(0.000001, 0, 0);
                Assert.AreEqual(0.000001, Vector3.DistancePointLine(X, A, B));
            }
        }
    }
}