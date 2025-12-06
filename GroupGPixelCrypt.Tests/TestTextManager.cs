
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GroupGPixelCrypt.Model;
using System.Text;

namespace GroupGPixelCrypt.Tests
{
    [TestClass]
    public class TestTextManager
    {
        private Encoding encoding = Encoding.ASCII;
        private List<byte> expectedBytes3Bpcc = new List<byte>{0b010, 0b010, 0b000, 0b110, 0b000, 0b101, 0b101, 0b100, 0b011, 0b011, 0b000, 0b110, 0b111, 0b100};
        [TestMethod]
        public void TestBreakDownText()
        {
            TextManager textManager = new TextManager();
            string testMessage = "Hallo";
            List<byte> result = (List<byte>)textManager.ConvertMessageToBytes(testMessage, 3);
            CollectionAssert.AreEqual(this.expectedBytes3Bpcc, result);
        }
    }
}
