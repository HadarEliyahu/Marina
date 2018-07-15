using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;

namespace Marina.Classes
{
    class LuisParser
    {

        // Marina app id in LUIS
        const string luisAppId = "fc17c62b-485c-4ba1-9bb3-e984dd358243";
        const string subscriptionKey = "86a2dc227c144d59b6d10a9896e4bb8b";

        // Luis result xml names
        public const string TOP_SCORING_INTENT = "topScoringIntent";
        public const string INTENT = "intent";
        public const string TYPE = "type";
        public const string ENTITY = "entity";
        public const string ENTITIES = "entities";

        // this function get Luis response and returns the intent
        public static string getIntent(string LuisResult)
        {
            Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string, object>>(LuisResult);

            JObject topScoringIntent = (JObject)JToken.FromObject(values[TOP_SCORING_INTENT]);

            return topScoringIntent.ToObject<Dictionary<string, string>>()[INTENT];
        }

        // this function get Luis response and returns the entities
        public static Dictionary<string, Entity> getEntities(string LuisResult)
        {
            Dictionary<string, Entity> result = new Dictionary<string, Entity>();

            Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string, object>>(LuisResult);

            JArray entities = (JArray)JToken.FromObject(values[ENTITIES]);

            Dictionary<string, string>[] entitiesArray = entities.Select(item => item.ToObject<Dictionary<string, string>>()).ToArray();

            for (int i = 0; i < entitiesArray.Length; i++)
            {
                result.Add(entitiesArray[i][TYPE], new Entity(entitiesArray[i][ENTITY], entitiesArray[i][TYPE]));
            }

            return result;
        }

        // this function requests from LUIS to understand the request
        public static async Task<string> MakeRequest(string query)
        {

            HttpClient client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // insert the subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // insert the query yo the request
            queryString["q"] = query;


            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;
            try
            {
                var response = await client.GetAsync(uri);


                var strResponseContent = await response.Content.ReadAsStringAsync();
                // return the response
                return strResponseContent.ToString();
            }
            catch (Exception)
            {
                throw new Exception("no internet connection");
            }


        }
    }
}
