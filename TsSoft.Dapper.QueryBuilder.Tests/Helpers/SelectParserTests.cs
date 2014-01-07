using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    [TestClass]
    public class SelectParserTests
    {
        private SelectParser _selectParser;

        [TestInitialize]
        public void Init()
        {
            _selectParser = new SelectParser();
        }

        [TestMethod]
        public void ParseTest1()
        {
            const string str = "Table:column1,column2,column3";
            var res = _selectParser.Parse(str);

            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(3, res["Table"].Count);
            var resultColumns = res["Table"].ToArray();
            Assert.AreEqual("column1", resultColumns[0].Select);
            Assert.AreEqual("column2", resultColumns[1].Select);
            Assert.AreEqual("column3", resultColumns[2].Select);
        }

        [TestMethod]
        public void ParseTest2()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column100";
            var res = _selectParser.Parse(str);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(3, res["Table"].Count);
            Assert.AreEqual(2, res["TableTwo"].Count);
            var result1Columns = res["Table"].ToArray();
            var result2Columns = res["TableTwo"].ToArray();
            Assert.AreEqual("column1", result1Columns[0].Select);
            Assert.AreEqual("column2", result1Columns[1].Select);
            Assert.AreEqual("column3", result1Columns[2].Select);

            Assert.AreEqual("column", result2Columns[0].Select);
            Assert.AreEqual("column100", result2Columns[1].Select);
        }

        [TestMethod]
        public void ParseTest3()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column100;Table:column4";
            var res = _selectParser.Parse(str);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(4, res["Table"].Count);
            Assert.AreEqual(2, res["TableTwo"].Count);
            var result1Columns = res["Table"].ToArray();
            var result2Columns = res["TableTwo"].ToArray();
            Assert.AreEqual("column1", result1Columns[0].Select);
            Assert.AreEqual("column2", result1Columns[1].Select);
            Assert.AreEqual("column3", result1Columns[2].Select);
            Assert.AreEqual("column4", result1Columns[3].Select);

            Assert.AreEqual("column", result2Columns[0].Select);
            Assert.AreEqual("column100", result2Columns[1].Select);
        }

        [TestMethod]
        [ExpectedException(typeof (DuplicateNameException))]
        public void ParseTestDuplicateNameException()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column1;Table:column1";
            _selectParser.Parse(str);
        }

        [TestMethod]
        public void ParseTestDuplicateNameNotStrict()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column1;Table:column1";
            var res = _selectParser.Parse(str, false);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(4, res["Table"].Count);
            Assert.AreEqual(2, res["TableTwo"].Count);
            var result1Columns = res["Table"].ToArray();
            var result2Columns = res["TableTwo"].ToArray();
            Assert.AreEqual("column1", result1Columns[0].Select);
            Assert.AreEqual("column2", result1Columns[1].Select);
            Assert.AreEqual("column3", result1Columns[2].Select);
            Assert.AreEqual("column1", result1Columns[3].Select);

            Assert.AreEqual("column", result2Columns[0].Select);
            Assert.AreEqual("column1", result2Columns[1].Select);
        }

        [TestMethod]
        public void ParseTestWithExpressions()
        {
            const string str =
                "Table:{{sum(x)}},one,two,{{three, four}},five,{{next}};" +
                "SecondTable:one,{{(select id from table2 where code=Table.Code)}}";
            var res = _selectParser.Parse(str);
            Assert.AreEqual(2, res.Count);
            var result = res["Table"].ToArray();
            Assert.IsTrue(result.All(x => x.Table.Equals("Table")));
            Assert.AreEqual(6, result.Length);
            Assert.AreEqual("sum(x)", result[0].Select);
            Assert.AreEqual(true, result[0].IsExpression);
            Assert.AreEqual("one", result[1].Select);
            Assert.AreEqual(false, result[1].IsExpression);
            Assert.AreEqual("two", result[2].Select);
            Assert.AreEqual(false, result[2].IsExpression);
            Assert.AreEqual("three, four", result[3].Select);
            Assert.AreEqual(true, result[3].IsExpression);
            Assert.AreEqual("five", result[4].Select);
            Assert.AreEqual(false, result[4].IsExpression);
            Assert.AreEqual("next", result[5].Select);
            Assert.AreEqual(true, result[5].IsExpression);

            var result2 = res["SecondTable"].ToArray();
            Assert.IsTrue(result2.All(x => x.Table.Equals("SecondTable")));
            Assert.AreEqual(2, result2.Length);
            Assert.AreEqual("one", result2[0].Select);
            Assert.AreEqual(false, result2[0].IsExpression);
            Assert.AreEqual("(select id from table2 where code=Table.Code)", result2[1].Select);
            Assert.AreEqual(true, result2[1].IsExpression);
        }
    }
}