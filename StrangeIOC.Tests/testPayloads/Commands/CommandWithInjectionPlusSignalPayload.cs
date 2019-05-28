using strange.extensions.command.impl;

namespace strange.unittests
{
    public class CommandWithInjectionPlusSignalPayload : Command
    {
        [Inject] public ISimpleInterface injected { get; set; }

        [Inject] public int intValue { get; set; }

        public override void Execute()
        {
            injected.intValue = 100;
        }
    }
}