using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests.UseCase
{
    [TestFixture, NUnit.Framework.Ignore]
    public class SimpleUseCase
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            //Inject your database provider here
            OrmLiteConfig.DialectProvider = new SqlServerOrmLiteDialectProvider();
        }

        public class User
        {
            public long Id { get; set; }

            [Index]
            public string Name { get; set; }

            public DateTime CreatedDate { get; set; }
        }

        public class Dual
        {
            [AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Test]
        public void Simple_CRUD_example()
        {
            //using (IDbConnection db = ":memory:".OpenDbConnection())

            var connStr = "Data Source=.;Initial Catalog=TestDb;Integrated Security=True";
            var sqlServerFactory = new OrmLiteConnectionFactory(connStr, SqlServerOrmLiteDialectProvider.Instance);

            using (IDbConnection db = sqlServerFactory.OpenDbConnection())
            using (IDbCommand dbCmd = db.CreateCommand())
            {
                dbCmd.CreateTable<Dual>(true);
                dbCmd.CreateTable<User>(true);

                dbCmd.Insert(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now });
                dbCmd.Insert(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now });
                dbCmd.Insert(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });

                dbCmd.Insert(new Dual { Name = "Dual" });
                var lastInsertId = dbCmd.GetLastInsertId();
                Assert.That(lastInsertId, Is.GreaterThan(0));

                var rowsB = dbCmd.Select<User>("Name = {0}", "B");

                Assert.That(rowsB, Has.Count.EqualTo(2));

                var rowIds = rowsB.ConvertAll(x => x.Id);
                Assert.That(rowIds, Is.EquivalentTo(new List<long> { 2, 3 }));

                rowsB.ForEach(x => dbCmd.Delete(x));

                rowsB = dbCmd.Select<User>("Name = {0}", "B");
                Assert.That(rowsB, Has.Count.EqualTo(0));

                var rowsLeft = dbCmd.Select<User>();
                Assert.That(rowsLeft, Has.Count.EqualTo(1));

                Assert.That(rowsLeft[0].Name, Is.EqualTo("A"));
            }
        }

    }

}