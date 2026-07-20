
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
            _db.Configuration.ProxyCreationEnabled = false;

        }

        public dynamic getDraft(int id)
        {
            var row = _db.SitePlanDrafts.Find(id);
            return row;
        }
        public async Task<dynamic> editDraft(int id, string edits)
        {
            try
            {
                var row = _db.SitePlanDrafts.Find(id);
                if (row != null)
                {
#pragma warning disable OPENAI001
                    CreateResponseOptions format = new CreateResponseOptions()
                    {
                        Model = "gpt-5.1",
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
                    format.InputItems.Add(ResponseItem.CreateUserMessageItem(_services.GeneratePrompt(null, row, false, edits, "")));
                    var client = new OpenAIClient(
                            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                        );
                    var imageClient = client.GetImageClient("gpt-image-1");
                    var responseClient = client.GetResponsesClient();

                    var answer = await responseClient.CreateResponseAsync(format);
                    string editPrompt = "I have a blueprint already drafted that I want to make the following edits to (please be specific about changing what I ask and not other things unless associated " + edits;
                    GeneratedImage image = await imageClient.GenerateImageEditAsync(prompt: editPrompt, imageFilePath: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, row.SiteOverview));
                    string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints");

                    string fileName = $"blueprint_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                    // turn image.ImageBytes 2 array and save the bytes as a file;
                    byte[] imageBytes = image.ImageBytes.ToArray();
                    System.IO.File.WriteAllBytes(Path.Combine(directory, fileName), imageBytes);



                    var txt = answer.Value.GetOutputText();
                    SitePlanDraft draft = JsonConvert.DeserializeObject<SitePlanDraft>(txt);
                    draft.SiteOverview = Path.Combine("blueprints", fileName);
                    draft.SitePlanRequestId = row.SitePlanRequestId;
                    draft.Reviewer = "Placeholder Name";
                    draft.CreatedAt = DateTime.UtcNow;
                    _db.SitePlanDrafts.Add(draft);



                    //Save draft to sql
                    _db.SaveChanges();
                    return draft;

                }
                return "Row Not Found";
            }
            catch (Exception ex) { return ex; }
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
                _db.SaveChanges();


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
