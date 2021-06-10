﻿using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class ReadOnlySessionMockTests
    {
        [Fact]
        public static void MustBeAbstractClass() =>
            typeof(ReadOnlySessionMock<>).Should().BeAbstract();

        [Fact]
        public static void MustDeriveFromDisposableMock() =>
            typeof(ReadOnlySessionMock<>).Should().BeDerivedFrom(typeof(DisposableMock<>));
    }
}