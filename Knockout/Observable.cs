// Observable.cs
// Script#/Libraries/Knockout
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Runtime.CompilerServices;

namespace KnockoutApi {

    /// <summary>
    /// Represents an object containing an observable value.
    /// </summary>
    /// <typeparam name="T">The type of the contained value.</typeparam>
    [Imported]
    [IgnoreNamespace]
    public class Observable<T> : Subscribable<T> {
        
        /// <summary>
        /// Creates an observable value.
        /// </summary>                
        [InlineCode("ko.observable()")]
        public Observable() {
        }

        /// <summary>
        /// Creates an observable value.
        /// </summary>        
        /// <param name="initialValue">The initial value.</param>        
        [InlineCode("ko.observable({initialValue})")]
        public Observable(T initialValue) {
        }
        
        /// <summary>
        /// Gets the current value within the observable object.
        /// </summary>
        /// <returns>The current value.</returns>
        [ScriptName("")]
        [Obsolete("Use the Value property instead.")]
        public T GetValue() {
            return default(T);
        }

        /// <summary>
        /// Sets the value within the observable object.
        /// </summary>
        /// <param name="value">The new value.</param>
        [ScriptName("")]
        [Obsolete("Use the Value property instead.")]
        public void SetValue(T value) {
        }

        /// <summary>
        /// Gets or sets the value within the observable object.
        /// </summary>
        public T Value {
            [ScriptName("")]
            get { return default(T); }

            [ScriptName("")]
            set { }
        }

        /// <summary>
        /// Returns the current value of the observable without creating a dependency
        /// </summary>
        public T Peek() {
            return default(T);
        }

        /// <summary>
        /// Notifies All Subscribers that the Value has Changed
        /// Called internally with SetValue
        /// </summary>
        public void ValueHasMutated() {
        }

        /// <summary>
        /// Notifies All Subscribers BEFORE the Value has Changed
        /// Called internally with SetValue
        /// </summary>
        public void ValueWillMutated() {
        }

        /// <summary>
        /// For Primitive Types ko will handle Equality internally
        /// For complex types a supplied function can be assigned to improve 
        /// change (mutation) detection
        /// </summary>
        [IntrinsicProperty]
        public Func<T, T, bool> EqualityComparer {
            get;
            set;
        }
    }
}
