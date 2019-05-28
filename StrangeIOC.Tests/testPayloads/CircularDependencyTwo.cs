namespace strange.unittests
{
    public class CircularDependencyTwo
    {
        [Inject] public CircularDependencyOne depends { get; set; }
    }
}