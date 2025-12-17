namespace Client.Dtos
{
    /// <summary>
    /// Extension methods for unwrapping API response wrappers
    /// </summary>
    public static class ResponseExtensions
    {
        /// <summary>
        /// Unwraps the data from an ApiResponse, returning the Data property.
        /// Returns null if Success is false or Data is null.
        /// </summary>
        public static T? Unwrap<T>(this ApiResponse<T>? response)
        {
            if (response == null || !response.Success)
                return default;

            return response.Data;
        }

        /// <summary>
        /// Unwraps the data from an ApiResponse, throwing an exception if the response is unsuccessful or null.
        /// </summary>
        /// <param name="response">The API response to unwrap</param>
        /// <param name="errorMessage">Optional custom error message if unwrap fails</param>
        /// <returns>The unwrapped data</returns>
        /// <exception cref="InvalidOperationException">Thrown when response is null, unsuccessful, or data is null</exception>
        public static T UnwrapOrThrow<T>(this ApiResponse<T>? response, string? errorMessage = null)
        {
            if (response == null)
                throw new InvalidOperationException(errorMessage ?? "API response is null");

            if (!response.Success)
                throw new InvalidOperationException(errorMessage ?? response.Message ?? "API request was unsuccessful");

            if (response.Data == null)
                throw new InvalidOperationException(errorMessage ?? "API response data is null");

            return response.Data;
        }

        /// <summary>
        /// Unwraps the data from an ApiResponse, returning a default value if unsuccessful or null.
        /// </summary>
        public static T UnwrapOrDefault<T>(this ApiResponse<T>? response, T defaultValue = default!)
        {
            if (response == null || !response.Success || response.Data == null)
                return defaultValue;

            return response.Data;
        }

        /// <summary>
        /// Unwraps the data from a PaginatedApiResponse, returning the Data property as a list.
        /// Returns an empty list if Success is false or Data is null.
        /// </summary>
        public static List<T> Unwrap<T>(this PaginatedApiResponse<T>? response)
        {
            if (response == null || !response.Success || response.Data == null)
                return new List<T>();

            return response.Data.ToList();
        }

        /// <summary>
        /// Unwraps the data from a PaginatedApiResponse, throwing an exception if the response is unsuccessful or null.
        /// </summary>
        public static List<T> UnwrapOrThrow<T>(this PaginatedApiResponse<T>? response, string? errorMessage = null)
        {
            if (response == null)
                throw new InvalidOperationException(errorMessage ?? "Paginated API response is null");

            if (!response.Success)
                throw new InvalidOperationException(errorMessage ?? response.Message ?? "Paginated API request was unsuccessful");

            if (response.Data == null)
                throw new InvalidOperationException(errorMessage ?? "Paginated API response data is null");

            return response.Data.ToList();
        }

        /// <summary>
        /// Gets the pagination metadata from a PaginatedApiResponse.
        /// Returns null if the response is null or unsuccessful.
        /// </summary>
        public static PaginationMetadata? GetPaginationMetadata<T>(this PaginatedApiResponse<T>? response)
        {
            if (response == null || !response.Success)
                return null;

            return new PaginationMetadata
            {
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalPages = response.TotalPages,
                TotalRecords = response.TotalRecords,
                HasPreviousPage = response.HasPreviousPage,
                HasNextPage = response.HasNextPage
            };
        }

        /// <summary>
        /// Checks if the API response is successful.
        /// </summary>
        public static bool IsSuccess<T>(this ApiResponse<T>? response)
        {
            return response != null && response.Success;
        }

        /// <summary>
        /// Checks if the paginated API response is successful.
        /// </summary>
        public static bool IsSuccess<T>(this PaginatedApiResponse<T>? response)
        {
            return response != null && response.Success;
        }

        /// <summary>
        /// Gets the error message from an ApiResponse or ApiError.
        /// </summary>
        public static string GetErrorMessage<T>(this ApiResponse<T>? response)
        {
            if (response == null)
                return "Response is null";

            return response.Message ?? "Unknown error";
        }
    }

    /// <summary>
    /// Pagination metadata extracted from PaginatedApiResponse
    /// </summary>
    public class PaginationMetadata
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}

