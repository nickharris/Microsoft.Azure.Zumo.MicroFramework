// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using Microsoft.Azure.Zumo.MicroFramework.Core;
using System.IO;
using Microsoft.Azure.Zumo.MicroFramework.Helper;
using System.Text;
using Microsoft.SPOT;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides basic access to Mobile Services.
    /// </summary>
    public sealed partial class MobileServiceClient
    {
        /// <summary>
        /// Name of the config setting that stores the installation ID.
        /// </summary>
        private const string ConfigureAsyncInstallationConfigPath = "MobileServices.Installation.config";

        /// <summary>
        /// Name of the JSON member in the config setting that stores the
        /// installation ID.
        /// </summary>
        private const string ConfigureAsyncApplicationIdKey = "applicationInstallationId";

        /// <summary>
        /// Name of the  JSON member in the config setting that stores the
        /// authentication token.
        /// </summary>
        private const string LoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Relative URI fragment of the login endpoint.
        /// </summary>
        private const string LoginAsyncUriFragment = "login";

        /// <summary>
        /// Relative URI fragment of the login/done endpoint.
        /// </summary>
        private const string LoginAsyncDoneUriFragment = "login/done";

        /// <summary>
        /// Name of the Installation ID header included on each request.
        /// </summary>
        private const string RequestInstallationIdHeader = "X-ZUMO-INSTALLATION-ID";

        /// <summary>
        /// Name of the application key header included when there's a key.
        /// </summary>
        private const string RequestApplicationKeyHeader = "X-ZUMO-APPLICATION";

        /// <summary>
        /// Name of the authentication header included when the user's logged
        /// in.
        /// </summary>
        private const string RequestAuthenticationHeader = "X-ZUMO-AUTH";

        /// <summary>
        /// Content type for request bodies and accepted responses.
        /// </summary>
        private const string RequestJsonContentType = "application/json";

        /// <summary>
        /// Gets or sets the ID used to identify this installation of the
        /// application to provide telemetry data.  It will either be retrieved
        /// from local settings or generated fresh.
        /// </summary>
        private static string applicationInstallationId = null;

        /// <summary>
        /// A JWT token representing the current user's successful OAUTH
        /// authorization.
        /// </summary>
        /// <remarks>
        /// This is passed on every request (when it exists) as the X-ZUMO-AUTH
        /// header.
        /// </remarks>
        private string currentUserAuthenticationToken = null;
        
        /// <summary>
        /// Indicates whether a login operation is currently in progress.
        /// </summary>
        public bool LoginInProgress
        {
            get;
            private set;
        }

        /// <summary>
        /// Initialize the shared applicationInstallationId.
        /// </summary>        
        static MobileServiceClient()
        {
            // Try to get the AppInstallationId from settings

            applicationInstallationId = Guid.Empty.ToString();

            //TODO: NH implement solution for local persitent storage of application settings
            /*
            if (IsolatedStorageSettings.ApplicationSettings.Contains(ConfigureAsyncApplicationIdKey))
            {
                JToken config = null;
                try
                {
                    config = JToken.Parse(IsolatedStorageSettings.ApplicationSettings[ConfigureAsyncApplicationIdKey] as string);
                    applicationInstallationId = config.Get(ConfigureAsyncApplicationIdKey).AsString();
                }
                catch (Exception)
                {
                }
            }

            // Generate a new AppInstallationId if we failed to find one
            if (applicationInstallationId == null)
            {
                applicationInstallationId = Guid.NewGuid().ToString();
                string configText =
                    new JObject()
                    .Set(ConfigureAsyncApplicationIdKey, applicationInstallationId)
                    .ToString();
                IsolatedStorageSettings.ApplicationSettings[ConfigureAsyncInstallationConfigPath] = configText;
            }
             * */
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUri">
        /// The Uri to the Mobile Services application.
        /// </param>
        public MobileServiceClient(Uri applicationUri)
            : this(applicationUri, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUri">
        /// The Uri to the Mobile Services application.
        /// </param>
        /// <param name="applicationKey">
        /// The application name for the Mobile Services application.
        /// </param>
        public MobileServiceClient(Uri applicationUri, string applicationKey)
        {
            if (applicationUri == null)
            {
                throw new ArgumentNullException("applicationUri");
            }

            this.ApplicationUri = applicationUri;
            this.ApplicationKey = applicationKey;
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class based
        /// on an existing instance.
        /// </summary>
        /// <param name="service">
        /// An existing instance of the MobileServices class to copy.
        /// </param>
        private MobileServiceClient(MobileServiceClient service)
        {            
            this.ApplicationUri = service.ApplicationUri;
            this.ApplicationKey = service.ApplicationKey;
            this.CurrentUser = service.CurrentUser;
            this.currentUserAuthenticationToken = service.currentUserAuthenticationToken;         
        }

        /// <summary>
        /// Gets the Uri to the Mobile Services application that is provided by
        /// the call to MobileServiceClient(...).
        /// </summary>
        public Uri ApplicationUri { get; private set; }

        /// <summary>
        /// Gets the Mobile Services application's name that is provided by the
        /// call to MobileServiceClient(...).
        /// </summary>
        public string ApplicationKey { get; private set; }

        /// <summary>
        /// The current authenticated user provided after a successful call to
        /// MobileServiceClient.Login().
        /// </summary>
        public MobileServiceUser CurrentUser { get; private set; }

        /// <summary>
        /// Gets a reference to a table and its data operations.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A reference to the table.</returns>
        public IMobileServiceTable GetTable(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            else if (tableName.IsNullOrEmpty())
            {                                
                throw new ArgumentException(
                    tableName + Resources.EmptyArgumentExceptionMessage);
            }
            
            return new MobileServiceTable(tableName, this);
        }                
        
         
        /// <summary>
        /// Log a user out of a Mobile Services application.
        /// </summary>       
        public void Logout()
        {
            this.CurrentUser = null;
            this.currentUserAuthenticationToken = null;
        }
        
        /// <summary>
        /// Perform a web request and include the standard Mobile Services
        /// headers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriFragment">
        /// URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="entity">
        /// Optional content to send to the resource.
        /// </param>
        /// <returns>The JSON value of the response.</returns>
        //internal async Task<JToken> RequestAsync(string method, string uriFragment, JToken content)
        internal string Request(string method, string uriFragment, IMobileServiceEntity entity)
        {
            Debug.Assert(!method.IsNullOrEmpty(), "method cannot be null or empty!");
            Debug.Assert(!uriFragment.IsNullOrEmpty(), "uriFragment cannot be null or empty!");

            string jsonResult = null;

            // Create the web request            
            //IServiceFilterRequest request = new ServiceFilterRequest();
            var uri = new Uri(this.ApplicationUri.AbsoluteUri + uriFragment);
            using(HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest)
            {
                request.Method = method.ToUpper();
                request.Accept = RequestJsonContentType;

                // Set Mobile Services authentication, application, and telemetry
                // headers
                request.Headers.Add(RequestInstallationIdHeader, applicationInstallationId);
                if (!this.ApplicationKey.IsNullOrEmpty())
                {
                    request.Headers.Add(RequestApplicationKeyHeader, this.ApplicationKey);
                }

                if (!this.currentUserAuthenticationToken.IsNullOrEmpty())
                {
                    request.Headers.Add(RequestAuthenticationHeader, this.currentUserAuthenticationToken);
                }

                // Add any request as JSON
                if (entity != null)
                {
                    var content = MobileServicesTableSerializer.Serialize(entity);

                    request.ContentType = RequestJsonContentType;
                    request.ContentLength = Encoding.UTF8.GetBytes(content).Length;
                    request.UserAgent = "Micro Framework";

                    try
                    {
                        using (var requestStream = request.GetRequestStream())
                        using (var streamWriter = new StreamWriter(requestStream))
                        {
                            streamWriter.Write(content);                            
                        }

                        using (var response = (HttpWebResponse)request.GetResponse())
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {                            
                            if ((int)response.StatusCode >= 400)
                            {
                                //TODO: NH implement
                                // ThrowInvalidResponse(request, response, body);
                                throw new ApplicationException("Status Code: " + response.StatusCode);
                            }
                           
                             jsonResult = streamReader.ReadToEnd();
                           // result = GetResponseJson(json);                          
                        }
                    }
                    catch (WebException)
                    {
                        //TODO: handle web ex
                    }
                }
      
                return jsonResult;//TODO NH: not yet implemented implement w/ JSON deserialize support + patch
            }
        } 
    }
}
