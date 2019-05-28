/*
 * Copyright 2013 ThirdMotion, Inc.
 *
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *
 *		http://www.apache.org/licenses/LICENSE-2.0
 *
 *		Unless required by applicable law or agreed to in writing, software
 *		distributed under the License is distributed on an "AS IS" BASIS,
 *		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *		See the License for the specific language governing permissions and
 *		limitations under the License.
 */

/**
 * @class strange.extensions.sequencer.impl.Sequencer
 * 
 * @deprecated
 */

using System;
using strange.extensions.command.impl;
using strange.extensions.dispatcher.api;
using strange.extensions.sequencer.api;
using strange.framework.api;

namespace strange.extensions.sequencer.impl
{
    public class Sequencer : CommandBinder, ISequencer, ITriggerable
    {
        public override IBinding GetRawBinding()
        {
            return new SequenceBinding(resolver);
        }

        public override void ReactTo(object key, object data)
        {
            var binding = GetBinding(key) as ISequenceBinding;
            if (binding != null)
            {
                nextInSequence(binding, data, 0);
            }
        }

        public void ReleaseCommand(ISequenceCommand command)
        {
            if (command.retain == false)
            {
                if (activeSequences.ContainsKey(command))
                {
                    var binding = activeSequences[command] as ISequenceBinding;
                    var data = command.data;
                    activeSequences.Remove(command);
                    nextInSequence(binding, data, command.sequenceId + 1);
                }
            }
        }

        public new virtual ISequenceBinding Bind<T>()
        {
            return base.Bind<T>() as ISequenceBinding;
        }

        public new virtual ISequenceBinding Bind(object value)
        {
            return base.Bind(value) as ISequenceBinding;
        }

        private void removeSequence(ISequenceCommand command)
        {
            if (activeSequences.ContainsKey(command))
            {
                command.Cancel();
                activeSequences.Remove(command);
            }
        }

        private void invokeCommand(Type cmd, ISequenceBinding binding, object data, int depth)
        {
            var command = createCommand(cmd, data);
            command.sequenceId = depth;
            trackCommand(command, binding);
            executeCommand(command);
            ReleaseCommand(command);
        }

        /// Instantiate and Inject the ISequenceCommand.
        protected new virtual ISequenceCommand createCommand(object cmd, object data)
        {
            injectionBinder.Bind<ISequenceCommand>().To(cmd);
            var command = injectionBinder.GetInstance<ISequenceCommand>();
            command.data = data;
            injectionBinder.Unbind<ISequenceCommand>();
            return command;
        }

        private void trackCommand(ISequenceCommand command, ISequenceBinding binding)
        {
            activeSequences[command] = binding;
        }

        private void executeCommand(ISequenceCommand command)
        {
            if (command == null)
            {
                return;
            }

            command.Execute();
        }

        private void nextInSequence(ISequenceBinding binding, object data, int depth)
        {
            var values = binding.value as object[];
            if (depth < values.Length)
            {
                var cmd = values[depth] as Type;
                invokeCommand(cmd, binding, data, depth);
            }
            else
            {
                if (binding.isOneOff)
                {
                    Unbind(binding);
                }
            }
        }

        private void failIf(bool condition, string message, SequencerExceptionType type)
        {
            if (condition)
            {
                throw new SequencerException(message, type);
            }
        }
    }
}