using System.Text.Json.Serialization;

namespace M_B_Bannerlord_ModPackTool.Functions
{
    public class NexusUser
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("is_premium?")]
        public bool IsPremium { get; set; }

        [JsonPropertyName("is_supporter?")]
        public bool IsSupporter { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("profile_url")]
        public string ProfileUrl { get; set; }

        [JsonPropertyName("is_supporter")]
        public bool IsSupporterAlt { get; set; }

        [JsonPropertyName("is_premium")]
        public bool IsPremiumAlt { get; set; }
    }
}
