using System.Threading;
using System.Threading.Tasks;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implements <see cref="IAsyncTransactionalSession" />.
    /// </summary>
    /// <typeparam name="T">Your subclass that derives from this type.</typeparam>
    public abstract class AsyncTransactionalSessionMock<T> : BaseTransactionalSessionMock<AsyncTransactionMock, T>, IAsyncTransactionalSession
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
        protected AsyncTransactionalSessionMock(bool ensurePreviousTransactionIsClosed = true)
            : base(ensurePreviousTransactionIsClosed) { }

        /// <summary>
        /// Creates a new <see cref="AsyncTransactionMock" /> instance, adds it to the list of
        /// transactions and returns it. If EnsurePreviousTransactionIsClosed is set to true
        /// (which is the default), this mock will also ensure that the previous transaction was disposed beforehand.
        /// </summary>
        public Task<IAsyncTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = CreateTransaction();
            return Task.FromResult<IAsyncTransaction>(transaction);
        }
    }
}