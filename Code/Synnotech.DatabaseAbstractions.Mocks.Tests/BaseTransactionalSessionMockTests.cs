using System;
using FluentAssertions;
using Humanizer;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class BaseTransactionalSessionMockTests
    {
        [Fact]
        public static void MustDeriveFromAsyncDisposableMock() =>
            typeof(BaseTransactionalSessionMock<,>).Should().BeDerivedFrom(typeof(AsyncDisposableMock<>));

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public static void ThrowExceptionWhenAPreviousTransactionWasNotDisposedOnBeginTransaction(int indexOfInvalidTransaction)
        {
            var session = new TransactionalSession();

            Action act = () =>
            {
                for (var i = 0; i < 5; i++)
                {
                    var transaction = session.BeginTransaction();
                    transaction.Commit();
                    if (i != indexOfInvalidTransaction)
                        transaction.Dispose();
                }
            };

            act.Should().Throw<TestException>()
               .And.Message.Should().Be($"The {(indexOfInvalidTransaction + 1).Ordinalize()} transaction was not disposed before the {(indexOfInvalidTransaction + 2).Ordinalize()} transaction was started.");
        }

        public static class WhenAllTransactionMustBeCommittedIsCalled
        {
            [Theory]
            [InlineData(1)]
            [InlineData(5)]
            [InlineData(12)]
            public static void NoExceptionWhenAllTransactionsAreCommitted(int numberOfTransactions)
            {
                var session = new TransactionalSession();

                for (var i = 0; i < numberOfTransactions; i++)
                {
                    var transaction = session.BeginTransaction();
                    session.Transactions[i].Should().BeSameAs(transaction);
                    transaction.Commit();
                    transaction.Dispose();
                }

                session.AllTransactionsMustBeCommitted().Should().BeSameAs(session);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(2)]
            public static void ExceptionWhenAnyTransactionIsNotCommitted(int indexOfInvalidTransaction)
            {
                var session = new TransactionalSession();

                for (var i = 0; i < 3; i++)
                {
                    var transaction = session.BeginTransaction();
                    if (i != indexOfInvalidTransaction)
                        transaction.Commit();
                    transaction.Dispose();
                }

                Action act = () => session.AllTransactionsMustBeCommitted();

                act.ShouldThrowTransactionWasNotCommitted(indexOfInvalidTransaction);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(2)]
            [InlineData(4)]
            public static void ExceptionWhenAnyTransactionIsCommittedMoreThanOnce(int indexOfInvalidTransaction)
            {
                var session = new TransactionalSession();
                var numberOfCommits = 0;
                for (var i = 0; i < 5; i++)
                {
                    var transaction = session.BeginTransaction();
                    transaction.CommitMaybeTooOften(i, indexOfInvalidTransaction, ref numberOfCommits);
                    transaction.Dispose();
                }

                Action act = () => session.AllTransactionsMustBeCommitted();

                act.ShouldThrowTransactionWasCommittedTooOften(indexOfInvalidTransaction, numberOfCommits);
            }

            [Fact]
            public static void EnsureThatThereIsAtLeastOneTransaction()
            {
                var session = new TransactionalSession();

                Action act = () => session.AllTransactionsMustBeCommitted();

                act.ShouldThrowNoTransactionsStartedException();
            }
        }

        public static class WhenAllTransactionsExceptTheLastMustBeCommittedIsCalled
        {
            [Theory]
            [InlineData(1)]
            [InlineData(3)]
            [InlineData(7)]
            public static void ExceptionWhenAllTransactionsAreCommitted(int numberOfTransactions)
            {
                var session = new TransactionalSession();

                for (var i = 0; i < numberOfTransactions; i++)
                {
                    var transaction = session.BeginTransaction();
                    transaction.Commit();
                    transaction.Dispose();
                }

                Action act = () => session.AllTransactionsExceptTheLastMustBeCommitted();
                
                act.ShouldThrowTransactionWasNotRolledBack(numberOfTransactions - 1);
            }

            [Theory]
            [InlineData(1)]
            [InlineData(5)]
            [InlineData(21)]
            public static void NoExceptionWhenAllButTheLastTransactionAreCommitted(int numberOfTransactions)
            {
                var session = new TransactionalSession();

                var lastIndex = numberOfTransactions - 1;
                for (var i = 0; i < numberOfTransactions; i++)
                {
                    var transaction = session.BeginTransaction();
                    if (i != lastIndex)
                        transaction.Commit();
                    transaction.Dispose();
                }

                session.AllTransactionsExceptTheLastMustBeCommitted().Should().BeSameAs(session);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            public static void ExceptionWhenAnyOfTheFirstTransactionsIsNotCommitted(int indexOfInvalidTransaction)
            {
                var session = new TransactionalSession();

                for (var i = 0; i < 5; i++)
                {
                    var transaction = session.BeginTransaction();
                    if (i != indexOfInvalidTransaction)
                        transaction.Commit();
                    transaction.Dispose();
                }

                Action act = () => session.AllTransactionsExceptTheLastMustBeCommitted();

                act.ShouldThrowTransactionWasNotCommitted(indexOfInvalidTransaction);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(2)]
            [InlineData(3)]
            public static void ExceptionWhenAnyTransactionIsCommittedSeveralTimes(int indexOfInvalidTransaction)
            {
                var session = new TransactionalSession();

                var numberOfCommits = 0;
                for (var i = 0; i < 5; i++)
                {
                    var transaction = session.BeginTransaction();
                    if (i < 4)
                        transaction.CommitMaybeTooOften(i, indexOfInvalidTransaction, ref numberOfCommits);

                    transaction.Dispose();
                }

                Action act = () => session.AllTransactionsExceptTheLastMustBeCommitted();

                act.ShouldThrowTransactionWasCommittedTooOften(indexOfInvalidTransaction, numberOfCommits);
            }

            [Fact]
            public static void EnsureThatThereIsAtLeastOneTransaction()
            {
                var session = new TransactionalSession();

                Action act = () => session.AllTransactionsExceptTheLastMustBeCommitted();

                act.ShouldThrowNoTransactionsStartedException();
            }
        }

        private static void CommitMaybeTooOften(this TransactionMock transaction,
                                                int currentIndex,
                                                int indexOfInvalidTransaction,
                                                ref int numberOfCommits)
        {
            transaction.Commit();
            if (currentIndex != indexOfInvalidTransaction)
                return;

            var random = new Random(); // You may want to provide a seed here for debugging purposes
            numberOfCommits = random.Next(1, 10);
            for (var i = 0; i < numberOfCommits; i++)
                transaction.Commit();

            numberOfCommits++;
        }

        private static void ShouldThrowTransactionWasNotCommitted(this Action act, int indexOfInvalidTransaction) =>
            act.Should().Throw<TestException>()
               .And.Message.Should().Be($"The {(indexOfInvalidTransaction + 1).Ordinalize()} transaction was not committed.");

        private static void ShouldThrowTransactionWasCommittedTooOften(this Action act, int indexOfInvalidTransaction, int numberOfCommits) =>
            act.Should().Throw<TestException>()
               .And.Message.Should().Be($"The {(indexOfInvalidTransaction + 1).Ordinalize()} transaction was committed too often ({numberOfCommits} times).");

        private static void ShouldThrowTransactionWasNotRolledBack(this Action act, int indexOfInvalidTransaction) =>
            act.Should().Throw<TestException>()
               .And.Message.Should().Be($"The {(indexOfInvalidTransaction + 1).Ordinalize()} transaction was committed, although it should be rolled back.");

        private static void ShouldThrowNoTransactionsStartedException(this Action act) =>
            act.Should().Throw<TestException>()
               .And.Message.Should().Be("No transactions were started.");

        private sealed class TransactionalSession : BaseTransactionalSessionMock<TransactionMock, TransactionalSession>
        {
            public TransactionalSession(bool ensurePreviousTransactionIsClosed = true)
                : base(ensurePreviousTransactionIsClosed) { }

            public TransactionMock BeginTransaction() => CreateTransaction();
        }
    }
}