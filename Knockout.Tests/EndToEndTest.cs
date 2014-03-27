using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Knockout.Tests {
	[TestFixture]
	public class EndToEndTest {
		private string ReadResource(string resourceName) {
			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Knockout.Tests." + resourceName)) {
				var result = new StreamReader(s).ReadToEnd();
				return result.Replace("\r\n", "\n");
			}
		}

		[Test]
		public void OutputShouldMatchTheExpectedOutput() {
			string expected = ReadResource("ExpectedTestScript.js"), actual = ReadResource("Knockout.TestScript.js");
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}
