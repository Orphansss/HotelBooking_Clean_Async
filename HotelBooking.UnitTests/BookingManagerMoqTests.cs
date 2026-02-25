using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class BookingManagerMoqTests
{
    private readonly Mock<IRepository<Booking>> bookingRepoMock;
    private readonly Mock<IRepository<Room>> roomRepoMock;
    private readonly IBookingManager bookingManager;

    public BookingManagerMoqTests()
    {
        bookingRepoMock = new Mock<IRepository<Booking>>();
        roomRepoMock = new Mock<IRepository<Room>>();

        // Default room setup - two rooms available
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>
        {
            new Room { Id = 1, Description = "A" },
            new Room { Id = 2, Description = "B" }
        });

        bookingManager = new BookingManager(bookingRepoMock.Object, roomRepoMock.Object);
    }

    // TEST 1 - InlineData: start > end skal kaste ArgumentException
    [Theory]
    [InlineData(10, 5)]
    [InlineData(20, 1)]
    [InlineData(1, 0)]
    public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException(
        int startOffset, int endOffset)
    {
        // Principle: "Tests should have strong assertions"
        // Ingen mock-setup nødvendig - exception kastes før repository kaldes

        // Arrange
        DateTime startDate = DateTime.Today.AddDays(startOffset);
        DateTime endDate = DateTime.Today.AddDays(endOffset);

        // Act
        Task Result() => bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Result);
    }

    // TEST 2 - InlineData: ingen bookinger skal returnere tom liste
    [Theory]
    [InlineData(1, 5)]
    [InlineData(1, 30)]
    [InlineData(5, 5)]
    public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList(
        int startOffset, int endOffset)
    {
        // Principle: "Tests should be repeatable and not flaky"
        // Mock stubber et tomt repository

        // Arrange - mock returnerer tom liste, ingen bookinger
        bookingRepoMock.Setup(b => b.GetAllAsync())
            .ReturnsAsync(new List<Booking>());

        DateTime startDate = DateTime.Today.AddDays(startOffset);
        DateTime endDate = DateTime.Today.AddDays(endOffset);

        // Act
        List<DateTime> result = await bookingManager
            .GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);

        // Verify: GetAllAsync blev kaldt præcist én gang
        bookingRepoMock.Verify(b => b.GetAllAsync(), Times.Once);
    }

    // TEST 3 - MemberData: inaktive bookinger skal aldrig tælles
    [Theory]
    [MemberData(nameof(InactiveBookingsTestData.Data), MemberType = typeof(InactiveBookingsTestData))]
    public async Task GetFullyOccupiedDates_AllBookingsInactive_ReturnsEmptyList(
        List<Booking> inactiveBookings)
    {
        // Principle: "Tests should break if the behavior changes"
        // "Tests should have a single and clear reason to fail"
        // Mock injicerer inaktive bookinger fra TestData-klassen
        
        // Arrange - mock returnerer de inaktive bookinger fra MemberData
        bookingRepoMock.Setup(b => b.GetAllAsync())
            .ReturnsAsync(inactiveBookings);

        DateTime startDate = DateTime.Today.AddDays(1);
        DateTime endDate = DateTime.Today.AddDays(30);

        // Act
        List<DateTime> result = await bookingManager
            .GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);

        // Verify: GetAllAsync blev kaldt præcist én gang
        bookingRepoMock.Verify(b => b.GetAllAsync(), Times.Once);
    }

}