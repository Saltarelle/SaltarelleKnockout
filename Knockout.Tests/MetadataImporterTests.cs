using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using Knockout.Plugin;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ScriptSemantics;

namespace Knockout.Tests {
	[TestFixture]
	public class MetadataImporterTests {
		private MockErrorReporter _errorReporter;
		private IMetadataImporter _metadata;
		private ReadOnlyCollection<Message> _allErrors;
		private ICompilation _compilation;

		private void Prepare(string source, IRuntimeLibrary runtimeLibrary = null, bool expectErrors = false, bool MinimizeNames = false) {
			IProjectContent project = new CSharpProjectContent();
			var parser = new CSharpParser();

			using (var rdr = new StringReader(source)) {
				var pf = new CSharpUnresolvedFile("File.cs");
				var syntaxTree = parser.Parse(rdr, pf.FileName);
				syntaxTree.AcceptVisitor(new TypeSystemConvertVisitor(pf));
				project = project.AddOrUpdateFiles(pf);
			}
			project = project.AddAssemblyReferences(new[] { Files.Mscorlib, Files.Web, Files.Knockout });

			_compilation = project.CreateCompilation();

			var options = new CompilerOptions { MinimizeScript = MinimizeNames };
			_errorReporter = new MockErrorReporter(!expectErrors);
			var prev = new CoreLib.Plugin.MetadataImporter(_errorReporter, _compilation, options);
			_metadata = new MetadataImporter(prev, _errorReporter, runtimeLibrary ?? new Mock<IRuntimeLibrary>().Object, new Mock<INamer>().Object, options);

			_metadata.Prepare(_compilation.GetAllTypeDefinitions());

			_allErrors = _errorReporter.AllMessages.ToList().AsReadOnly();
			if (expectErrors) {
				Assert.That(_allErrors, Is.Not.Empty, "Compile should have generated errors");
			}
			else {
				Assert.That(_allErrors, Is.Empty, "Compile should not generate errors");
			}
		}

		private PropertyScriptSemantics FindProperty(string name) {
			return _metadata.GetPropertySemantics(_compilation.FindType(new FullTypeName("C")).GetProperties().Single(p => p.Name == name));
		}

		private MethodScriptSemantics FindMethod(string name) {
			return _metadata.GetMethodSemantics(_compilation.FindType(new FullTypeName("C")).GetMethods().Single(p => p.Name == name));
		}

		[Test]
		public void KnockoutPropertyAttributeWorks() {
			Prepare(
@"using KnockoutApi;
public class C {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public int P2 { get; set; }
	public int P3 { get; set; }
	[KnockoutProperty(false)] public int P4 { get; set; }
}");

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("p1"));
			Assert.That(p1.GetMethod.GeneratedMethodName, Is.Null);
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("p1"));
			Assert.That(p1.SetMethod.GeneratedMethodName, Is.Null);

			var p2 = FindProperty("P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("p2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("p2"));

			var p3 = FindProperty("P3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_p3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_p3"));

			var p4 = FindProperty("P4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p4.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.GetMethod.Name, Is.EqualTo("get_p4"));
			Assert.That(p4.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.SetMethod.Name, Is.EqualTo("set_p4"));
		}

		[Test]
		public void KnockoutModelAttributeWorksButCanBeOverridden() {
			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public int P2 { get; set; }
	public int P3 { get; set; }
	[KnockoutProperty(false)] public int P4 { get; set; }
}");

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("p1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("p1"));

			var p2 = FindProperty("P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("p2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("p2"));

			var p3 = FindProperty("P3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("p3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("p3"));

			var p4 = FindProperty("P4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p4.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.GetMethod.Name, Is.EqualTo("get_p4"));
			Assert.That(p4.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.SetMethod.Name, Is.EqualTo("set_p4"));
		}

		[Test]
		public void StaticPropertyOnKnockoutModelIsNotKnockoutProperty() {
			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C {
	public static int P1 { get; set; }
	[KnockoutProperty(false)] public static int P2 { get; set; }
}");

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_p1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_p1"));

			var p2 = FindProperty("P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_p2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_p2"));
		}

		[Test]
		public void KnockoutPropertyAttributeOnStaticPropertyIsAnError() {
			Prepare(
@"using KnockoutApi;
public class C1 {
	[KnockoutProperty]
	public static int P1 { get; set; }
}", expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("static") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("KnockoutPropertyAttribute")));
		}

		[Test]
		public void ExplicitlyImplementedKnockoutPropertyIsAnError() {
			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C1 {
	public int P1 { get { return 0; } set {} }
}", expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("not an auto-property") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("knockout property")));

			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C1 {
	public int P1 { get { return 0; } }
}", expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("not an auto-property") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("knockout property")));

			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C1 {
	public int P1 { set {} }
}", expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("not an auto-property") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("knockout property")));
		}

		[Test]
		public void KnockoutPropertyRespectsScriptName() {
			Prepare(
@"using KnockoutApi;
using System.Runtime.CompilerServices;
public class C {
	[KnockoutProperty, ScriptName(""renamed"")] public int P1 { get; set; }
}");

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("renamed"));
		}

		[Test]
		public void KnockoutPropertyReservesName() {
			Prepare(
@"using KnockoutApi;
using System.Runtime.CompilerServices;
public class C {
	[KnockoutProperty, ScriptName(""renamed"")] public int P1 { get; set; }
	public void Renamed() {}
}");

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("renamed"));

			var m = FindMethod("Renamed");
			Assert.That(m.Name, Is.EqualTo("renamed$1"));
		}

		[Test]
		public void KnockoutPropertyWorksWhenDesiredNameIsUsed() {
			Prepare(
@"using KnockoutApi;
using System.Runtime.CompilerServices;
public class B {
	public void TheName() {}
}
public class C : B{
	[KnockoutProperty] public int TheName { get; set; }
}");

			var p1 = FindProperty("TheName");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("theName$1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("theName$1"));
		}

		[Test]
		public void KnockoutPropertyWorksWithMinifiedNames() {
			Prepare(
@"using KnockoutApi;
using System.Runtime.CompilerServices;
public class C {
	[KnockoutProperty] int P1 { get; set; }
}", MinimizeNames: true);

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("$0"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("$0"));
		}
	}
}
