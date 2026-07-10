# PaginationHelper

A utility class for normalizing, calculating, and applying pagination parameters to in-memory collections and metadata. It provides methods to validate and normalize paging inputs, compute pagination metadata, and split collections into pages while maintaining type safety.

## API

### `ValidateAndNormalizePaging(int pageNumber, int pageSize, int totalCount)`

Validates and normalizes paging parameters to ensure they fall within acceptable ranges. Returns a tuple containing the validated and normalized page number and page size.

- **Parameters**
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The requested number of items per page.
  - `totalCount`: The total number of items available.
- **Return Value**: A tuple `(int pageNumber, int pageSize)` with normalized values.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1.
  - Throws `ArgumentException` if `pageSize` exceeds `totalCount` when `totalCount` is non-zero.

---

### `CalculateOffset(int pageNumber, int pageSize)`

Calculates the zero-based offset for a given page number and page size.

- **Parameters**
  - `pageNumber`: The 1-based page number.
  - `pageSize`: The number of items per page.
- **Return Value**: The zero-based offset as an integer.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1.

---

### `CreateMetadata(int pageNumber, int pageSize, int totalCount)`

Creates pagination metadata based on the current page, page size, and total item count.

- **Parameters**
  - `pageNumber`: The current 1-based page number.
  - `pageSize`: The number of items per page.
  - `totalCount`: The total number of items available.
- **Return Value**: A `PaginationMetadata` instance containing pagination details.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1.
  - Throws `ArgumentException` if `pageSize` exceeds `totalCount` when `totalCount` is non-zero.

---

### `PaginateInMemory<T>(IEnumerable<T> source, int pageNumber, int pageSize)`

Splits an in-memory collection into a single page of results based on the given page number and page size.

- **Type Parameters**
  - `T`: The type of elements in the source collection.
- **Parameters**
  - `source`: The collection to paginate.
  - `pageNumber`: The 1-based page number to retrieve.
  - `pageSize`: The number of items per page.
- **Return Value**: An `IEnumerable<T>` containing the items for the requested page.
- **Exceptions**
  - Throws `ArgumentNullException` if `source` is `null`.
  - Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1.

---

### `GetItemRange(int pageNumber, int pageSize, int totalCount)`

Returns a string describing the range of items on the current page (e.g., "1-10 of 100").

- **Parameters**
  - `pageNumber`: The 1-based page number.
  - `pageSize`: The number of items per page.
  - `totalCount`: The total number of items available.
- **Return Value**: A string representing the item range.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1.
  - Throws `ArgumentException` if `pageSize` exceeds `totalCount` when `totalCount` is non-zero.

---

### Properties

#### `PageNumber`
Gets the current 1-based page number.

- **Type**: `int`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---

#### `PageSize`
Gets the number of items per page.

- **Type**: `int`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---
#### `TotalCount`
Gets the total number of items available.

- **Type**: `int`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---
#### `TotalPages`
Gets the total number of pages available.

- **Type**: `int`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---
#### `HasNextPage`
Indicates whether there is a next page available.

- **Type**: `bool`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---
#### `HasPreviousPage`
Indicates whether there is a previous page available.

- **Type**: `bool`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---
#### `Items`
Gets the list of items on the current page.

- **Type**: `List<T>`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---
#### `Pagination`
Gets the pagination metadata for the current instance.

- **Type**: `PaginationMetadata`
- **Exceptions**
  - Throws `InvalidOperationException` if accessed when the instance is not properly initialized.

---

## Usage

### Example 1: Basic Pagination
