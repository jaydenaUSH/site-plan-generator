
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Images;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Drawing;
using System.Drawing.Imaging;
using iLovePdf;
using iLovePdf.Core;
using iLovePdf.Model.Enums;
using iLovePdf.Model.Task;
using iLovePdf.Model.TaskParams;

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

        public async Task<dynamic> getAllRequestDrafts(int reqId)
        {
            return await _db.SitePlanDrafts.Where(w => w.SitePlanRequestId == reqId).OrderByDescending(w => w.CreatedAt).ToListAsync();
        }
        public async Task<dynamic> editDraft(int id, string edits)
        {

            var row = _db.SitePlanDrafts.Find(id);
            var reqRow = _db.SitePlanRequests.Find(row.SitePlanRequestId);
            if (reqRow == null)
            {
                throw new InvalidOperationException($"Request not found for draft {id}");
            }

#pragma warning disable OPENAI001
            //Prepare context from folder
            var contextImages = new List<ResponseContentPart>();
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "context");
            var files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                byte[] fileBytes = File.ReadAllBytes(file);
                if (Path.GetExtension(file).ToLower() == ".pdf")
                {
                    contextImages.Add(ResponseContentPart.CreateInputFilePart(
                        BinaryData.FromBytes(fileBytes, "application/pdf"),
                            "application/pdf",
                               Path.GetFileName(file)
                        ));
                }
                else

                {
                    contextImages.Add(ResponseContentPart.CreateInputImagePart(BinaryData.FromBytes(fileBytes, "image/png"),
        imageDetailLevel: ResponseImageDetailLevel.High));
                }
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
                        
                        ""Timeline"":          { ""type"": ""string"" },
                        ""Risks"":             { ""type"": ""string"" },
                        ""PMReviewChecklist"": { ""type"": ""string"" },
                        ""ImageInstruction"" : { ""type"": ""string"" }

                },
                ""required"": [""SiteOverview"",""RecommendedLayout"",""Timeline"",""Risks"",""PMReviewChecklist"", ""ImageInstruction""],
                ""additionalProperties"": false
            }"),
            jsonSchemaIsStrict: true)

                }
            };

            var dimensionsContext = $"\n\nVenue room dimensions are: {reqRow.RoomDimensions}. Consider these when analyzing the edits.";


            format.InputItems.Add(ResponseItem.CreateUserMessageItem(_services.GeneratePrompt(null, row, false, edits, "")));
            format.InputItems.Add(ResponseItem.CreateUserMessageItem(dimensionsContext));
            format.InputItems.Add(ResponseItem.CreateUserMessageItem(contextImages));



            var client = new OpenAIClient(
                            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                        );
            var imageClient = client.GetImageClient("gpt-image-1");
            var responseClient = client.GetResponsesClient();

            var answer = await responseClient.CreateResponseAsync(format);
            var txt = answer.Value.GetOutputText();
            var imageInstructions = (string)JObject.Parse(txt)["ImageInstruction"];

            //Use result in image edit
            string editPrompt = "I have a blueprint already drafted using the following instructions that you should also adhere to" + imageInstructions + " . I want to make the following edits to (please be specific about changing what I ask and not other things unless associated " + edits +
                                ". The output image should have dimensions that are proportionally scaled of the venue's room dimensions listed here:" + reqRow.RoomDimensions + ". Things from the draft should carry over like Volunteer Flow (path fow volunteers to enter) and Supply Flow should be labeled with arrows";
            GeneratedImage image;
            //IF PDF CONVERT TO REGULAR IMAGE
            if (row.SiteOverview != null && row.SiteOverview != "")
            {
                byte[] pngBytes;

                {
                    //if pdf
                    if (row.SiteOverview.Contains(".png") != true)
                    {
                        string publicProjectID = "project_public_4f29a93e98a06f34f1b40ba60a4c5000_jJzxH74a4c30d8049af15ff99d7763ef444fd";
                        string apiKey = Environment.GetEnvironmentVariable("ILOVEPDF_KEY");
                        var api = new iLovePdfApi(publicProjectID, apiKey);

                        var taskPDFtoJPG = api.CreateTask<PdfToJpgTask>();
                        var file1 = taskPDFtoJPG.AddFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, row.SiteOverview));
                        taskPDFtoJPG.Process(new PdftoJpgParams { PdfJpgMode = PdfToJpgModes.Pages });
                        var jpgBytes = await taskPDFtoJPG.DownloadFileAsByteArrayAsync();
                        using (MemoryStream jpgStream = new MemoryStream(jpgBytes))
                        using (Image img = Image.FromStream(jpgStream))
                        using (MemoryStream pngStream = new MemoryStream())
                        {
                            img.Save(pngStream, ImageFormat.Png);
                            pngBytes = pngStream.ToArray();
                        }


                    }
                    else
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, row.SiteOverview);

                        pngBytes = File.ReadAllBytes(fullPath);
                    }
                }

                using (MemoryStream stream = new MemoryStream(pngBytes))
                {

                    image = await imageClient.GenerateImageEditAsync(prompt: editPrompt, image: stream, imageFilename: row.SiteOverview);
                }
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
            throw new InvalidOperationException($"No blueprint found to edit for draft {id}");

        }
        public async Task<SitePlanFinal> finalizeDraft(int id)
        {

            var row = _db.SitePlanDrafts.Find(id);
            SitePlanFinal finale = new SitePlanFinal
            {
                SitePlanRequestId = row.SitePlanRequestId,
                SiteOverview = row.SiteOverview,
                RecommendedLayout = row.RecommendedLayout,
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
