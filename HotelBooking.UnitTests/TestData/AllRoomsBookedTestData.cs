using System;
using System.Collections.Generic;
using HotelBooking.Core;

namespace HotelBooking.UnitTests.TestData;

// Provides test data for the scenario where all rooms are booked.
public static class AllRoomsBookedTestData
{
    public static IEnumerable<object[]> Data =>
        new[]
        {
            new object[]
            {
                new List<Booking>
                {
                    new Booking
                    {
                        Id = 1, 
                        RoomId = 1, 
                        IsActive = true,
                        StartDate = DateTime.Today.AddDays(1),
                        EndDate = DateTime.Today.AddDays(10)
                    },
                    
                    new Booking
                    {
                        Id = 2, 
                        RoomId = 2, 
                        IsActive = true,
                        StartDate = DateTime.Today.AddDays(1),
                        EndDate = DateTime.Today.AddDays(10)
                    }
                }
            }
        };
}