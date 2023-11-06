using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clipboard_LLM
{
    class Palm2
    {
        const string PALM_KEY = API_KEY.PALM_KEY;

        public static async Task<string> GetAnswer(string question)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta3/models/text-bison-001:generateText?key={PALM_KEY}";

            var requestContent = $"{{\"prompt\": {{\"text\": \"{question}\"}}}}";

            using (var client = new HttpClient())
            {
                var content = new StringContent(requestContent, System.Text.Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                try
                {
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseString);
                    if (json["candidates"] != null && json["candidates"][0] != null && json["candidates"][0]["output"] != null)
                    {
                        string outputValue = json["candidates"][0]["output"].ToString();
                        return outputValue;
                    }
                    else
                    {
                        throw new Exception("답변 생성 실패");
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new Exception("답변 생성 실패");
                }
            }
        }
    }
}

