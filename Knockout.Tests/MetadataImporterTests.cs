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
using Saltarelle.Compiler.ScriptSemantics;

namespace Knockout.Tests {
	[TestFixture]
	public class MetadataImporterTests {
		private MockErrorReporter _errorReporter;
		private IMetadataImporter _metadata;
		private ReadOnlyCollection<Message> _allErrors;
		private ICompilation _compilation;

		private void Prepare(string source, IMetadataImporter prev, IRuntimeLibrary runtimeLibrary = null, bool expectErrors = false) {
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

			_errorReporter = new MockErrorReporter(!expectErrors);
			_metadata = new MetadataImporter(prev, _errorReporter, runtimeLibrary ?? new Mock<IRuntimeLibrary>().Object);

			_metadata.Prepare(_compilation.GetAllTypeDefinitions(), false, _compilation.MainAssembly);

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

		[Test]
		public void KnockoutPropertyAttributeWorks() {
			var md = new Mock<IMetadataImporter>();
			md.Setup(_ => _.GetPropertySemantics(It.IsAny<IProperty>())).Returns<IProperty>(p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$" + p.Name), MethodScriptSemantics.NormalMethod("set_$" + p.Name)));
			Prepare(
@"using KnockoutApi;
public class C {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public int P2 { get; set; }
	public int P3 { get; set; }
	[KnockoutProperty(false)] public int P4 { get; set; }
}", md.Object);

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("{this}.$P1()"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("{this}.$P1({value})"));

			var p2 = FindProperty("P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p2.GetMethod.LiteralCode, Is.EqualTo("{this}.$P2()"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p2.SetMethod.LiteralCode, Is.EqualTo("{this}.$P2({value})"));

			var p3 = FindProperty("P3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_$P3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_$P3"));

			var p4 = FindProperty("P4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p4.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.GetMethod.Name, Is.EqualTo("get_$P4"));
			Assert.That(p4.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.SetMethod.Name, Is.EqualTo("set_$P4"));
		}

		[Test]
		public void KnockoutModelAttributeWorksButCanBeOverridden() {
			var md = new Mock<IMetadataImporter>();
			md.Setup(_ => _.GetPropertySemantics(It.IsAny<IProperty>())).Returns<IProperty>(p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$" + p.Name), MethodScriptSemantics.NormalMethod("set_$" + p.Name)));
			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C {
	[KnockoutProperty] public int P1 { get; set; }
	[KnockoutProperty(true)] public int P2 { get; set; }
	public int P3 { get; set; }
	[KnockoutProperty(false)] public int P4 { get; set; }
}", md.Object);

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("{this}.$P1()"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("{this}.$P1({value})"));

			var p2 = FindProperty("P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p2.GetMethod.LiteralCode, Is.EqualTo("{this}.$P2()"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p2.SetMethod.LiteralCode, Is.EqualTo("{this}.$P2({value})"));

			var p3 = FindProperty("P3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p3.GetMethod.LiteralCode, Is.EqualTo("{this}.$P3()"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p3.SetMethod.LiteralCode, Is.EqualTo("{this}.$P3({value})"));

			var p4 = FindProperty("P4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p4.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.GetMethod.Name, Is.EqualTo("get_$P4"));
			Assert.That(p4.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p4.SetMethod.Name, Is.EqualTo("set_$P4"));
		}

		[Test]
		public void StaticPropertyOnKnockoutModelIsNotKnockoutProperty() {
			var md = new Mock<IMetadataImporter>();
			md.Setup(_ => _.GetPropertySemantics(It.IsAny<IProperty>())).Returns<IProperty>(p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$" + p.Name), MethodScriptSemantics.NormalMethod("set_$" + p.Name)));
			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C {
	public static int P1 { get; set; }
	[KnockoutProperty(false)] public static int P2 { get; set; }
}", md.Object);

			var p1 = FindProperty("P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$P1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$P1"));

			var p2 = FindProperty("P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_$P2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_$P2"));
		}

		[Test]
		public void KnockoutPropertyAttributeOnStaticPropertyIsAnError() {
			var md = new Mock<IMetadataImporter>();
			Prepare(
@"using KnockoutApi;
public class C1 {
	[KnockoutProperty]
	public static int P1 { get; set; }
}", md.Object, expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("static") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("KnockoutPropertyAttribute")));
		}

		[Test]
		public void ExplicitlyImplementedKnockoutPropertyIsAnError() {
			var md = new Mock<IMetadataImporter>();
			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C1 {
	public int P1 { get { return 0; } set {} }
}", md.Object, expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("not an auto-property") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("knockout property")));

			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C1 {
	public int P1 { get { return 0; } }
}", md.Object, expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("not an auto-property") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("knockout property")));

			Prepare(
@"using KnockoutApi;
[KnockoutModel]
public class C1 {
	public int P1 { set {} }
}", md.Object, expectErrors: true);
			Assert.That(_errorReporter.AllMessages.Any(m => m.FormattedMessage.Contains("not an auto-property") && m.FormattedMessage.Contains("C1.P1") && m.FormattedMessage.Contains("knockout property")));
		}
	}
}
