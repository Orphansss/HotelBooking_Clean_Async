using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }
        
        

        [Theory]
        [InlineData(10, 5)]   // start is 5 days after end
        [InlineData(20, 1)]   // start is 19 days after end
        [InlineData(1, 0)]    // start is 1 day after end
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException(
            int startOffset, int endOffset)
        {
            // Principle: "Tests should have strong assertions"
            // We assert the exact exception type, not just that something went wrong.
            
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(startOffset);
            DateTime endDate = DateTime.Today.AddDays(endOffset);

            // Act
            Task Result() => bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Result);
        }
        
        [Theory]
        [InlineData(1, 5)]    // short range in near future
        [InlineData(1, 30)]   // longer range
        [InlineData(5, 5)]    // single day range
        public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList(
            int startOffset, int endOffset)
        {
            // Principle: "Tests should be repeatable and not flaky"
            // Deterministic inputs via a fresh FakeBookingRepository with no bookings each time.
            
            // Arrange - use a booking repository with zero bookings
            IRepository<Booking> emptyBookingRepo = new FakeBookingRepository();
            IRepository<Room> roomRepository = new FakeRoomRepository();
            IBookingManager managerWithNoBookings =
                new BookingManager(emptyBookingRepo, roomRepository);

            DateTime startDate = DateTime.Today.AddDays(startOffset);
            DateTime endDate = DateTime.Today.AddDays(endOffset);

            // Act
            List<DateTime> result = await managerWithNoBookings
                .GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Empty(result);
        }
        
        [Theory]
        [MemberData(nameof(InactiveBookingsTestData))]
        public async Task GetFullyOccupiedDates_AllBookingsInactive_ReturnsEmptyList(
            List<Booking> inactiveBookings)
        {
            // Principle: "Tests should break if the behavior changes" and
            // "Tests should have a single and clear reason to fail"
            // If someone accidentally counts inactive bookings, these tests will catch it.
            
            // Arrange
            IRepository<Booking> fakeRepo = new FakeBookingRepository(inactiveBookings);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            IBookingManager manager = new BookingManager(fakeRepo, roomRepository);

            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(30);

            // Act
            List<DateTime> result = await manager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Empty(result);
        }
        
        // MemberData source - returns multiple scenarios of inactive booking lists.
        // Each object[] entry is one set of arguments passed to the test method.
        public static IEnumerable<object[]> InactiveBookingsTestData =>
            new List<object[]>
            {
                // Scenario A: one inactive booking covering the entire range
                new object[]
                {
                    new List<Booking>
                    {
                        new Booking
                        {
                            Id = 1, RoomId = 1, IsActive = false,
                            StartDate = DateTime.Today.AddDays(1),
                            EndDate = DateTime.Today.AddDays(30)
                        }
                    }
                },
                // Scenario B: two inactive bookings with overlapping dates
                new object[]
                {
                    new List<Booking>
                    {
                        new Booking
                        {
                            Id = 2, RoomId = 1, IsActive = false,
                            StartDate = DateTime.Today.AddDays(5),
                            EndDate = DateTime.Today.AddDays(15)
                        },
                        new Booking
                        {
                            Id = 3, RoomId = 2, IsActive = false,
                            StartDate = DateTime.Today.AddDays(5),
                            EndDate = DateTime.Today.AddDays(15)
                        }
                    }
                },
                // Scenario C: many inactive bookings across different rooms and dates
                new object[]
                {
                    new List<Booking>
                    {
                        new Booking
                        {
                            Id = 4, RoomId = 1, IsActive = false,
                            StartDate = DateTime.Today.AddDays(1),
                            EndDate = DateTime.Today.AddDays(10)
                        },
                        new Booking
                        {
                            Id = 5, RoomId = 2, IsActive = false,
                            StartDate = DateTime.Today.AddDays(10),
                            EndDate = DateTime.Today.AddDays(20)
                        },
                        new Booking
                        {
                            Id = 6, RoomId = 1, IsActive = false,
                            StartDate = DateTime.Today.AddDays(20),
                            EndDate = DateTime.Today.AddDays(30)
                        }
                    }
                }
            };
    }
    
}
