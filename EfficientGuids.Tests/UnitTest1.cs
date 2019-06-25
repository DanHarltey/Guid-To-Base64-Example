using System;
using Xunit;

namespace EfficientGuids.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Test(GuidExtensions.EncodeBase64String);
            Test(GuidExtensions.EncodeBase64StringImproved);

            Test(GuidExtensions.ToBase64);
            Test(GuidExtensions.ToBase64Unrolled);
        }

        private static void Test(Func<Guid, string> toTest)
        {
            for (int i = 0; i < 100; i++)
            {
                Guid guid = Guid.NewGuid();

                string expected = Orginal(guid);
                string actual = toTest(guid);

                Assert.Equal(expected, actual);
            }
        }

        private static string Orginal(Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                   .Replace("/", "-")
                   .Replace("+", "_")
                   .Replace("=", "");
        }
    }
}
