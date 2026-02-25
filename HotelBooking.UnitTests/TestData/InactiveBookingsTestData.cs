using System;
using System.Collections.Generic;
using HotelBooking.Core;

namespace HotelBooking.UnitTests.TestData;

public static class InactiveBookingsTestData
{
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            // Scenario A: én inaktiv booking der dækker hele perioden
            new object[]
            {
                new List<Booking>
                {
                    new Booking { Id = 1, RoomId = 1, IsActive = false,
                        StartDate = DateTime.Today.AddDays(1),
                        EndDate = DateTime.Today.AddDays(30) }
                }
            },
            // Scenario B: to overlappende inaktive bookinger
            new object[]
            {
                new List<Booking>
                {
                    new Booking { Id = 2, RoomId = 1, IsActive = false,
                        StartDate = DateTime.Today.AddDays(5),
                        EndDate = DateTime.Today.AddDays(15) },
                    new Booking { Id = 3, RoomId = 2, IsActive = false,
                        StartDate = DateTime.Today.AddDays(5),
                        EndDate = DateTime.Today.AddDays(15) }
                }
            },
            // Scenario C: mange inaktive bookinger fordelt over hele perioden
            new object[]
            {
                new List<Booking>
                {
                    new Booking { Id = 4, RoomId = 1, IsActive = false,
                        StartDate = DateTime.Today.AddDays(1),
                        EndDate = DateTime.Today.AddDays(10) },
                    new Booking { Id = 5, RoomId = 2, IsActive = false,
                        StartDate = DateTime.Today.AddDays(10),
                        EndDate = DateTime.Today.AddDays(20) },
                    new Booking { Id = 6, RoomId = 1, IsActive = false,
                        StartDate = DateTime.Today.AddDays(20),
                        EndDate = DateTime.Today.AddDays(30) }
                }
            }
        };
}