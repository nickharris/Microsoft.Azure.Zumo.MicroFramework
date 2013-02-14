// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.Azure.Zumo.MicroFramework.Core;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    internal partial class MobileServiceTable
    {
        /// <summary>
        /// Gets the name of the results key in an inline count response
        /// object.
        /// </summary>
        protected const string InlineCountResultsKey = "results";

        /// <summary>
        /// Gets the name of the count key in an inline count response object.
        /// </summary>
        protected const string InlineCountCountKey = "count";

        /// <summary>
        /// Insert a new object into a table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        //TODO: return type to be updated once json deserializer in place
        public string Insert(IMobileServiceEntity instance)
        {
            return this.SendInsert(instance);
        }
      
    }
}
