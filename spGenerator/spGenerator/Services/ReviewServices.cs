
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Images;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
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

        public async Task<dynamic> getDraft(int id)
        {
            var row = await _db.SitePlanDrafts.FindAsync(id);
            return row;
        }
        public async Task<dynamic> editDraft(int id, string edits)
        {
           
                var row = _db.SitePlanDrafts.Find(id);
               
                    #pragma warning disable OPENAI001
                    //Prepare context from folder
                    var contextImages = new List<ResponseContentPart>();
                    string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "context");
                    var files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        byte[] imgBytes = File.ReadAllBytes(file);

                        contextImages.Add(ResponseContentPart.CreateInputImagePart(BinaryData.FromBytes(imgBytes, "image/png"),
                imageDetailLevel: ResponseImageDetailLevel.High));

                    }
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
                        ""PMReviewChecklist"": { ""type"": ""string"" },
                        ""ImageInstruction"" : { ""type"": ""string"" }

                },
                ""required"": [""SiteOverview"",""RecommendedLayout"",""VolunteerFlow"",""SupplyFlow"",""Timeline"",""Risks"",""PMReviewChecklist"", ""ImageInstruction""],
                ""additionalProperties"": false
            }"),
                    jsonSchemaIsStrict: true)

                        }
                    };
                    format.InputItems.Add(ResponseItem.CreateUserMessageItem(_services.GeneratePrompt(null, row, false, edits, "")));
                    format.InputItems.Add(ResponseItem.CreateUserMessageItem(contextImages));

                    var client = new OpenAIClient(
                            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                        );
                    var imageClient = client.GetImageClient("gpt-image-1");
                    var responseClient = client.GetResponsesClient();

                    var answer = await responseClient.CreateResponseAsync(format);
                    var txt = answer.Value.GetOutputText();
                    var imageInstructions = (string)JObject.Parse(txt)["ImageInstruction"];

                    //Use rseult in image edit
                    string editPrompt = "I have a blueprint already drafted with the following instructions that you should also adhere to"+imageInstructions+" . I want to make the following edits to (please be specific about changing what I ask and not other things unless associated " + edits;
                    GeneratedImage image = await imageClient.GenerateImageEditAsync(prompt: editPrompt, imageFilePath: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, row.SiteOverview));
                    string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints");

                    string fileName = $"blueprint_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                    // turn image.ImageBytes 2 array and save the bytes as a file;
                    byte[] imageBytes = image.ImageBytes.ToArray();
                    System.IO.File.WriteAllBytes(Path.Combine(directory, fileName), imageBytes);



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
        public async Task<SitePlanFinal> finalizeDraft(int id)
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


                return finale;
                //Save reviewer, and timestamp
            
        }
    }

}
