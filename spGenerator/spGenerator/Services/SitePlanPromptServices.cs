
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Images;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
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



        public string GeneratePrompt(SitePlanRequest req, SitePlanDraft draft, bool image, string edits, string context)
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
            prompt.Append("Here are some images showing a template fro amking blueprints and past crated blueprints for context as you make the new ones", context);
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
                prompt.Append("\n I want to make the following edits: " + edits);
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
            //Prepare context from folder
            var contextImages = new List<ResponseContentPart>();
            var folder = ;
            var files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                byte[] imgBytes = File.ReadAllBytes(file);

                contextImages.Add(ResponseContentPart.CreateInputImagePart(BinaryData.FromBytes(imgBytes, "image/png"),
        imageDetailLevel: ResponseImageDetailLevel.Low));

            }
            // Create the format and options for Responses API call
            CreateResponseOptions format = new CreateResponseOptions()
            {
                Model = "gpt-5.1",
                Instructions = GeneratePrompt(req, null, false, "", ""),
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
                        ""PMReviewChecklist"": { ""type"": ""string"" },
                        ""ImageInstruction"" : {""type"":""string""}
                },
                ""required"": [""SiteOverview"",""RecommendedLayout"",""VolunteerFlow"",""SupplyFlow"",""Timeline"",""Risks"",""PMReviewChecklist""],
                ""additionalProperties"": false
            }"),
            jsonSchemaIsStrict: true)

                }
            };
            format.InputItems.Add(ResponseItem.CreateUserMessageItem(GeneratePrompt(req, null, false, "", "")));
            format.InputItems.Add(ResponseItem.CreateUserMessageItem(contextImages));


            //Make AI API instance 
            var client = new OpenAIClient(
                        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    );
            var imageClient = client.GetImageClient("gpt-image-1");
            var responseClient = client.GetResponsesClient();

            var answer = await responseClient.CreateResponseAsync(format);
            var txt = answer.Value.GetOutputText();
            //Use result from the Responses API to input into images API

            var imageInstructions = (string)JObject.Parse(txt)["ImageInstruction"];
            GeneratedImage image = await imageClient.GenerateImageAsync(prompt: GeneratePrompt(req, null, true, "", imageInstructions));

            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints");

            string fileName = $"blueprint_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            // turn image.ImageBytes 2 array and save the bytes as a file;
            byte[] imageBytes = image.ImageBytes.ToArray();
            System.IO.File.WriteAllBytes(Path.Combine(directory, fileName), imageBytes);


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