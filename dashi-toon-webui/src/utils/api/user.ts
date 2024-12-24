import { fetchApi } from ".";
import { Result } from "../err";
import * as gtag from "@/utils/gtag";
import * as env from "@/utils/env";

export interface PaymentHistoryItem {
    id: string;
    detail: string;
    price: {
        amount: number;
        currency: string;
    };
    status: number;
    completedAt: string;
}

export interface PaymentHistory {
    items: PaymentHistoryItem[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getPaymentHistory(pageNumber: number, pageSize: number): Promise<Result<PaymentHistory>> {
    return await fetchApi("GET", `/api/Users/payments`, undefined, {
        params: {
            pageNumber,
            pageSize,
        },
    });
}

export async function logoutUser(): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Users/logout`, undefined, {
        preventAutoLogin: true,
    });
}

export interface UserSession {
    email: string;
    userId: string;
    username: string;
    roles: string[];
}

export async function getUserInfo(): Promise<Result<UserSession>> {
    return await fetchApi("GET", `/api/Users/info`, undefined, {
        preventAutoLogin: true,
    });
}

export interface FollowedSeries {
    thumbnail: string;
    title: string;
    type: number;
    status: number;
    seriesId: number;
    latestVolumeReadId: number;
    latestChapterReadId: number;
    progress: number;
    totalChapters: number;
    isNotified: boolean;
}

export interface FollowedSeriesList {
    items: FollowedSeries[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getFollowedSeries(
    pageNumber: number,
    pageSize: number,
    hasRead?: boolean | null,
    sortBy: "LastRead" | "Title" = "LastRead",
    sortOrder: "ASC" | "DESC" = "DESC",
): Promise<Result<FollowedSeriesList>> {
    const params: Record<string, any> = {
        pageNumber,
        pageSize,
        sortBy,
        sortOrder,
    };

    if (hasRead !== null) {
        params.hasRead = hasRead;
    }

    return await fetchApi("GET", `/api/Users/followed-series`, undefined, {
        params,
    });
}

export async function followSeries(seriesId: number): Promise<Result<void>> {
    gtag.eventFollow(seriesId.toString());
    return await fetchApi("POST", `/api/Series/${seriesId}/follows`, {
        seriesId: seriesId,
    });
}

export async function unfollowSeries(seriesId: number): Promise<Result<void>> {
    gtag.eventUnfollow(seriesId.toString());
    return await fetchApi("DELETE", `/api/Series/${seriesId}/follows`);
}

export async function getFollowedSeriesById(id: number): Promise<Result<boolean>> {
    return await fetchApi("GET", `/api/Users/followed-series/${id}`, undefined, {
        preventAutoLogin: true,
    });
}

export interface FollowSeries2 {
    isFollowed: boolean;
    detail: FollowedSeries;
}

export async function getFollowedSeriesById2(id: number): Promise<Result<FollowSeries2>> {
    return await fetchApi("GET", `/api/Users/followed-series2/${id}`, undefined, {
        preventAutoLogin: true,
    });
}

export async function setUsernameAndLogin(token: string, username: string, returnUrl: string): Promise<Result<string>> {
    const url = `${env.getBackendHost()!}/api/Users/set-username-and-login?token=${token}`;

    const resp = await fetch(url, {
        method: "POST",
        body: JSON.stringify({ username, returnUrl }),
        redirect: "manual",
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
        },
    });

    if (resp.status >= 400) {
        return [undefined, resp.text()];
    }

    const redir = resp.headers.get("Location");
    window.location.href = redir ?? "/";
    return ["?", undefined];
}
