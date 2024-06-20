using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stash.Models;
using UnityEngine.Networking;

namespace Stash.Core
{
    public static class RestClient
    {
        /// <summary>
        /// Sends a GET request to the specified Stash API endpoint with optional headers and returns a Response object.
        /// </summary>
        /// <param name="url">The URL to send the GET request to.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <returns>A Response object containing the result of the request.</returns>
        public static async Task<Response> Get(string url, IEnumerable<RequestHeader> headers = null)
        {
            // Create a UnityWebRequest for the specified URL
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);

            // Add any specified headers to the request
            if (headers != null)
            {
                foreach (RequestHeader header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            // Send the web request asynchronously
            var getRequest = webRequest.SendWebRequest();

            // Wait for the request to complete
            while (!getRequest.isDone)
                await Task.Yield();

            // Check the result of the request and return a Response object accordingly
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    return new Response
                    {
                        StatusCode = webRequest.responseCode,
                        Data = webRequest.downloadHandler.text,
                    };
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                default:
                    return new Response
                    {
                        StatusCode = webRequest.responseCode,
                        Error = webRequest.error
                    };
            }
        }


        /// <summary>
        /// Sends a POST request to the specified Stash API endpoint with the provided body and optional headers.
        /// </summary>
        /// <param name="url">The URL to send the POST request to.</param>
        /// <param name="body">The body of the POST request.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <returns>Returns a Response object with the result of the POST request.</returns>
        public static async Task<Response> Post(string url, string body, IEnumerable<RequestHeader> headers = null)
        {
            // Convert body to byte array
            byte[] bodyPayload = Encoding.UTF8.GetBytes(body);

            // Create a UnityWebRequest for the POST request
            using UnityWebRequest webRequest = new(url, "POST");

            // Add optional headers to the request
            if (headers != null)
            {
                foreach (RequestHeader header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            // Set default headers for a JSON request
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");

            // Set the body payload and download handler for the request
            webRequest.uploadHandler = new UploadHandlerRaw(bodyPayload);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // Send the web request asynchronously
            var postRequest = webRequest.SendWebRequest();

            // Wait for the request to complete
            while (!postRequest.isDone)
                await Task.Yield();

            // Check the result of the request and return the appropriate Response
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    return (new Response
                    {
                        StatusCode = webRequest.responseCode,
                        Data = webRequest.downloadHandler.text,
                    });
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                default:
                    return (new Response
                    {
                        StatusCode = webRequest.responseCode,
                        Error = webRequest.error,
                        Data = webRequest.downloadHandler.text,
                    });
            }
        }
    }
}