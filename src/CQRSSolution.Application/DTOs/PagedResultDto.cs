namespace CQRSSolution.Application.DTOs;

/// <summary>
///     Represents a paged result for queries, containing a list of items for the current page
///     and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of the items in the paged list.</typeparam>
public class PagedResultDto<T>
{
    public PagedResultDto(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    ///     Gets or sets the list of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    ///     Gets or sets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    ///     Gets or sets the current page number (1-indexed).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    ///     Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Gets the total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    ///     Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    ///     Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}