namespace strange.unittests
{
    public class PostConstructSimple
    {
        public static int PostConstructCount { get; set; }

        [PostConstruct]
        public void MultiplyBy2()
        {
            PostConstructCount++;
        }
    }
}