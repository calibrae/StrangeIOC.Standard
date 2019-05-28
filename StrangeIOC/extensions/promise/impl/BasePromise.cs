﻿/*
 * Copyright 2015 StrangeIoC
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
 * @class strange.extensions.promise.impl.BasePromise
 *
 * @see strange.extensions.promise.api.IPromise
 */

using System;
using System.Linq;
using strange.extensions.promise.api;

namespace strange.extensions.promise.impl
{
    public abstract class BasePromise : IBasePromise
    {
        public enum PromiseState
        {
            Fulfilled,
            Failed,
            Pending
        }

        private Exception exception;

        protected BasePromise()
        {
            State = PromiseState.Pending;
        }

        protected bool Pending => State == PromiseState.Pending;
        protected bool Resolved => State != PromiseState.Pending;
        protected bool Fulfilled => State == PromiseState.Fulfilled;
        protected bool Failed => State == PromiseState.Failed;

        public PromiseState State { get; protected set; }

        public void ReportFail(Exception ex)
        {
            exception = ex;
            State = PromiseState.Failed;
            if (OnFail != null)
                OnFail(ex);
            Finally();
        }

        public void ReportProgress(float progress)
        {
            if (OnProgress != null)
                OnProgress(progress);
        }

        public IBasePromise Progress(Action<float> listener)
        {
            OnProgress = AddUnique(OnProgress, listener);
            return this;
        }

        public IBasePromise Fail(Action<Exception> listener)
        {
            if (Failed)
            {
                listener(exception);
                Finally();
            }
            else
                OnFail = AddUnique(OnFail, listener);

            return this;
        }

        public IBasePromise Finally(Action listener)
        {
            if (Resolved)
                listener();
            else
                OnFinally = AddUnique(OnFinally, listener);

            return this;
        }

        public void RemoveProgressListeners()
        {
            OnProgress = null;
        }

        public void RemoveFailListeners()
        {
            OnFail = null;
        }

        public virtual void RemoveAllListeners()
        {
            OnProgress = null;
            OnFail = null;
            OnFinally = null;
        }

        public abstract int ListenerCount();
        private event Action<float> OnProgress;
        private event Action<Exception> OnFail;
        private event Action OnFinally;

        /// <summary>
        ///     Returns false if the Promise has yet to be resolved. If resolved,
        ///     sets the state to Fulfilled and returns true.
        /// </summary>
        protected bool Fulfill()
        {
            if (Resolved) return false;

            State = PromiseState.Fulfilled;
            return true;
        }

        /// <summary>
        ///     Trigger Finally callbacks
        /// </summary>
        protected void Finally()
        {
            if (OnFinally != null)
                OnFinally();
            RemoveAllListeners();
        }

        /// <summary>
        ///     Adds a listener to a callback queue.
        /// </summary>
        /// <returns>The complete list of associated listeners.</returns>
        /// <param name="listeners">Any existing callback queue.</param>
        /// <param name="callback">A callback to add to the queue.</param>
        protected Action AddUnique(Action listeners, Action callback)
        {
            if (listeners == null || !listeners.GetInvocationList().Contains(callback))
            {
                listeners += callback;
            }

            return listeners;
        }

        /// <summary>
        ///     Adds a listener to a callback queue, specifying the Action parameter Type of the listener.
        /// </summary>
        /// <returns>The complete list of associated listeners.</returns>
        /// <param name="listeners">Any existing callback queue.</param>
        /// <param name="callback">A callback to add to the queue.</param>
        protected Action<T> AddUnique<T>(Action<T> listeners, Action<T> callback)
        {
            if (listeners == null || !listeners.GetInvocationList().Contains(callback))
            {
                listeners += callback;
            }

            return listeners;
        }
    }
}