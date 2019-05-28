using strange.extensions.command.impl;

namespace strange.unittests
{
    public class CommandWithInjection : Command
    {
        [Inject] public ISimpleInterface injected { get; set; }

        public override void Execute()
        {
            injected.intValue = 100;
        }
    }
}