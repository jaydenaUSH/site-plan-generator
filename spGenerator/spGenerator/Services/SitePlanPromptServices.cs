
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Images;
using OpenAI.Responses;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace spGenerator
{
    public class SitePlanPromptServices
    {
        private readonly SitePlanAIPOCEntities _db;
        public SitePlanPromptServices()
        {
            _db = new SitePlanAIPOCEntities();
        }
        public string GeneratePrompt(SitePlanRequest req, SitePlanDraft draft, bool image, string edits)
        {
            var prompt = new StringBuilder();
            if (req == null)
            {
                prompt.Append("I have a blueprint for setting up a venue for assembling prepackaged meals that I need to edit."); ;

            }
            else
            {
                prompt.Append("I need help planning out the blueprint for setting up a venue for assembling prepackaged meals."); ;

            }
            prompt.Append("Notes to consider: assembly line tables are 6 or 8 ft long by 2.5 ft wide. If you consider the setup as a grid, the standard setup has a 5 ft gap between rows and 10 between columns.\n");
            prompt.Append("The size of the document generated must scale the dimensions of the site. Attached below is a json object with information and notes about the site to consider while creating the venue." +
                " Notes to consider: assembly line tables are 6 or 8 ft long by 2.5 ft wide. If you consider the setup as a grid, the standard setup has a 5 ft gap between rows and 10 between columns. " +
                "The generated image should be a PDF. Furthermore, the size of the document generated must scale the dimensions of the site. Below is the JSON object with information to make the blueprint\n");
            if (req != null)
            {
                prompt.Append(JsonConvert.SerializeObject(req));
            }
            else
            {
                prompt.Append(JsonConvert.SerializeObject(draft, new JsonSerializerSettings
{
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
}));
                prompt.Append("\n I want to make the following edits: "+ edits);
            }
            if (!image)
            {
                prompt.Append("I already asked the image generator for the above prompt and it already made the image. " +
                    "I just wanted text context and considerations for the blueprint. The response must be structured based on the give format\n");
            }


            return prompt.ToString();
        }

        public async Task<string> askAI(SitePlanRequest req)
        {
            #pragma warning disable OPENAI001
            CreateResponseOptions format = new CreateResponseOptions()
            {
                Model = "gpt-5.1",
                Instructions = GeneratePrompt(req, null, false, ""),
                TextOptions = new ResponseTextOptions
                {
                    TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "site_plan",
                    jsonSchema: BinaryData.FromString(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""SiteOverview"":      { ""type"": ""string"" },
                        ""RecommendedLayout"": { ""type"": ""string"" },
                        ""VolunteerFlow"":     { ""type"": ""string"" },
                        ""SupplyFlow"":        { ""type"": ""string"" },
                        ""Timeline"":          { ""type"": ""string"" },
                        ""Risks"":             { ""type"": ""string"" },
                        ""PMReviewChecklist"": { ""type"": ""string"" }
                },
                ""required"": [""SiteOverview"",""RecommendedLayout"",""VolunteerFlow"",""SupplyFlow"",""Timeline"",""Risks"",""PMReviewChecklist""],
                ""additionalProperties"": false
            }"),
            jsonSchemaIsStrict: true)

                }
            };
            format.InputItems.Add(ResponseItem.CreateUserMessageItem(GeneratePrompt(req, null, false, "")));

            var vectorContext = new OpenAI.Images.ImageGenerationOptions();

            //Make AI API instance 
            var client = new OpenAIClient(
                        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    );
            var imageClient = client.GetImageClient("gpt-image-1");
            var responseClient = client.GetResponsesClient();

            var answer = await responseClient.CreateResponseAsync(format);
            GeneratedImage image = await imageClient.GenerateImageAsync(prompt: GeneratePrompt(req, null, true, ""));
            //SitePlanRequestId, CreatedAt, Reviewer, finalVersion

            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints");

            string fileName = $"blueprint_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            // turn image.ImageBytes 2 array and save the bytes as a file;
            byte[] imageBytes = image.ImageBytes.ToArray();
            System.IO.File.WriteAllBytes(Path.Combine(directory, fileName), imageBytes);



            var txt = answer.Value.GetOutputText();
            SitePlanDraft draft = JsonConvert.DeserializeObject<SitePlanDraft>(txt);    
            draft.SiteOverview = Path.Combine(directory, fileName);
            draft.SitePlanRequestId = req.Id;
            draft.Reviewer = "Placeholder Name";
            draft.CreatedAt = DateTime.UtcNow;
            _db.SitePlanDrafts.Add(draft);



            //Save draft to sql
            _db.SaveChanges();
            //return draft
            return "SitePlanDraft table updated, and image generated in root project folder";




        }
    }
}