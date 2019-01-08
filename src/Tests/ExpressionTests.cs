using System;
using System.Linq.Expressions;
using BuildItEasy.Tests.Sample.Domain;
using NUnit.Framework;

namespace BuildItEasy.Tests
{
    [TestFixture]
    public class ExpressionTests
    {
        [Test]
        public void MethodName_PreCondition_Assertion()
        {
            Test<Order>(o => o.History.Count >= 2);
        }
        
        private void Test<T>(Expression<Func<T, bool>> expression)
        {
            Console.WriteLine(expression.ToString());
        }
    }
}
