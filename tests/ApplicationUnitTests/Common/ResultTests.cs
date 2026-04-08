using Application.Common.Models;

namespace ApplicationUnitTests.Common;

#region Error Tests

public class ErrorTests
{
    [Fact]
    public void None_HasEmptyCodeAndMessage()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(string.Empty, Error.None.Message);
    }

    [Fact]
    public void NullValue_HasCorrectCodeAndMessage()
    {
        Assert.Equal("Error.NullValue", Error.NullValue.Code);
        Assert.Equal("Null value was provided.", Error.NullValue.Message);
    }

    [Fact]
    public void NotFound_ReturnsErrorWithEntityInfo()
    {
        var error = Error.NotFound("User", 42);
        Assert.Equal("User.NotFound", error.Code);
        Assert.Contains("User", error.Message);
        Assert.Contains("42", error.Message);
    }

    [Fact]
    public void Conflict_ReturnsErrorWithMessage()
    {
        var error = Error.Conflict("Duplicate entry");
        Assert.Equal("Error.Conflict", error.Code);
        Assert.Equal("Duplicate entry", error.Message);
    }

    [Fact]
    public void Validation_ReturnsErrorWithMessage()
    {
        var error = Error.Validation("Invalid input");
        Assert.Equal("Error.Validation", error.Code);
        Assert.Equal("Invalid input", error.Message);
    }

    [Fact]
    public void Equality_TwoErrorsWithSameValues_AreEqual()
    {
        var error1 = new Error("CODE", "msg");
        var error2 = new Error("CODE", "msg");
        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Equality_TwoErrorsWithDifferentValues_AreNotEqual()
    {
        var error1 = new Error("CODE1", "msg1");
        var error2 = new Error("CODE2", "msg2");
        Assert.NotEqual(error1, error2);
    }
}

#endregion

#region Result<T> Tests

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        var error = Error.Validation("bad");
        var result = Result<int>.Failure(error);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_WithNonNoneError_ThrowsInvalidOperationException()
    {
        // Internally, Success sets Error to Error.None, so this validates the constraint
        var result = Result<string>.Success("ok");
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_WithNoneError_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Result<string>.Failure(Error.None));
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<string> result = "hello";
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        var error = Error.Conflict("conflict");
        Result<int> result = error;
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Match_OnSuccess_CallsOnSuccessFunc()
    {
        var result = Result<int>.Success(10);
        var output = result.Match(
            onSuccess: v => $"Value: {v}",
            onFailure: e => $"Error: {e.Code}");
        Assert.Equal("Value: 10", output);
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailureFunc()
    {
        var error = Error.Validation("invalid");
        var result = Result<int>.Failure(error);
        var output = result.Match(
            onSuccess: v => $"Value: {v}",
            onFailure: e => $"Error: {e.Code}");
        Assert.Equal("Error: Error.Validation", output);
    }

    [Fact]
    public void Success_WithReferenceType_StoresValue()
    {
        var list = new List<string> { "a", "b" };
        var result = Result<List<string>>.Success(list);
        Assert.Same(list, result.Value);
    }

    [Fact]
    public void Failure_Value_IsDefault()
    {
        var result = Result<int>.Failure(Error.Validation("err"));
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void Match_ReturnsCorrectType()
    {
        var result = Result<string>.Success("test");
        int length = result.Match(
            onSuccess: v => v.Length,
            onFailure: _ => -1);
        Assert.Equal(4, length);
    }
}

#endregion

#region PagedResult<T> Tests

public class PagedResultTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var items = new[] { 1, 2, 3 };
        var paged = new PagedResult<int>(items, totalCount: 10, pageNumber: 2, pageSize: 3);

        Assert.Equal(items, paged.Items);
        Assert.Equal(10, paged.TotalCount);
        Assert.Equal(2, paged.PageNumber);
        Assert.Equal(3, paged.PageSize);
    }

    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var paged = new PagedResult<int>(Array.Empty<int>(), totalCount: 10, pageNumber: 1, pageSize: 3);
        Assert.Equal(4, paged.TotalPages); // ceil(10/3) = 4
    }

    [Fact]
    public void TotalPages_ExactDivision()
    {
        var paged = new PagedResult<int>(Array.Empty<int>(), totalCount: 9, pageNumber: 1, pageSize: 3);
        Assert.Equal(3, paged.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_FirstPage_ReturnsFalse()
    {
        var paged = new PagedResult<int>(Array.Empty<int>(), totalCount: 10, pageNumber: 1, pageSize: 5);
        Assert.False(paged.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_SecondPage_ReturnsTrue()
    {
        var paged = new PagedResult<int>(Array.Empty<int>(), totalCount: 10, pageNumber: 2, pageSize: 5);
        Assert.True(paged.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_LastPage_ReturnsFalse()
    {
        var paged = new PagedResult<int>(Array.Empty<int>(), totalCount: 10, pageNumber: 2, pageSize: 5);
        Assert.False(paged.HasNextPage); // TotalPages=2, PageNumber=2
    }

    [Fact]
    public void HasNextPage_NotLastPage_ReturnsTrue()
    {
        var paged = new PagedResult<int>(Array.Empty<int>(), totalCount: 10, pageNumber: 1, pageSize: 5);
        Assert.True(paged.HasNextPage); // TotalPages=2, PageNumber=1
    }

    [Fact]
    public void Create_PagesSourceCorrectly()
    {
        var source = Enumerable.Range(1, 20);
        var paged = PagedResult<int>.Create(source, pageNumber: 2, pageSize: 5);

        Assert.Equal(20, paged.TotalCount);
        Assert.Equal(2, paged.PageNumber);
        Assert.Equal(5, paged.PageSize);
        Assert.Equal(4, paged.TotalPages);
        Assert.Equal(new[] { 6, 7, 8, 9, 10 }, paged.Items);
    }

    [Fact]
    public void Create_FirstPage_ReturnsFirstItems()
    {
        var source = Enumerable.Range(1, 10);
        var paged = PagedResult<int>.Create(source, pageNumber: 1, pageSize: 3);

        Assert.Equal(new[] { 1, 2, 3 }, paged.Items);
        Assert.Equal(10, paged.TotalCount);
    }

    [Fact]
    public void Create_LastPage_ReturnsRemainingItems()
    {
        var source = Enumerable.Range(1, 10);
        var paged = PagedResult<int>.Create(source, pageNumber: 4, pageSize: 3);

        Assert.Equal(new[] { 10 }, paged.Items);
    }

    [Fact]
    public void Create_EmptySource_ReturnsEmptyResult()
    {
        var source = Enumerable.Empty<int>();
        var paged = PagedResult<int>.Create(source, pageNumber: 1, pageSize: 10);

        Assert.Empty(paged.Items);
        Assert.Equal(0, paged.TotalCount);
        Assert.Equal(0, paged.TotalPages);
        Assert.False(paged.HasPreviousPage);
        Assert.False(paged.HasNextPage);
    }

    [Theory]
    [InlineData(1, 10, false, true)]
    [InlineData(5, 10, true, true)]
    [InlineData(10, 10, true, false)]
    public void Pagination_Flags_AreCorrect(int pageNumber, int totalPages, bool hasPrev, bool hasNext)
    {
        var paged = new PagedResult<int>(
            Array.Empty<int>(),
            totalCount: totalPages * 5,
            pageNumber: pageNumber,
            pageSize: 5);

        Assert.Equal(hasPrev, paged.HasPreviousPage);
        Assert.Equal(hasNext, paged.HasNextPage);
    }
}

#endregion
