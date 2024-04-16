using TestCsv;

namespace TestProject1
{
    public class CalcuclatorTests
    {
        [Fact]
        public void TestAddNumbers_ExcpectedResult()
        {
            var calc = new Calc();

            var result = calc.AddTwoNumber(2, 2);

            Assert.Equal(4, result);
        }

        [Fact]
        public void TestAddNumbers()
        {
            var calc = new Calc();

            var result = calc.AddTwoNumber(2, 3);

            Assert.Equal(5, result);
        }
    }
}