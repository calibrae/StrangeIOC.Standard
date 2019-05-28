namespace strange.unittests
{
    public class HasANamedInjection2
    {
        [Inject(SomeEnum.ONE)] public ClassToBeInjected injected { get; set; }
    }
}