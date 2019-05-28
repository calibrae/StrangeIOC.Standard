using strange.extensions.sequencer.impl;

namespace strange.unittests
{
    public class SequenceCommandWithInjection : SequenceCommand
    {
        [Inject] public ISimpleInterface injected { get; set; }

        public override void Execute()
        {
            injected.intValue = 100;
        }
    }
}