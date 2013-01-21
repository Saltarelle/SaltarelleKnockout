// BindingContext.cs
// Script#/Libraries/Knockout
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KnockoutApi {
	#if !PLUGIN
	[NonScriptable]
	#endif
	[AttributeUsage(AttributeTargets.Class)]
	public class KnockoutModelAttribute : Attribute {
	}

	#if !PLUGIN
	[NonScriptable]
	#endif
	[AttributeUsage(AttributeTargets.Property)]
	public class KnockoutPropertyAttribute : Attribute {
		public bool IsKnockoutProperty { get; private set; }

		public KnockoutPropertyAttribute() {
			IsKnockoutProperty = true;
		}

		public KnockoutPropertyAttribute(bool isKnockoutProperty) {
			IsKnockoutProperty = isKnockoutProperty;
		}
	}
}
