using strange.extensions.command.impl;

namespace strange.unittests
{
    public class MarkablePoolCommand : Command
    {
        public static int incrementValue;


        public override void Execute()
        {
            incrementValue++;
        }
    }
}