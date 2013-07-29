// ComputedObservable.cs
// Script#/Libraries/Knockout
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace KnockoutApi {

    /// <summary>
    /// Represents an object containing a value that is omputed from other observable values.
    /// </summary>
    /// <typeparam name="T">The type of the contained value.</typeparam>
    [Imported]
    [IgnoreNamespace]
    public sealed class ComputedObservable<T> : Subscribable<T> {

        /// <summary>
        /// Creates an observable with a value computed from one or more other values.
        /// </summary>        
        /// <param name="function">A function to compute the value.</param> 
        [InlineCode("ko.computed({function})")]       
        public ComputedObservable(Func<T> function) {            
        }

        /// <summary>
        /// Creates an observable with a value computed from one or more other values.
        /// </summary>        
        /// <param name="options">Options for the dependent observable.</param>
        [InlineCode("ko.computed({options})")]
        public ComputedObservable(ComputedOptions<T> options) {            
        }

        /// <summary>
        /// Gets the current computed value.
        /// </summary>
        /// <returns>The current value.</returns>
        [ScriptName("")]
        [Obsolete("Use the Value property instead.")]
        public T GetValue() {
            return default(T);
        }

        /// <summary>
        /// Gets or sets the current computed value.
        /// </summary>
        public T Value {
            [ScriptName("")]
            get { return default(T); }

            [ScriptName("")]
            set { }
        }

        /// <summary>
        /// Get Dependencies Count
        /// </summary>
        /// <returns>Returns the Number of Dependencies</returns>
        public int GetDependenciesCount() { 
            return 0; 
        }
    }
}
