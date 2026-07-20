using System;
using System.Text.RegularExpressions;
using FeatureFlags.Utilities;
using Xunit;

namespace FeatureFlags.Tests
{
    public class PaginationHelperTests
    {
        private static (int start, int end, int total) ParseRange(string range)
        {
            // Expected format: "X-Y of Z"
            var match = Regex.Match(range, @"^\s*(\d+)-(\d+)\s+of\s+(\d+)\s*$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new FormatException($"Unexpected range format: '{range}'");
            }

            return (
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value)
            );
        }

        [Fact]
        public void GetItemRange_NormalPage_ReturnsCorrectRange()
        {
            // Arrange
            int pageNumber = 2;
            int pageSize = 10;
            int totalCount = 35;

            // Act
            string result = PaginationHelper.GetItemRange(pageNumber, pageSize, totalCount);

            // Assert
            var (start, end, total) = ParseRange(result);
            Assert.Equal(11, start);
            Assert.Equal(20, end);
            Assert.Equal(totalCount, total);
        }

        [Fact]
        public void GetItemRange_LastPartialPage_ReturnsCorrectRange()
        {
            // Arrange
            int pageNumber = 4;
            int pageSize = 10;
            int totalCount = 35;

            // Act
            string result = PaginationHelper.GetItemRange(pageNumber, pageSize, totalCount);

            // Assert
            var (start, end, total) = ParseRange(result);
            Assert.Equal(31, start);
            Assert.Equal(35, end);
            Assert.Equal(totalCount, total);
        }

        [Fact]
        public void GetItemRange_PageBeyondRange_ReturnsRangeWithStartGreaterThanEnd()
        {
            // Arrange
            int pageNumber = 5; // beyond the last page (only 4 pages exist)
            int pageSize = 10;
            int totalCount = 35;

            // Act
            string result = PaginationHelper.GetItemRange(pageNumber, pageSize, totalCount);

            // Assert
            var (start, end, total) = ParseRange(result);
            Assert.Equal(41, start); // 5 * 10 - 9
            Assert.Equal(35, end);   // capped at totalCount
            Assert.Equal(totalCount, total);
        }

        [Fact]
        public void GetItemRange_PageSizeZero_ThrowsArgumentException()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 0;
            int totalCount = 35;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                PaginationHelper.GetItemRange(pageNumber, pageSize, totalCount));
        }

        [Fact]
        public void GetItemRange_PageSizeNegative_ThrowsArgumentException()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = -5;
            int totalCount = 35;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                PaginationHelper.GetItemRange(pageNumber, pageSize, totalCount));
        }

        [Fact]
        public void GetItemRange_TotalCountMath_CalculatesCorrectly()
        {
            // Arrange
            int pageNumber = 3;
            int pageSize = 7;
            int totalCount = 20;

            // Act
            string result = PaginationHelper.GetItemRange(pageNumber, pageSize, totalCount);

            // Assert
            var (start, end, total) = ParseRange(result);
            Assert.Equal(15, start); // (3-1)*7 + 1
            Assert.Equal(20, end);   // capped at totalCount
            Assert.Equal(totalCount, total);
        }
    }
}
