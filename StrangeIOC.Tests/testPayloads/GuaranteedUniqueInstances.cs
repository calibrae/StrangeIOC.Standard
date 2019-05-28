namespace strange.unittests
{
    public class GuaranteedUniqueInstances
    {
        private static int counter;

        public GuaranteedUniqueInstances()
        {
            uid = ++counter;
        }

        public int uid { get; set; }
    }
}