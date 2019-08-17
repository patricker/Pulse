using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wallhaven;

namespace wallhaven_tests
{
    [TestClass]
    public class WallhavenSettingTests
    {
        [TestMethod]
        public void PurityTests()
        {
            WallhavenImageSearchSettings wh = new WallhavenImageSearchSettings();

            wh.SFW = wh.SKETCHY = wh.NSFW = false;
            Assert.AreEqual("000", wh.BuildPurityString());

            wh.SFW = wh.SKETCHY = wh.NSFW = true;
            Assert.AreEqual("111", wh.BuildPurityString());

            wh.SFW = wh.SKETCHY = wh.NSFW = false;
            wh.SFW = true;
            Assert.AreEqual("100", wh.BuildPurityString());

            wh.SFW = wh.SKETCHY = wh.NSFW = false;
            wh.SKETCHY = true;
            Assert.AreEqual("010", wh.BuildPurityString());

            wh.SFW = wh.SKETCHY = wh.NSFW = false;
            wh.NSFW = true;
            Assert.AreEqual("001", wh.BuildPurityString());
        }

        [TestMethod]
        public void CategoryTests()
        {
            WallhavenImageSearchSettings wh = new WallhavenImageSearchSettings();

            wh.General = wh.Anime = wh.People = false;
            Assert.AreEqual("000", wh.BuildCategoryString());

            wh.General = wh.Anime = wh.People = true;
            Assert.AreEqual("111", wh.BuildCategoryString());

            wh.General = wh.Anime = wh.People = false;
            wh.General = true;
            Assert.AreEqual("100", wh.BuildCategoryString());

            wh.General = wh.Anime = wh.People = false;
            wh.Anime = true;
            Assert.AreEqual("010", wh.BuildCategoryString());

            wh.General = wh.Anime = wh.People = false;
            wh.People = true;
            Assert.AreEqual("001", wh.BuildCategoryString());
        }
    }
}
