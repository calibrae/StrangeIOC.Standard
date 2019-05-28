using System;
using strange.extensions.command.impl;

namespace strange.unittests
{
    public class CommandThrowsErrorIfEventIsNull : EventCommand
    {
        public static int timesExecuted;

        public static int result;

        public override void Execute()
        {
            if (evt == null)
            {
                throw new Exception("CommandThrowsErrorIfEventIsNull had a null event");
            }

            timesExecuted++;

            var evtData = (int) evt.data;
            result = evtData * 2;
        }
    }
}