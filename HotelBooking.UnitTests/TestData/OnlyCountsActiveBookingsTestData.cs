using System;
using System.Collections.Generic;
using HotelBooking.Core;

namespace HotelBooking.UnitTests.TestData
{
    public class OnlyCountsActiveBookingsTestData
    {
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[]
                {
                    DateTime.Today.AddDays(10),
                    DateTime.Today.AddDays(12),

                    new List<Booking>
                    {
                        // Room 1 aktiv hele perioden
                        new Booking
                        {
                            RoomId = 1,
                            StartDate = DateTime.Today.AddDays(10),
                            EndDate = DateTime.Today.AddDays(12),
                            IsActive = true
                        },

                        // Room 2 INAKTIV hele perioden (må ikke tælle)
                        new Booking
                        {
                            RoomId = 2,
                            StartDate = DateTime.Today.AddDays(10),
                            EndDate = DateTime.Today.AddDays(12),
                            IsActive = false
                        },

                        // Room 2 aktiv kun på dag 11 (så kun dag 11 bliver fully occupied)
                        new Booking
                        {
                            RoomId = 2,
                            StartDate = DateTime.Today.AddDays(11),
                            EndDate = DateTime.Today.AddDays(11),
                            IsActive = true
                        }
                    },

                    new List<DateTime>
                    {
                        DateTime.Today.AddDays(11)
                    }
                }
            };
    }
}