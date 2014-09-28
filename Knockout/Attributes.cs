using System;
using System.Runtime.CompilerServices;
#if PLUGIN
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;
using CoreLib.Plugin;
using Saltarelle.Compiler.Roslyn;

#endif

namespace KnockoutApi {
#if PLUGIN
	public class KnockoutModelAttribute : PluginAttributeBase {
#else
	[NonScriptable]
	[AttributeUsage(AttributeTargets.Class)]
	public class KnockoutModelAttribute : Attribute {
#endif

#if PLUGIN
		public override void ApplyTo(ISymbol entity, IAttributeStore attributeStore, IErrorReporter errorReporter) {
			foreach (var p in ((INamedTypeSymbol)entity).GetProperties()) {
				if (attributeStore.AttributesFor(p).HasAttribute<KnockoutPropertyAttribute>())
					continue;
				if (p.IsStatic)
					continue;

				if (Knockout.Plugin.Utils.IsAutoProperty(p) == false) {
					errorReporter.Message(DiagnosticSeverity.Error, "CS8001", "The property {0} is not an auto-property so because its containing type has a [KnockoutModelAttribute], the property must be decorated with [KnockoutPropertyAttribute(false)]", p.FullyQualifiedName());
					continue;
				}

				KnockoutPropertyAttribute.MakeKnockoutProperty(p, attributeStore);
			}
		}
#endif
	}

#if PLUGIN
	public class KnockoutPropertyAttribute : PluginAttributeBase {
#else
	[NonScriptable]
	[AttributeUsage(AttributeTargets.Property)]
	public class KnockoutPropertyAttribute : Attribute {
#endif
		public bool IsKnockoutProperty { get; private set; }

		public KnockoutPropertyAttribute() {
			IsKnockoutProperty = true;
		}

		public KnockoutPropertyAttribute(bool isKnockoutProperty) {
			IsKnockoutProperty = isKnockoutProperty;
		}

#if PLUGIN
		public override void ApplyTo(ISymbol entity, IAttributeStore attributeStore, IErrorReporter errorReporter) {
			if (IsKnockoutProperty) {
				var p = (IPropertySymbol)entity;

				if (p.IsStatic) {
					errorReporter.Message(DiagnosticSeverity.Error, "CS8000", "The property {0} cannot have a [KnockoutPropertyAttribute] because it is static", p.FullyQualifiedName());
				}
				else if (Knockout.Plugin.Utils.IsAutoProperty(p) == false) {
					errorReporter.Message(DiagnosticSeverity.Error, "CS8001", "The property {0} cannot be a knockout property because it is not an auto-property", p.FullyQualifiedName());
				}
				else {
					MakeKnockoutProperty((IPropertySymbol)entity, attributeStore);
				}
			}
		}

		public static void MakeKnockoutProperty(IPropertySymbol property, IAttributeStore attributeStore) {
			var getter = attributeStore.AttributesFor(property.GetMethod);
			getter.ReplaceAttribute(new ScriptNameAttribute("{owner}"));
			getter.ReplaceAttribute(new DontGenerateAttribute());
			var setter = attributeStore.AttributesFor(property.SetMethod);
			setter.ReplaceAttribute(new ScriptNameAttribute("{owner}"));
			setter.ReplaceAttribute(new DontGenerateAttribute());
			var prop = attributeStore.AttributesFor(property);
			prop.ReplaceAttribute(new CustomInitializationAttribute("{$KnockoutApi.Knockout}.observable({value})"));
			prop.ReplaceAttribute(new BackingFieldNameAttribute("{owner}"));
		}
#endif
	}
}
