using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Knockout.Plugin {
	public static class Utils {
		private static bool IsEmptyAccessorBody(IMethodSymbol method) {
			var syntax = (AccessorDeclarationSyntax)method.DeclaringSyntaxReferences[0].GetSyntax();
			return syntax.Body == null;
		}

		public static bool? IsAutoProperty(IPropertySymbol property) {
			return IsEmptyAccessorBody(property.GetMethod ?? property.SetMethod);
		}
	}
}
