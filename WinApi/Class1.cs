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
        public string Id { get; set; }
        public int Saas { get; set; }
        public int Inst { get; set; }
    }

    public class Filters
    {
        public Policy Policy { get; set; }

        [JsonProperty("owner.entity")]
        public OwnerEntity OwnerEntity { get; set; }
    }

    public class OwnerEntity
    {
        public List<Eq> Eq { get; set; }
    }

    public class Policy
    {
        public List<string> Cabinetmatchedrulesequals { get; set; }
    }

    public class RootRequest
    {
        public Filters Filters { get; set; }

        public int Skip { get; set; }
    }

    public class RootResponse
    {
        public List<ResponseRecord> Data { get; set; }
        public bool HasNext { get; set; }
        public int Max { get; set; }
        public int Total { get; set; }
        public bool MoreThanTotal { get; set; }
    }

    public class ResponseRecord
    {
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public string AppName { get; set; }
        public string AlternateLink { get; set; }
        public string CreatedDate { get; set; }
        public string FilePath { get; set; }
    }
}


