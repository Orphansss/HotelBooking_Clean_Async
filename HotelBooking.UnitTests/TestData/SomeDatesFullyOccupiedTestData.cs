using System;
using System.Collections.Generic;
using System.Text;
using HotelBooking.Core;

namespace HotelBooking.UnitTests.TestData
{
    public static class SomeDatesFullyOccupiedTestData
    {
        public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            // Scenario A: kun nogle dage er fully occupied
            // Hotellet har 2 rooms (Id 1 og 2)
            // Room 1 er booket hele perioden
            // Room 2 er booket kun midten (only those middle dates er fully occupied)
            new object[]
            {
                DateTime.Today.AddDays(10), // startDate
                DateTime.Today.AddDays(13), // endDate

                new List<Booking> // bookings
                {
                    new Booking
                    {
                        Id = 1, RoomId = 1, IsActive = true,
                        StartDate = DateTime.Today.AddDays(10),
                        EndDate = DateTime.Today.AddDays(13)
                    },
                    new Booking
                    {
                        Id = 2, RoomId = 2, IsActive = true,
                        StartDate = DateTime.Today.AddDays(11),
                        EndDate = DateTime.Today.AddDays(12)
                    }
                },

                new List<DateTime> // expected fully occupied dates
                {
                    DateTime.Today.AddDays(11),
                    DateTime.Today.AddDays(12)
                }
            },

            // Scenario B: kun én dag er fully occupied
            new object[]
            {
                DateTime.Today.AddDays(20),
                DateTime.Today.AddDays(22),

                new List<Booking>
                {
                    // Room 1 booket alle dage 20-22
                    new Booking
                    {
                        Id = 3, RoomId = 1, IsActive = true,
                        StartDate = DateTime.Today.AddDays(20),
                        EndDate = DateTime.Today.AddDays(22)
                    },
                    // Room 2 booket kun dag 21
                    new Booking
                    {
                        Id = 4, RoomId = 2, IsActive = true,
                        StartDate = DateTime.Today.AddDays(21),
                        EndDate = DateTime.Today.AddDays(21)
                    }
                },

                new List<DateTime>
                {
                    DateTime.Today.AddDays(21)
                }
            }
        };
    }


}

