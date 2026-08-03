
using iLovePdf;
using iLovePdf.Core;
using iLovePdf.Model.Enums;
using iLovePdf.Model.Task;
using iLovePdf.Model.TaskParams;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Images;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace spGenerator
{
    public class SitePlanPromptServices
    {
        private readonly SitePlanAIPOCEntities _db;
        public SitePlanPromptServices()
        {
            _db = new SitePlanAIPOCEntities();
            _db.Configuration.ProxyCreationEnabled = false;

        }



        public string GeneratePrompt(SitePlanRequest req, SitePlanDraft draft, bool image, string edits, string context)
        {
            var prompt = new StringBuilder();

            //Clean JSON to reduce token use
            if (req == null)
            {
                prompt.Append("I have a blueprint for setting up a venue for assembling prepackaged meals that I need to edit."); ;

            }
            else
            {
                prompt.Append("I need help rendering/planning out the blueprint for setting up a venue for assembling prepackaged meals."); ;

            }
            prompt.Append("Notes to consider: assembly line tables are usually 6 or 8 ft long by 2.5 ft wide unless otherwise stated below by the attribute Table Sizes. If you consider the setup as a grid, the standard setup has a 5 ft gap between rows and 10 ft between columns.\n");
            if (!image) { prompt.Append("Here are some images showing a template for making blueprints and past created blueprints for context as you make the new ones. Make instructions so other models who can't see the templates can make new blueprints that adhere to the template" + context); }
            else { prompt.Append("Use these guidelines " + context); }
            prompt.Append("The size of the document generated must proportionally scale the dimensions of the site the attribute Room Dimensions. Attached below is a json object with information and notes about the site to consider while creating the venue. The Volunteer Flow (path fow volunteers to enter) and Supply Flow should be labeled with arrows");
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

        public async Task<SitePlanDraft> askAI(SitePlanRequest req)
        {


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
                ""required"": [""SiteOverview"",""RecommendedLayout"",""VolunteerFlow"",""SupplyFlow"",""Timeline"",""Risks"",""PMReviewChecklist"", ""ImageInstruction""],
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
            //Use blueprint if given one, resort to making from scratch otherwise
            GeneratedImage image;
            //IF PDF CONVERT TO REGULAR IMAGE

            if (req.RoomBlueprintFilePath != null && req.RoomBlueprintFilePath != "")
            {
                byte[] pngBytes;
                {
                    //if pdf
                    if (req.RoomBlueprintFilePath.Contains(".png") != true)
                    {
                        string publicProjectID = "project_public_4f29a93e98a06f34f1b40ba60a4c5000_jJzxH74a4c30d8049af15ff99d7763ef444fd";
                        string apiKey = Environment.GetEnvironmentVariable("ILOVEPDF_KEY");
                        var api = new iLovePdfApi(publicProjectID, apiKey);

                        var taskPDFtoJPG = api.CreateTask<PdfToJpgTask>();
                        var file1 = taskPDFtoJPG.AddFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints", req.RoomBlueprintFilePath));
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
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints", req.RoomBlueprintFilePath);

                        pngBytes = File.ReadAllBytes(fullPath);
                    }
                }
                using (MemoryStream stream = new MemoryStream(pngBytes))
                {

                    image = await imageClient.GenerateImageEditAsync(image: stream, "blueprint" + req.RoomBlueprintFilePath, prompt: GeneratePrompt(req, null, true, "", imageInstructions));
                }
            }
            else
            {
                image = await imageClient.GenerateImageAsync(prompt: GeneratePrompt(req, null, true, "", imageInstructions));
            }

            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints");

            string fileName = $"blueprint_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            // turn image.ImageBytes 2 array and save the bytes as a file;
            byte[] imageBytes = image.ImageBytes.ToArray();
            System.IO.File.WriteAllBytes(Path.Combine(directory, fileName), imageBytes);


            SitePlanDraft draft = JsonConvert.DeserializeObject<SitePlanDraft>(txt);
            draft.SiteOverview = Path.Combine("blueprints", fileName);
            draft.SitePlanRequestId = req.Id;
            draft.Reviewer = "Placeholder Name";
            draft.CreatedAt = DateTime.UtcNow;
            _db.SitePlanDrafts.Add(draft);



            //Save draft to sql
            _db.SaveChanges();
            //return draft
            return draft;




        }
    }
}