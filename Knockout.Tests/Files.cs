using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Knockout.Tests {
	internal class Files {
		public static readonly string MscorlibPath = Path.GetFullPath("mscorlib.dll");
		public static readonly string WebPath = Path.GetFullPath("Saltarelle.Web.dll");
		public static readonly string KnockoutPath = Path.GetFullPath("Saltarelle.Knockout.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		private static readonly Lazy<MetadataReference> _webLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(WebPath));
		internal static MetadataReference Web { get { return _webLazy.Value; } }

		private static readonly Lazy<MetadataReference> _knockoutLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(KnockoutPath));
		internal static MetadataReference Knockout { get { return _knockoutLazy.Value; } }
	}
}
