using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Tests
{
    [TestClass]
    public class QueryBuilderTest
    {
        [TestMethod]
        public void TestSimple()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria());
            Query query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName", SimplifyString(query.Sql));
            Assert.AreEqual("Id", query.SplitOn);
            DynamicParameters tmp;
            Assert.IsNotNull((tmp = query.Parameters as DynamicParameters));
            Assert.AreEqual(0, tmp.ParameterNames.Count());
        }

        [TestMethod]
        public void TestWhereEq()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    Id = 1,
                });
            Query query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Id = @TableNameId",
                            SimplifyString(query.Sql));
            DynamicParameters parameters = ToDynamicParameters(query.Parameters);
            Assert.AreEqual(1, parameters.ParameterNames.Count());
            Assert.AreEqual("TableNameId", parameters.ParameterNames.Single());
        }

        [TestMethod]
        public void TestLike()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    Name = "123",
                });
            Query query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Name Like @TableNameName",
                            SimplifyString(query.Sql));
            DynamicParameters parameters = ToDynamicParameters(query.Parameters);
            Assert.AreEqual(1, parameters.ParameterNames.Count());
            Assert.AreEqual("TableNameName", parameters.ParameterNames.Single());
        }

        [TestMethod]
        public void TestGtEq()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    DateFrom = DateTime.Now,
                });
            Query query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom",
                            SimplifyString(query.Sql));
            DynamicParameters parameters = ToDynamicParameters(query.Parameters);
            Assert.AreEqual(1, parameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateFrom", parameters.ParameterNames.Single());
        }

        [TestMethod]
        public void TestGtEqAndLtEq()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now
                });
            Query query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom AND TableName.Date <= @TableNameDateTo",
                SimplifyString(query.Sql));
            DynamicParameters parameters = ToDynamicParameters(query.Parameters);
            Assert.AreEqual(2, parameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateFrom", parameters.ParameterNames.First());
            Assert.AreEqual("TableNameDateTo", parameters.ParameterNames.Last());
        }

        private static string SimplifyString(string str)
        {
            return
                new Regex("\\s+").Replace(
                    str.Replace("\\r\\n", " ").Replace("\\r", " ").Replace("\\n", " ").Replace(Environment.NewLine, " "),
                    " ").Trim().Replace("  ", " ");
        }

        private static DynamicParameters ToDynamicParameters(object o)
        {
            return o as DynamicParameters;
        }

        [Table(Name = "TableName")]
        private class TestCriteria : Criteria
        {
            [Where]
            public int? Id { get; set; }

            [Where(WhereType = WhereType.Like)]
            public string Name { get; set; }

            [Where("Date", WhereType = WhereType.GtEq)]
            public DateTime? DateFrom { get; set; }

            [Where("Date", WhereType = WhereType.LtEq)]
            public DateTime? DateTo { get; set; }
        }

        private class TestQueryBuilder<T> : QueryBuilder<T> where T : Criteria
        {
            public TestQueryBuilder(T criteria)
                : base(criteria)
            {
            }
        }
    }
}