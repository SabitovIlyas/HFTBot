using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsEngine.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Views.Tests
{
    [TestClass()]
    public class AcceptedTextTests
    {
        [DataTestMethod()]
        [DataRow("-1.2345", false)]
        [DataRow("-1.a345", false)]
        [DataRow("-1.2.45", false)]
        [DataRow("-1.2-45", false)]
        [DataRow("1.2-45", false)]
        [DataRow(".12345", false)]
        [DataRow("-.12345", false)]
        [DataRow("0.12345", true)]
        [DataRow("12345", true)]
        [DataRow("a0.12345", false)]
        [DataRow("12345a", false)]
        public void CheckForUDecimalTextBoxTest(string text, bool expected)
        {
            var acceptedText = AcceptedText.Create();
            acceptedText.Text = text;
            var actual = acceptedText.CheckForUnsignedDecimalTextBox();
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod()]
        [DataRow("-1.2345", true)]
        [DataRow("-1.a345", false)]
        [DataRow("-1.2.45", false)]
        [DataRow("-1.2-45", false)]
        [DataRow("1.2-45", false)]
        [DataRow(".12345", false)]
        [DataRow("-.12345", false)]
        [DataRow("0.12345", true)]
        [DataRow("12345", true)]
        [DataRow("a0.12345", false)]
        [DataRow("12345a", false)]
        public void CheckForDecimalTextBoxTest(string text, bool expected)
        {
            var acceptedText = AcceptedText.Create();
            acceptedText.Text = text;
            var actual = acceptedText.CheckForDecimalTextBox();
            Assert.AreEqual(expected, actual);
        }
    }
}