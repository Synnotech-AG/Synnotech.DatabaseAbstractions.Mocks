using System;
using Light.GuardClauses;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents the base class for mocks that track how often a session was opened.
    /// </summary>
    /// <typeparam name="TSession">The type of your session. Usually, the abstract type is used.</typeparam>
    /// <typeparam name="TMock">Your subclass that derives from this class.</typeparam>
    public abstract class BaseSessionFactoryMock<TSession, TMock> : AsyncDisposableMock<TMock>
        where TMock : BaseSessionFactoryMock<TSession, TMock>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BaseSessionFactoryMock{TSession, TMock}" />.
        /// </summary>
        /// <param name="session">The session instance that will be returned when a new session is requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        protected BaseSessionFactoryMock(TSession session)
        {
            Session = session.MustNotBeNullReference(nameof(session));
        }

        /// <summary>
        /// Gets the session instance that will be returned when a new session is requested.
        /// </summary>
        public TSession Session { get; }

        /// <summary>
        /// Gets the number of times the session was opened.
        /// </summary>
        public int OpenSessionCallCount { get; protected set; }

        /// <summary>
        /// Increments the <see cref="OpenSessionCallCount" /> and protects it from overflows.
        /// </summary>
        protected void IncrementOpenSessionCallCount()
        {
            checked { OpenSessionCallCount++; }
        }

        /// <summary>
        /// Checks if OpenSession has never been called, or otherwise throws a <see cref="TestException" />.
        /// </summary>
        public TMock OpenSessionMustNotHaveBeenCalled()
        {
            if (OpenSessionCallCount != 0)
                throw new TestException($"OpenSessionAsync must not have been called, but it was actually called {OpenSessionCallCount} {(OpenSessionCallCount == 1 ? "time" : "times")}.");
            return (TMock) this;
        }

        /// <summary>
        /// Checks if OpenSession was called exactly once, or otherwise throws a <see cref="TestException" />.
        /// </summary>
        public TMock OpenSessionMustHaveBeenCalled()
        {
            if (OpenSessionCallCount == 0)
                throw new TestException("OpenSessionAsync must have been called exactly once, but it was actually never called.");
            if (OpenSessionCallCount > 1)
                throw new TestException($"OpenSessionAsync must have been called exactly once, but it was actually called {OpenSessionCallCount} times.");
            return (TMock) this;
        }
    }
}