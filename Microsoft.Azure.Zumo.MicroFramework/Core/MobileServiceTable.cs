// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


using System;
using System.IO;
using Microsoft.Azure.Zumo.MicroFramework.Core;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    internal partial class MobileServiceTable : IMobileServiceTable
    {
        /// <summary>
        /// Name of the reserved Mobile Services ID member.
        /// </summary>
        /// <remarks>
        /// Note: This value is used by other areas like serialiation to find
        /// the name of the reserved ID member.
        /// </remarks>
        internal const string IdPropertyName = "id";
        

        /// <summary>
        /// The route separator used to denote the table in a uri like
        /// .../{app}/tables/{coll}.
        /// </summary>
        internal const string TableRouteSeperatorName = "tables";

        /// <summary>
        /// Initializes a new instance of the MobileServiceTables class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="client">
        /// Reference to the MobileServiceClient associated with this table.
        /// </param>
        public MobileServiceTable(string tableName, MobileServiceClient client)
        {
            this.TableName = tableName;
            this.MobileServiceClient = client;            
        }

        /// <summary>
        /// Gets a reference to the MobileServiceClient associated with this
        /// table.
        /// </summary>
        public MobileServiceClient MobileServiceClient { get; private set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Get a uri fragment representing the resource corresponding to the
        /// table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        private static string GetUriFragment(string tableName)
        {
            return Path.Combine(TableRouteSeperatorName, tableName);
        }

        /// <summary>
        /// Get a uri fragment representing the resource corresponding to the
        /// given id in the table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="id">The id of the instance.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        private static string GetUriFragment(string tableName, object id)
        {
            string uriFragment = GetUriFragment(tableName);
            return Path.Combine(uriFragment, TypeExtensions.ToUriConstant(id));
        }

    

        /// <summary>
        /// Insert a new object into a table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        internal string SendInsert(IMobileServiceEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            
            // Make sure the instance doesn't have its ID set for an insertion
            if (instance.Id > 0)
            {
                throw new ArgumentException(                   
                    //TODO:NH use regex to implement format as an extension method on string
                    Resources.CannotInsertWithExistingIdMessage + IdPropertyName, "instance");
                    /*string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.CannotInsertWithExistingIdMessage,
                        IdPropertyName),
                    "instance");*/
            }

            string url = GetUriFragment(this.TableName);            
         
            //JToken response = await this.MobileServiceClient.RequestAsync("POST", url, instance);
            var responseJson = this.MobileServiceClient.Request("POST", url, instance);

            //TODO: do only if worthwhile as will have mem+perf hit.. i think most apps just want to offload the data..
            //var result = JSONSerializer.Deserialize(responseJson, instance.GetType());                        
            //JToken patched = Patch(instance, response);                            
            //return patched;

            return responseJson;
        }
    }
}
