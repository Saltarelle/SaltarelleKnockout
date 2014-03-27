using System;
using System.Runtime.CompilerServices;
#if PLUGIN
using Saltarelle.Compiler;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
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
		public override void ApplyTo(IEntity entity, IAttributeStore attributeStore, IErrorReporter errorReporter) {
			foreach (var p in ((ITypeDefinition)entity).GetProperties(options: GetMemberOptions.IgnoreInheritedMembers)) {
				if (attributeStore.AttributesFor(p).HasAttribute<KnockoutPropertyAttribute>())
					continue;
				if (p.IsStatic)
					continue;

				if (MetadataUtils.IsAutoProperty(p) == false) {
					errorReporter.Message(MessageSeverity.Error, 8001, "The property {0} is not an auto-property so because its containing type has a [KnockoutModelAttribute], the property must be decorated with [KnockoutPropertyAttribute(false)]", p.FullName);
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
		public override void ApplyTo(IEntity entity, IAttributeStore attributeStore, IErrorReporter errorReporter) {
			if (IsKnockoutProperty) {
				var p = (IProperty)entity;

				if (p.IsStatic) {
					errorReporter.Message(MessageSeverity.Error, 8000, "The property {0} cannot have a [KnockoutPropertyAttribute] because it is static", p.FullName);
				}
				else if (MetadataUtils.IsAutoProperty(p) == false) {
					errorReporter.Message(MessageSeverity.Error, 8001, "The property {0} cannot be a knockout property because it is not an auto-property", p.FullName);
				}
				else {
					MakeKnockoutProperty((IProperty)entity, attributeStore);
				}
			}
		}

		public static void MakeKnockoutProperty(IProperty property, IAttributeStore attributeStore) {
			var getter = attributeStore.AttributesFor(property.Getter);
			getter.ReplaceAttribute(new ScriptNameAttribute("{owner}"));
			getter.ReplaceAttribute(new DontGenerateAttribute());
			var setter = attributeStore.AttributesFor(property.Setter);
			setter.ReplaceAttribute(new ScriptNameAttribute("{owner}"));
			setter.ReplaceAttribute(new DontGenerateAttribute());
			var prop = attributeStore.AttributesFor(property);
			prop.ReplaceAttribute(new CustomInitializationAttribute("{$KnockoutApi.Knockout}.observable({value})"));
			prop.ReplaceAttribute(new BackingFieldNameAttribute("{owner}"));
		}
#endif
	}
}
