using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using KnockoutApi;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.Decorators;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Knockout.Plugin {
	public class MetadataImporter : MetadataImporterDecoratorBase, IJSTypeSystemRewriter {
		private readonly IErrorReporter _errorReporter;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly INamer _namer;
		private readonly bool _minimizeNames;
		private readonly Dictionary<IProperty, string> _knockoutProperties = new Dictionary<IProperty, string>();

		public MetadataImporter(IMetadataImporter prev, IErrorReporter errorReporter, IRuntimeLibrary runtimeLibrary, INamer namer, CompilerOptions options) : base(prev) {
			_errorReporter  = errorReporter;
			_runtimeLibrary = runtimeLibrary;
			_namer          = namer;
			_minimizeNames  = options.MinimizeScript;
		}

		private bool IsKnockoutProperty(IProperty property) {
			var propAttr = AttributeReader.ReadAttribute<KnockoutPropertyAttribute>(property);
			if (propAttr != null)
				return propAttr.IsKnockoutProperty;
			return AttributeReader.HasAttribute<KnockoutModelAttribute>(property.DeclaringTypeDefinition);
		}

		private void PrepareKnockoutProperty(IProperty p) {
			if (p.IsStatic) {
				if (AttributeReader.HasAttribute<KnockoutPropertyAttribute>(p)) {
					_errorReporter.Region = p.Region;
					_errorReporter.Message(MessageSeverity.Error, 8000, "The property {0} cannot have a [KnockoutPropertyAttribute] because it is static", p.FullName);
				}
			}
			else if (IsAutoProperty(p) == false) {
				_errorReporter.Region = p.Region;
				_errorReporter.Message(MessageSeverity.Error, 8001, "The property {0} cannot be a knockout property because it is not an auto-property", p.FullName);
			}
			else {
				var preferredName = MetadataUtils.DeterminePreferredMemberName(p, _minimizeNames);
				string name = preferredName.Item2 ? preferredName.Item1 : MetadataUtils.GetUniqueName(preferredName.Item1, n => IsMemberNameAvailable(p.DeclaringTypeDefinition, n, false));
				base.ReserveMemberName(p.DeclaringTypeDefinition, name, false);
				base.SetPropertySemantics(p, PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("{this}." + name + "()"), MethodScriptSemantics.InlineCode("{this}." + name + "({value})")));
				_knockoutProperties[p] = name;
			}
		}

		private bool? IsAutoProperty(IProperty property) {
			if (property.Region == default(DomRegion))
				return null;
			return property.Getter != null && property.Setter != null && property.Getter.BodyRegion == default(DomRegion) && property.Setter.BodyRegion == default(DomRegion);
		}

		public override void Prepare(ITypeDefinition type) {
			foreach (var p in type.Properties.Where(IsKnockoutProperty)) {
				PrepareKnockoutProperty(p);
			}

			base.Prepare(type);
		}

		// Implementation and helpers for IJSTypeSystemRewriter

		private JsFunctionDefinitionExpression InsertInitializers(ITypeDefinition type, JsFunctionDefinitionExpression orig, IEnumerable<JsStatement> initializers) {
			if (orig.Body.Statements.Count > 0 && orig.Body.Statements[0] is JsExpressionStatement) {
				// Find out if we are doing constructor chaining. In this case the first statement in the constructor will be {Type}.call(this, ...) or {Type}.namedCtor.call(this, ...)
				var expr = ((JsExpressionStatement)orig.Body.Statements[0]).Expression;
				if (expr is JsInvocationExpression && ((JsInvocationExpression)expr).Method is JsMemberAccessExpression && ((JsInvocationExpression)expr).Arguments.Count > 0 && ((JsInvocationExpression)expr).Arguments[0] is JsThisExpression) {
					expr = ((JsInvocationExpression)expr).Method;
					if (expr is JsMemberAccessExpression && ((JsMemberAccessExpression)expr).MemberName == "call") {
						expr = ((JsMemberAccessExpression)expr).Target;
						if (expr is JsMemberAccessExpression)
							expr = ((JsMemberAccessExpression)expr).Target;	// Named constructor
						if (expr is JsTypeReferenceExpression && ((JsTypeReferenceExpression)expr).Type.Equals(type))
							return orig;	// Yes, we are chaining. Don't initialize the knockout properties.
					}
				}
			}

			return JsExpression.FunctionDefinition(orig.ParameterNames, new JsBlockStatement(initializers.Concat(orig.Body.Statements)), orig.Name);
		}

		private JsType InitializeKnockoutProperties(JsType type) {
			var c = type as JsClass;
			if (c == null)
				return type;

			var knockoutProperties = type.CSharpTypeDefinition.Properties.Where(p => _knockoutProperties.ContainsKey(p)).ToList();
			if (knockoutProperties.Count == 0)
				return type;

			var initializers = knockoutProperties.Select(p => new JsExpressionStatement(
			                                                      JsExpression.Assign(
			                                                          JsExpression.Member(
			                                                              JsExpression.This,
			                                                              _knockoutProperties[p]),
			                                                          JsExpression.Invocation(
			                                                              JsExpression.Member(
			                                                                  JsExpression.Identifier("ko"),
			                                                                  "observable"),
			                                                              _runtimeLibrary.Default(p.ReturnType, tp => JsExpression.Identifier(_namer.GetTypeParameterName(tp)))))))
			                                     .ToList();

			var result = c.Clone();
			if (result.UnnamedConstructor != null) {
				result.UnnamedConstructor = InsertInitializers(c.CSharpTypeDefinition, result.UnnamedConstructor, initializers);
			}
			var namedConstructors = result.NamedConstructors.Select(x => new JsNamedConstructor(x.Name, InsertInitializers(c.CSharpTypeDefinition, x.Definition, initializers))).ToList();
			result.NamedConstructors.Clear();
			foreach (var x in namedConstructors)
				result.NamedConstructors.Add(x);

			return result;
		}

		public IEnumerable<JsType> Rewrite(IEnumerable<JsType> types) {
			return types.Select(InitializeKnockoutProperties);
		}
	}
}
