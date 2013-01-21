using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.TypeSystem;
using KnockoutApi;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Decorators;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Knockout.Plugin {
    public class MetadataImporter : MetadataImporterDecoratorBase, IJSTypeSystemRewriter {
	    private readonly IErrorReporter _errorReporter;
		private readonly IRuntimeLibrary _runtimeLibrary;

	    public MetadataImporter(IMetadataImporter prev, IErrorReporter errorReporter, IRuntimeLibrary runtimeLibrary) : base(prev) {
		    _errorReporter  = errorReporter;
			_runtimeLibrary = runtimeLibrary;
	    }

		private bool IsKnockoutProperty(IProperty property) {
			var propAttr = AttributeReader.ReadAttribute<KnockoutPropertyAttribute>(property);
			if (propAttr != null)
				return propAttr.IsKnockoutProperty;
			return AttributeReader.HasAttribute<KnockoutModelAttribute>(property.DeclaringTypeDefinition);
		}

		private string GetPropertyName(PropertyScriptSemantics result) {
			string fromGetter = result.GetMethod != null && result.GetMethod.Type == MethodScriptSemantics.ImplType.NormalMethod && result.GetMethod.Name.StartsWith("get_") ? result.GetMethod.Name.Substring(4) : null;
			string fromSetter = result.SetMethod != null && result.SetMethod.Type == MethodScriptSemantics.ImplType.NormalMethod && result.SetMethod.Name.StartsWith("set_") ? result.SetMethod.Name.Substring(4) : null;
			if (fromGetter != null && fromSetter != null && fromGetter != fromSetter)
				return null;
			return fromGetter ?? fromSetter;
		}

		private Dictionary<IProperty, PropertyScriptSemantics> _propertyCache;
		private Dictionary<IProperty, string> _knockoutProperties;

		public override void Prepare(IEnumerable<ITypeDefinition> allTypes, bool minimizeNames, IAssembly mainAssembly) {
			base.Prepare(allTypes, minimizeNames, mainAssembly);
			_propertyCache = new Dictionary<IProperty, PropertyScriptSemantics>();
			_knockoutProperties = new Dictionary<IProperty, string>();
		}

	    public override PropertyScriptSemantics GetPropertySemantics(IProperty property) {
			PropertyScriptSemantics result;
			if (_propertyCache.TryGetValue(property, out result))
				return result;

			result = base.GetPropertySemantics(property);
			if (IsKnockoutProperty(property)) {
				if (property.IsStatic) {
					// TODO: Error
				}
				else if (!IsAutoProperty(property)) {
					// TODO: Error
				}
				else if (result.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods) {
					string name = GetPropertyName(result);
					if (name != null) {
						result = PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("{this}." + name + "()"), MethodScriptSemantics.InlineCode("{this}." + name + "({value})"));
						_knockoutProperties[property] = name;
					}
					else {
						// TODO: Error
					}
				}
				else {
					// TODO: Error
				}
			}
			_propertyCache[property] = result;
			return result;
		}

		private static bool IsAutoProperty(IProperty property) {
			// TODO
			return true;
		}

		// Implementation and helpers for IJSTypeSystemRewriter

		private JsFunctionDefinitionExpression InsertInitializers(JsFunctionDefinitionExpression orig, IEnumerable<JsStatement> initializers) {
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
			                                                              _runtimeLibrary.Default(p.ReturnType)))))
			                                     .ToList();

			var result = c.Clone();
			if (result.UnnamedConstructor != null) {
				result.UnnamedConstructor = InsertInitializers(result.UnnamedConstructor, initializers);
			}
			var namedConstructors = result.NamedConstructors.Select(x => new JsNamedConstructor(x.Name, InsertInitializers(x.Definition, initializers))).ToList();
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
