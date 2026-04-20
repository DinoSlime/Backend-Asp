using System.Text.Json.Serialization;

namespace Backend.DTOs
{
    public class OrderResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // Gộp logic ánh xạ total_money và totalMoney vào đây
        [JsonPropertyName("total_money")]
        public long TotalPrice { get; set; }

        // C# không dùng setter alias như Java, ta dùng thuộc tính bổ trợ nếu cần
        [JsonIgnore]
        public long TotalMoney { get => TotalPrice; set => TotalPrice = value; }
    }
}