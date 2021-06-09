using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implements <see cref="IAsyncTransactionalSession" />.
    /// </summary>
    /// <typeparam name="T">Your subclass that derives from this type.</typeparam>
    public abstract class AsyncTransactionalSessionMock<T> : AsyncReadOnlySessionMock<T>, IAsyncTransactionalSession
        where T : AsyncTransactionalSessionMock<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AsyncReadOnlySessionMock{T}" />.
        /// </summary>
        /// <param name="ensurePreviousTransactionIsClosed">
        /// The value indicating whether this mock checks if a previous transaction
        /// was disposed when <see cref="BeginTransactionAsync" /> is called.
        /// The default value is true.
        /// </param>
        protected AsyncTransactionalSessionMock(bool ensurePreviousTransactionIsClosed = true) =>
            EnsurePreviousTransactionIsClosed = ensurePreviousTransactionIsClosed;

        /// <summary>
        /// Gets the value indicating whether this mock checks if a previous transaction
        /// was disposed when <see cref="BeginTransactionAsync" /> is called. The default value
        /// is true.
        /// </summary>
        public bool EnsurePreviousTransactionIsClosed { get; }

        /// <summary>
        /// Gets the transactions that were started using <see cref="BeginTransactionAsync" />.
        /// </summary>
        public List<AsyncTransactionMock> Transactions { get; } = new ();

        /// <summary>
        /// Creates a new <see cref="AsyncTransactionMock" /> instance, adds it to the list of
        /// transactions and returns it. If <see cref="EnsurePreviousTransactionIsClosed" /> is set to true
        /// (which is the default), this mock will also ensure that the previous transaction was disposed beforehand.
        /// </summary>
        public Task<IAsyncTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            EnsurePreviousTransactionIsClosedIfNecessary();

            var transaction = new AsyncTransactionMock();
            Transactions.Add(transaction);
            return Task.FromResult<IAsyncTransaction>(transaction);

            void EnsurePreviousTransactionIsClosedIfNecessary()
            {
                if (!EnsurePreviousTransactionIsClosed || Transactions.Count == 0)
                    return;

                var lastIndex = Transactions.Count - 1;
                var lastTransaction = Transactions[lastIndex];
                if (lastTransaction.DisposeCallCount == 0)
                    throw new TestException($"The {(lastIndex + 1).Ordinalize()} transaction was not disposed before the {(lastIndex + 2).Ordinalize()} transaction was started.");
            }
        }

        /// <summary>
        /// Checks if all transactions were committed. This method also checks if at least one transaction was started.
        /// </summary>
        public T AllTransactionsMustBeCommitted() => CheckThatTransactionsAreCommitted(Transactions.Count);

        /// <summary>
        /// Checks if all but the last transactions were committed. This method also checks if at least one transaction was started.
        /// </summary>
        public T AllTransactionsExceptTheLastMustBeCommitted() => CheckThatTransactionsAreCommitted(Transactions.Count - 1);

        /// <summary>
        /// Checks if the specified transactions are committed. This method also checks if at least one transaction was started.
        /// </summary>
        /// <param name="indexes">The indexes of the transactions that should be committed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexes" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="indexes" /> is an empty array.</exception>
        public T TransactionsWithIndexesMustBeCommitted(params int[] indexes)
        {
            indexes.MustNotBeNullOrEmpty(nameof(indexes));

            EnsureTransactionsWereStarted();

            for (var i = 0; i < Transactions.Count; i++)
            {
                if (indexes.Contains(i))
                    CheckIfTransactionWasCommitted(i);
            }

            return (T) this;
        }

        /// <summary>
        /// Checks if all transactions were rolled-back. This method also checks if at least one transaction was started.
        /// </summary>
        public T AllTransactionsMustBeRolledBack()
        {
            EnsureTransactionsWereStarted();

            for (var i = 0; i < Transactions.Count; i++)
            {
                CheckIfTransactionWasRolledBack(i);
            }

            return (T) this;
        }

        /// <summary>
        /// Checks if the specified transactions are rolled back. This method also checks if at least one transaction was started.
        /// </summary>
        /// <param name="indexes">The indexes of the transactions that should be rolled back.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexes" /> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="indexes" /> is an empty array.</exception>
        public T TransactionsWithIndexesMustBeRolledBack(params int[] indexes)
        {
            indexes.MustNotBeNullOrEmpty(nameof(indexes));

            EnsureTransactionsWereStarted();

            for (var i = 0; i < Transactions.Count; i++)
            {
                if (indexes.Contains(i))
                    CheckIfTransactionWasRolledBack(i);
            }

            return (T) this;
        }

        private T CheckThatTransactionsAreCommitted(int upperLimit)
        {
            EnsureTransactionsWereStarted();

            for (var i = 0; i < upperLimit; i++)
            {
                CheckIfTransactionWasCommitted(i);
            }

            return (T) this;
        }

        private void EnsureTransactionsWereStarted()
        {
            if (Transactions.Count == 0)
                throw new TestException("No transactions were started.");
        }

        private void CheckIfTransactionWasCommitted(int i)
        {
            var transaction = Transactions[i];
            switch (transaction.CommitCallCount)
            {
                case 0:
                    throw new TestException($"The {(i + 1).Ordinalize()} transaction was not committed.");
                case > 1:
                    throw new TestException($"The {(i + 1).Ordinalize()} transaction was committed to often ({transaction.CommitCallCount} times)");
            }
        }

        private void CheckIfTransactionWasRolledBack(int i)
        {
            var transaction = Transactions[i];
            if (transaction.CommitCallCount != 0)
                throw new TestException($"The {(i + 1).Ordinalize()} transaction was committed, although it should be rolled back.");
            if (transaction.DisposeCallCount == 0)
                throw new TestException($"The {(i + 1).Ordinalize()} transaction was not rolled back.");
        }

        /// <summary>
        /// Checks if this session and all transactions that were created with it were disposed.
        /// </summary>
        public override T MustBeDisposed()
        {
            for (var i = 0; i < Transactions.Count; i++)
            {
                var transaction = Transactions[i];
                if (transaction.DisposeCallCount == 0)
                    throw new TestException($"The {(i + 1).Ordinalize()} transaction was not disposed");
            }

            return base.MustBeDisposed();
        }
    }
}