"use client";

import { Bell } from "lucide-react";
import { formatDistanceToNow } from "date-fns";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import {
    getNotifications,
    markAllNotificationsAsRead,
    markNotificationAsRead,
    Notification,
} from "@/utils/api/reader/notification";
import { useEffect, useState } from "react";
import { formatDateAgoWithRange } from "@/lib/date-ago";
import { UserSession } from "@/utils/api/user";
import Link from "next/link";
import { Skeleton } from "./ui/skeleton";

interface NotificationProps {
    session: UserSession | null;
}

export default function NotificationsPopover({ session }: NotificationProps) {
    const [notifications, setNotifications] = useState<Notification[] | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function fetchData() {
            setLoading(true);
            const [result, _] = await getNotifications(1, 10);
            if (result) {
                setNotifications(result.items);
            }
            setLoading(false);
        }
        fetchData();
    }, []);

    const handleMarkAsRead = async (notificationId: string) => {
        const [result, _] = await markNotificationAsRead(notificationId);
        if (result) {
            const [newNotifications, _] = await getNotifications(1, 10);
            if (newNotifications) {
                setNotifications(newNotifications.items);
            }
        }
    };

    const handleMarkAllAsRead = async () => {
        const [result, _] = await markAllNotificationsAsRead();
        if (result) {
            const [newNotifications, __] = await getNotifications(1, 10);
            if (newNotifications) {
                setNotifications(newNotifications.items);
            }
        }
    };

    const unreadCount = notifications?.filter((item) => !item.isRead).length ?? 0;

    if (!session) return null;

    const NotificationSkeleton = () => (
        <div className="relative">
            <div className="flex items-start space-x-4 p-4">
                <div className="flex-1 space-y-2">
                    <div className="flex items-center gap-2">
                        <Skeleton className="h-4 w-[200px]" />
                    </div>
                    <Skeleton className="h-4 w-[300px]" />
                    <Skeleton className="h-3 w-[100px]" />
                </div>
                <Skeleton className="h-8 w-16" />
            </div>
            <Separator />
        </div>
    );

    return (
        <Popover>
            <PopoverTrigger asChild>
                <div className="relative">
                    <Bell className="h-6 w-6 cursor-pointer transition-colors duration-200 hover:text-blue-500" />
                    {unreadCount > 0 && (
                        <Badge
                            variant="destructive"
                            className="absolute -right-2 -top-2 flex h-5 w-5 items-center justify-center rounded-full p-0"
                        >
                            {unreadCount}
                        </Badge>
                    )}
                </div>
            </PopoverTrigger>
            <PopoverContent className="w-[380px] p-0 shadow-lg" align="end">
                <Card className="overflow-hidden border-0">
                    <CardHeader className="border-b bg-primary/5 px-4 py-3 shadow-sm">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center space-x-2">
                                <CardTitle className="text-lg">Thông báo</CardTitle>
                                {unreadCount > 0 && <Badge variant="secondary">{unreadCount}</Badge>}
                            </div>
                            <span className="inline-flex h-8 cursor-pointer items-center justify-center rounded-md border border-transparent px-4 text-blue-500 hover:bg-primary/10">
                                Xem tất cả
                            </span>
                        </div>
                    </CardHeader>
                    <ScrollArea className="h-auto max-h-[400px] bg-background">
                        <CardContent className="p-0">
                            {loading ? (
                                <>
                                    <NotificationSkeleton />
                                    <NotificationSkeleton />
                                    <NotificationSkeleton />
                                </>
                            ) : notifications && notifications.length > 0 ? (
                                notifications.map((item) => (
                                    <div key={item.id} className="relative">
                                        <div
                                            className={`flex items-start space-x-4 p-4 ${
                                                !item.isRead ? "bg-muted/40" : ""
                                            } transition-colors duration-200 hover:bg-muted/20`}
                                        >
                                            <div className="flex-1 space-y-1">
                                                <div className="flex items-center gap-2">
                                                    <p className="text-sm font-medium">{item.title}</p>
                                                    {!item.isRead && (
                                                        <span className="h-2 w-2 rounded-full bg-blue-600" />
                                                    )}
                                                </div>
                                                <p className="text-sm text-muted-foreground">{item.content}</p>
                                                <p className="text-xs text-muted-foreground">
                                                    {formatDateAgoWithRange(item.timestamp)}
                                                </p>
                                            </div>
                                            <Link
                                                href={`/series/${item.seriesId}/vol/${item.volumeId}/chap/${item.chapterId}/${item.type === 0 ? "novel" : "comic"}`}
                                                onClick={() => handleMarkAsRead(item.id)}
                                                className="inline-flex cursor-pointer items-center justify-center rounded-md border border-transparent px-2 py-1 text-blue-500 hover:bg-primary/10"
                                            >
                                                Đọc
                                            </Link>
                                        </div>
                                        <Separator />
                                    </div>
                                ))
                            ) : (
                                <p className="p-4 text-sm text-muted-foreground">Không có thông báo nào.</p>
                            )}
                        </CardContent>
                    </ScrollArea>
                    <CardFooter className="border-t bg-primary/5 p-3 shadow-sm">
                        <div className="flex w-full items-center justify-between">
                            <span
                                onClick={handleMarkAllAsRead}
                                className="w-full cursor-pointer justify-start text-muted-foreground hover:underline"
                            >
                                Đánh dấu tất cả là đã đọc
                            </span>
                        </div>
                    </CardFooter>
                </Card>
            </PopoverContent>
        </Popover>
    );
}
