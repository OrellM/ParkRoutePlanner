namespace ParkRoutePlanner
{
    public static class ParkLoadTracker
    {
        // מטריצה ששומרת כמה מבקרים יש בכל מתקן לפי שעה
        public static Dictionary<int, Dictionary<int, int>> DynamicLoadMatrix { get; set; } = new();

        // אתחול המטריצה לראשונה
        public static void InitializeMatrix(int attractionCount)
        {
            for (int i = 0; i < attractionCount; i++)
            {
                DynamicLoadMatrix[i] = new Dictionary<int, int>();
                for (int hour = 10; hour <= 21; hour++)
                {
                    DynamicLoadMatrix[i][hour] = 0;
                }
            }
        }

        // אופציונלי – איפוס של כל המטריצה (למקרה שתרצי לאתחל מחדש)
        public static void ResetMatrix()
        {
            DynamicLoadMatrix.Clear();
        }
    }
}
