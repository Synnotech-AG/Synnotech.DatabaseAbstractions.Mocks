using System;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class SessionMockTests
    {
        [Fact]
        public static void MustBeAbstract()
        {
            typeof(SessionMock<>).Should().BeAbstract();
            typeof(SessionMock).Should().BeAbstract();
        }

        [Fact]
        public static void NonGenericTypeMustDeriveFromGenericType() =>
            typeof(SessionMock).Should().BeDerivedFrom<SessionMock<SessionMock>>();

        [Fact]
        public static void MustDeriveFromAsyncReadOnlySessionMock() =>
            typeof(SessionMock<>).Should().BeDerivedFrom(typeof(ReadOnlySessionMock<>));

        [Fact]
        public static void ExceptionWhenSaveChangesWasNotCalled()
        {
            var session = new Session();

            session.CheckException(0);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(23)]
        public static void ExceptionWhenSaveChangesAsyncWasCalledTooOften(int numberOfCalls)
        {
            var session = new Session();

            for (var i = 0; i < numberOfCalls; i++)
            {
                session.SaveChanges();
            }

            session.CheckException(numberOfCalls);
        }

        private static void CheckException(this Session session, int numberOfCalls)
        {
            Action act = () => session.SaveChangesMustHaveBeenCalled();

            act.Should().Throw<TestException>()
               .And.Message.Should().Be($"SaveChanges must have been called exactly once, but it was called {numberOfCalls} times.");
        }

        [Fact]
        public static void NoExceptionWhenSaveChangesWasCalledExactlyOnce()
        {
            var session = new Session();

            session.SaveChanges();

            session.SaveChangesMustHaveBeenCalled().Should().BeSameAs(session);
        }

        [Fact]
        public static void CallCountIncrementationMustBeChecked()
        {
            var session = new Session().SetSaveChangesCallCountToMaximum();

            Action act = () => session.SaveChanges();

            act.Should().Throw<OverflowException>();
        }

        private sealed class Session : SessionMock
        {
            public Session SetSaveChangesCallCountToMaximum()
            {
                SaveChangesCallCount = int.MaxValue;
                return this;
            }
        }
    }
}