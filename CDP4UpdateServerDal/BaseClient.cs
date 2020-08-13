// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseClient.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4UpdateServerDal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using DevExpress.XtraRichEdit.Layout;

    using Newtonsoft.Json;

    /// <summary>
    /// The <see cref="BaseClient"/> provides a way to handle http request to the provided Update Server
    /// </summary>
    public abstract class BaseClient
    {
        /// <summary>
        /// Holds the Base address of the target Update Server
        /// </summary>
        private readonly Uri baseAddress;

        internal HttpClient HttpClient { get; set; }

        /// <summary>
        /// Builds the <see cref="string"/> request from the provided type
        /// </summary>
        /// <typeparam name="TDto">The <see cref="Type"/></typeparam>
        /// <param name="action">The action</param>
        /// <returns>Http request string</returns>
        private string BuildRequest<TDto>(string action)
        {
            return $"api/{typeof(TDto).Name.Replace("Dto", string.Empty).ToLower()}{(action is null ? string.Empty : $"/{action}")}";
        }

        /// <summary>
        /// Initializes a new <see cref="BaseClient"/>
        /// </summary>
        /// <param name="serverBaseAddress">The targeted server base address</param>
        internal BaseClient(Uri serverBaseAddress)
        {
            this.baseAddress = serverBaseAddress;
        }
        
        /// <summary>
        /// Serializes the provided object instance and wrap it so it can be sent through a Http Request
        /// </summary>
        /// <param name="instance">The instance to be serialised</param>
        /// <returns>A <see cref="HttpContent"/></returns>
        protected HttpContent WrapContent(object instance)
        {
            var json = JsonConvert.SerializeObject(instance);
           return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Queries the API and returns a stream
        /// </summary>
        /// <typeparam name="TDto">The dto object of the query</typeparam>
        /// <param name="action">The action to perform </param>
        /// <param name="method">The <see cref="HttpVerbs"/> verb</param>
        /// <param name="content">The reauest payload</param>
        /// <returns>A Task of type <see cref="Stream"/></returns>
        internal async Task<Stream> QueryStream<TDto>(HttpVerbs method, string action = null,  HttpContent content = null) where TDto : new()
        {
            var response = await this.Query<TDto>(method, action, content);
            return await response.ReadAsStreamAsync();
        }

        /// <summary>
        /// Queries the API and returns the body of the response
        /// </summary>
        /// <typeparam name="TDto">The dto object of the query</typeparam>
        /// <param name="action">The action to perform </param>
        /// <param name="method">The <see cref="HttpVerbs"/> verb</param>
        /// <param name="content">The reauest payload</param>
        /// <returns>A Task of type <see cref="string"/></returns>
        internal async Task<string> QueryString<TDto>(HttpVerbs method, string action = null,  HttpContent content = null) where TDto : new()
        {
            var response = await this.Query<TDto>(method, action, content);
            return await response.ReadAsStringAsync();
        }

        /// <summary>
        /// Queries the API and deserializes the response to the type 
        /// </summary>
        /// <typeparam name="TDto">The dto object of the query</typeparam>
        /// <typeparam name="TReturn">The <see cref="Type"/> to return</typeparam>
        /// <param name="action">The action to perform </param>
        /// <param name="method">The <see cref="HttpVerbs"/> verb</param>
        /// <param name="content">The reauest payload</param>
        /// <returns>A Task of type <see cref="TReturn"/></returns>
        internal async Task<TReturn> Query<TDto, TReturn>(HttpVerbs method, string action = null,  HttpContent content = null) 
            where TDto : new() 
            where TReturn : new()
        {
            var response = await this.QueryString<TDto>(method, action, content);
            return JsonConvert.DeserializeObject<TReturn>(response);
        }

        /// <summary>
        /// Queries the API and return an <see cref="HttpContent"/>
        /// </summary>
        /// <typeparam name="TDto">The dto object of the query</typeparam>
        /// <param name="action">The action to perform </param>
        /// <param name="method">The <see cref="HttpVerbs"/> verb</param>
        /// <param name="content">The reauest payload</param>
        /// <returns>A Task of type <see cref="HttpContent"/></returns>
        internal async Task<HttpContent> Query<TDto>(HttpVerbs method, string action = null,  HttpContent content = null)
        {
            HttpContent result = null;
            HttpResponseMessage response = null;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            using (var client = new HttpClient() { BaseAddress = this.baseAddress })
            {
                var request = this.BuildRequest<TDto>(action);
                
                response = method switch
                {
                    HttpVerbs.Get => await client.GetAsync(request),
                    HttpVerbs.Post => await client.PostAsync(request, content),
                    HttpVerbs.Put => await client.PutAsync(request, content),
                    HttpVerbs.Delete => await client.DeleteAsync(request),
                    HttpVerbs.Head => throw new NotSupportedException($"Method {nameof(HttpVerbs.Head)} is not yet supported"),
                    HttpVerbs.Patch => throw new NotSupportedException($"Method {nameof(HttpVerbs.Patch)} is not yet supported"),
                    HttpVerbs.Options => throw new NotSupportedException($"Method {nameof(HttpVerbs.Options)} is not yet supported"),
                    _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
                };
            }

            if (response?.IsSuccessStatusCode == true)
            {
                result = response.Content;
            }
            else
            {
                throw new HttpRequestException($"The request has failed {response?.ReasonPhrase} with code {response?.StatusCode}");
            }

            return result;
        }
    }
}
