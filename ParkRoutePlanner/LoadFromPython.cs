using System.Net;
using System.Text.Json;

public static class LoadManager
{
    // ���� ����� ����� �����
    private static readonly string driveFileId = "1u-ZhXc5WHb0aNk8H6s5LvyDXy4PrN3U-";
    // ����� ��� ����� ������ - �� �����
    private static readonly string localPath = "loads-{0}.json";

    // ����� ������ ������� ������� �������
    public static Dictionary<string, Dictionary<string, double>> loadsData;

    // ����� ������ ����� ������ ������
    private static DateTime? lastLoadDate = null;

    // ������� ������ ������� - ������ �� ��� ����
    public static void LoadDailyLoads()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");

        // �� ��� ���� �� ������� ����, ��� �� ������ ����
        if (lastLoadDate.HasValue && lastLoadDate.Value.Date == DateTime.Today)
        {
            Console.WriteLine("[LoadManager] Data already loaded today. Skipping reload.");
            return;
        }

        string fileName = string.Format(localPath, today);

        // �� ����� �� ���� �����, ���� ���� �������
        if (!File.Exists(fileName))
        {
            Console.WriteLine("[LoadManager] File not found locally. Downloading from Drive...");
            DownloadFromDrive(fileName);
        }

        // ��� �� ���� ����� ������� JSON
        string json = File.ReadAllText(fileName);

        // ���� JSON ����� ������ �� ����� ���� �����
        loadsData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json);

        Console.WriteLine($"[LoadManager] Loaded data from {fileName}");

        // ���� �� ����� ������ ������ ���� ������
        lastLoadDate = DateTime.Today;
    }

    // ������� ������ ����� ����� �����
    private static void DownloadFromDrive(string fileName)
    {
        string url = $"https://drive.google.com/uc?export=download&id={driveFileId}";
        using WebClient client = new WebClient();
        client.DownloadFile(url, fileName);
    }

    // ������� ����� ���� ����� ����� ����� ����� ��� ����� ����� �����

}
