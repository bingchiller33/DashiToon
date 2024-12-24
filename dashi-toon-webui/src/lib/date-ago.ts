import { formatDistanceToNow, parseISO } from "date-fns";
import { vi } from "date-fns/locale";

export function formatDateAgo(dateString: string) {
    const date = parseISO(dateString);
    return formatDistanceToNow(date, {
        addSuffix: true,
        locale: vi,
    });
}
export function formatDateAgoWithRange(dateString: string) {
    const date = parseISO(dateString);
    const now = new Date();
    const diffInMinutes = Math.floor(
        (now.getTime() - date.getTime()) / (1000 * 60),
    );

    if (diffInMinutes < 1) {
        return "vừa xong";
    }

    if (diffInMinutes < 60) {
        return `${diffInMinutes} ${diffInMinutes === 1 ? "phút" : "phút"} trước`;
    }

    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) {
        return `${diffInHours} ${diffInHours === 1 ? "giờ" : "giờ"} trước`;
    }

    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 7) {
        return `${diffInDays} ${diffInDays === 1 ? "ngày" : "ngày"} trước`;
    }

    const diffInWeeks = Math.floor(diffInDays / 7);
    if (diffInWeeks < 4) {
        return `${diffInWeeks} ${diffInWeeks === 1 ? "tuần" : "tuần"} trước`;
    }

    const diffInMonths = Math.floor(diffInDays / 30);
    if (diffInMonths < 12) {
        return `${diffInMonths} ${diffInMonths === 1 ? "tháng" : "tháng"} trước`;
    }

    const diffInYears = Math.floor(diffInDays / 365);
    return `${diffInYears} ${diffInYears === 1 ? "năm" : "năm"} trước`;
}
