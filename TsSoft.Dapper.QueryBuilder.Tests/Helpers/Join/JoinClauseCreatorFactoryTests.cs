using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    [TestClass]
    public class JoinClauseCreatorFactoryTests
    {
        private JoinClauseCreatorFactory joinClauseCreatorFactory;

        [TestInitialize]
        public void Init()
        {
            joinClauseCreatorFactory = new JoinClauseCreatorFactory();
        }

        [TestMethod]
        public void GetTest()
        {
            IJoinClauseCreator res = joinClauseCreatorFactory.Get(typeof (SimpleJoinAttribute));
            Assert.AreEqual(typeof (SimpleJoinClauseCreator), res.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void GetTestArgumentOutOfRangeException()
        {
            joinClauseCreatorFactory.Get(typeof (JoinAttribute));
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void GetTestArgumentException()
        {
            joinClauseCreatorFactory.Get(typeof (string));
        }
    }
}