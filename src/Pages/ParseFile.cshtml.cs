using AutoFixture;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using AppRegAccessTests.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AppRegAppTests.Models;

namespace AppRegAccessTests.Pages
{
    public class ParseFileModel : PageModel
    {
        private const string DeckAreaKey = "DeckArea";
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly string _fileName;
        public ParseFileModel(IWebHostEnvironment webHostEnv)
        {
            _webHostEnv = webHostEnv;
            _fileName = Path.Combine(_webHostEnv.WebRootPath, "doc", "PreliminaryCertificate.html");
        }

        public string FileName { get; private set; }
        public string FileContent { get; private set; }

        public void OnGet()
        {
            FileName = _fileName;
            //FileContent = GetFileContent(_fileName);

            var fixture = new Fixture();
            PrelimCertModel model = fixture.Create<PrelimCertModel>();
            
            Random rnd = new Random();
            int index = rnd.Next(0, 2);

            model.TypeOfCargo = new string[] { "Concentrates", "Grain", "Timber" }[index];
            fixture.AddManyTo<DeficiencyDetail>(model.DeficiencyDetails, rnd.Next(1, 20));
            IDictionary<string, string> modelDict = GetPrelimModelDictionary(model);

            var outputFilename = _fileName.Replace(".html", $"_def_{model.DeficiencyId}.html");
            string certHtmlString = GetCertificateHtmlString(_fileName, modelDict, outputFilename);

            FileContent = certHtmlString;// String.Join("\n", 
                //modelDict.Select(kvp => { return $"{kvp.Key}: {kvp.Value}"; }).ToArray());
        }

        private string GetCertificateHtmlString(string inputFilename, IDictionary<string, string> model, string outputFilename)
        {
            string content = "";
            HtmlDocument doc = new HtmlDocument();
            doc.Load(inputFilename);
            string nodeSelector;
            //var htmlNode = new HtmlNode();

            foreach (var kvp in model)
            {
                if (kvp.Key == nameof(PrelimCertModel.TypeOfCargo)) // set cargo type radio
                {
                    nodeSelector = $"//input[@id='{kvp.Value}']";
                    HtmlNode targetNode = doc.DocumentNode.SelectSingleNode(nodeSelector);
                    if (targetNode != null) targetNode.SetAttributeValue("checked", "true");
                }
                else if (kvp.Key.StartsWith(DeckAreaKey))
                {
                    nodeSelector = "//tbody[@id='deficiencies']";
                    HtmlNode targetNode = doc.DocumentNode.SelectSingleNode(nodeSelector);
                    if (targetNode != null) {
                        HtmlNode deficiencyRowNode = HtmlNode.CreateNode(kvp.Value);
                        targetNode.AppendChild(deficiencyRowNode);
                    }
                }
                else
                {
                    nodeSelector = $"//b[@name='{kvp.Key}']";
                    HtmlNode targetNode = doc.DocumentNode.SelectSingleNode(nodeSelector);
                    if (targetNode != null) targetNode.InnerHtml = kvp.Value; 
                }
            }

            if(!string.IsNullOrEmpty(outputFilename)) doc.Save(outputFilename);
            content = doc.Text;
            
            return content;
        }

        private IDictionary<string, string> GetPrelimModelDictionary(PrelimCertModel model)
        {
            var dict = new Dictionary<string, string>();

            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop.Name == nameof(PrelimCertModel.DeficiencyDetails))
                {
                    var key = "";
                    var rowHtml = "";
                    var deckArea = "";
                    var requirements = "";
                    //style="height: 425px"
                    int rowHeight = 425/ model.DeficiencyDetails.Count;

                    for (int i = 0; i < model.DeficiencyDetails.Count; i++)
                    {
                        deckArea = model.DeficiencyDetails[i].DeckArea;
                        requirements = model.DeficiencyDetails[i].Requirements;

                        key = $"{DeckAreaKey}_{i}";
                        rowHtml = $"<tr style=\"border:none;height:{rowHeight}px;\" >" +
                                    $"<td scope=\"col\"><b>{deckArea}</b></td>" +
                                    $"<td scope=\"col\"><b>{requirements}</b></td>" +    
                                  "</tr>";
                 
                        dict.Add(key, rowHtml);
                    }
                }
                else
                {
                    dict.Add(prop.Name, prop.GetValue(model, null)?.ToString());
                }
            }

            return dict;
        }

        private string GetFileContent(string fileName)
        {
            string content = "";
            HtmlDocument doc = new HtmlDocument();
            doc.Load(fileName);

            HtmlNode htmlNode = doc.DocumentNode.SelectSingleNode("//b[@name='VesselName']");

            // test writing
            htmlNode.InnerHtml = "Written Vessel Name Value";

            content = htmlNode.InnerHtml;

            //doc.Save(fileName);
            return content; // doc.Text;
        }
    }
}
