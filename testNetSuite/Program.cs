using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace testNetSuite
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Fill These vars with your netsuite information **********************************************************************
                String urlString = "";              //RESTlet uri

                String ckey = "";                   //Consumer Key
                String csecret = "";                //Consumer Secret
                String tkey = "";                   //Token ID
                String tsecret = "";                //Token Secret
                String netsuiteAccount = "";        //Netsuite Account to connect to (i.e 5624525_SB1)
                //**********************************************************************************************************************

                Uri url = new Uri(urlString);
                OAuthBase req = new OAuthBase();
                string timestamp = req.GenerateTimeStamp();
                string nonce = req.GenerateNonce();
                string norm = "";
                string norm1 = "";
                string signature = req.GenerateSignature(url, ckey, csecret, tkey, tsecret, "POST", timestamp, nonce, out norm, out norm1);
               // string signature = req.GenerateSignature(url, ckey, csecret, tkey, tsecret, "GET", timestamp, nonce, out norm, out norm1);


                //Percent Encode (Hex Escape) plus character
                if (signature.Contains("+"))
                {
                    signature = signature.Replace("+", "%2B");
                }

                string header = "Authorization: OAuth ";
                header += "oauth_signature=\"" + signature + "\",";
                header += "oauth_version=\"1.0\",";
                header += "oauth_nonce=\"" + nonce + "\",";
                header += "oauth_signature_method=\"HMAC-SHA256\",";
                header += "oauth_consumer_key=\"" + ckey + "\",";
                header += "oauth_token=\"" + tkey + "\",";
                header += "oauth_timestamp=\"" + timestamp + "\",";
                header += "realm=\"" + netsuiteAccount + "\"";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
                request.ContentType = "application/json";
                // request.Method = "GET";
                request.Method = "POST";

                var postData = new Root()
                {

                    q = "Select * from customer "
                };
                string jsonString = JsonConvert.SerializeObject(postData);

                var data = Encoding.ASCII.GetBytes(jsonString);
                request.ContentLength = data.Length;

                request.Headers.Add(header);

                request.Headers.Add("Prefer", "transient");

                using (Stream responseStream = request.GetRequestStream())
                {
                    responseStream.Write(data, 0, data.Length);

                    // StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                   // Console.WriteLine(JsonConvert.DeserializeObject(reader.ReadToEnd()));
                }

                WebResponse response = request.GetResponse();
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                
                 var test = JsonConvert.DeserializeObject<Root2>(responseString);

                Console.WriteLine(JsonConvert.DeserializeObject(responseString));

            }
            catch (System.Net.WebException e)
            {
                HttpWebResponse response = e.Response as HttpWebResponse;
                switch ((int)response.StatusCode)
                {
                    case 401:
                        Console.WriteLine("Unauthorized, please check your credentials");
                        break;
                    case 403:
                        Console.WriteLine("Forbidden, please check your credentials");
                        break;
                    case 404:
                        Console.WriteLine("Invalid Url");
                        break;
                }
                Console.WriteLine("Code: " + (int)response.StatusCode);

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine("Response: " + JsonConvert.DeserializeObject(reader.ReadToEnd()));

                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
