using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApi
{
    public class Eq
    {
        public string id { get; set; }
        public int saas { get; set; }
        public int inst { get; set; }
    }

    public class Filters
    {
        public Policy policy { get; set; }

        [JsonProperty("owner.entity")]
        public OwnerEntity OwnerEntity { get; set; }
    }

    public class OwnerEntity
    {
        public List<Eq> eq { get; set; }
    }

    public class Policy
    {
        public List<string> cabinetmatchedrulesequals { get; set; }
    }

    public class RootRequest
    {
        public Filters filters { get; set; }

        public int skip { get; set; }
    }

    public class RootResponse
    {
        public List<ResponseRecord> data { get; set; }
        public bool hasNext { get; set; }
        public int max { get; set; }
        public int total { get; set; }
        public bool moreThanTotal { get; set; }
    }

    public class ResponseRecord
    {
        public string name { get; set; }
        public string ownerName { get; set; }
        public string appName { get; set; }
        public string alternateLink { get; set; }
        public string createdDate { get; set; }
        public string filePath { get; set; }
        public string[] fTags { get; set; }
    }

}


