namespace strange.unittests
{
    public class NonpublicInjection
    {
        [Inject] public InjectableSuperClass injectionOne { get; set; }

        // Oops! Forgot to mark as public
        [Inject] private string injectionTwo { get; set; }
    }
}