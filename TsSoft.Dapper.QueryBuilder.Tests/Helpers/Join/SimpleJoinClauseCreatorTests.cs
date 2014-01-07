using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    [TestClass]
    public class SimpleJoinClauseCreatorTests
    {
        [TestMethod]
        public void CreateTest()
        {
            var creator = new SimpleJoinClauseCreator();
            var attr = new SimpleJoinAttribute("CurrentTableField", JoinType.Left, "JoinedTable")
            {
                CurrentTable = "CurrentTable",
                JoinedTableField = "JoinedField"
            };
            var res = creator.Create(attr);
            Assert.AreEqual(JoinType.Left, res.JoinType);
            Assert.AreEqual(1, res.JoinSqls.Count());
            Assert.AreEqual(1, res.SelectsSql.Count());
            Assert.AreEqual("JoinedTable.*", res.SelectsSql.First());
            Assert.AreEqual("SplitOnJoinedTableJoinedField", res.Splitter);
            Assert.AreEqual("JoinedTable on JoinedTable.JoinedField = CurrentTable.CurrentTableField",
                res.JoinSqls.First());
            Assert.IsTrue(res.HasJoin);
        }

        [TestMethod]
        public void CreateNotJoinTest()
        {
            var creator = new SimpleJoinClauseCreator();
            var attr = new SimpleJoinAttribute("CurrentTableField", JoinType.Left, "JoinedTable")
            {
                CurrentTable = "CurrentTable",
                JoinedTableField = "JoinedField"
            };
            var res = creator.CreateNotJoin(attr);
            Assert.AreEqual(JoinType.Left, res.JoinType);
            Assert.IsTrue(res.JoinSqls == null || !res.JoinSqls.Any());
            Assert.AreEqual("SplitOnJoinedTableJoinedField", res.Splitter);
            Assert.IsFalse(res.HasJoin);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void CreateTestException()
        {
            var creator = new SimpleJoinClauseCreator();
            creator.Create(new TestJoinAttr(JoinType.Inner));
        }

        private class TestJoinAttr : JoinAttribute
        {
            public TestJoinAttr(JoinType joinType) : base("cf", joinType)
            {
            }
        }
    }
}