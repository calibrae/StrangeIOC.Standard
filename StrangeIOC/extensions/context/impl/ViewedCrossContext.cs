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
 * @class strange.extensions.context.impl.CrossContext
 * 
 * Provides the capabilities that allow a Context to communicate across
 * the Context boundary. Specifically, CrossContext provides
 * - A CrossContextInjectionBinder that allows injections to be shared cross-context
 * - An EventDispatcher that allows messages to be sent between Contexts
 * - Methods (the ICrossContextCapable API) for adding and removing the hooks between Contexts.
 */

using strange.extensions.context.api;
using strange.extensions.dispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.impl;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.framework.api;

namespace strange.extensions.context.impl
{
    public class ViewedCrossContext : CrossContext, IViewedContext, ICrossContextCapable
    {
        private IBinder _crossContextBridge;

        /// A specific instance of EventDispatcher that communicates 
        /// across multiple contexts. An event sent across this 
        /// dispatcher will be re-dispatched by the various context-wide 
        /// dispatchers. So a dispatch to other contexts is simply 
        /// 
        /// `crossContextDispatcher.Dispatch(MY_EVENT, payload)`;
        /// 
        /// Other contexts don't need to listen to the cross-context dispatcher
        /// as such, just map the necessary event to your local context
        /// dispatcher and you'll receive it.
        protected IEventDispatcher _crossContextDispatcher;

        private ICrossContextInjectionBinder _injectionBinder;


        public ViewedCrossContext(object view) : this(view, ContextStartupFlags.AUTOMATIC)
        {
        }


        public ViewedCrossContext(object view, ContextStartupFlags flags)
        {
            //If firstContext was unloaded, the contextView will be null. Assign the new context as firstContext.
            if (firstContext == null || firstContext is IViewedContext context && context.GetContextView() == null)
            {
                firstContext = this;
            }
            else
            {
                firstContext.AddContext(this);
            }

            SetContextView(view);
            addCoreComponents();
            autoStartup = (flags & ContextStartupFlags.MANUAL_LAUNCH) != ContextStartupFlags.MANUAL_LAUNCH;
            if ((flags & ContextStartupFlags.MANUAL_MAPPING) != ContextStartupFlags.MANUAL_MAPPING)
            {
                Start();
            }
        }


        public ViewedCrossContext(object view, bool autoMapping) : this(view,
            autoMapping
                ? ContextStartupFlags.MANUAL_MAPPING
                : ContextStartupFlags.MANUAL_LAUNCH | ContextStartupFlags.MANUAL_MAPPING)
        {
        }

        /// The top of the View hierarchy.
        /// In MVCSContext, this is your top-level GameObject
        public object contextView { get; set; }

        public virtual IBinder crossContextBridge
        {
            get
            {
                if (_crossContextBridge == null)
                {
                    _crossContextBridge = injectionBinder.GetInstance<CrossContextBridge>();
                }

                return _crossContextBridge;
            }
            set => _crossContextDispatcher = value as IEventDispatcher;
        }

        /// A Binder that handles dependency injection binding and instantiation
        public ICrossContextInjectionBinder injectionBinder
        {
            get => _injectionBinder ?? (_injectionBinder = new CrossContextInjectionBinder());
            set => _injectionBinder = value;
        }

        public virtual void AssignCrossContext(ICrossContextCapable childContext)
        {
            childContext.crossContextDispatcher = crossContextDispatcher;
            childContext.injectionBinder.CrossContextBinder = injectionBinder.CrossContextBinder;
        }

        public virtual void RemoveCrossContext(ICrossContextCapable childContext)
        {
            if (childContext.crossContextDispatcher != null)
            {
                (childContext.crossContextDispatcher as ITriggerProvider).RemoveTriggerable(
                    childContext.GetComponent<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER) as ITriggerable);
                childContext.crossContextDispatcher = null;
            }
        }

        public virtual IDispatcher crossContextDispatcher
        {
            get => _crossContextDispatcher;
            set => _crossContextDispatcher = value as IEventDispatcher;
        }

        public virtual object GetContextView()
        {
            return contextView;
        }

        public override IContext AddContext(IContext context)
        {
            base.AddContext(context);
            if (context is ICrossContextCapable)
            {
                AssignCrossContext((ICrossContextCapable) context);
            }

            return this;
        }

        public override IContext RemoveContext(IContext context)
        {
            if (context is ICrossContextCapable)
            {
                RemoveCrossContext((ICrossContextCapable) context);
            }

            return base.RemoveContext(context);
        }

        protected override void addCoreComponents()
        {
            base.addCoreComponents();
            if (injectionBinder.CrossContextBinder == null
            ) //Only null if it could not find a parent context / firstContext
            {
                injectionBinder.CrossContextBinder = new CrossContextInjectionBinder();
            }

            if (firstContext == this)
            {
                injectionBinder.Bind<IEventDispatcher>().To<EventDispatcher>().ToSingleton()
                    .ToName(ContextKeys.CROSS_CONTEXT_DISPATCHER).CrossContext();
                injectionBinder.Bind<CrossContextBridge>().ToSingleton().CrossContext();
            }
        }

        protected override void instantiateCoreComponents()
        {
            base.instantiateCoreComponents();

            var dispatcherBinding = injectionBinder.GetBinding<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);

            if (dispatcherBinding != null)
            {
                var dispatcher = injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);

                if (dispatcher != null)
                {
                    crossContextDispatcher =
                        injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CROSS_CONTEXT_DISPATCHER);
                    (crossContextDispatcher as ITriggerProvider).AddTriggerable(dispatcher as ITriggerable);
                    (dispatcher as ITriggerProvider).AddTriggerable(crossContextBridge as ITriggerable);
                }
            }
        }

        /// Set the object that represents the top of the ViewedContext hierarchy.
        /// In MVCSContext, this would be a GameObject.
        public virtual IContext SetContextView(object view)
        {
            contextView = view;
            return this;
        }
    }
}