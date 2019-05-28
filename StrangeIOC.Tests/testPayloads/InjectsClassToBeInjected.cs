namespace strange.unittests
{
    public class InjectsClassToBeInjected
    {
        [Inject] public ClassToBeInjected injected { get; set; }
    }
}