using strange.extensions.signal.impl;

namespace strange.unittests
{
    public class CommandWithInjectionAndSignal : CommandWithInjection
    {
        [Inject] public Signal<SimpleInterfaceImplementer> signal { get; set; }

        public override void Execute()
        {
            injected.intValue = 100;
            signal.Dispatch(injected as SimpleInterfaceImplementer);
        }
    }
}