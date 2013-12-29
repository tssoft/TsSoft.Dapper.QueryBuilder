using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    [TestClass]
    public class ManyToManyClauseCreatorTests
    {
        [TestMethod]
        public void CreateTest()
        {
            var attr = new ManyToManyJoinAttribute("CurrentTableField", JoinType.Left, "JoinedTable",
                                                   "CommunicationTable", "CommunicationTableCurrentTableField",
                                                   "CommunicationTableJoinedTableField")
                {
                    SelectColumns = "CurrentTable:Id,Name;CommunicationTable:Required;JoinedTable:Id,Name",
                    CurrentTable = "CurrentTable",
                    JoinedTableField = "Id",
                    CurrentTableField = "CurrentTableField",
                };
            var creator = new ManyToManyClauseCreator();
            JoinClause res = creator.Create(attr);
            Assert.IsTrue(res.HasJoin);
            Assert.AreEqual(JoinType.Left, res.JoinType);
            Assert.AreEqual(2, res.JoinSqls.Count());
            Assert.AreEqual(5, res.SelectsSql.Count());
            Assert.AreEqual("SplitOnJoinedTableId", res.Splitter);

            Assert.AreEqual("CurrentTable.Id", res.SelectsSql.ToArray()[0]);
            Assert.AreEqual("CurrentTable.Name", res.SelectsSql.ToArray()[1]);
            Assert.AreEqual("CommunicationTable.Required", res.SelectsSql.ToArray()[2]);
            Assert.AreEqual("JoinedTable.Id", res.SelectsSql.ToArray()[3]);
            Assert.AreEqual("JoinedTable.Name", res.SelectsSql.ToArray()[4]);

            Assert.AreEqual(
                "CommunicationTable on CommunicationTable.CommunicationTableCurrentTableField = CurrentTable.CurrentTableField",
                res.JoinSqls.ToArray()[0]);
            Assert.AreEqual("JoinedTable on JoinedTable.Id = CommunicationTable.CommunicationTableJoinedTableField",
                            res.JoinSqls.ToArray()[1]);
        }
    }
}