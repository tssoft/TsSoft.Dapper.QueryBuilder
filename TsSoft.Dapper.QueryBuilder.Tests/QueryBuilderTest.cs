using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Formatters;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

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
            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);

            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameId", dynamicParameters.ParameterNames.Single());
            Assert.AreEqual(1, parameters["TableNameId"]);
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
            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);

            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameName", dynamicParameters.ParameterNames.Single());
            Assert.AreEqual("%123%", parameters["TableNameName"]);
        }

        [TestMethod]
        public void TestGtEq()
        {
            var crit = new TestCriteria
                {
                    DateFrom = DateTime.Now,
                };
            var builder = new TestQueryBuilder<TestCriteria>(crit);
            Query query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom",
                            SimplifyString(query.Sql));
            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);
            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateFrom", dynamicParameters.ParameterNames.Single());
            Assert.AreEqual(crit.DateFrom, parameters["TableNameDateFrom"]);
        }

        [TestMethod]
        public void TestGtEqAndLtEq()
        {
            var testCriteria = new TestCriteria
                {
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            Query query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom AND TableName.Date <= @TableNameDateTo",
                SimplifyString(query.Sql));
            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);
            Assert.AreEqual(2, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateFrom", dynamicParameters.ParameterNames.First());
            Assert.AreEqual("TableNameDateTo", dynamicParameters.ParameterNames.Last());
            Assert.AreEqual(testCriteria.DateFrom, parameters["TableNameDateFrom"]);
            Assert.AreEqual(testCriteria.DateTo, parameters["TableNameDateTo"]);
        }

        [TestMethod]
        public void TestIn()
        {
            var testCriteria = new TestCriteria
                {
                    Codes = new[] {"1", "2", "3"},
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            Query query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.Code in @TableNameCodes",
                SimplifyString(query.Sql));

            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);

            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameCodes", dynamicParameters.ParameterNames.Single());
            CollectionAssert.AreEqual(testCriteria.Codes.ToList(), (string[]) parameters["TableNameCodes"]);
        }

        [TestMethod]
        public void TestExpression()
        {
            var testCriteria = new TestCriteria
                {
                    DateWithExpression = DateTime.Now,
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            Query query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE ((TableName.Date is not null and TableName.Date >= @TableNameDateWithExpression) or (TableName.DateSecond >= @TableNameDateWithExpression))"
                , SimplifyString(query.Sql));
            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);
            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateWithExpression", dynamicParameters.ParameterNames.Single());
            Assert.AreEqual(testCriteria.DateWithExpression, parameters["TableNameDateWithExpression"]);
        }

        [TestMethod]
        public void TestFormatter()
        {
            var testCriteria = new TestCriteria
            {
                DateTimeWithFormatter = DateTime.Now,
            };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            Query query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE DateTimeWithFormatter = @DateTimeWithFormatter"
                , SimplifyString(query.Sql));
            DynamicParameters dynamicParameters = ToDynamicParameters(query.Parameters);
            Dictionary<string, object> parameters = GetKeyValues(dynamicParameters);
            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("DateTimeWithFormatter", dynamicParameters.ParameterNames.Single());
            Assert.AreEqual("1", parameters["DateTimeWithFormatter"]);
        }

        private static string SimplifyString(string str)
        {
            return
                new Regex("\\s+").Replace(
                    str.Replace("\\r\\n", " ").Replace("\\r", " ").Replace("\\n", " ").Replace(Environment.NewLine, " "),
                    " ").Trim().Replace("  ", " ");
        }

        private static Dictionary<string, object> GetKeyValues(DynamicParameters dp)
        {
            BindingFlags all = Enum.GetValues(typeof (BindingFlags))
                                   .Cast<BindingFlags>()
                                   .Aggregate((BindingFlags) 0, (flags, bindingFlags) => flags | bindingFlags);
            FieldInfo fieldInfo = dp.GetType().GetField("parameters", all);
            if (fieldInfo == null)
            {
                throw new InvalidOperationException();
            }
            object paramInfos = fieldInfo.GetValue(dp);
            var dictionary = new Dictionary<string, object>();
            foreach (string name in dp.ParameterNames)
            {
                var paramInfo = (paramInfos as IDictionary);
                if (paramInfo == null)
                {
                    throw new InvalidOperationException();
                }
                var value = paramInfo[name];
                dictionary.Add(name, value.GetType().GetProperty("Value").GetValue(value));
            }
            return dictionary;
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

            [Where]
            [Format(typeof(FormatterTest))]
            public DateTime DateTimeWithFormatter { get; set; }
        }

        private class TestQueryBuilder<T> : QueryBuilder<T> where T : Criteria
        {
            public TestQueryBuilder(T criteria)
                : base(criteria)
            {
            }
        }

        private class FormatterTest : IFormatter
        {
            public object Format(object input)
            {
                return "1";
            }
        }
    }
}