import { fetchApi } from "..";
import { Result } from "./../../err";

export interface SeriesReview {
    id: string;
    content: string;
    isRecommended: boolean;
    dislikes: number;
    likes: number;
    userId: string;
    reviewDate: string;
    username: string;
    userAvatar: string;
}

export type ReviewSortBy = "Worst" | "Best" | "Relevance" | "Newest" | "Oldest";

export interface SeriesReviewsResponse {
    items: SeriesReview[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getSeriesReviews(
    id: string,
    page: number,
    pageSize: number,
    sortBy: ReviewSortBy = "Relevance",
): Promise<Result<SeriesReviewsResponse>> {
    return await fetchApi("GET", `/api/Series/${id}/reviews`, undefined, {
        preventAutoLogin: true,
        params: {
            page,
            pageSize,
            sortBy,
        },
    });
}

export async function createSeriesReview(
    seriesId: string,
    content: string,
    isRecommended: boolean,
): Promise<Result<SeriesReview>> {
    return await fetchApi("POST", `/api/Series/${seriesId}/reviews`, {
        seriesId,
        content,
        isRecommended,
    });
}

export async function getCurrentUserSeriesReview(
    seriesId: string,
): Promise<Result<SeriesReview>> {
    return await fetchApi(
        "GET",
        `/api/Series/${seriesId}/reviews/current-user`,
        undefined,
        { preventAutoLogin: true },
    );
}

export async function updateSeriesReview(
    seriesId: string,
    reviewId: string,
    content: string,
    isRecommended: boolean,
): Promise<Result<SeriesReview>> {
    return await fetchApi(
        "PUT",
        `/api/Series/${seriesId}/reviews/${reviewId}`,
        {
            seriesId,
            reviewId,
            content,
            isRecommended,
        },
    );
}

export async function rateSeriesReview(
    seriesId: string,
    reviewId: string,
    isLiked: boolean,
): Promise<Result<SeriesReview>> {
    return await fetchApi(
        "POST",
        `/api/Series/${seriesId}/reviews/${reviewId}/rate`,
        {
            seriesId,
            reviewId,
            isLiked,
        },
    );
}
