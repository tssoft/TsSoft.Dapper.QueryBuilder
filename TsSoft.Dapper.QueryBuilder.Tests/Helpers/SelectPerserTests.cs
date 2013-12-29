using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    [TestClass]
    public class SelectPerserTests
    {
        private SelectParser selectParser;

        [TestInitialize]
        public void Init()
        {
            selectParser = new SelectParser();
        }

        [TestMethod]
        public void ParseTest1()
        {
            const string str = "Table:column1,column2,column3";
            IDictionary<string, ICollection<string>> res = selectParser.Parse(str);

            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(3, res["Table"].Count);
            string[] resultColumns = res["Table"].ToArray();
            Assert.AreEqual("column1", resultColumns[0]);
            Assert.AreEqual("column2", resultColumns[1]);
            Assert.AreEqual("column3", resultColumns[2]);
        }

        [TestMethod]
        public void ParseTest2()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column100";
            IDictionary<string, ICollection<string>> res = selectParser.Parse(str);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(3, res["Table"].Count);
            Assert.AreEqual(2, res["TableTwo"].Count);
            string[] result1Columns = res["Table"].ToArray();
            string[] result2Columns = res["TableTwo"].ToArray();
            Assert.AreEqual("column1", result1Columns[0]);
            Assert.AreEqual("column2", result1Columns[1]);
            Assert.AreEqual("column3", result1Columns[2]);

            Assert.AreEqual("column", result2Columns[0]);
            Assert.AreEqual("column100", result2Columns[1]);
        }

        [TestMethod]
        public void ParseTest3()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column100;Table:column4";
            IDictionary<string, ICollection<string>> res = selectParser.Parse(str);

            Assert.IsNotNull(res);
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(4, res["Table"].Count);
            Assert.AreEqual(2, res["TableTwo"].Count);
            string[] result1Columns = res["Table"].ToArray();
            string[] result2Columns = res["TableTwo"].ToArray();
            Assert.AreEqual("column1", result1Columns[0]);
            Assert.AreEqual("column2", result1Columns[1]);
            Assert.AreEqual("column3", result1Columns[2]);
            Assert.AreEqual("column4", result1Columns[3]);

            Assert.AreEqual("column", result2Columns[0]);
            Assert.AreEqual("column100", result2Columns[1]);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void ParseTestArgumentException()
        {
            const string str = "Table:column1:1:2:3:,column2,column3;TableTwo:column,column1;Table:column4";
            selectParser.Parse(str);
        }

        [TestMethod]
        [ExpectedException(typeof (DuplicateNameException))]
        public void ParseTestDuplicateNameException()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column1;Table:column1";
            selectParser.Parse(str);
        }
    }
}