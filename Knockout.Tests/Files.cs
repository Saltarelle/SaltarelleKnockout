using System;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;

namespace Knockout.Tests {
	internal class Files {
		public static readonly string MscorlibPath = Path.GetFullPath("mscorlib.dll");
		public static readonly string WebPath = Path.GetFullPath("Saltarelle.Web.dll");
		public static readonly string KnockoutPath = Path.GetFullPath("Saltarelle.Knockout.dll");

		private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new IkvmLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(MscorlibPath));
		internal static IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

		private static readonly Lazy<IAssemblyReference> _webLazy = new Lazy<IAssemblyReference>(() => new IkvmLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(WebPath));
		internal static IAssemblyReference Web { get { return _webLazy.Value; } }

		private static readonly Lazy<IAssemblyReference> _knockoutLazy = new Lazy<IAssemblyReference>(() => new IkvmLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(KnockoutPath));
		internal static IAssemblyReference Knockout { get { return _knockoutLazy.Value; } }
	}
}
