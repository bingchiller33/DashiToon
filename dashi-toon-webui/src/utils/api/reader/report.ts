import { Result } from "@/utils/err";
import { fetchApi } from "@/utils/api";

export async function reportComment(commentId: string, reason: string): Promise<Result<void>> {
    return await fetchApi("POST", "/api/Moderation/comments/reports", {
        commentId,
        reason,
    });
}
export async function reportReview(reviewId: string, reason: string): Promise<Result<void>> {
    return await fetchApi("POST", "/api/Moderation/reviews/reports", {
        reviewId,
        reason,
    });
}

export async function reportChapter(chapterId: number, reason: string): Promise<Result<void>> {
    return await fetchApi("POST", "/api/Moderation/chapters/reports", {
        chapterId,
        reason,
    });
}

export async function reportSeries(seriesId: number, reason: string): Promise<Result<void>> {
    return await fetchApi("POST", "/api/Moderation/series/reports", {
        seriesId,
        reason,
    });
}
