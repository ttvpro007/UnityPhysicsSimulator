namespace ViTiet.DataStructure
{
    public static class IDGenerator
    {
        public static string GenerateID(int count, int depth, bool isLeft)
        {
            return "" + count + "-" + depth + "-" + (isLeft ? "L" : "R");
        }

        public static string GenerateRootID()
        {
            return "" + 0 + "-" + 0 + "-" + "M";
        }
    }
}