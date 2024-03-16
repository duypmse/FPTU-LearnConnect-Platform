using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BAL.Models
{
    public partial class FlagDetail
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int? AtTime { get; set; }
        public int ContentModerationId { get; set; }
        [JsonIgnore]
        public virtual ContentModeration ContentModeration { get; set; } = null!;
    }
}
