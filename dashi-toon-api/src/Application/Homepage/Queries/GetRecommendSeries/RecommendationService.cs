namespace DashiToon.Api.Application.Homepage.Queries.GetRecommendSeries;

public static class RecommendationService
{
    // Lấy danh sách các phim gợi ý cho một người dùng
    public static int[] GetRecommendations(
        string userId,
        IEnumerable<UserLikedSeriesDto> userLikedSeriesInput,
        int k)
    {
        // Chuyển đổi dữ liệu `userLikedSeries` thành một Dictionary để dễ xử lý
        Dictionary<string, HashSet<int>>? userLikedSeries = userLikedSeriesInput
            .ToDictionary(
                uls => uls.UserId,
                uls => uls.SeriesArray.ToHashSet()
            );

        // Kiểm tra xem người dùng có tồn tại trong danh sách không
        if (!userLikedSeries.TryGetValue(userId, out HashSet<int>? currentUserLiked))
        {
            return [];
        }

        // Lấy danh sách phim mà người dùng đã thích

        // Tính độ tương đồng Jaccard giữa userId và các người dùng khác
        Dictionary<string, double>? similarities = new();
        foreach (string? otherUserId in userLikedSeries.Keys)
        {
            if (otherUserId != userId)
            {
                double similarity = JaccardSimilarity(currentUserLiked, userLikedSeries[otherUserId]);
                similarities[otherUserId] = similarity;
            }
        }

        // Lấy top k người dùng tương đồng
        List<string>? similarUsers = similarities
            .OrderByDescending(s => s.Value)
            .Take(k)
            .Select(s => s.Key)
            .ToList();

        // Tìm các series từ những người dùng tương đồng
        Dictionary<int, int> candidateSeries = new();
        foreach (int movieId in similarUsers
                     .SelectMany(similarUser => userLikedSeries[similarUser]
                         .Where(movieId => !currentUserLiked
                             .Contains(movieId))))
        {
            candidateSeries.TryAdd(movieId, 0);

            candidateSeries[movieId]++;
        }

        // Sắp xếp các phim theo số lần xuất hiện (ưu tiên phim xuất hiện nhiều)
        return candidateSeries
            .OrderByDescending(c => c.Value)
            .Take(10)
            .Select(c => c.Key)
            .ToArray();
    }

    private static double JaccardSimilarity(HashSet<int> setA, HashSet<int> setB)
    {
        int intersection = setA.Intersect(setB).Count();
        int union = setA.Union(setB).Count();
        return (double)intersection / union;
    }
}
