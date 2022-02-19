namespace AppRegAppTests.Models
{
    //public class PrelimCertModel : CertificateModel
    public class PrelimCertModel
    {
        public PrelimCertModel()
        {
            DeficiencyDetails = new List<DeficiencyDetail>();
        }
        public int DeficiencyId { get; set; }        
        public string VesselName { get; set; }
        public string PortOfRegistry { get; set; }
        public string IMO { get; set; }        
        public string PortOfIssue { get; set; }
        public string PortOfLoading { get; set; }
        public string IssueDate { get; set; }
        public string IssueLocalTime { get; set; }        
        public string TypeOfCargo { get; set; }
        public string InspectorName { get; set; }
        public List<DeficiencyDetail> DeficiencyDetails { get; set; }        
    }
}

