using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TroutDash.DatabaseImporter.Convention.DatabaseImporter;
using TroutDash.DatabaseImporter.Convention.DataImporter;
using TroutDash.DatabaseImporter.Convention.GeometryImporter;

namespace TroutDash.DatabaseImporter.Convention.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var packager = A.Fake<IDataPackager>();
            A.CallTo(() => packager.Unpack())
                .Returns(new DirectoryInfo("."));

            var connectionFactory = new DatabaseConnectionFactory();
            var shapeImporterFactory = new Dictionary<string, IGeometryImporter>();
//            var importer = msdaf.CreateDatabaseImporter<PostGisDatabaseImporter>(myDirectory);

            shapeImporterFactory.Add(".shp", new ShapefileImporter());
            var manifest = new DatabaseManifest(connectionFactory, new DatabaseImporterFactory(connectionFactory, shapeImporterFactory));
            var items = manifest.GetDatabaseImporters(packager.Unpack());
            foreach (var item in items) using(item)
            {
                var name = item.DatabaseName;
                var shapes = item.ShapeFiles;
                item.CreateDatabase();
                item.Import();
            }
        }
    }
}
