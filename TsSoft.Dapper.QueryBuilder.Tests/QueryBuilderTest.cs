using System;
using System.Collections.Generic;
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

        [TestMethod]
        public void TestIn()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    Codes = new string[3] {"1", "2", "3"},
                });
            Query query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.Code in @TableNameCodes",
                SimplifyString(query.Sql));
            DynamicParameters parameters = ToDynamicParameters(query.Parameters);
            Assert.AreEqual(1, parameters.ParameterNames.Count());
            Assert.AreEqual("TableNameCodes", parameters.ParameterNames.Single());
        }

        [TestMethod]
        public void TestExpression()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
            {
                DateWithExpression = DateTime.Now,
            });
            Query query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE ((TableName.Date is not null and TableName.Date >= @TableNameDateWithExpression) or (TableName.DateSecond >= @TableNameDateWithExpression))"
                , SimplifyString(query.Sql));
            DynamicParameters parameters = ToDynamicParameters(query.Parameters);
            Assert.AreEqual(1, parameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateWithExpression", parameters.ParameterNames.Single());
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

            [Where("Code", WhereType = WhereType.In)]
            public IEnumerable<string> Codes { get; set; }

            [Where("Date",
                Expression =
                    "(/**TableName**/./**FieldName**/ is not null and /**TableName**/./**FieldName**/ /**CompareOperation**/ /**Parameter**/)" +
                    " or " +
                    "(/**TableName**/.DateSecond /**CompareOperation**/ /**Parameter**/)",
                WhereType = WhereType.GtEq)]
            public DateTime? DateWithExpression { get; set; }
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