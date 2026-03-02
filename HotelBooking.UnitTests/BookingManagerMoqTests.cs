using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using Moq;
using Xunit;
using System.Linq;

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
    #region Tobias 
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

    #endregion

    #region Carolina



    // TEST 1 - Some dates fully occupied -> returnér KUN de datoer
    [Theory]
    // Principle: Data-driven tests (MemberData) - en test kan køre flere scenarier uden copy-paste.
    // Principle: Easy to change/evolve - man kan tilføje nye scenarier i TestData uden at skrive nye tests.
    [MemberData(nameof(SomeDatesFullyOccupiedTestData.Data), MemberType = typeof(SomeDatesFullyOccupiedTestData))]
    public async Task GetFullyOccupiedDates_SomeDatesFullyOccupied_ReturnsOnlyThoseDates(
        DateTime startDate,
        DateTime endDate,
        List<Booking> bookings,
        List<DateTime> expectedDates)
    {
        // Arrange
        // Principle: Fast + Isolated - bruger Moq i stedet for rigtig DB (ingen eksterne ressourcer).
        // Principle: Repeatable / not flaky - kontrollerer præcis hvilke bookings metoden ser.
        bookingRepoMock.Setup(b => b.GetAllAsync())
            .ReturnsAsync(bookings);

        // Act
        // Principle: Cohesive - testen gør en ting: kalder metoden og ser hvad den returnerer.
        List<DateTime> result = await bookingManager
            .GetFullyOccupiedDates(startDate, endDate);

        // Assert (kun dato, ikke klokkeslæt)
        // Principle: Strong assertions - vi tjekker præcis hvilke datoer der kommer ud (ikke bare "not empty").
        // Principle: Repeatable / not flaky - .Date gør at tidspunkter ikke kan give falske failures.
        // Principle: Breaks if behavior changes - hvis logikken ændres, kommer listen ikke til at matche.
        Assert.Equal(expectedDates.Select(d => d.Date), result.Select(d => d.Date));

        // Verify
        // Principle: Strong assertions (interaction) - vi tjekker også at repos faktisk blev kaldt.
        // Principle: Single clear reason to fail - hvis noget er galt, ser man hurtigt om det er output eller repo-kald.
        bookingRepoMock.Verify(b => b.GetAllAsync(), Times.Once);
        roomRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }


    // TEST 2 - All dates fully occupied -> returnér ALLE datoer i range
    [Theory]
    // Principle: Data-driven tests (InlineData) - samme test køres med flere ranges (1, 2, 8 dage).
    // Principle: Easy to write + easy to read - hurtigt at se hvilke cases der testes.
    [InlineData(10, 0)]  // 1 dag (start == end)
    [InlineData(10, 1)]  // 2 dage
    [InlineData(10, 7)]  // 8 dage
    public async Task GetFullyOccupiedDates_AllDatesFullyOccupied_ReturnsAllDatesInRange(
        int startOffset,
        int rangeLength)
    {
        // Arrange
        // Principle: Repeatable / not flaky - vi vælger altid datoer i fremtiden relativt til Today.
        DateTime startDate = DateTime.Today.AddDays(startOffset);
        DateTime endDate = startDate.AddDays(rangeLength);

        // Begge rooms booket hele perioden - hver dag er fully occupied
        // Principle: Fixtures not too general - vi laver kun den booking-data vi har brug for til testen.
        var bookings = new List<Booking>
    {
        new Booking { RoomId = 1, IsActive = true, StartDate = startDate, EndDate = endDate },
        new Booking { RoomId = 2, IsActive = true, StartDate = startDate, EndDate = endDate }
    };

        // Principle: Fast + Isolated - mock i stedet for DB.
        bookingRepoMock.Setup(b => b.GetAllAsync())
            .ReturnsAsync(bookings);

        // expected = alle datoer fra start til end (inklusive)
        // Principle: Strong assertions - vi bygger præcis den forventede liste og sammenligner 1:1.
        // Principle: Breaks if behavior changes - hvis metoden pludselig ikke inkluderer endDate, fejler testen.
        var expectedDates = Enumerable.Range(0, rangeLength + 1)
            .Select(i => startDate.AddDays(i).Date)
            .ToList();

        // Act
        List<DateTime> result = await bookingManager
            .GetFullyOccupiedDates(startDate, endDate);

        // Assert
        // Principle: Strong assertions - præcis match af alle datoer.
        Assert.Equal(expectedDates, result.Select(d => d.Date).ToList());

        // Verify
        // Principle: Strong assertions (interaction)
        bookingRepoMock.Verify(b => b.GetAllAsync(), Times.Once);
        roomRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }


    // TEST 3 - Only counts active bookings - inaktive må ikke gøre dagen “fully occupied”
    [Theory]
    // Principle: Data-driven tests (MemberData) - flere scenarier med blandet aktiv/inaktiv data uden nye tests.
    // Principle: Easy to change/evolve - tilføj flere cases i OnlyCountsActiveBookingsTestData senere.
    [MemberData(nameof(OnlyCountsActiveBookingsTestData.Data), MemberType = typeof(OnlyCountsActiveBookingsTestData))]
    public async Task GetFullyOccupiedDates_OnlyCountsActiveBookings(
        DateTime startDate,
        DateTime endDate,
        List<Booking> bookings,
        List<DateTime> expectedDates)
    {
        // Arrange
        // Principle: Repeatable / not flaky - vi styrer præcis hvilke bookinger tælles (aktiv/inaktiv).
        bookingRepoMock.Setup(b => b.GetAllAsync())
            .ReturnsAsync(bookings);

        // Act
        List<DateTime> result = await bookingManager
            .GetFullyOccupiedDates(startDate, endDate);

        // Assert
        // Principle: Strong assertions + Breaks if behavior changes
        // Hvis metoden pludselig tæller IsActive=false med, så bliver listen forkert og testen fejler.
        Assert.Equal(expectedDates.Select(d => d.Date), result.Select(d => d.Date));

        // Verify
        bookingRepoMock.Verify(b => b.GetAllAsync(), Times.Once);
        roomRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }



    #endregion

    #region Jan

    #endregion

    #region Oliver

    #endregion

}