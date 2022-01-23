using System;
using Newtonsoft.Json;

namespace HttpContainerApp
{
    public class CallbackModel
    {

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("basePath")]
        public string BasePath { get; set; }

        [JsonProperty("queries")]
        public QueriesModel Queries { get; set; }

    }

    public class QueriesModel
    {    

        [JsonProperty("sig")]
        public string Signature { get; set; }

    }

    public class ZipModel
    {

        [JsonProperty("zip")]
        public string Zip { get; set; }

    }
}
