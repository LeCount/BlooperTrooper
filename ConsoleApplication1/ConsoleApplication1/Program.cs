using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri address = new Uri("http://simplesocialnetworkfarpfu3vzb.devcloud.acquia-sites.com/");

            if (address == null) { throw new ArgumentNullException("address"); }

            try
            {
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;


                // Add authentication to request
                request.Credentials = new NetworkCredential("erik", "test123");


                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                // Create the data we want to send  
                string appId = "Tja Erik!";
                string context = "Test";
                string query = "QueryTest";

                StringBuilder data = new StringBuilder();

                data.Append("appid=" + Uri.EscapeUriString(appId));
                data.Append("&context=" + Uri.EscapeUriString(context));
                data.Append("&query=" + Uri.EscapeUriString(query));


                // Create a byte array of the data we want to send  
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                // Set the content length in the request headers  
                request.ContentLength = byteData.Length;

                // Write data  
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                // Get response  
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                Console.WriteLine(reader.ReadToEnd());

                Console.ReadKey();
                response.Close();
            }
            catch(WebException wex)  
            {
                // This exception will be raised if the server didn't return 200 - OK  
                // Try to retrieve more information about the network error  
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        Console.WriteLine(
                            "The server returned '{0}' with the status code {1} ({2:d}).",
                            errorResponse.StatusDescription, errorResponse.StatusCode,
                            errorResponse.StatusCode);
                    } 
                }
            }
        }
    }
}
