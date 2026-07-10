# DateTimeExtensions

Provides a set of extension methods for `System.DateTime` that simplify common date‑time calculations such as converting to/from Unix timestamps, retrieving period boundaries, checking ranges, and formatting relative timestamps.

## API

### ToUnixTimestamp
```csharp
public static long ToUnixTimestamp(this DateTime dateTime)
```
**Purpose** – Returns the number of seconds that have elapsed since 00:00:00 UTC on 1 January 1970 (Unix epoch).  
**Parameters** – `dateTime`: The DateTime instance to convert; its Kind is treated as UTC if it is `DateTimeKind.Utc`, otherwise it is assumed to be local time and converted to UTC.  
**Return value** – A signed 64‑bit integer representing the elapsed seconds.  
**Exceptions** – `ArgumentOutOfRangeException` if the resulting timestamp lies outside the range of `Int64`.

### FromUnixTimestamp
```csharp
public static DateTime FromUnixTimestamp(this long unixTimestamp)
```
**Purpose** – Creates a DateTime from a Unix timestamp (seconds since epoch).  
**Parameters** – `unixTimestamp`: The number of seconds (positive or negative) since 00:00:00 UTC on 1 January 1970.  
**Return value** – A DateTime representing the corresponding UTC moment, with `Kind` set to `Utc`.  
**Exceptions** – `ArgumentOutOfRangeException` if the timestamp cannot be represented as a DateTime (i.e., outside `DateTime.MinValue`/`MaxValue`).

### StartOfDay
```csharp
public static DateTime StartOfDay(this DateTime dateTime)
```
**Purpose** – Returns the DateTime set to the first tick of the day (00:00:00) that contains `dateTime`.  
**Parameters** – `dateTime`: The input date.  
**Return value** – A DateTime with the same Kind as `dateTime` and time set to 00:00:00.  
**Exceptions** – None.

### EndOfDay
```csharp
public static DateTime EndOfDay(this DateTime dateTime)
```
**Purpose** – Returns the DateTime set to the last tick of the day (23:59:59.9999999) that contains `dateTime`.  
**Parameters** – `dateTime`: The input date.  
**Return value** – A DateTime with the same Kind as `dateTime` representing the end of that day.  
**Exceptions** – None.

### StartOfWeek
```csharp
public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Sunday)
```
**Purpose** – Returns the DateTime set to the first tick of the week that contains `dateTime`, based on the supplied start‑of‑week day.  
**Parameters** –  
- `dateTime`: The input date.  
- `startOfWeek`: The DayOfWeek that should be considered the first day of the week (default: Sunday).  
**Return value** – A DateTime with the same Kind as `dateTime` representing midnight on the first day of the week.  
**Exceptions** – `ArgumentException` if `startOfWeek` is not a defined DayOfWeek value.

### StartOfMonth
```csharp
public static DateTime StartOfMonth(this DateTime dateTime)
```
**Purpose** – Returns the DateTime set to the first tick of the month that contains `dateTime`.  
**Parameters** – `dateTime`: The input date.  
**Return value** – A DateTime with the same Kind as `dateTime` representing 00:00:00 on the first day of the month.  
**Exceptions** – None.

### EndOfMonth
```csharp
public static DateTime EndOfMonth(this DateTime dateTime)
```
**Purpose** – Returns the DateTime set to the last tick of the month that contains `dateTime`.  
**Parameters** – `dateTime`: The input date.  
**Return value** – A DateTime with the same Kind as `dateTime` representing the final moment of the month.  
**Exceptions** – None.

### StartOfYear
```csharp
public static DateTime StartOfYear(this DateTime dateTime)
```
**Purpose** – Returns the DateTime set to the first tick of the year that contains `dateTime`.  
**Parameters** – `dateTime`: The input date.  
**Return value** – A DateTime with the same Kind as `dateTime` representing 00:00:00 on January 1 of that year.  
**Exceptions** – None.

### IsBetween
```csharp
public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end, bool inclusive = true)
```
**Purpose** – Determines whether `dateTime` falls within the interval defined by `start` and `end`.  
**Parameters** –  
- `dateTime`: The value to test.  
- `start`: The lower bound of the interval.  
- `end`: The upper bound of the interval.  
- `inclusive`: If true, the bounds are considered part of the interval; otherwise they are exclusive.  
**Return value** – `true` if `dateTime` is between `start` and `end` (respecting the inclusivity flag); otherwise `false`.  
**Exceptions** – `ArgumentException` if `start` is later than `end`.

### GetBusinessDaysBetween
```csharp
public static int GetBusinessDaysBetween(this DateTime start, DateTime end)
```
**Purpose** – Counts the number of weekdays (Monday‑Friday) between two dates.  
**Parameters** –  
- `start`: The start of the period (inclusive).  
- `end`: The end of the period (exclusive).  
**Return value** – An integer representing the count of business days. Returns 0 when `start` equals `end`.  
**Exceptions** – `ArgumentException` if `start` is later than `end`.

### ToRelativeTime
```csharp
public static string ToRelativeTime(this DateTime dateTime)
```
**Purpose** – Produces a human‑readable, approximate string describing how far `dateTime` is from the current moment (e.g., “2 hours ago”, “in 5 minutes”).  
**Parameters** – `dateTime`: The DateTime to format.  
**Return value** – A localized relative time string.  
**Exceptions** – None.

### IsToday
```csharp
public static bool IsToday(this DateTime dateTime)
```
**Purpose** – Checks whether the date component of `dateTime` matches today's date (ignoring time).  
**Parameters** – `dateTime`: The DateTime to evaluate.  
**Return value** – `true` if `dateTime` falls on the current day; otherwise `false`.  
**Exceptions** – None.

### IsPast
```csharp
public static bool IsPast(this DateTime dateTime)
```
**Purpose** – Determines whether `dateTime` occurs before the current moment.  
**Parameters** – `dateTime`: The DateTime to evaluate.  
**Return value** – `true` if `dateTime` is earlier than `DateTime.Now`; otherwise `false`.  
**Exceptions** – None.

### IsFuture
```csharp
public static bool IsFuture(this DateTime dateTime)
```
**Purpose** – Determines whether `dateTime` occurs after the current moment.  
**Parameters** – `dateTime`: The DateTime to evaluate.  
**Return value** – `true` if `dateTime` is later than `DateTime.Now`; otherwise `false`.  
**Exceptions** – None.

### RoundTo
```csharp
public static DateTime RoundTo(this DateTime dateTime, TimeSpan interval)
```
**Purpose** – Rounds `dateTime` to the nearest multiple of the supplied `interval`.  
**Parameters** –  
- `dateTime`: The DateTime to round.  
- `interval`: The TimeSpan representing the rounding step (must be greater than zero).  
**Return value** – A DateTime rounded to the nearest interval; ties are rounded away from zero.  
**Exceptions** – `ArgumentException` if `interval` is less than or equal to `TimeSpan.Zero`.

## Usage

```csharp
using static DateTimeExtensions;

// Convert a DateTime to a Unix timestamp and back
DateTime now = DateTime.UtcNow;
long stamp = now.ToUnixTimestamp();
DateTime roundTrip = stamp.FromUnixTimestamp();
// roundTrip will be equal to now (within second precision)

// Determine if a date is within the last 7 business days
DateTime today = DateTime.Today;
DateTime weekAgo = today.AddDays(-7);
int businessDays = weekAgo.GetBusinessDaysBetween(today);
bool withinWeek = businessDays > 0 && today.IsBetween(weekAgo, today, inclusive: true);
```

```csharp
using static DateTimeExtensions;

// Find the start of the current week (Monday as first day) and format a relative time
DateTime startOfWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
string relative = startOfWeek.ToRelativeTime();
// Example output: "3 days ago" if today is Thursday

// Round a timestamp to the nearest 15‑minute interval
DateTime arbitrary = new DateTime(2025, 11, 2, 14, 7, 30);
DateTime rounded = arbitrary.RoundTo(TimeSpan.FromMinutes(15));
// rounded => 2025-11-02 14:15:00
```

## Notes

- All extension methods operate on the value they are called on and do not modify the original instance; they return a new `DateTime` or primitive value.
- The methods that accept a `DateTime` parameter preserve the `Kind` property of the input unless explicitly noted (e.g., `FromUnixTimestamp` always returns `Utc`).
- Thread safety: Since these methods rely only on their inputs and static system values (`DateTime.Now`), they are safe to call concurrently from multiple threads.
- Edge cases:  
  - `ToUnixTimestamp` and `FromUnixTimestamp` handle dates before the Unix epoch by returning negative timestamps; however, values that would cause the resulting `DateTime` to fall outside the supported .NET range will throw.  
  - `GetBusinessDaysBetween` treats Saturday and Sunday as non‑working days; it does not account for holidays.  
  - `RoundTo` will throw if a zero or negative interval is supplied; the rounding algorithm follows conventional “round half away from zero” semantics.  
  - `IsBetween` expects the lower bound to be chronologically earlier than the upper bound; supplying them reversed results in an `ArgumentException`.  
  - When using `StartOfWeek`, the supplied `startOfWeek` value must be a valid member of the `DayOfWeek` enumeration; otherwise an `ArgumentException` is raised.
