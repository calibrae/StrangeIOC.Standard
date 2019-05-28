namespace strange.unittests
{
    public class HasANamedInjection
    {
        [Inject(SomeEnum.ONE)] public ClassToBeInjected injected { get; set; }
    }
}