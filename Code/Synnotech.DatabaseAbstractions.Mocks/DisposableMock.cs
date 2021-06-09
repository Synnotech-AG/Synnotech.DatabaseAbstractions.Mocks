using System;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implement <see cref="IDisposable" />
    /// </summary>
    /// <typeparam name="T">The subtype that derives from this class.</typeparam>
    public abstract class DisposableMock<T> : IDisposable
        where T : DisposableMock<T>
    {
        /// <summary>
        /// Gets the number of times <see cref="Dispose" /> was called.
        /// </summary>
        public int DisposeCallCount { get; protected set; }

        /// <summary>
        /// Increments the <see cref="DisposeCallCount" />.
        /// </summary>
        public void Dispose()
        {
            checked { DisposeCallCount++; }
        }

        /// <summary>
        /// Checks if this session was disposed (<see cref="DisposeCallCount" /> must be greater or equal to 1),
        /// or otherwise throws a <see cref="TestException" />.
        /// </summary>
        public T MustBeDisposed()
        {
            if (DisposeCallCount < 1)
                throw new TestException($"\"{GetType().Name}\" was not disposed");
            return (T) this;
        }
    }
}