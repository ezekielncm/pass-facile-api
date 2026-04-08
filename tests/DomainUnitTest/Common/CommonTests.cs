using Domain.Common;

namespace DomainUnitTest.Common;

public class GuardAgainstNullTests
{
    [Fact]
    public void Null_WithNullValue_ThrowsArgumentNullException()
    {
        string? value = null;
        var ex = Assert.Throws<ArgumentNullException>(() => Guard.Against.Null(value, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void Null_WithNonNullValue_DoesNotThrow()
    {
        Guard.Against.Null("valid", "param");
    }

    [Fact]
    public void Null_WithCustomMessage_IncludesMessage()
    {
        string? value = null;
        var ex = Assert.Throws<ArgumentNullException>(() =>
            Guard.Against.Null(value, "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstNullOrEmptyStringTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NullOrEmpty_WithInvalidString_ThrowsArgumentException(string? value)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.NullOrEmpty(value!, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void NullOrEmpty_WithValidString_DoesNotThrow()
    {
        Guard.Against.NullOrEmpty("hello", "param");
    }

    [Fact]
    public void NullOrEmpty_String_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.NullOrEmpty("", "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstNullOrEmptyCollectionTests
{
    [Fact]
    public void NullOrEmpty_WithNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<int>? value = null;
        Assert.Throws<ArgumentNullException>(() =>
            Guard.Against.NullOrEmpty(value!, "param"));
    }

    [Fact]
    public void NullOrEmpty_WithEmptyCollection_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.NullOrEmpty(Array.Empty<int>(), "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void NullOrEmpty_WithNonEmptyCollection_DoesNotThrow()
    {
        Guard.Against.NullOrEmpty(new[] { 1, 2, 3 }, "param");
    }

    [Fact]
    public void NullOrEmpty_Collection_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.NullOrEmpty(Array.Empty<string>(), "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstNegativeTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Negative_Int_WithNegativeValue_ThrowsArgumentException(int value)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.Negative(value, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Negative_Int_WithNonNegativeValue_DoesNotThrow(int value)
    {
        Guard.Against.Negative(value, "param");
    }

    [Fact]
    public void Negative_Int_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.Negative(-1, "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }

    [Fact]
    public void Negative_Decimal_WithNegativeValue_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.Negative(-0.01m, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100.5)]
    public void Negative_Decimal_WithNonNegativeValue_DoesNotThrow(double value)
    {
        Guard.Against.Negative((decimal)value, "param");
    }
}

public class GuardAgainstNegativeOrZeroTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void NegativeOrZero_WithInvalidValue_ThrowsArgumentException(int value)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.NegativeOrZero(value, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void NegativeOrZero_WithPositiveValue_DoesNotThrow(int value)
    {
        Guard.Against.NegativeOrZero(value, "param");
    }

    [Fact]
    public void NegativeOrZero_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.NegativeOrZero(0, "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstOutOfRangeTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void OutOfRange_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(int value)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Guard.Against.OutOfRange(value, "param", 0, 10));
        Assert.Equal("param", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void OutOfRange_WithInRangeValue_DoesNotThrow(int value)
    {
        Guard.Against.OutOfRange(value, "param", 0, 10);
    }

    [Fact]
    public void OutOfRange_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Guard.Against.OutOfRange(20, "param", 0, 10, "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }

    [Fact]
    public void OutOfRange_WorksWithStrings()
    {
        Guard.Against.OutOfRange("b", "param", "a", "z");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Guard.Against.OutOfRange("z", "param", "a", "m"));
    }
}

public class GuardAgainstStringLengthTests
{
    [Fact]
    public void StringTooLong_WithTooLongString_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.StringTooLong("hello world", "param", 5));
        Assert.Equal("param", ex.ParamName);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("hi")]
    [InlineData("")]
    public void StringTooLong_WithValidString_DoesNotThrow(string value)
    {
        Guard.Against.StringTooLong(value, "param", 5);
    }

    [Fact]
    public void StringTooLong_WithNullString_DoesNotThrow()
    {
        Guard.Against.StringTooLong(null!, "param", 5);
    }

    [Fact]
    public void StringTooLong_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.StringTooLong("toolong", "param", 3, "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }

    [Fact]
    public void StringTooShort_WithTooShortString_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.StringTooShort("hi", "param", 5));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void StringTooShort_WithValidString_DoesNotThrow()
    {
        Guard.Against.StringTooShort("hello", "param", 5);
    }

    [Fact]
    public void StringTooShort_WithNullString_DoesNotThrow()
    {
        Guard.Against.StringTooShort(null!, "param", 5);
    }

    [Fact]
    public void StringTooShort_WithEmptyString_DoesNotThrow()
    {
        Guard.Against.StringTooShort("", "param", 5);
    }

    [Fact]
    public void StringTooShort_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.StringTooShort("ab", "param", 5, "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstDateTests
{
    [Fact]
    public void PastDate_WithPastDate_ThrowsArgumentException()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.PastDate(past, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void PastDate_WithFutureDate_DoesNotThrow()
    {
        var future = DateTime.UtcNow.AddDays(1);
        Guard.Against.PastDate(future, "param");
    }

    [Fact]
    public void PastDate_WithCustomMessage_IncludesMessage()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.PastDate(past, "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }

    [Fact]
    public void FutureDate_WithFutureDate_ThrowsArgumentException()
    {
        var future = DateTime.UtcNow.AddDays(1);
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.FutureDate(future, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void FutureDate_WithPastDate_DoesNotThrow()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        Guard.Against.FutureDate(past, "param");
    }

    [Fact]
    public void FutureDate_WithCustomMessage_IncludesMessage()
    {
        var future = DateTime.UtcNow.AddDays(1);
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.FutureDate(future, "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstInvalidEmailTests
{
    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void InvalidEmail_WithInvalidFormat_ThrowsArgumentException(string email)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.InvalidEmail(email, "email"));
        Assert.Equal("email", ex.ParamName);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.org")]
    [InlineData("a@b.co")]
    public void InvalidEmail_WithValidFormat_DoesNotThrow(string email)
    {
        Guard.Against.InvalidEmail(email, "email");
    }

    [Fact]
    public void InvalidEmail_WithNullOrEmpty_DoesNotThrow()
    {
        Guard.Against.InvalidEmail(null!, "email");
        Guard.Against.InvalidEmail("", "email");
    }

    [Fact]
    public void InvalidEmail_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.InvalidEmail("bad", "email", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }
}

public class GuardAgainstBooleanTests
{
    [Fact]
    public void False_WithFalseCondition_ThrowsBusinessRuleValidationException()
    {
        var ex = Assert.Throws<BusinessRuleValidationException>(() =>
            Guard.Against.False(false, "Rule violated"));
        Assert.Equal("Rule violated", ex.Message);
    }

    [Fact]
    public void False_WithTrueCondition_DoesNotThrow()
    {
        Guard.Against.False(true, "Rule violated");
    }

    [Fact]
    public void True_WithTrueCondition_ThrowsBusinessRuleValidationException()
    {
        var ex = Assert.Throws<BusinessRuleValidationException>(() =>
            Guard.Against.True(true, "Rule violated"));
        Assert.Equal("Rule violated", ex.Message);
    }

    [Fact]
    public void True_WithFalseCondition_DoesNotThrow()
    {
        Guard.Against.True(false, "Rule violated");
    }
}

public class GuardAgainstInvalidInputTests
{
    [Fact]
    public void InvalidInput_WhenPredicateReturnsFalse_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.InvalidInput(5, x => x > 10, "param"));
        Assert.Equal("param", ex.ParamName);
    }

    [Fact]
    public void InvalidInput_WhenPredicateReturnsTrue_DoesNotThrow()
    {
        Guard.Against.InvalidInput(15, x => x > 10, "param");
    }

    [Fact]
    public void InvalidInput_WithCustomMessage_IncludesMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Guard.Against.InvalidInput("bad", _ => false, "param", "custom msg"));
        Assert.Contains("custom msg", ex.Message);
    }

    [Fact]
    public void InvalidInput_WithStringPredicate_ValidatesCorrectly()
    {
        Guard.Against.InvalidInput("hello", s => s.Length <= 10, "param");
        Assert.Throws<ArgumentException>(() =>
            Guard.Against.InvalidInput("a very long string here", s => s.Length <= 10, "param"));
    }
}

public class GuardEnsureTests
{
    [Fact]
    public void Ensure_WithFalseCondition_ThrowsBusinessRuleValidationException()
    {
        var ex = Assert.Throws<BusinessRuleValidationException>(() =>
            Guard.Ensure(false, "ERR_CODE", "Something went wrong"));
        Assert.Equal("ERR_CODE", ex.Code);
        Assert.Equal("Something went wrong", ex.Message);
    }

    [Fact]
    public void Ensure_WithTrueCondition_DoesNotThrow()
    {
        Guard.Ensure(true, "ERR_CODE", "Something went wrong");
    }
}

public class BusinessRuleValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsCodeAndMessage()
    {
        var ex = new BusinessRuleValidationException("RULE_001", "Business rule broken");
        Assert.Equal("RULE_001", ex.Code);
        Assert.Equal("Business rule broken", ex.Message);
    }

    [Fact]
    public void IsDomainException()
    {
        var ex = new BusinessRuleValidationException("RULE", "msg");
        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
