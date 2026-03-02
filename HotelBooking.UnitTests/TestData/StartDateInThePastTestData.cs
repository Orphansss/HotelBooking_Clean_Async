
using System;
using System.Collections.Generic;

namespace HotelBooking.UnitTests.TestData;

// Provides test data for scenarios where the start date is in the past.
public static class StartDateInThePastTestData
{
    public static IEnumerable<object[]> StartDateInThePastCases =>
        new List<object[]>
        {
            new object[] {DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1) },
            new object[] {DateTime.Today.AddDays(-5), DateTime.Today.AddDays(5) },
            new object[] {DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30) },
        };
}