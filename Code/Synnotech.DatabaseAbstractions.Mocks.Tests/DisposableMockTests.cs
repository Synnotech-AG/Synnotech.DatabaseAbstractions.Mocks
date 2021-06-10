using System;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class DisposableMockTests
    {
        [Fact]
        public static void MustBeAbstractClass() =>
            typeof(DisposableMock<>).Should().BeAbstract();

        [Fact]
        public static void MustImplementIDisposable() =>
            typeof(DisposableMock<>).Should().Implement<IDisposable>();

        [Fact]
        public static void MustImplementIDisposableMock() =>
            typeof(DisposableMock<>).Should().Implement<IDisposableMock>();

        [Fact]
        public static void ThrowExceptionWHenNotDisposed()
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

            disposable.MustBeDisposed().Should().BeSameAs(disposable);
        }

        private sealed class DisposableMock : DisposableMock<DisposableMock> { }
    }
}