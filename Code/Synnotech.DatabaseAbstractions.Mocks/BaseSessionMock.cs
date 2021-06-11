﻿using Light.GuardClauses;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents the base class for mocks that implement <see cref="IAsyncSession" /> or <see cref="ISession" />.
    /// </summary>
    /// <typeparam name="T">
    /// The subtype that derives from this class.
    /// It is used as the return type of the fluent API.
    /// </typeparam>
    public abstract class BaseSessionMock<T> : AsyncDisposableMock<T>
        where T : BaseSessionMock<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BaseSessionMock{T}" />.
        /// </summary>
        /// <param name="saveChangesMethodName">
        /// The name of the method that saves the changes for the session.
        /// The usual names are "SaveChanges" or "SaveChangesAsync". This name
        /// will be included in exceptions when sessions are not handled properly.
        /// </param>
        protected BaseSessionMock(string saveChangesMethodName) =>
            SaveChangesMethodName = saveChangesMethodName.MustNotBeNullOrWhiteSpace(nameof(saveChangesMethodName));

        private string SaveChangesMethodName { get; }

        /// <summary>
        /// Gets the number of times SaveChanges was called.
        /// </summary>
        public int SaveChangesCallCount { get; protected set; }

        /// <summary>
        /// Checks if SaveChanges has been called exactly once, or otherwise
        /// throws a <see cref="TestException" />.
        /// </summary>
        public T SaveChangesMustHaveBeenCalled()
        {
            if (SaveChangesCallCount != 1)
                throw new TestException($"{SaveChangesMethodName} must have been called exactly once, but it was called {SaveChangesCallCount} times.");
            return (T) this;
        }

        /// <summary>
        /// Increments the <see cref="SaveChangesCallCount" />, checking for possible integer overflows.
        /// </summary>
        protected void IncrementSaveChangesCallCount()
        {
            checked { SaveChangesCallCount++; }
        }

        /// <summary>
        /// Checks if SaveChanges has not been called, or otherwise
        /// throws a <see cref="TestException" />.
        /// </summary>
        /// <returns></returns>
        public T SaveChangesMustNotHaveBeenCalled()
        {
            if (SaveChangesCallCount != 0)
                throw new TestException($"{SaveChangesMethodName} must not have been called, but it was called {SaveChangesCallCount} {(SaveChangesCallCount == 1 ? "time" : "times")}.");
            return (T) this;
        }
    }
}