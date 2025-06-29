namespace ParkRoutePlanner
{
    public static class ParkLoadTracker
    {
        // ������ ������ ��� ������ �� ��� ���� ��� ���
        public static Dictionary<int, Dictionary<int, int>> DynamicLoadMatrix { get; set; } = new();

        // ����� ������� �������
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

        // ��������� � ����� �� �� ������� (����� ����� ����� ����)
        public static void ResetMatrix()
        {
            DynamicLoadMatrix.Clear();
        }
    }
}
