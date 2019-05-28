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
 * @class strange.extensions.mediation.impl.MediationBinder
 * 
 * Binds Views to Mediators.
 * 
 * Please read strange.extensions.mediation.api.IMediationBinder
 * where I've extensively explained the purpose of View mediation
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.framework.api;
using Xamarin.Forms;
using Binder = strange.framework.impl.Binder;

namespace strange.extensions.mediation.impl
{
    //TODO remove the view model mapping since the mediator is perfectly able to be the view model 
    public class MediationBinder : Binder, IMediationBinder
    {
        private readonly Dictionary<Element, List<IMediator>> _boundMediators =
            new Dictionary<Element, List<IMediator>>();

        [Inject] public IInjectionBinder injectionBinder { get; set; }


        public override IBinding GetRawBinding()
        {
            return new MediationBinding(resolver);
        }

        public void Trigger(MediationEvent evt, Element view)
        {
            if (view == null)
            {
                return;
            }

//            Debug.WriteLine("Trigger for " + view.GetType());
            var viewType = view.GetType();
            if (viewType.ToString() == "TimeKeeper.SleepAgendaView")
            {
                Debug.WriteLine("What the fuck again");
            }

            var binding = GetBinding(viewType) as IMediationBinding;
            if (binding != null)
            {
                switch (evt)
                {
                    case MediationEvent.AWAKE:
                        injectViewAndChildren(view, binding.NoChildrenSwitch);
                        MapView(view, binding);
                        break;
                    case MediationEvent.DESTROYED:
                        TriggerSubChildren(viewChildren(view), MediationEvent.DESTROYED);
                        unmapView(view);
                        break;
                }
            }
            else if (evt == MediationEvent.AWAKE)
            {
                //Even if not mapped, Views (and their children) have potential to be injected
                injectViewAndChildren(view);
            }
        }

        public new IMediationBinding Bind<T>()
        {
            return base.Bind<T>() as IMediationBinding;
        }

        IMediationBinding IMediationBinder.BindView<T>()
        {
            return base.Bind<T>() as IMediationBinding;
        }

        /// Initialize all IViews within this view
        protected virtual void injectViewAndChildren(Element view, bool noChildren = false)
        {
            injectionBinder.injector.Inject(view, false);
            if (!noChildren)
            {
                TriggerSubChildren(viewChildren(view));
            }
        }

        protected virtual IEnumerable<View> viewChildren(Element view)
        {
            if (view is ContentPage)
            {
                var child = new List<View>();
                child.Add((view as ContentPage).Content);
                return child;
            }

            if (view is ScrollView)
            {
                var child = new List<View>();
                child.Add((view as ScrollView).Content);
                return child;
            }

            if (view is ContentView)
            {
                var child = new List<View>();
                child.Add((view as ContentView).Content);
                return child;
            }

            if (view is Layout<View>)
            {
                return (view as Layout<View>).Children;
            }

            if (view is ViewCell)
            {
                var child = new List<View>();
                child.Add((view as ViewCell).View);
                return child;
            }

            return new List<View>();
        }

        protected virtual void TriggerSubChildren(IEnumerable<View> views, MediationEvent evt = MediationEvent.AWAKE)
        {
            foreach (var view in views)
            {
                if (view == null)
                {
                    continue;
                }

                Trigger(evt, view);
            }
        }

        /// Creates and registers one or more Mediators for a specific View instance.
        /// Takes a specific View instance and a binding and, if a binding is found for that type, creates and registers a Mediator.
        protected virtual void MapView(Element view, IMediationBinding binding)
        {
            var viewType = view.GetType();
            _boundMediators.Add(view, new List<IMediator>());

            if (bindings.ContainsKey(viewType))
            {
                var values = binding.value as object[];
                var aa = values.Length;
                for (var a = 0; a < aa; a++)
                {
                    var mediatorType = values[a] as Type;
                    if (mediatorType == viewType)
                    {
                        throw new MediationException(
                            viewType + "mapped to itself. The result would be a stack overflow.",
                            MediationExceptionType.MEDIATOR_VIEW_STACK_OVERFLOW);
                    }

                    IMediator mediator = null;
                    Type viewModelTypeToInject = null;

                    //handling view model
                    if (view.BindingContext != null)
                    {
                        viewModelTypeToInject = binding.viewModel == null ||
                                                binding.viewModel.Equals(BindingConst.NULLOID)
                            ? null
                            : binding.viewModel as Type;
                        if (view.BindingContext is IMediator)
                        {
                            // binding contexts are cascading from parent to child.. We dont want that.
                            if (view.Parent == null || view.BindingContext != view.Parent.BindingContext)
                            {
                                // the view model is the mediator itself...
                                mediator = view.BindingContext as IMediator;
                                if (viewModelTypeToInject != null)
                                {
                                    throw new MediationException(
                                        "Trying to inject a viewmodel type while the view model type is itself the mediator",
                                        MediationExceptionType.MEDIATOR_VIEW_STACK_OVERFLOW);
                                }
                            }
                        }
                        else
                        {
                            var bindingContextType = view.BindingContext.GetType();
                            if (viewModelTypeToInject != null)
                            {
                                if (
                                    !viewModelTypeToInject.GetTypeInfo()
                                        .IsAssignableFrom(bindingContextType.GetTypeInfo()))
                                {
                                    throw new MediationException(
                                        "The declared ViewModel in the View of type " + bindingContextType +
                                        " is not assignable from " + viewModelTypeToInject,
                                        MediationExceptionType.VIEWMODEL_NOT_ASSIGNABLE);
                                }
                            }
                            else
                            {
                                viewModelTypeToInject = bindingContextType;
                            }

                            injectionBinder.Bind(viewModelTypeToInject).ToValue(view.BindingContext);
                        }
                    }

                    if (mediator == null)
                    {
                        mediator = (IMediator) Activator.CreateInstance(mediatorType);
                    }

                    mediator.PreRegister();

                    var typeToInject = binding.abstraction == null || binding.abstraction.Equals(BindingConst.NULLOID)
                        ? viewType
                        : binding.abstraction as Type;
                    injectionBinder.Bind(typeToInject).ToValue(view).ToInject(false);
                    injectionBinder.injector.Inject(mediator);
                    injectionBinder.Unbind(typeToInject);
                    if (viewModelTypeToInject != null)
                    {
                        injectionBinder.Unbind(viewModelTypeToInject);
                    }

                    // handling case where binding context is not set at all 
                    if (binding.ForceBindingContextSwitch)
                    {
                        view.BindingContext = mediator;
                    }

                    mediator.OnRegister();
                    _boundMediators[view].Add(mediator);
                }
            }
        }

        /// Removes a mediator when its view is destroyed
        protected virtual void unmapView(Element view)
        {
            if (!_boundMediators.ContainsKey(view)) return;
            var mediators = _boundMediators[view];
            if (mediators == null)
            {
                return;
            }

            foreach (var mediator in mediators)
            {
                mediator.OnRemove();
            }

            _boundMediators.Remove(view);
        }
    }
}