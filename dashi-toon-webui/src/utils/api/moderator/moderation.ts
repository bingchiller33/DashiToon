import { Result } from "@/utils/err";
import { fetchApi } from "@/utils/api";

export interface Report {
    reportedByUsername: string;
    reason: string;
    reportedAt: string;
    flagged: boolean;
    flaggedCategories: {
        [key: string]: number;
    };
}

export interface PaginatedResponse<T> {
    items: T[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export interface ReportedComment {
    commentId: string;
    commentContent: string;
    commentUser: string;
    chapterNumber: string;
    volumeNumber: string;
    seriesTitle: string;
    reports: Report[];
}

export interface ReportedReview {
    reviewId: string;
    reviewContent: string;
    reviewUser: string;
    seriesId: number;
    seriesTitle: string;
    reports: Report[];
}

export interface ReportedChapter {
    chapterId: number;
    chapterNumber: number;
    volumeId: number;
    volumeNumber: number;
    seriesId: number;
    seriesTitle: string;
    seriesType: number;
    seriesAuthor: string;
    reports: Report[];
}

export interface ReportedSeries {
    seriesId: number;
    seriesTitle: string;
    seriesThumbnail: string;
    seriesSynopsis: string;
    seriesAuthor: string;
    reports: Report[];
}

export type ReportedCommentsResponse = PaginatedResponse<ReportedComment>;
export type ReportedReviewsResponse = PaginatedResponse<ReportedReview>;
export type ReportedChaptersResponse = PaginatedResponse<ReportedChapter>;
export type ReportedSeriesResponse = PaginatedResponse<ReportedSeries>;

export async function getReportedComments(
    pageNumber: number = 1,
    pageSize?: number,
): Promise<Result<ReportedCommentsResponse>> {
    return await fetchApi("GET", "/api/Moderation/comments/reports", undefined, {
        params: {
            pageNumber,
            pageSize,
        },
    });
}

export async function getReportedReviews(
    pageNumber: number = 1,
    pageSize?: number,
): Promise<Result<ReportedReviewsResponse>> {
    return await fetchApi("GET", "/api/Moderation/reviews/reports", undefined, {
        params: {
            pageNumber,
            pageSize,
        },
    });
}

export async function getReportedChapters(
    pageNumber: number = 1,
    pageSize?: number,
): Promise<Result<ReportedChaptersResponse>> {
    return await fetchApi("GET", "/api/Moderation/chapters/reports", undefined, {
        params: {
            pageNumber,
            pageSize,
        },
    });
}

export async function getReportedSeries(
    pageNumber: number = 1,
    pageSize?: number,
): Promise<Result<ReportedSeriesResponse>> {
    return await fetchApi("GET", "/api/Moderation/series/reports", undefined, {
        params: {
            pageNumber,
            pageSize,
        },
    });
}

export async function resolveChapterReport(
    chapterId: string,
    restrictedDurationInDays?: number,
): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Moderation/chapters/${chapterId}/reports/resolve`, {
        chapterId,
        restrictedDurationInDays,
    });
}

export async function resolveSeriesReport(seriesId: string, restrictedDurationInDays?: number): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Moderation/series/${seriesId}/reports/resolve`, {
        seriesId,
        restrictedDurationInDays,
    });
}

export async function resolveCommentReport(commentId: string, mutedDurationInDays?: number): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Moderation/comment/${commentId}/reports/resolve`, {
        commentId,
        mutedDurationInDays,
    });
}

export async function resolveReviewReport(reviewId: string, mutedDurationInDays?: number): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Moderation/review/${reviewId}/reports/resolve`, {
        reviewId,
        mutedDurationInDays,
    });
}

export async function dismissReport(
    entityId: string,
    type: "review" | "comment" | "content" | "series",
): Promise<Result<void>> {
    const typeMap = {
        review: 0,
        comment: 1,
        content: 2,
        series: 3,
    };

    return await fetchApi("POST", "/api/Moderation/dismiss-report", {
        entityId,
        type: typeMap[type],
    });
}

export interface Permission {
    isAllowed: boolean;
    notAllowedUntil?: string;
}

export async function isAllowedToReviewOrComment(): Promise<Result<Permission>> {
    return await fetchApi("GET", "/api/Moderation/is-allowed-review-or-comment");
}
