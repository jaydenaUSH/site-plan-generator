
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
            prompt.Append("Notes to consider: Assembly line tables are 6x2.5 ft or 8x2.5 ft unless otherwise stated. An assembly line is made by joining at least 2 tables. If you consider the setup as a grid, the standard setup has a 5 ft gap between rows and 10 ft between columns unless otherwise mentioned.\n");
            if (!image) { prompt.Append(" The attached context contains BEFORE and AFTER examples. Files with the same filename represent the same venue before and after drafting. In every pair the AFTER is the BEFORE drawing completely unchanged with an overlay " +
                "added on top, so the transformation is purely additive. Learn where the overlay elements get placed and why. Make instructions so other models who can't see the images can make new blueprints similar" + context); }
            else {
                return "REDRAW NOTHING. The attached image is a real venue floor plan and it must survive exactly: every wall, door swing, column, stair, escalator, fixture and existing dimension text stays" +
                    " in its exact position, at its exact size, in its exact line weight. Do not straighten, simplify, rescale, recrop, recenter or restyle any existing line. Do not add a room outline, border," +
                    " page frame, grid background, scale bar or overall dimension line. You may fade the existing linework to light grey but nothing may move. Everything new goes on top like a transparent overlay." +
                    " Keep the same framing and aspect ratio as the input. Apply these specifics: " + " SCALE IS THE MOST IMPORTANT RULE. Everything you add must be drawn at the same feet per inch as the floor plan itself, measured against the dimension numbers already printed on it." +
                    " A table is 6 ft long and 2.5 ft deep, so a table is a rectangle about two and a half times longer than it is deep. An assembly line is two tables joined end to end, 12 ft long and 2.5 ft deep," +
                    " so an assembly line is a long thin bar about five times longer than it is deep. Never draw a table or an assembly line as a square or a fat block. Stacked lines sit 5 ft apart, so the gap" +
                    " between two lines is far smaller than the length of a line. If the room is 100 ft across then one assembly line covers roughly an eighth of that width." +
                    " Draw assembly lines as light grey bars with a thin black outline. Draw pallet storage as small tan brown filled squares placed against the walls. Always draw the stage, as a light grey rectangle labelled Stage." +
                    " Electricity is thin straight red lines. Each red line runs straight along one row of assembly lines and touches at most 5 tables. Do not draw red lines over empty floor, do not curve them," +
                    " do not turn them into arrows, and never draw more red lines than there are rows of tables." +
                    " Print only 4 to 6 measurement labels in total, just enough to show the viewer the scale. Set every label in a clean printed sans serif typeface like Arial, sharp and machine set, never sketched or hand lettered." +
                    " Every word on the image must be spelled correctly. If you are unsure how to spell a word, leave it out." +
                    " Apply these specifics: " + context;
            }
            if(req == null || (req.RoomBlueprintFilePath != null && req.RoomBlueprintFilePath != ""))
            {
                prompt.Append("A venue floor plan is attached and its geometry is authoritative. Ignore the Room Dimensions attribute entirely. Do not draw a room outline and do not draw an overall dimension line. ");

            }
            else
{
                prompt.Append("The size of the document generated must proportionally scale the dimensions of the site the attribute Room Dimensions. The Volunteer Flow (path for volunteers to enter) and Supply Flow should be labeled with arrows");
}
            prompt.Append("Attached below is a json object with information and notes about the site to consider while creating the venue.");

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
                    "I just wanted text context and considerations for the blueprint. The response must be structured based on the give format. " +
                    "ImageInstruction must be under 1200 characters, imperative placement directions only, stating literal counts of lines and rows, " +
                    "no json, no ids, no dates, no explanation, and no instruction to draw the room itself");
            }


            return prompt.ToString();
        }

        

        public async Task<SitePlanDraft> askAI(SitePlanRequest req)
        {


#pragma warning disable OPENAI001
            //Prepare context from folder
            var contextImages = new List<ResponseContentPart>();
            string beforeFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "context", "before");
            string afterFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "context", "after");
            var beforefiles = Directory.GetFiles(beforeFolder);
            var afterFiles = Directory.GetFiles(afterFolder);

            foreach (string file in beforefiles)
            {
                byte[] fileBytes = File.ReadAllBytes(file);
                contextImages.Add(ResponseContentPart.CreateInputTextPart(
                        "Before example " + Path.GetFileName(file)));
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
            foreach (string file in afterFiles)
            {
                byte[] fileBytes = File.ReadAllBytes(file);
                contextImages.Add(ResponseContentPart.CreateInputTextPart(
                        "After example " + Path.GetFileName(file)));
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
                        ""ImageInstruction"" : {""type"":""string""}
                },
                ""required"": [""SiteOverview"",""RecommendedLayout"",""Timeline"",""Risks"",""PMReviewChecklist"", ""ImageInstruction""],
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
                        var file1 = taskPDFtoJPG.AddFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints", "ogInput", req.RoomBlueprintFilePath));
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
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints", "ogInput", req.RoomBlueprintFilePath);

                        pngBytes = File.ReadAllBytes(fullPath);
                    }
                }
                using (MemoryStream stream = new MemoryStream(pngBytes))
                {

                    image = await imageClient.GenerateImageEditAsync(image: stream, "blueprint" + req.RoomBlueprintFilePath, prompt: GeneratePrompt(req, null, true, "", imageInstructions), options: new ImageEditOptions { Size = GeneratedImageSize.W1024xH1536, Quality = GeneratedImageQuality.HighQuality, InputFidelity = ImageInputFidelity.High });
                }
            }
            
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