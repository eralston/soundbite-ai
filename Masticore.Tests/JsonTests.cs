using Xunit;

namespace Masticore.Tests
{
    public class JsonTests : TestBase
    {
        private class MyTestObject
        {
            public string Name { get; set; }
            public bool IsTrue { get; set; }
            public int SomeInt { get; set; }
        }

        /// <summary>
        /// Ensure that JSON parsing works for different casing of JSON.
        /// </summary>
        [Fact]
        public void TestJsonCasing()
        {
            MyTestObject testObjA = JsonUtils.FromJson<MyTestObject>("{ \"name\": \"Some Name Here\", \"isTrue\": true, \"someInt\": 5 }"); // Camelcase
            MyTestObject testObjB = JsonUtils.FromJson<MyTestObject>("{ \"Name\": \"Some Name Here\", \"IsTrue\": true, \"SomeInt\": 5 }"); // Titlecase
            MyTestObject testObjC = JsonUtils.FromJson<MyTestObject>("{ \"NAME\": \"Some Name Here\", \"ISTRUE\": true, \"SOMEINT\": 5 }"); // Uppercase
            MyTestObject testObjD = JsonUtils.FromJson<MyTestObject>("{ \"name\": \"Some Name Here\", \"istrue\": true, \"someint\": 5 }"); // Lowercase

            Assert.NotNull(testObjA);
            Assert.NotNull(testObjB);
            Assert.NotNull(testObjC);
            Assert.NotNull(testObjD);
            Assert.Equal("Some Name Here", testObjA.Name);
            Assert.Equal("Some Name Here", testObjB.Name);
            Assert.Equal("Some Name Here", testObjC.Name);
            Assert.Equal("Some Name Here", testObjD.Name);
            Assert.True(testObjA.IsTrue);
            Assert.True(testObjB.IsTrue);
            Assert.True(testObjC.IsTrue);
            Assert.True(testObjD.IsTrue);
            Assert.Equal(5, testObjA.SomeInt);
            Assert.Equal(5, testObjB.SomeInt);
            Assert.Equal(5, testObjC.SomeInt);
            Assert.Equal(5, testObjD.SomeInt);
        }
    }
}