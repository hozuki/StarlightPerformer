using System;

namespace StarlightPerformer.Core {
    public abstract class DisposableBase : IDisposable {

        ~DisposableBase() {
            if (!_isDisposed) {
                Dispose(false);
                _isDisposed = true;
            }
        }

        public bool IsDisposed => _isDisposed;

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            Dispose(true);
            _isDisposed = true;
        }

        protected abstract void Dispose(bool disposing);

        private bool _isDisposed;

    }
}
