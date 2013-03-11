using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using MetadataImporter = Knockout.Plugin.MetadataImporter;

namespace Knockout.Tests {
	[TestFixture]
	public class JSTypeSystemRewriterTests {
		private MockErrorReporter _errorReporter;
		private IMetadataImporter _metadata;
		private ReadOnlyCollection<Message> _allErrors;
		private ICompilation _compilation;

		private void AssertEqual(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:" + Environment.NewLine + expected + Environment.NewLine + Environment.NewLine + "Actual:" + actual);
		}

		private JsClass Compile(string source, IMetadataImporter prev = null, IRuntimeLibrary runtimeLibrary = null, bool expectErrors = false) {
			var pc = PreparedCompilation.CreateCompilation(new[] { new MockSourceFile("File1.cs", source) }, new[] { Files.Mscorlib, Files.Web, Files.Knockout }, new List<string>());
			_compilation = pc.Compilation;

			_errorReporter = new MockErrorReporter(!expectErrors);
			prev = prev ?? new CoreLib.Plugin.MetadataImporter(_errorReporter, _compilation, new CompilerOptions());
			var namer = new Namer();
			runtimeLibrary = runtimeLibrary ?? new CoreLib.Plugin.RuntimeLibrary(prev, _errorReporter, _compilation, namer);

			_metadata = new MetadataImporter(prev, _errorReporter, runtimeLibrary, new Mock<INamer>().Object, new CompilerOptions());

			_metadata.Prepare(_compilation.GetAllTypeDefinitions());

			_allErrors = _errorReporter.AllMessages.ToList().AsReadOnly();
			if (expectErrors) {
				Assert.That(_allErrors, Is.Not.Empty, "Compile should have generated errors");
			}
			else {
				Assert.That(_allErrors, Is.Empty, "Compile should not generate errors");
			}

			var c = new Compiler(_metadata, namer, runtimeLibrary, _errorReporter);

			var types = ((IJSTypeSystemRewriter)_metadata).Rewrite(c.Compile(pc)).OfType<JsClass>().ToList();
			return types.Single(t => t.CSharpTypeDefinition.Name == "C");
		}

		[Test]
		public void KnockoutPropertyBackingFieldsAreInitializedWhenConstructorsAreNotChained() {
			var c = Compile(
@"using KnockoutApi;
public class C {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public string P2 { get; set; }
	public int P3 { get; set; }

	public C() {
		int i = 0;
	}

	public C(int x) {
		int j = 0;
	}

	public C(string y) {
		int k = 0;
	}
}");

			AssertEqual(OutputFormatter.Format(c.UnnamedConstructor.Body),
@"{
	this.p1 = ko.observable(0);
	this.p2 = ko.observable(null);
	this.$1$P3Field = 0;
	var i = 0;
}
");

			AssertEqual(OutputFormatter.Format(c.NamedConstructors[0].Definition.Body),
@"{
	this.p1 = ko.observable(0);
	this.p2 = ko.observable(null);
	this.$1$P3Field = 0;
	var j = 0;
}
");

			AssertEqual(OutputFormatter.Format(c.NamedConstructors[1].Definition.Body),
@"{
	this.p1 = ko.observable(0);
	this.p2 = ko.observable(null);
	this.$1$P3Field = 0;
	var k = 0;
}
");
		}

		[Test]
		public void KnockoutPropertyBackingFieldsAreInitializedWhenInvokingBaseConstructor() {
			var c = Compile(
@"using KnockoutApi;
public class B {
	public B(int i) {}
}

public class C : B {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public string P2 { get; set; }
	public int P3 { get; set; }

	public C() : base(0) {
		int i = 0;
	}
}");

			AssertEqual(OutputFormatter.Format(c.UnnamedConstructor.Body, allowIntermediates: true),
@"{
	this.p1 = ko.observable(0);
	this.p2 = ko.observable(null);
	this.$2$P3Field = 0;
	{B}.call(this, 0);
	var i = 0;
}
");
		}

		[Test]
		public void KnockoutPropertyBackingFieldsAreNotInitializedWhenChainingConstructors() {
			var c = Compile(
@"using KnockoutApi;
public class C {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public string P2 { get; set; }
	public int P3 { get; set; }

	public C() : this("""") {
		int i = 0;
	}

	public C(int i) : this() {
		int j = 0;
	}

	public C(string s) {
	}
}");

			AssertEqual(OutputFormatter.Format(c.UnnamedConstructor.Body, allowIntermediates: true),
@"{
	{C}.$ctor2.call(this, '');
	var i = 0;
}
");

			AssertEqual(OutputFormatter.Format(c.NamedConstructors[0].Definition.Body, allowIntermediates: true),
@"{
	{C}.call(this);
	var j = 0;
}
");
		}
	}
}
