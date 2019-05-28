namespace strange.unittests
{
    public class PostConstructClass
    {
        [Inject] public float floatVal { get; set; }

        [PostConstruct]
        public void MultiplyBy2()
        {
            floatVal *= 2f;
        }
    }
}