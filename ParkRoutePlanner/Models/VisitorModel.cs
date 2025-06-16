using System.Text.Json.Serialization;

public class VisitorModel
{
    public int Age { get; set; }
    public double Height { get; set; }  // ���� ��: double, �� int
    public List<string> PreferredCategories { get; set; }
    public string VisitStartTime { get; set; }
    public string VisitEndTime { get; set; }
    public List<string> PreferredAttractions { get; set; }

    [JsonPropertyName("excludeList")]
    public List<string> ExcludedAttractions { get; set; } = new();

}
