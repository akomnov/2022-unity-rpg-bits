using System;
using System.Collections;
using System.Collections.Generic;

namespace RPG.Core.Shared.Utils {
    public static class Functions {
        /// <summary>
        /// Makes the coroutine call us back on exception or on completion.
        /// See https://web.archive.org/web/20170818040958/https://jacksondunstan.com/articles/3718
        /// for explanation.
        /// This version allows us to yield nested coroutines directly without having to wrap them in ThrowingEnumerator
        /// (as was the case with the old version).
        /// Note that in order to wait for multiple nested coroutines in parallel you still need to wrap,
        /// since you'll be using StartCoroutine again.
        /// If it's too much pain we might come up with something, but it kinda makes sense:
        /// you might want fine control over execution when you're doing things in (pseudo-)parallel,
        /// so implementing a general solution before looking at exact use cases would be misguided.
        /// </summary>
        /// <param name="coroutine">The coroutine</param>
        /// <param name="onDone">This will be called with a System.Exception or with the result object if completed successfully</param>
        /// <returns></returns>
        public static IEnumerator CoroutineAndCallBack(
            IEnumerator coroutine,
            Action<Exception, object> onDone
        ) {
            object _result = null;
            var _coroutineStack = new Stack<IEnumerator>();
            _coroutineStack.Push(coroutine);
            while (_coroutineStack.Count > 0) {
                var _topCoroutine = _coroutineStack.Peek();
                try {
                    if (_topCoroutine.MoveNext() == false) {
                        _coroutineStack.Pop();
                        continue;
                    }
                    _result = _topCoroutine.Current;
                } catch (Exception ex) {
                    onDone(ex, null);
                    yield break;
                }
                if (_result is IEnumerator _nestedCoroutine) {
                    _coroutineStack.Push(_nestedCoroutine);
                } else {
                    yield return _result;
                }
            }
            onDone(null, _result);
        }
    }
}
