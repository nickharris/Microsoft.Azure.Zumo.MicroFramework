// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Diagnostics;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents the type information needed to serialize instance of a given
    /// type with the MobileServiceTableSerializer.
    /// </summary>
    [DebuggerDisplay("{Type.FullName,nq}")]
    internal class SerializableType
    {
        /// <summary>
        /// The name of the property representing the ID of an element if we're
        /// searching for the ID member of a POCO.  We'll search for this text
        /// as well as upper and lowercase variants.
        /// </summary>
        public const string IdPropertyName = "id";
    
        /// <summary>
        /// Check if the value for an ID property is the default value.
        /// </summary>
        /// <param name="value">The value of the ID property.</param>
        /// <returns>
        /// A value indicating whether the ID property has its default value.
        /// </returns>
        public static bool IsDefaultIdValue(object value)
        {
            // We'll consider either null or 0 to be default
            return value == null ||
                object.Equals(value, 0) ||
                object.Equals(value, 0L);
        }
    }
}
