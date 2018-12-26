using System;
using BuildItEasy.Reflection;
using NUnit.Framework;

namespace BuildItEasy.Tests.Reflection
{
    [TestFixture]
    public class ExpressionUtilsTests
    {
        private class TestClass
        {
            public int PrimitiveProperty { get; set; }
            public int PrimitiveField;

            public object ReferenceProperty { get; set; }
            public object ReferenceField;

            public DateTime ValueTypeProperty { get; set; }
            public DateTime ValueTypeField;

            public TestClass Parent { get; set; }
        }
        
        [Test]
        public void GetMemberName_PrimitiveProperty_ReturnsPropertyName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, int>(c => c.PrimitiveProperty);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.PrimitiveProperty)));
        }
        
        [Test]
        public void GetMemberName_PrimitiveField_ReturnsFieldName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, int>(c => c.PrimitiveField);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.PrimitiveField)));
        }
        
        [Test]
        public void GetMemberName_ReferenceProperty_ReturnsPropertyName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, object>(c => c.ReferenceProperty);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ReferenceProperty)));
        }
        
        [Test]
        public void GetMemberName_ReferenceField_ReturnsFieldName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, object>(c => c.ReferenceField);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ReferenceField)));
        }
        
        [Test]
        public void GetMemberName_ValueTypeProperty_ReturnsPropertyName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, DateTime>(c => c.ValueTypeProperty);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ValueTypeProperty)));
        }
        
        [Test]
        public void GetMemberName_ValueTypeField_ReturnsFieldName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, DateTime>(c => c.ValueTypeField);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ValueTypeField)));
        }
        
        [Test]
        public void GetMemberName_CastedProperty_ReturnsPropertyName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, object>(c => c.ValueTypeProperty);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ValueTypeProperty)));
        }
        
        [Test]
        public void GetMemberName_CastedField_ReturnsFieldName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, object>(c => c.ValueTypeField);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ValueTypeField)));
        }
        
        [Test]
        public void GetMemberName_ChildProperty_ReturnsPropertyName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, DateTime>(c => c.Parent.ValueTypeProperty);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ValueTypeProperty)));
        }
        
        [Test]
        public void GetMemberName_ChildField_ReturnsFieldName()
        {
            var memberName = ExpressionUtils.GetMemberName<TestClass, DateTime>(c => c.Parent.ValueTypeField);
            Assert.That(memberName, Is.EqualTo(nameof(TestClass.ValueTypeField)));
        }
    }
}
