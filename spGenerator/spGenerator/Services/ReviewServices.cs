
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Images;
using OpenAI.Responses;
using System;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Threading.Tasks;

namespace spGenerator
{
    public class ReviewServices
    {
        private readonly SitePlanAIPOCEntities _db;
        private readonly SitePlanPromptServices _services;

        public ReviewServices()
        {
            _db = new SitePlanAIPOCEntities();
            _services = new SitePlanPromptServices();
        }

        public string getDraft(int id)
        {
            var row = _db.SitePlanDrafts.Find(id);
            if (row != null)
            {
                return "Draft successfully retrieved";
            }
            else
            {
                return "Error finding draft";
            }
        }
        public async Task<string> editDraft(int id){
            var row = _db.SitePlanDrafts.Find(id);
            if (row != null)
            {
#               pragma warning disable OPENAI001
                CreateResponseOptions format = new CreateResponseOptions()
                {
                    Model = "gpt-5.1",
                    Instructions = _services.GeneratePrompt(null, row, false),
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
        format.InputItems.Add(ResponseItem.CreateUserMessageItem(_services.GeneratePrompt(null, row, false)));
                var client = new OpenAIClient(
                        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    );
        var imageClient = client.GetImageClient("gpt-image-1");
        var responseClient = client.GetResponsesClient();

        var answer = await responseClient.CreateResponseAsync(format);
        GeneratedImage image = await imageClient.GenerateImageAsync(prompt: _services.GeneratePrompt(null, row, true));
        string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints");

        string fileName = $"blueprint_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
        // turn image.ImageBytes 2 array and save the bytes as a file;
        byte[] imageBytes = image.ImageBytes.ToArray();
        System.IO.File.WriteAllBytes(Path.Combine(directory, fileName), imageBytes);



        var txt = answer.Value.GetOutputText();
        SitePlanDraft draft = JsonConvert.DeserializeObject<SitePlanDraft>(txt);
        draft.SitePlanRequestId = row.SitePlanRequestId;
        draft.Reviewer = "Placeholder Name";
        draft.CreatedAt = DateTime.UtcNow;
        _db.SitePlanDrafts.Add(draft);



        //Save draft to sql
        _db.SaveChanges();
                return "Successful edit";
    }
            else
            {
                return "Edit was unsuccessful";
}
        }
        public string finalizeDraft(int id)
        {
            try
            {
                var row = _db.SitePlanDrafts.Find(id);
                SitePlanFinal finale = new SitePlanFinal
                {
                    SitePlanRequestId = row.SitePlanRequestId,
                    SiteOverview = row.SiteOverview,
                    RecommendedLayout = row.RecommendedLayout,
                    VolunteerFlow = row.VolunteerFlow,
                    SupplyFlow = row.SupplyFlow,
                    Timeline = row.Timeline,
                    Risks = row.Risks,
                    PMReviewChecklist = row.PMReviewChecklist,
                    Reviewer = row.Reviewer,
                    AdditionalNotes = row.AdditionalNotes
                };
                _db.SitePlanFinals.Add(finale);
                _db.SaveChanges();

                row.finalVersion = finale.Id;

                return "Draft successfully finalized";
                //Save reviewer, and timestamp
            }
            catch 
            {
                return "Draft could not be finalized";
            }
        }
    }
        
    }
