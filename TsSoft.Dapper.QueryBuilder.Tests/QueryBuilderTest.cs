using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Formatters;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;
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
            var query = builder.Build();
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
            var query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Id = @TableNameId",
                            SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);

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
            var query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Name Like @TableNameName",
                            SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);

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
            var query = builder.Build();
            Assert.AreEqual("Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom",
                            SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
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
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom AND TableName.Date <= @TableNameDateTo",
                SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
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
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.Code in @TableNameCodes",
                SimplifyString(query.Sql));

            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);

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
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE ((TableName.Date is not null and TableName.Date >= @TableNameDateWithExpression) or (TableName.DateSecond >= @TableNameDateWithExpression))"
                , SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
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
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* from TableName WHERE TableName.DateTimeWithFormatter = @TableNameDateTimeWithFormatter"
                , SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
            Assert.AreEqual(1, dynamicParameters.ParameterNames.Count());
            Assert.AreEqual("TableNameDateTimeWithFormatter", dynamicParameters.ParameterNames.Single());
            Assert.AreEqual("1", parameters["TableNameDateTimeWithFormatter"]);
        }

        [TestMethod]
        public void TestSimpleJoin()
        {
            var testCriteria = new TestJoinCriteria
                {
                    WithAnotherTable = true,
                };
            var builder = new TestQueryBuilder<TestJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* , 0 as SplitOnAnotherTableCurrentTableId , AnotherTable.* from TableName LEFT JOIN AnotherTable on AnotherTable.CurrentTableId = TableName.CurrentTableId"
                , SimplifyString(query.Sql)
                );
        }

        [TestMethod]
        public void TestSimpleJoinEmpty()
        {
            var testCriteria = new TestJoinCriteria
                {
                    WithAnotherTable = false,
                };
            var builder = new TestQueryBuilder<TestJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* , 0 as SplitOnAnotherTableCurrentTableId from TableName"
                , SimplifyString(query.Sql)
                );
        }

        [TestMethod]
        public void TestManyToManyJoin()
        {
            var testCriteria = new TestManyToManyJoinCriteria
                {
                    WithAnotherTable = true,
                };
            var builder = new TestQueryBuilder<TestManyToManyJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* , 0 as SplitOnAnotherTableAnotherId , AnotherTable.* from TableName " +
                "LEFT JOIN AnotherTableCurrentTable on AnotherTableCurrentTable.CurrentId = TableName.CurrentId " +
                "LEFT JOIN AnotherTable on AnotherTable.AnotherId = AnotherTableCurrentTable.AnotherId"
                , SimplifyString(query.Sql)
                );
        }

        [TestMethod]
        public void TestManyToManyJoinEmpty()
        {
            var testCriteria = new TestManyToManyJoinCriteria
                {
                    WithAnotherTable = false,
                };
            var builder = new TestQueryBuilder<TestManyToManyJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select TableName.* , 0 as SplitOnAnotherTableAnotherId from TableName"
                , SimplifyString(query.Sql)
                );
        }

        [TestMethod]
        public void TestJoinOrder()
        {
            var testCriteria = new TestJoinOrderCriteria
                {
                    WithAirplans = true,
                    WithCars = true,
                    WithHouses = true,
                };
            var builder = new TestQueryBuilder<TestJoinOrderCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Persons.* , 0 as SplitOnCarsPersonId , Cars.* , 0 as SplitOnAirplansPersonId , Airplans.* , 0 as SplitOnHousesPersonId , Houses.* from Persons " +
                "LEFT JOIN Cars on Cars.PersonId = Persons.Id " +
                "LEFT JOIN Airplans on Airplans.PersonId = Persons.Id " +
                "LEFT JOIN Houses on Houses.PersonId = Persons.Id"
                , SimplifyString(query.Sql)
                );
        }

        [TestMethod]
        public void TestSelect()
        {
            var testCriteria = new TestSelectCriteria
                {
                    WithSum = true,
                    SelectClause = null,
                    AddSelect = "Shipments:Name,Mass"
                };
            var builder = new TestQueryBuilder<TestSelectCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Shipments.Name , Shipments.Mass , Sum(Shipments.Price) from Shipments",
                SimplifyString(query.Sql));
        }

        [TestMethod]
        public void TestJoinOrderAnotherJoins()
        {
            var criteria = new TestAnotherJoinCriteria
                {
                    WithOwner = true,
                    WithPersons = true,
                };
            var builder = new QueryBuilder<TestAnotherJoinCriteria>(criteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnPersonsHouseId , Persons.* , 0 as SplitOnOwnersId , Owners.* from Houses LEFT JOIN Persons on Persons.HouseId = Houses.Id INNER JOIN Owners on Owners.Id = Houses.OwnerId",
                SimplifyString(query.Sql));
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
            var all = Enum.GetValues(typeof (BindingFlags))
                          .Cast<BindingFlags>()
                          .Aggregate((BindingFlags) 0, (flags, bindingFlags) => flags | bindingFlags);
            var fieldInfo = dp.GetType().GetField("parameters", all);
            if (fieldInfo == null)
            {
                throw new InvalidOperationException();
            }
            var paramInfos = fieldInfo.GetValue(dp);
            var dictionary = new Dictionary<string, object>();
            foreach (var name in dp.ParameterNames)
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

        [TestMethod]
        public void TestMultipleJoin()
        {
            var criteria = new TestMultipleJoinCriteria
                {
                    WithPersonsAndPersonInfo = true
                };
            var builder = new QueryBuilder<TestMultipleJoinCriteria>(criteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnPersonsHouseId , Persons.* , 0 as SplitOnPersonInfosPersonId , PersonInfos.* from Houses LEFT JOIN Persons on Persons.HouseId = Houses.Id INNER JOIN PersonInfos on PersonInfos.PersonId = Persons.Id",
                SimplifyString(query.Sql));
        }

        [TestMethod]
        public void BoolWhereCriteriaTrueTest()
        {
            var criteria = new TestWhereBoolCriteria
                {
                    OnlySingleStorey = true,
                };
            var builder = new QueryBuilder<TestWhereBoolCriteria>(criteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* from Houses WHERE (Houses.FloorsCount = 1)",
                SimplifyString(query.Sql));
        }

        [TestMethod]
        public void BoolWhereCriteriaFalseTest()
        {
            var criteria = new TestWhereBoolCriteria
                {
                    OnlySingleStorey = false,
                };
            var builder = new QueryBuilder<TestWhereBoolCriteria>(criteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* from Houses",
                SimplifyString(query.Sql));
        }

        private class FormatterTest : IFormatter
        {
            public void Format(ref object input)
            {
                input = "1";
            }
        }

        [Table(Name = "Houses")]
        private class TestAnotherJoinCriteria : Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "Persons", JoinedTableField = "HouseId", Order = 1)]
            public bool WithPersons { get; set; }

            [SimpleJoin("OwnerId", JoinType.Inner, "Owners", JoinedTableField = "Id", Order = 2)]
            public bool WithOwner { get; set; }
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
            [Format(typeof (FormatterTest))]
            public DateTime? DateTimeWithFormatter { get; set; }
        }

        [Table(Name = "TableName")]
        private class TestJoinCriteria : Criteria
        {
            [SimpleJoin("CurrentTableId", JoinType.Left, "AnotherTable", JoinedTableField = "CurrentTableId")]
            public bool WithAnotherTable { get; set; }

            [Where]
            public int? Id { get; set; }
        }

        [Table(Name = "Persons")]
        private class TestJoinOrderCriteria : Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "Houses", JoinedTableField = "PersonId")]
            public bool WithHouses { get; set; }

            [SimpleJoin("Id", JoinType.Left, "Airplans", JoinedTableField = "PersonId", Order = 2)]
            public bool WithAirplans { get; set; }

            [SimpleJoin("Id", JoinType.Left, "Cars", JoinedTableField = "PersonId", Order = 1)]
            public bool WithCars { get; set; }
        }

        [Table(Name = "TableName")]
        private class TestManyToManyJoinCriteria : Criteria
        {
            [ManyToManyJoin("CurrentId", JoinType.Left, "AnotherTable", "AnotherTableCurrentTable", "CurrentId",
                "AnotherId", JoinedTableField = "AnotherId")]
            public bool WithAnotherTable { get; set; }

            [Where]
            public int? Id { get; set; }
        }

        [Table(Name = "Houses")]
        private class TestMultipleJoinCriteria : Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "Persons", JoinedTableField = "HouseId", Order = 1)]
            [SimpleJoin("Id", JoinType.Inner, "PersonInfos", CurrentTable = "Persons", JoinedTableField = "PersonId",
                Order = 2)]
            public bool WithPersonsAndPersonInfo { get; set; }
        }

        private class TestQueryBuilder<T> : QueryBuilder<T> where T : Criteria
        {
            public TestQueryBuilder(T criteria)
                : base(criteria)
            {
            }
        }

        [Table(Name = "Shipments")]
        private class TestSelectCriteria : Criteria
        {
            [AddSelect]
            public string AddSelect { get; set; }

            [AddSelect(SelectColumns = "TableName:{{Sum(Shipments.Price)}}")]
            public bool WithSum { get; set; }
        }

        [Table(Name = "Houses")]
        private class TestWhereBoolCriteria : Criteria
        {
            [Where("FloorsCount", Expression = "/**TableName**/./**FieldName**/ = 1")]
            public bool OnlySingleStorey { get; set; }
        }

        [Table(Name = "Houses")]
        private class TestWhereMultipleCriteria : Criteria
        {
            [Where("OwnerId")]
            [Where("OwnerId", WhereType = WhereType.IsNotNull)]
            public Guid? HasOwnerNotThis { get; set; } 
        }

        [TestMethod]
        public void TestWhereMultiple()
        {
            var testCriteria = new TestWhereMultipleCriteria
            {
                HasOwnerNotThis = Guid.NewGuid()
            };
            var builder = new TestQueryBuilder<TestWhereMultipleCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* from Houses WHERE Houses.OwnerId = @HousesHasOwnerNotThis AND Houses.OwnerId is not null",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class TestIncludingCriteria : Criteria
        {
            [SimpleJoin("OwnerId", JoinType.Left, "Owners", JoinedTableField = "Id", Including = "WithOwners", SelectColumns = "Owners:")]
            [Where("OwnerName", TableName = "Owners", WhereType = WhereType.Like)]
            public string OwnerName { get; set; }

            [SimpleJoin("OwnerId", JoinType.Left, "Owners", JoinedTableField = "Id")]
            public bool WithOwners { get; set; }
        }

        [TestMethod]
        public void TestIncludingIncludingFieldIsExists()
        {
            var testCriteria = new TestIncludingCriteria
            {
                OwnerName = "Vasya",
                WithOwners = true,
            };
            var builder = new TestQueryBuilder<TestIncludingCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnOwnersId , Owners.* from Houses LEFT JOIN Owners on Owners.Id = Houses.OwnerId WHERE Owners.OwnerName Like @OwnersOwnerName",
                SimplifyString(query.Sql));
        }

        [TestMethod]
        public void TestIncludingIncludingFieldIsNotExists()
        {
            var testCriteria = new TestIncludingCriteria
            {
                OwnerName = "Vasya",
                WithOwners = false,
            };
            var builder = new TestQueryBuilder<TestIncludingCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnOwnersId from Houses LEFT JOIN Owners on Owners.Id = Houses.OwnerId WHERE Owners.OwnerName Like @OwnersOwnerName",
                SimplifyString(query.Sql));
        }
        [Table("Houses")]
        private class JoinWithoutJoinedFieldCriteria : Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Persons")]
            public bool WithPersons { get; set; } 
        }

        [TestMethod]
        public void JoinWithoutJoinedField()
        {
            var testCriteria = new JoinWithoutJoinedFieldCriteria
            {
                WithPersons = true
            };
            var builder = new TestQueryBuilder<JoinWithoutJoinedFieldCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnPersonsHouseId , Persons.* from Houses LEFT JOIN Persons on Persons.HouseId = Houses.HouseId",
                SimplifyString(query.Sql));
        }

        [TestMethod]
        public void JoinNoSplitTest()
        {
            var testCriteria = new JoinNoSplitCriteria
            {
                OwnerId = 1
            };
            var builder = new TestQueryBuilder<JoinNoSplitCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnOwnersId from Houses LEFT JOIN HouseOwners on HouseOwners.HouseId = Houses.Id WHERE HouseOwners.OwnerId = @HouseOwnersOwnerId",
                SimplifyString(query.Sql));
            Assert.AreEqual("SplitOnOwnersId", query.SplitOn);
            
        }

        [Table("Houses")]
        private class JoinNoSplitCriteria : Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "HouseOwners", JoinedTableField = "HouseId", SelectColumns = "HouseOwners:", NoSplit = true, Including = "WithOwners")]
            [Where("OwnerId", TableName = "HouseOwners")]
            public int? OwnerId { get; set; }

            [ManyToManyJoin("Id", JoinType.Left, "Owners", "HouseOwners", "HouseId", "OwnerId", JoinedTableField = "Id")]
            public bool WithOwners { get; set; }
        }

        [TestMethod]
        public void JoinNoSplitTest2()
        {
            var testCriteria = new JoinNoSplitCriteria2
            {
                OwnerId = 1
            };
            var builder = new TestQueryBuilder<JoinNoSplitCriteria2>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* from Houses LEFT JOIN HouseOwners on HouseOwners.HouseId = Houses.Id WHERE HouseOwners.OwnerId = @HouseOwnersOwnerId",
                SimplifyString(query.Sql));
            Assert.AreEqual("", query.SplitOn);

        }

        [Table("Houses")]
        private class JoinNoSplitCriteria2 : Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "HouseOwners", JoinedTableField = "HouseId", SelectColumns = "HouseOwners:", NoSplit = true)]
            [Where("OwnerId", TableName = "HouseOwners")]
            public int? OwnerId { get; set; }
        }

        [Table("Houses")]
        private class JoinAddOnCriteria : Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Owners", AddOnClause = "Owners.OwnerId in (1,2,3)", SelectColumns = "Owners:")]
            public bool WithOwners { get; set; }
        }

        [TestMethod]
        public void JoinAddOnTest()
        {
            var testCriteria = new JoinAddOnCriteria
            {
                WithOwners = true
            };
            var builder = new TestQueryBuilder<JoinAddOnCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnOwnersHouseId from Houses LEFT JOIN Owners on Owners.HouseId = Houses.HouseId AND Owners.OwnerId in (1,2,3)",
                SimplifyString(query.Sql));
            
        }

        [TestMethod]
        public void JoinAddOnTypeTest()
        {
            var testCriteria = new JoinAddOnTypeCriteria
            {
                WithPeople = true
            };
            var builder = new TestQueryBuilder<JoinAddOnTypeCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnPeoplePeopleId , People.* from Houses LEFT JOIN HousePeople on HousePeople.HouseId = Houses.HouseId AND HousesPeople.Required = 1 LEFT JOIN People on People.PeopleId = HousePeople.PeopleId",
                SimplifyString(query.Sql));

        }

        [Table("Houses")]
        private class JoinAddOnTypeCriteria : Criteria
        {
            [ManyToManyJoin("HouseId", JoinType.Left, "People", "HousePeople", "HouseId", "PeopleId", JoinedTableField = "PeopleId", AddOnType = AddOnType.ForCommunication, AddOnClause = "HousesPeople.Required = 1")]
            public bool WithPeople { get; set; }
        }

        [TestMethod]
        public void SelectNullableTest()
        {
            var testCriteria = new SelectNullableCriteria
                {
                    Id = 1
                };
            var builder = new TestQueryBuilder<SelectNullableCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , Houses.Name from Houses WHERE Houses.Id = @HousesId",
                SimplifyString(query.Sql));
        }

        [TestMethod]
        public void SelectNullableNullTest()
        {
            var testCriteria = new SelectNullableCriteria
            {
            };
            var builder = new TestQueryBuilder<SelectNullableCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* from Houses",
                SimplifyString(query.Sql));
        }


        [Table("Houses")]
        private class SelectNullableCriteria : Criteria
        {
            [Where]
            [AddSelect(SelectColumns = "Houses:Name")]
            public int? Id { get; set; }
        }

        [TestMethod]
        public void JoinSelectTest()
        {
            var testCriteria = new JoinSelectCriteria
            {
                WithOwners = true
            };
            var builder = new TestQueryBuilder<JoinSelectCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* , 0 as SplitOnOwnersHouseId , Owners.Name , Owners.Id , Type as OwnerType from Houses LEFT JOIN Owners on Owners.HouseId = Houses.HouseId",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class JoinSelectCriteria : Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Owners", SelectColumns = "Owners:Name,Id,{{Type as OwnerType}}")]
            public bool WithOwners { get; set; }
        }

        [TestMethod]
        public void JoinReferenceTest()
        {
            var testCriteria = new JoinReferenceCriteria
            {
                OwnerIds = new List<int>()
                    {
                        1,2,3,4
                    }
            };
            var builder = new TestQueryBuilder<JoinReferenceCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Houses.* from Houses LEFT JOIN Owners on Owners.HouseId = Houses.HouseId WHERE Owners.Id in @OwnersOwnerIds",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class JoinReferenceCriteria : Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Owners", NoSplit = true, SelectColumns = "Owners:")]
            [Where("Id", TableName = "Owners", WhereType = WhereType.In)]
            public IEnumerable<int> OwnerIds { get; set; }
        }

        [TestMethod]
        public void GroupByTest()
        {
            var testCriteria = new GroupByCriteria
            {
                GroupBy = new []{"Houses.OwnerId", "Houses.Category"},
                SelectClause = new SelectClause
                    {
                        Select = "Count(1) , Houses.OwnerId , Houses.Category",
                        IsExpression = true
                    }
            };
            var builder = new TestQueryBuilder<GroupByCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select Count(1) , Houses.OwnerId , Houses.Category from Houses GROUP BY Houses.OwnerId , Houses.Category",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class GroupByCriteria : Criteria
        {
        }

        [TestMethod]
        public void BaseTest()
        {
            var testCriteria = new RealCriteria
            {
                Id = Guid.NewGuid(),
                CustomerId = 1,
            };
            var builder = new TestQueryBuilder<RealCriteria>(testCriteria);
            var query = builder.Build();
            Assert.AreEqual(
                "Select RealHouses.* from RealHouses WHERE RealHouses.Id = @RealHousesId AND RealHouses.CustomerId = @RealHousesCustomerId",
                SimplifyString(query.Sql));
        }

        private abstract class BaseCriteria : Criteria
        {
            [Where]
            public Guid? Id { get; set; }

            [Where]
            public int? CustomerId { get; set; }
        }

        [Table("RealHouses")]
        private class RealCriteria : BaseCriteria
        {
        }
    }
}