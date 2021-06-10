using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class AsyncDisposableMockTests
    {
        [Fact]
        public static void MustBeAbstractClass() =>
            typeof(AsyncDisposableMock<>).Should().BeAbstract();

        [Fact]
        public static void MustImplementIDisposable() =>
            typeof(AsyncDisposableMock<>).Should().Implement<IDisposable>();

        [Fact]
        public static void MustImplementIAsyncDisposable() =>
            typeof(AsyncDisposableMock<>).Should().Implement<IAsyncDisposable>();

        [Fact]
        public static void ThrowExceptionWhenNotDisposed()
        {
            var disposable = new DisposableMock();

            Action act = () => disposable.MustBeDisposed();

            act.Should().Throw<TestException>()
               .And.Message.Should().Be("\"DisposableMock\" was not disposed");
        }

        [Fact]
        public static void NoExceptionWhenDisposed()
        {
            var disposable = new DisposableMock();

            disposable.Dispose();

            disposable.EnsureNoExceptionIsThrown();
        }

        [Fact]
        public static async Task NoExceptionWhenDisposedAsync()
        {
            var disposable = new DisposableMock();

            await disposable.DisposeAsync();

            disposable.EnsureNoExceptionIsThrown();
        }

        private static void EnsureNoExceptionIsThrown(this DisposableMock disposable) =>
            disposable.MustBeDisposed().Should().BeSameAs(disposable);

        private sealed class DisposableMock : AsyncDisposableMock<DisposableMock> { }
    }
}