using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Clipboard_LLM
{
    internal class Translator
    {
        const string CLIENT_ID = API_KEY.CLIENT_ID;
        const string CLIENT_SECRET = API_KEY.CLIENT_SECRET;

        public static string detectLangs(string query)
        {
            string url = "https://openapi.naver.com/v1/papago/detectLangs";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Naver-Client-Id", CLIENT_ID);
            request.Headers.Add("X-Naver-Client-Secret", CLIENT_SECRET);
            request.Method = "POST";
            byte[] byteDataParams = Encoding.UTF8.GetBytes("query=" + query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteDataParams, 0, byteDataParams.Length);
            st.Close();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string text = reader.ReadToEnd();
                stream.Close();
                response.Close();
                reader.Close();
                JObject ret = JObject.Parse(text);
                return ret["langCode"].ToString();
            }
            catch {
                throw new Exception("언어 감지 실패");
            }
        }

        public static string translate(string query, string source="ko", string target="en")
        {
            string url = "https://openapi.naver.com/v1/papago/n2mt";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Naver-Client-Id", CLIENT_ID);
            request.Headers.Add("X-Naver-Client-Secret", CLIENT_SECRET);
            request.Method = "POST";
            byte[] byteDataParams = Encoding.UTF8.GetBytes($"source={source}&target={target}&text={query}");
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;
            Stream st = request.GetRequestStream();
            st.Write(byteDataParams, 0, byteDataParams.Length);
            st.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            stream.Close();
            response.Close();
            reader.Close();

            JObject ret = JObject.Parse(text);
            return ret["message"]["result"]["translatedText"].ToString();
        }

        static async Task<string> translater(string source, string target, string text)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Naver-Client-Id", CLIENT_ID);
                client.DefaultRequestHeaders.Add("X-Naver-Client-Secret", CLIENT_SECRET);

                var formContent = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("source", source),
                new KeyValuePair<string, string>("target", target),
                new KeyValuePair<string, string>("text", text)
            });

                var response = await client.PostAsync("https://openapi.naver.com/v1/papago/n2mt", formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Translation successful. Response:");
                    JObject ret = JObject.Parse(responseContent);
                    return ret["message"]["result"]["translatedText"].ToString();
                }
                else
                {
                    Console.WriteLine("Translation failed. Response:");
                    Console.WriteLine(responseContent);
                }
                return responseContent;
            }
        }
    }
}
