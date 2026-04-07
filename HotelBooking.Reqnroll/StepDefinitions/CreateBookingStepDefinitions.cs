using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.Reqnroll.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.Reqnroll.StepDefinitions
{
    [Binding]
    public class CreateBookingStepDefinitions
    {
        private List<Room> rooms = new();
        private BookingManager bookingManager = null!;
        private DateTime requestedStartDate;
        private DateTime requestedEndDate;
        private bool? createBookingResult;
        private List<DateTime> fullyOccupiedDates = new();
        private Exception? capturedException;

        [Given(@"a hotel with (.*) rooms")]
        public void GivenAHotelWithRooms(int roomCount)
        {
            rooms = Enumerable.Range(1, roomCount)
                .Select(i => new Room { Id = i, Description = $"Room {i}" })
                .ToList();
        }

        [Given(@"a fully occupied range from (.*) to (.*) days from today")]
        public void GivenAFullyOccupiedRangeFromToDaysFromToday(int occupiedStartOffset, int occupiedEndOffset)
        {
            var occupiedStart = DateTime.Today.AddDays(occupiedStartOffset);
            var occupiedEnd = DateTime.Today.AddDays(occupiedEndOffset);

            var bookings = rooms.Select(r => new Booking
            {
                Id = r.Id,
                RoomId = r.Id,
                CustomerId = r.Id,
                StartDate = occupiedStart,
                EndDate = occupiedEnd,
                IsActive = true
            }).ToList();

            var bookingRepository = new InMemoryRepository<Booking>(bookings);
            var roomRepository = new InMemoryRepository<Room>(rooms);

            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Given(@"a booking request from (.*) to (.*) days from today")]
        public void GivenABookingRequestFromToDaysFromToday(int startOffset, int endOffset)
        {
            requestedStartDate = DateTime.Today.AddDays(startOffset);
            requestedEndDate = DateTime.Today.AddDays(endOffset);
            capturedException = null;
            createBookingResult = null;
        }

        [When(@"I create the booking")]
        public async Task WhenICreateTheBooking()
        {
            try
            {
                createBookingResult = await bookingManager.CreateBooking(new Booking
                {
                    StartDate = requestedStartDate,
                    EndDate = requestedEndDate,
                    CustomerId = 999
                });
            }
            catch (Exception ex)
            {
                capturedException = ex;
            }
        }

        [Then(@"booking should (.*)")]
        public void ThenBookingShould(string outcome)
        {
            Assert.Null(capturedException);
            Assert.NotNull(createBookingResult);

            if (string.Equals(outcome, "succeed", StringComparison.OrdinalIgnoreCase))
            {
                Assert.True(createBookingResult!.Value);
            }
            else
            {
                Assert.False(createBookingResult!.Value);
            }
        }

        [Then(@"an ArgumentException should be thrown")]
        public void ThenAnArgumentExceptionShouldBeThrown()
        {
            Assert.NotNull(capturedException);
            Assert.IsType<ArgumentException>(capturedException);
        }

        [When(@"I request fully occupied dates from (.*) to (.*) days from today")]
        public async Task WhenIRequestFullyOccupiedDatesFromToDaysFromToday(int startOffset, int endOffset)
        {
            var start = DateTime.Today.AddDays(startOffset);
            var end = DateTime.Today.AddDays(endOffset);
            fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(start, end);
        }

        [Then(@"fully occupied dates should be (.*), (.*), (.*) days from today")]
        public void ThenFullyOccupiedDatesShouldBeDaysFromToday(int day1, int day2, int day3)
        {
            var expected = new List<DateTime>
            {
                DateTime.Today.AddDays(day1).Date,
                DateTime.Today.AddDays(day2).Date,
                DateTime.Today.AddDays(day3).Date
            };

            Assert.Equal(expected, fullyOccupiedDates.Select(d => d.Date).ToList());
        }
    }
}
