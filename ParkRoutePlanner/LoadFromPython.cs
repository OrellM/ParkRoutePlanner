using System.Net;
using System.Text.Json;

public static class LoadManager
{
    // מזהה הקובץ בגוגל דרייב
    private static readonly string driveFileId = "1u-ZhXc5WHb0aNk8H6s5LvyDXy4PrN3U-";
    // פורמט השם לקובץ המקומי - עם תאריך
    private static readonly string localPath = "loads-{0}.json";

    // משתנה לשמירת הנתונים הטעונים בזיכרון
    public static Dictionary<string, Dictionary<string, double>> loadsData;

    // משתנה לשמירת תאריך הטעינה האחרון
    private static DateTime? lastLoadDate = null;

    // פונקציה לטעינת הנתונים - מורידה רק פעם ביום
    public static void LoadDailyLoads()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");

        // אם כבר טענת את הנתונים היום, דלג על הטעינה מחדש
        if (lastLoadDate.HasValue && lastLoadDate.Value.Date == DateTime.Today)
        {
            Console.WriteLine("[LoadManager] Data already loaded today. Skipping reload.");
            return;
        }

        string fileName = string.Format(localPath, today);

        // אם הקובץ לא קיים במחשב, הורד אותו מהדרייב
        if (!File.Exists(fileName))
        {
            Console.WriteLine("[LoadManager] File not found locally. Downloading from Drive...");
            DownloadFromDrive(fileName);
        }

        // קרא את תוכן הקובץ למחרוזת JSON
        string json = File.ReadAllText(fileName);

        // המרת JSON למבנה נתונים של מילון בתוך מילון
        loadsData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json);

        Console.WriteLine($"[LoadManager] Loaded data from {fileName}");

        // עדכן את תאריך הטעינה האחרון ליום הנוכחי
        lastLoadDate = DateTime.Today;
    }

    // פונקציה להורדת הקובץ מגוגל דרייב
    private static void DownloadFromDrive(string fileName)
    {
        string url = $"https://drive.google.com/uc?export=download&id={driveFileId}";
        using WebClient client = new WebClient();
        client.DownloadFile(url, fileName);
    }

    // פונקציה לקבלת עומס עתידי למתקן מסוים במרחק זמן מסוים מדקות מהיום

}
