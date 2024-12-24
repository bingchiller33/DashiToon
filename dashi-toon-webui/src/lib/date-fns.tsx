import { formatDistanceToNow, format, parseISO } from "date-fns";
import { vi } from "date-fns/locale";

export function formatUpdatedAt(dateString: string) {
    const date = parseISO(dateString);
    const now = new Date();

    // If it's today, show the time
    if (format(date, "yyyy-MM-dd") === format(now, "yyyy-MM-dd")) {
        return format(date, "h:mm a", { locale: vi }); // "6:28 PM"
    }

    // If it's within the last 7 days, show relative time
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
    if (date > sevenDaysAgo) {
        return formatDistanceToNow(date, { addSuffix: true, locale: vi });
    }

    // Otherwise, show the date
    return format(date, "MMM d, yyyy", { locale: vi });
}
