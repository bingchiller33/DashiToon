import { Result } from "@/utils/err";
import { fetchApi } from "@/utils/api";

export interface Notification {
    id: string;
    title: string;
    content: string;
    isRead: boolean;
    timestamp: string;
    chapterId: number;
    volumeId: number;
    seriesId: number;
    type: number;
}

export interface NotificationsResponse {
    items: Notification[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getNotifications(pageNumber: number, pageSize: number): Promise<Result<NotificationsResponse>> {
    return await fetchApi("GET", `/api/Notifications`, undefined, {
        params: {
            pageNumber,
            pageSize,
        },
        preventAutoLogin: true,
    });
}

export async function markNotificationAsRead(notificationId: string): Promise<Result<void>> {
    return await fetchApi("PUT", `/api/Notifications/${notificationId}/mark-as-read`);
}

export async function markAllNotificationsAsRead(): Promise<Result<void>> {
    return await fetchApi("PUT", `/api/Notifications/mark-all-as-read`);
}
