using System.Text.Json;

namespace Task3_ChatApp_vSem3
{
    internal class Message
    {
        public string? FromName { get; set; }
        public DateTime Date { get; set; }
        public string? Text { get; set; }

        // Метод для сериализации в JSON
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        // Статический метод для десериализации JSON в объект Message
        public static Message? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Message>(json);
        }

    }
}