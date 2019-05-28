namespace strange.unittests
{
    public class CircularDependencyOne
    {
        [Inject] public CircularDependencyTwo depends { get; set; }
    }
}