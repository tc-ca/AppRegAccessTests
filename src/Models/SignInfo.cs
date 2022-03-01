using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace AppRegAppTests.Models
{
    public class SignInfo
    {
        private string emailSubject = "Please sign this certificate";
        private int maxSubjectLength;

        public SignInfo()
        {
        }
        public int NumberOfDocuments { get; set; }
        public string SendersName { get; set; }
        public string SenderEmail { get; set; }
        public string SignersName { get; set; }
        public string SignersEmail { get; set; }
        public string[] HtmlContent { get; set; }
        public int[] SignatureXPosition { get; set; }
        public int[] SignatureYPosition { get; set; }
        public string[] DocumentName { get; set; }
        public string EmailSubject
        {
            get { return emailSubject; }
            set
            {                
                maxSubjectLength = 100;
                emailSubject = value.Length < maxSubjectLength ? value : value.Substring(0, maxSubjectLength - 4) + "...";
            }
        }
        //public Uri ReturnUrl { get; set; }
        public string ReturnUrl { get; set; }
    }
}
