using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Helpers.Where;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilderTests.Helpers
{
    [TestClass]
    public class WhereAttributeManagerTests
    {
        private WhereAttributeManager whereAttributeManager;

        [TestInitialize]
        public void Init()
        {
            whereAttributeManager = new WhereAttributeManager();
        }

        [TestMethod]
        public void TestIsWithoutValue()
        {
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.Eq));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.Gt));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.GtEq));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.In));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.Like));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.Lt));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.LtEq));
            Assert.IsFalse(whereAttributeManager.IsWithoutValue(WhereType.NotEq));

            Assert.IsTrue(whereAttributeManager.IsWithoutValue(WhereType.IsNotNull));
            Assert.IsTrue(whereAttributeManager.IsWithoutValue(WhereType.IsNull));
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void TestIsWithoutValueExpExc()
        {
            whereAttributeManager.IsWithoutValue((WhereType) 100000);
        }

        [TestMethod]
        public void TestGetSelector()
        {
            Assert.AreEqual("=", whereAttributeManager.GetSelector(WhereType.Eq));
            Assert.AreEqual("<>", whereAttributeManager.GetSelector(WhereType.NotEq));
            Assert.AreEqual(">", whereAttributeManager.GetSelector(WhereType.Gt));
            Assert.AreEqual("<", whereAttributeManager.GetSelector(WhereType.Lt));
            Assert.AreEqual(">=", whereAttributeManager.GetSelector(WhereType.GtEq));
            Assert.AreEqual("<=", whereAttributeManager.GetSelector(WhereType.LtEq));
            Assert.AreEqual("Like", whereAttributeManager.GetSelector(WhereType.Like));
            Assert.AreEqual("is null", whereAttributeManager.GetSelector(WhereType.IsNull));
            Assert.AreEqual("is not null", whereAttributeManager.GetSelector(WhereType.IsNotNull));
            Assert.AreEqual("in", whereAttributeManager.GetSelector(WhereType.In));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestGetSelectorExpExc()
        {
            whereAttributeManager.GetSelector((WhereType)100000);
        }

        [TestMethod]
        public void TestGetExpression()
        {
            Assert.AreEqual("= @Name", whereAttributeManager.GetExpression(WhereType.Eq, "@Name"));
            Assert.AreEqual("<> @Name", whereAttributeManager.GetExpression(WhereType.NotEq, "@Name"));
            Assert.AreEqual("> @Name", whereAttributeManager.GetExpression(WhereType.Gt, "@Name"));
            Assert.AreEqual("< @Name", whereAttributeManager.GetExpression(WhereType.Lt, "@Name"));
            Assert.AreEqual(">= @Name", whereAttributeManager.GetExpression(WhereType.GtEq, "@Name"));
            Assert.AreEqual("<= @Name", whereAttributeManager.GetExpression(WhereType.LtEq, "@Name"));
            Assert.AreEqual("Like @Name", whereAttributeManager.GetExpression(WhereType.Like, "@Name"));
            Assert.AreEqual("in @Name", whereAttributeManager.GetExpression(WhereType.In, "@Name"));
            Assert.AreEqual("is null", whereAttributeManager.GetExpression(WhereType.IsNull, "@Name"));
            Assert.AreEqual("is not null", whereAttributeManager.GetExpression(WhereType.IsNotNull, "@Name"));
            Assert.AreEqual("is null", whereAttributeManager.GetExpression(WhereType.IsNull, string.Empty));
            Assert.AreEqual("is not null", whereAttributeManager.GetExpression(WhereType.IsNotNull, string.Empty));
            Assert.AreEqual("is null", whereAttributeManager.GetExpression(WhereType.IsNull, null));
            Assert.AreEqual("is not null", whereAttributeManager.GetExpression(WhereType.IsNotNull, null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestGetExpressionExpExc()
        {
            whereAttributeManager.GetExpression((WhereType)100000, "ParameterNAme");
        }
    }
}
