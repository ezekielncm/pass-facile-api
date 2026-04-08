using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;

namespace DomainUnitTests.ValueObjects;

#region Money

public class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(99.99)]
    public void From_ValidAmount_CreatesMoney(decimal amount)
    {
        var money = Money.From(amount);

        Assert.Equal(amount, money.Amount);
        Assert.Equal("XOF", money.Currency);
    }

    [Fact]
    public void From_DefaultCurrency_IsXOF()
    {
        var money = Money.From(10);

        Assert.Equal("XOF", money.Currency);
    }

    [Fact]
    public void From_CustomCurrency_NormalizesToUpperCase()
    {
        var money = Money.From(10, "eur");

        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void From_RoundsToTwoDecimalPlaces()
    {
        var money = Money.From(10.999m);

        Assert.Equal(11.00m, money.Amount);
    }

    [Fact]
    public void From_NegativeAmount_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() => Money.From(-1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyCurrency_ThrowsArgumentException(string? currency)
    {
        Assert.Throws<ArgumentException>(() => Money.From(10, currency!));
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummedAmount()
    {
        var a = Money.From(10);
        var b = Money.From(20);

        var result = a.Add(b);

        Assert.Equal(30m, result.Amount);
        Assert.Equal("XOF", result.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsBusinessRuleValidationException()
    {
        var xof = Money.From(10, "XOF");
        var eur = Money.From(10, "EUR");

        Assert.Throws<BusinessRuleValidationException>(() => xof.Add(eur));
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsSubtractedAmount()
    {
        var a = Money.From(30);
        var b = Money.From(10);

        var result = a.Subtract(b);

        Assert.Equal(20m, result.Amount);
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsBusinessRuleValidationException()
    {
        var xof = Money.From(10, "XOF");
        var eur = Money.From(5, "EUR");

        Assert.Throws<BusinessRuleValidationException>(() => xof.Subtract(eur));
    }

    [Fact]
    public void Subtract_ResultNegative_ThrowsBusinessRuleValidationException()
    {
        var a = Money.From(5);
        var b = Money.From(10);

        Assert.Throws<BusinessRuleValidationException>(() => a.Subtract(b));
    }

    [Fact]
    public void Multiply_ReturnsCorrectProduct()
    {
        var money = Money.From(10);

        var result = money.Multiply(3);

        Assert.Equal(30m, result.Amount);
    }

    [Fact]
    public void Multiply_FractionalFactor_RoundsResult()
    {
        var money = Money.From(10);

        var result = money.Multiply(0.333m);

        Assert.Equal(3.33m, result.Amount);
    }

    [Fact]
    public void LessThanOrEqual_SameCurrency_ComparesAmounts()
    {
        var a = Money.From(10);
        var b = Money.From(20);

        Assert.True(a <= b);
        Assert.True(a <= Money.From(10));
        Assert.False(b <= a);
    }

    [Fact]
    public void GreaterThanOrEqual_SameCurrency_ComparesAmounts()
    {
        var a = Money.From(20);
        var b = Money.From(10);

        Assert.True(a >= b);
        Assert.True(a >= Money.From(20));
        Assert.False(b >= a);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.From(100, "XOF");
        var b = Money.From(100, "XOF");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        var a = Money.From(100);
        var b = Money.From(200);

        Assert.NotEqual(a, b);
    }
}

#endregion

#region PhoneNumber

public class PhoneNumberTests
{
    [Fact]
    public void Ctor_LocalNumber_DefaultsCountryCodeTo226()
    {
        var phone = new PhoneNumber("70000000");

        Assert.Equal("226", phone.CountryCode);
        Assert.Equal("70000000", phone.NationalNumber);
        Assert.Equal("+22670000000", phone.Value);
    }

    [Fact]
    public void Ctor_InternationalFormat_ParsesCountryCode()
    {
        var phone = new PhoneNumber("+22670000000");

        Assert.Equal("226", phone.CountryCode);
        Assert.Equal("70000000", phone.NationalNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Ctor_EmptyValue_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => new PhoneNumber(value!));
    }

    [Fact]
    public void Ctor_TooShortNumber_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PhoneNumber("123"));
    }

    [Fact]
    public void Ctor_TwoArgs_CreatesPhoneNumber()
    {
        var phone = new PhoneNumber("226", "70000000");

        Assert.Equal("226", phone.CountryCode);
        Assert.Equal("70000000", phone.NationalNumber);
        Assert.Equal("+22670000000", phone.Value);
    }

    [Fact]
    public void Ctor_TwoArgs_StripsLeadingPlus()
    {
        var phone = new PhoneNumber("+226", "70000000");

        Assert.Equal("226", phone.CountryCode);
    }

    [Theory]
    [InlineData("", "70000000")]
    [InlineData("226", "")]
    [InlineData("  ", "70000000")]
    public void Ctor_TwoArgs_EmptyArg_ThrowsArgumentException(string cc, string nn)
    {
        Assert.Throws<ArgumentException>(() => new PhoneNumber(cc, nn));
    }

    [Fact]
    public void ToInternationalFormat_ReturnsFormattedString()
    {
        var phone = new PhoneNumber("226", "70000000");

        Assert.Equal("+226 70000000", phone.ToInternationalFormat());
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var phone = new PhoneNumber("226", "70000000");

        Assert.Equal("+22670000000", phone.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = new PhoneNumber("+22670000000");
        var b = new PhoneNumber("226", "70000000");

        Assert.Equal(a, b);
    }
}

#endregion

#region Venue

public class VenueTests
{
    [Fact]
    public void Create_ValidArgs_CreatesVenue()
    {
        var venue = Venue.Create("Stadium", "Ouaga", "Av. de la Liberté");

        Assert.Equal("Stadium", venue.Name);
        Assert.Equal("Ouaga", venue.City);
        Assert.Equal("Av. de la Liberté", venue.Address);
        Assert.Null(venue.GpsCoordinates);
    }

    [Fact]
    public void Create_WithGps_SetsCoordinates()
    {
        var venue = Venue.Create("Stadium", "Ouaga", "Av. de la Liberté", "12.3,1.5");

        Assert.Equal("12.3,1.5", venue.GpsCoordinates);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var venue = Venue.Create("  Stadium  ", " Ouaga ", " Av. X ");

        Assert.Equal("Stadium", venue.Name);
        Assert.Equal("Ouaga", venue.City);
        Assert.Equal("Av. X", venue.Address);
    }

    [Fact]
    public void Create_WhitespaceOnlyGps_SetsNull()
    {
        var venue = Venue.Create("Stadium", "Ouaga", "Av. X", "   ");

        Assert.Null(venue.GpsCoordinates);
    }

    [Theory]
    [InlineData(null, "City", "Address")]
    [InlineData("", "City", "Address")]
    [InlineData("Name", null, "Address")]
    [InlineData("Name", "", "Address")]
    [InlineData("Name", "City", null)]
    [InlineData("Name", "City", "")]
    public void Create_NullOrEmptyRequiredField_ThrowsArgumentException(
        string? name, string? city, string? address)
    {
        Assert.Throws<ArgumentException>(() => Venue.Create(name!, city!, address!));
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var venue = Venue.Create("Stadium", "Ouaga", "Av. X");

        Assert.Equal("Stadium, Av. X, Ouaga", venue.ToString());
    }

    [Fact]
    public void Equality_SameFields_AreEqual()
    {
        var a = Venue.Create("S", "C", "A");
        var b = Venue.Create("S", "C", "A");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentGps_AreNotEqual()
    {
        var a = Venue.Create("S", "C", "A", "1,2");
        var b = Venue.Create("S", "C", "A", "3,4");

        Assert.NotEqual(a, b);
    }
}

#endregion

#region EventSlug

public class EventSlugTests
{
    [Fact]
    public void Create_ValidSlug_NormalizesToLowercase()
    {
        var slug = EventSlug.Create("My-Event-2024");

        Assert.Equal("my-event-2024", slug.Value);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var slug = EventSlug.Create("  abc  ");

        Assert.Equal("abc", slug.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => EventSlug.Create(value!));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("x")]
    public void Create_TooShort_ThrowsBusinessRuleValidationException(string value)
    {
        Assert.Throws<BusinessRuleValidationException>(() => EventSlug.Create(value));
    }

    [Fact]
    public void Create_TooLong_ThrowsBusinessRuleValidationException()
    {
        var longSlug = new string('a', 81);

        Assert.Throws<BusinessRuleValidationException>(() => EventSlug.Create(longSlug));
    }

    [Fact]
    public void Create_ExactlyMinLength_Succeeds()
    {
        var slug = EventSlug.Create("abc");

        Assert.Equal("abc", slug.Value);
    }

    [Fact]
    public void Create_ExactlyMaxLength_Succeeds()
    {
        var slug = EventSlug.Create(new string('a', 80));

        Assert.Equal(80, slug.Value.Length);
    }

    [Theory]
    [InlineData("invalid slug")]
    [InlineData("hello_world")]
    [InlineData("slug@123")]
    [InlineData("a.b.c")]
    public void Create_InvalidChars_ThrowsBusinessRuleValidationException(string value)
    {
        Assert.Throws<BusinessRuleValidationException>(() => EventSlug.Create(value));
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("event2024")]
    [InlineData("abc")]
    public void Create_ValidFormats_Succeeds(string value)
    {
        var slug = EventSlug.Create(value);

        Assert.Equal(value.ToLowerInvariant(), slug.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var slug = EventSlug.Create("my-slug");

        Assert.Equal("my-slug", slug.ToString());
    }
}

#endregion

#region SalesPeriod

public class SalesPeriodTests
{
    [Fact]
    public void Create_ValidRange_CreatesPeriod()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(7);

        var period = SalesPeriod.Create(start, end);

        Assert.Equal(start, period.StartDate);
        Assert.Equal(end, period.EndDate);
    }

    [Fact]
    public void Create_EndBeforeStart_ThrowsBusinessRuleValidationException()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(-1);

        Assert.Throws<BusinessRuleValidationException>(() => SalesPeriod.Create(start, end));
    }

    [Fact]
    public void Create_EndEqualsStart_ThrowsBusinessRuleValidationException()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<BusinessRuleValidationException>(() => SalesPeriod.Create(now, now));
    }

    [Fact]
    public void HasStarted_NowAfterStart_ReturnsTrue()
    {
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow.AddDays(1);
        var period = SalesPeriod.Create(start, end);

        Assert.True(period.HasStarted(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void HasStarted_NowBeforeStart_ReturnsFalse()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = DateTimeOffset.UtcNow.AddDays(2);
        var period = SalesPeriod.Create(start, end);

        Assert.False(period.HasStarted(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void HasStarted_NowEqualsStart_ReturnsTrue()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);
        var period = SalesPeriod.Create(start, end);

        Assert.True(period.HasStarted(start));
    }

    [Fact]
    public void HasEnded_NowAfterEnd_ReturnsTrue()
    {
        var start = DateTimeOffset.UtcNow.AddDays(-2);
        var end = DateTimeOffset.UtcNow.AddDays(-1);
        var period = SalesPeriod.Create(start, end);

        Assert.True(period.HasEnded(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void HasEnded_NowBeforeEnd_ReturnsFalse()
    {
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow.AddDays(1);
        var period = SalesPeriod.Create(start, end);

        Assert.False(period.HasEnded(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void HasEnded_NowEqualsEnd_ReturnsFalse()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddDays(1);
        var period = SalesPeriod.Create(start, end);

        Assert.False(period.HasEnded(end));
    }
}

#endregion

#region Capacity

public class CapacityTests
{
    [Fact]
    public void From_ValidArgs_CreatesCapacity()
    {
        var cap = Capacity.From(100, 25);

        Assert.Equal(100, cap.Total);
        Assert.Equal(25, cap.UsedCount);
    }

    [Fact]
    public void From_DefaultUsedCount_IsZero()
    {
        var cap = Capacity.From(50);

        Assert.Equal(0, cap.UsedCount);
    }

    [Fact]
    public void From_ZeroTotal_Succeeds()
    {
        var cap = Capacity.From(0);

        Assert.Equal(0, cap.Total);
    }

    [Fact]
    public void From_NegativeTotal_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() => Capacity.From(-1));
    }

    [Fact]
    public void From_NegativeUsedCount_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() => Capacity.From(10, -1));
    }

    [Fact]
    public void Add_IncreasesTotal()
    {
        var cap = Capacity.From(100, 10);

        var result = cap.Add(50);

        Assert.Equal(150, result.Total);
        Assert.Equal(10, result.UsedCount);
    }

    [Fact]
    public void IsFull_UsedEqualsTotal_ReturnsTrue()
    {
        var cap = Capacity.From(10, 10);

        Assert.True(cap.IsFull());
    }

    [Fact]
    public void IsFull_UsedExceedsTotal_ReturnsTrue()
    {
        var cap = Capacity.From(10, 15);

        Assert.True(cap.IsFull());
    }

    [Fact]
    public void IsFull_UsedLessThanTotal_ReturnsFalse()
    {
        var cap = Capacity.From(10, 5);

        Assert.False(cap.IsFull());
    }

    [Theory]
    [InlineData(100, 80, 0.8, true)]
    [InlineData(100, 79, 0.8, false)]
    [InlineData(100, 100, 1.0, true)]
    [InlineData(100, 50, 0.5, true)]
    public void IsNearFull_ReturnsCorrectResult(int total, int used, decimal threshold, bool expected)
    {
        var cap = Capacity.From(total, used);

        Assert.Equal(expected, cap.IsNearFull(threshold));
    }

    [Fact]
    public void IsNearFull_ZeroTotal_ReturnsFalse()
    {
        var cap = Capacity.From(0, 0);

        Assert.False(cap.IsNearFull(0.5m));
    }

    [Fact]
    public void ToString_ReturnsUsedSlashTotal()
    {
        var cap = Capacity.From(100, 25);

        Assert.Equal("25/100", cap.ToString());
    }

    [Fact]
    public void Equality_SameTotalAndUsed_AreEqual()
    {
        var a = Capacity.From(100, 50);
        var b = Capacity.From(100, 50);

        Assert.Equal(a, b);
    }
}

#endregion

#region QRCodePayload

public class QRCodePayloadTests
{
    [Fact]
    public void Create_ValidArgs_CreatesPayload()
    {
        var payload = QRCodePayload.Create("REF1", "EVT1", "CAT1", "SIG1");

        Assert.Equal("REF1", payload.TicketRef);
        Assert.Equal("EVT1", payload.EventId);
        Assert.Equal("CAT1", payload.CategoryId);
        Assert.Equal("SIG1", payload.Signature);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var payload = QRCodePayload.Create(" REF1 ", " EVT1 ", " CAT1 ", " SIG1 ");

        Assert.Equal("REF1", payload.TicketRef);
        Assert.Equal("EVT1", payload.EventId);
        Assert.Equal("CAT1", payload.CategoryId);
        Assert.Equal("SIG1", payload.Signature);
    }

    [Theory]
    [InlineData("", "e", "c", "s")]
    [InlineData("r", "", "c", "s")]
    [InlineData("r", "e", "", "s")]
    [InlineData("r", "e", "c", "")]
    public void Create_EmptyArg_ThrowsArgumentException(
        string ticketRef, string eventId, string categoryId, string signature)
    {
        Assert.Throws<ArgumentException>(() =>
            QRCodePayload.Create(ticketRef, eventId, categoryId, signature));
    }

    [Fact]
    public void From_ValidString_CreatesBackwardCompatiblePayload()
    {
        var payload = QRCodePayload.From("some-ticket-ref");

        Assert.Equal("some-ticket-ref", payload.TicketRef);
        Assert.Equal(string.Empty, payload.EventId);
        Assert.Equal(string.Empty, payload.CategoryId);
        Assert.Equal(string.Empty, payload.Signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => QRCodePayload.From(value!));
    }

    [Fact]
    public void Encode_ReturnsColonSeparatedString()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "SIG");

        Assert.Equal("REF:EVT:CAT:SIG", payload.Encode());
    }

    [Fact]
    public void Value_EqualsEncode()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "SIG");

        Assert.Equal(payload.Encode(), payload.Value);
    }

    [Fact]
    public void Sign_ProducesNonEmptySignature()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "placeholder");

        var signed = payload.Sign("my-secret");

        Assert.NotEmpty(signed.Signature);
        Assert.NotEqual("placeholder", signed.Signature);
    }

    [Fact]
    public void Sign_EmptySecret_ThrowsArgumentException()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "SIG");

        Assert.Throws<ArgumentException>(() => payload.Sign(""));
    }

    [Fact]
    public void Verify_WithCorrectSecret_ReturnsTrue()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "placeholder");
        var signed = payload.Sign("my-secret");

        Assert.True(signed.Verify("my-secret"));
    }

    [Fact]
    public void Verify_WithWrongSecret_ReturnsFalse()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "placeholder");
        var signed = payload.Sign("my-secret");

        Assert.False(signed.Verify("wrong-secret"));
    }

    [Fact]
    public void Sign_DeterministicForSameInput()
    {
        var p1 = QRCodePayload.Create("REF", "EVT", "CAT", "x").Sign("secret");
        var p2 = QRCodePayload.Create("REF", "EVT", "CAT", "y").Sign("secret");

        Assert.Equal(p1.Signature, p2.Signature);
    }

    [Fact]
    public void ToString_ReturnsEncodedValue()
    {
        var payload = QRCodePayload.Create("REF", "EVT", "CAT", "SIG");

        Assert.Equal("REF:EVT:CAT:SIG", payload.ToString());
    }
}

#endregion

#region ScanResult

public class ScanResultTests
{
    [Fact]
    public void Create_ValidArgs_CreatesResult()
    {
        var result = ScanResult.Create(ScanStatus.Valid, "Ticket valid", "REF1", "VIP");

        Assert.Equal(ScanStatus.Valid, result.Status);
        Assert.Equal("Ticket valid", result.Message);
        Assert.Equal("REF1", result.TicketRef);
        Assert.Equal("VIP", result.Category);
    }

    [Fact]
    public void Create_OptionalFieldsDefaultToNull()
    {
        var result = ScanResult.Create(ScanStatus.Invalid, "Bad ticket");

        Assert.Null(result.TicketRef);
        Assert.Null(result.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_EmptyMessage_ThrowsArgumentException(string? message)
    {
        Assert.Throws<ArgumentException>(() =>
            ScanResult.Create(ScanStatus.Valid, message!));
    }

    [Theory]
    [InlineData("VALID", ScanStatus.Valid)]
    [InlineData("INVALID", ScanStatus.Invalid)]
    [InlineData("DUPLICATE", ScanStatus.AlreadyUsed)]
    [InlineData("ALREADYUSED", ScanStatus.AlreadyUsed)]
    [InlineData("EXPIRED", ScanStatus.Expired)]
    public void From_KnownValues_MapsCorrectly(string value, ScanStatus expected)
    {
        var result = ScanResult.From(value);

        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void From_CaseInsensitive()
    {
        var result = ScanResult.From("valid");

        Assert.Equal(ScanStatus.Valid, result.Status);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var result = ScanResult.From("  VALID  ");

        Assert.Equal(ScanStatus.Valid, result.Status);
    }

    [Fact]
    public void From_UnknownValue_DefaultsToInvalid()
    {
        var result = ScanResult.From("SOMETHING");

        Assert.Equal(ScanStatus.Invalid, result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => ScanResult.From(value!));
    }

    [Fact]
    public void Value_ReturnsUppercaseStatusString()
    {
        var result = ScanResult.Create(ScanStatus.Valid, "ok");

        Assert.Equal("VALID", result.Value);
    }

    [Fact]
    public void Value_AlreadyUsed_ReturnsALREADYUSED()
    {
        var result = ScanResult.Create(ScanStatus.AlreadyUsed, "dup");

        Assert.Equal("ALREADYUSED", result.Value);
    }
}

#endregion

#region WalletBalance

public class WalletBalanceTests
{
    [Fact]
    public void Create_ValidArgs_CreatesBalance()
    {
        var available = Money.From(1000);
        var pending = Money.From(200);

        var balance = WalletBalance.Create(available, pending);

        Assert.Equal(1000m, balance.Available.Amount);
        Assert.Equal(200m, balance.Pending.Amount);
    }

    [Fact]
    public void AddPending_IncreasesPendingAmount()
    {
        var balance = WalletBalance.Create(Money.From(1000), Money.From(200));

        var updated = balance.AddPending(Money.From(100));

        Assert.Equal(300m, updated.Pending.Amount);
        Assert.Equal(1000m, updated.Available.Amount);
    }

    [Fact]
    public void MovePendingToAvailable_TransfersCorrectly()
    {
        var balance = WalletBalance.Create(Money.From(1000), Money.From(500));

        var updated = balance.MovePendingToAvailable(Money.From(200));

        Assert.Equal(1200m, updated.Available.Amount);
        Assert.Equal(300m, updated.Pending.Amount);
    }

    [Fact]
    public void MovePendingToAvailable_AmountEqualsPending_Throws()
    {
        var balance = WalletBalance.Create(Money.From(1000), Money.From(500));

        Assert.Throws<BusinessRuleValidationException>(() =>
            balance.MovePendingToAvailable(Money.From(500)));
    }

    [Fact]
    public void MovePendingToAvailable_AmountExceedsPending_Throws()
    {
        var balance = WalletBalance.Create(Money.From(1000), Money.From(200));

        Assert.Throws<BusinessRuleValidationException>(() =>
            balance.MovePendingToAvailable(Money.From(300)));
    }

    [Fact]
    public void Withdraw_DeductsFromAvailable()
    {
        var balance = WalletBalance.Create(Money.From(1000), Money.From(100));

        var updated = balance.Withdraw(Money.From(300));

        Assert.Equal(700m, updated.Available.Amount);
        Assert.Equal(100m, updated.Pending.Amount);
    }

    [Fact]
    public void Withdraw_AmountEqualsAvailable_Throws()
    {
        var balance = WalletBalance.Create(Money.From(500), Money.From(100));

        Assert.Throws<BusinessRuleValidationException>(() =>
            balance.Withdraw(Money.From(500)));
    }

    [Fact]
    public void Withdraw_AmountExceedsAvailable_Throws()
    {
        var balance = WalletBalance.Create(Money.From(500), Money.From(100));

        Assert.Throws<BusinessRuleValidationException>(() =>
            balance.Withdraw(Money.From(600)));
    }
}

#endregion

#region TicketReference

public class TicketReferenceTests
{
    [Fact]
    public void From_ValidValue_NormalizesToUpperCase()
    {
        var ticketRef = TicketReference.From("abc-123");

        Assert.Equal("ABC-123", ticketRef.Value);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var ticketRef = TicketReference.From("  ref  ");

        Assert.Equal("REF", ticketRef.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => TicketReference.From(value!));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var ticketRef = TicketReference.From("ref-1");

        Assert.Equal("REF-1", ticketRef.ToString());
    }

    [Fact]
    public void Equality_SameUpperCaseValue_AreEqual()
    {
        var a = TicketReference.From("abc");
        var b = TicketReference.From("ABC");

        Assert.Equal(a, b);
    }
}

#endregion

#region DeviceId

public class DeviceIdTests
{
    [Fact]
    public void From_ValidValue_CreatesDeviceId()
    {
        var device = DeviceId.From("device-001");

        Assert.Equal("device-001", device.Value);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var device = DeviceId.From("  dev  ");

        Assert.Equal("dev", device.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => DeviceId.From(value!));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = DeviceId.From("dev-1");
        var b = DeviceId.From("dev-1");

        Assert.Equal(a, b);
    }
}

#endregion

#region Channel

public class ChannelTests
{
    [Fact]
    public void From_ValidValue_NormalizesToUpperCase()
    {
        var channel = Channel.From("sms");

        Assert.Equal("SMS", channel.Value);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var channel = Channel.From("  email  ");

        Assert.Equal("EMAIL", channel.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => Channel.From(value!));
    }

    [Fact]
    public void Equality_SameUpperCaseValue_AreEqual()
    {
        var a = Channel.From("sms");
        var b = Channel.From("SMS");

        Assert.Equal(a, b);
    }
}

#endregion

#region MessageTemplate

public class MessageTemplateTests
{
    [Fact]
    public void From_ValidCode_NormalizesToUpperCase()
    {
        var template = MessageTemplate.From("welcome");

        Assert.Equal("WELCOME", template.Code);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var template = MessageTemplate.From("  otp_sent  ");

        Assert.Equal("OTP_SENT", template.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => MessageTemplate.From(value!));
    }

    [Fact]
    public void Equality_SameUpperCaseCode_AreEqual()
    {
        var a = MessageTemplate.From("otp");
        var b = MessageTemplate.From("OTP");

        Assert.Equal(a, b);
    }
}

#endregion

#region TransactionId

public class TransactionIdTests
{
    [Fact]
    public void From_ValidValue_CreatesTransactionId()
    {
        var txn = TransactionId.From("txn-abc-123");

        Assert.Equal("txn-abc-123", txn.Value);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var txn = TransactionId.From("  txn-1  ");

        Assert.Equal("txn-1", txn.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => TransactionId.From(value!));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var txn = TransactionId.From("txn-1");

        Assert.Equal("txn-1", txn.ToString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = TransactionId.From("txn-1");
        var b = TransactionId.From("txn-1");

        Assert.Equal(a, b);
    }
}

#endregion

#region PaymentMethod

public class PaymentMethodTests
{
    [Fact]
    public void From_ValidValue_NormalizesToUpperCase()
    {
        var method = PaymentMethod.From("orange-money");

        Assert.Equal("ORANGE-MONEY", method.Value);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var method = PaymentMethod.From("  cash  ");

        Assert.Equal("CASH", method.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => PaymentMethod.From(value!));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var method = PaymentMethod.From("cash");

        Assert.Equal("CASH", method.ToString());
    }

    [Fact]
    public void Equality_SameUpperCaseValue_AreEqual()
    {
        var a = PaymentMethod.From("cash");
        var b = PaymentMethod.From("CASH");

        Assert.Equal(a, b);
    }
}

#endregion

#region RecipientContact

public class RecipientContactTests
{
    [Fact]
    public void From_ValidValue_CreatesContact()
    {
        var contact = RecipientContact.From("user@example.com");

        Assert.Equal("user@example.com", contact.Value);
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var contact = RecipientContact.From("  test@test.com  ");

        Assert.Equal("test@test.com", contact.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => RecipientContact.From(value!));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = RecipientContact.From("test@test.com");
        var b = RecipientContact.From("test@test.com");

        Assert.Equal(a, b);
    }
}

#endregion

#region Identity Types (EventId, OrderId, UserId)

public class EventIdTests
{
    [Fact]
    public void NewId_CreatesUniqueIds()
    {
        var a = EventId.NewId();
        var b = EventId.NewId();

        Assert.NotEqual(a.Value, b.Value);
        Assert.NotEqual(Guid.Empty, a.Value);
    }

    [Fact]
    public void From_SpecificGuid_CreatesEventId()
    {
        var guid = Guid.NewGuid();

        var eventId = EventId.From(guid);

        Assert.Equal(guid, eventId.Value);
    }

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(EventId.From(guid), EventId.From(guid));
    }

    [Fact]
    public void Equality_DifferentGuids_AreNotEqual()
    {
        Assert.NotEqual(EventId.NewId(), EventId.NewId());
    }
}

public class OrderIdTests
{
    [Fact]
    public void NewId_CreatesUniqueIds()
    {
        var a = OrderId.NewId();
        var b = OrderId.NewId();

        Assert.NotEqual(a.Value, b.Value);
        Assert.NotEqual(Guid.Empty, a.Value);
    }

    [Fact]
    public void From_SpecificGuid_CreatesOrderId()
    {
        var guid = Guid.NewGuid();

        var orderId = OrderId.From(guid);

        Assert.Equal(guid, orderId.Value);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var orderId = OrderId.From(guid);

        Assert.Equal(guid.ToString(), orderId.ToString());
    }

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(OrderId.From(guid), OrderId.From(guid));
    }
}

public class UserIdTests
{
    [Fact]
    public void NewId_CreatesUniqueIds()
    {
        var a = UserId.NewId();
        var b = UserId.NewId();

        Assert.NotEqual(a.Value, b.Value);
        Assert.NotEqual(Guid.Empty, a.Value);
    }

    [Fact]
    public void FromGuid_SpecificGuid_CreatesUserId()
    {
        var guid = Guid.NewGuid();

        var userId = UserId.FromGuid(guid);

        Assert.Equal(guid, userId.Value);
    }

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(UserId.FromGuid(guid), UserId.FromGuid(guid));
    }

    [Fact]
    public void Equality_DifferentGuids_AreNotEqual()
    {
        Assert.NotEqual(UserId.NewId(), UserId.NewId());
    }
}

#endregion
