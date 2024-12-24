"use client";

import React from "react";
import { Bell, BookOpen, ImageIcon, Lock, MoreVertical, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Progress } from "@/components/ui/progress";
import { useMediaQuery } from "usehooks-ts";
import { FollowedSeries, unfollowSeries } from "@/utils/api/user";
import Image from "next/image";
import Link from "next/link";
import { toast } from "sonner";
import { HiBellAlert, HiBellSlash } from "react-icons/hi2";
import { STATUS_COLOR_MAP, STATUS_DISPLAY_MAP, STATUS_MAP } from "@/utils/consts";

interface FollowingCardProps {
    series: FollowedSeries;
    view: "grid" | "list";
    onUnfollow?: () => void;
}

export default function FollowingCard({ series, view, onUnfollow }: FollowingCardProps) {
    const isTablet = useMediaQuery("(max-width: 900px)");

    const handleUnfollow = async () => {
        try {
            const [_, error] = await unfollowSeries(series.seriesId);
            if (error) {
                toast.error("Không thể bỏ theo dõi truyện", error);
                return;
            }

            onUnfollow?.();
        } catch (error) {
            toast.error("Đã xảy ra lỗi khi bỏ theo dõi truyện");
        }
    };

    const getContinueReadingLink = (series: FollowedSeries) => {
        if (series.latestVolumeReadId === null || series.latestChapterReadId === null) {
            return `/series/${series.seriesId}`;
        }
        return `/series/${series.seriesId}/vol/${series.latestVolumeReadId}/chap/${series.latestChapterReadId}/${series.type === 1 ? "novel" : "comic"}`;
    };

    const getReadingButtonText = (series: FollowedSeries) => {
        if (series.latestVolumeReadId === null || series.latestChapterReadId === null) {
            return "Bắt đầu đọc";
        }
        return "Tiếp tục đọc";
    };

    const MoreActions = () => (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button
                    size="icon"
                    variant="ghost"
                    className="h-8 w-8 text-neutral-400 hover:bg-blue-500 hover:text-white"
                >
                    <MoreVertical className="h-4 w-4" />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="border-neutral-700 bg-neutral-800 text-neutral-200">
                <DropdownMenuItem className="bg-blue-500 text-white hover:bg-blue-600">
                    <Link href={getContinueReadingLink(series)} className="flex w-full items-center">
                        <svg
                            className="mr-2 h-4 w-4"
                            viewBox="0 0 24 24"
                            fill="none"
                            xmlns="http://www.w3.org/2000/svg"
                        >
                            <path
                                d="M5 5.2V18.8C5 19.9201 6.21309 20.6 7.2 20L18.4 13.2C19.3869 12.6 19.3869 11.4 18.4 10.8L7.2 4C6.21309 3.4 5 4.07989 5 5.2Z"
                                fill="currentColor"
                            />
                        </svg>
                        {getReadingButtonText(series)}
                    </Link>
                </DropdownMenuItem>
                {/* <DropdownMenuItem className="hover:bg-neutral-800">
                    {series.isNotified ? (
                        <HiBellAlert className="mr-2 h-4 w-4" />
                    ) : (
                        <HiBellSlash className="mr-2 h-4 w-4 text-neutral-400" />
                    )}
                    {series.isNotified ? "Tắt thông báo" : "Bật thông báo"}
                </DropdownMenuItem> */}
            </DropdownMenuContent>
        </DropdownMenu>
    );

    if (view === "list") {
        return (
            <div className="flex items-center gap-3 border-b border-neutral-700 px-3 py-2 hover:bg-neutral-800/50">
                <div className="flex min-w-0 flex-1 items-center justify-between">
                    <div className="min-w-0 space-y-1">
                        <Link href={getContinueReadingLink(series)}>
                            <h3 className="truncate font-bold text-white hover:text-blue-500">{series.title}</h3>
                        </Link>
                        <p className="text-sm text-neutral-400">
                            Bạn đã đọc {series.progress}/{series.totalChapters}
                        </p>
                    </div>
                    <div className="flex items-center gap-2 pl-3">
                        {!isTablet && (
                            <>
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-8 bg-blue-500 text-white hover:bg-blue-600"
                                >
                                    <Link href={getContinueReadingLink(series)} className="flex items-center">
                                        <svg
                                            className="mr-2 h-4 w-4"
                                            viewBox="0 0 24 24"
                                            fill="none"
                                            xmlns="http://www.w3.org/2000/svg"
                                        >
                                            <path
                                                d="M5 5.2V18.8C5 19.9201 6.21309 20.6 7.2 20L18.4 13.2C19.3869 12.6 19.3869 11.4 18.4 10.8L7.2 4C6.21309 3.4 5 4.07989 5 5.2Z"
                                                fill="currentColor"
                                            />
                                        </svg>
                                        {getReadingButtonText(series)}
                                    </Link>
                                </Button>
                                {/* <div className={`${series.isNotified ? "text-blue-500" : "text-neutral-400"}`}>
                                    {series.isNotified ? (
                                        <HiBellAlert className="h-6 w-6 hover:text-neutral-300" />
                                    ) : (
                                        <HiBellSlash className="h-4 w-4 hover:text-blue-500" />
                                    )}
                                </div> */}
                            </>
                        )}
                        {isTablet ? (
                            <MoreActions />
                        ) : (
                            <X
                                className="h-6 w-6 cursor-pointer text-neutral-400 hover:text-blue-500"
                                onClick={handleUnfollow}
                            />
                        )}
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className={`relative rounded-lg border-none bg-neutral-900 shadow-lg shadow-black/50 backdrop-blur-sm`}>
            <p className="absolute left-0 top-0 z-20 bg-black/50 text-sm">
                {series.type === 2 ? "Truyện tranh" : "Tiểu thuyết"}
            </p>
            <div className="flex gap-4">
                <div className="relative">
                    <Image
                        alt={series.title}
                        className="rounded object-cover"
                        src={series.thumbnail}
                        width={150}
                        height={200}
                    />
                </div>
                <div className="flex min-w-0 flex-1 flex-col justify-between p-4">
                    <div className="min-w-0 space-y-2">
                        <Link href={`/series/${series.seriesId}`} className="flex items-center" title={series.title}>
                            <h3 className="truncate pr-8 font-bold text-white hover:text-blue-500">{series.title}</h3>
                        </Link>
                        <div className="mb-3 flex items-center gap-2">
                            <div
                                className={`h-2 w-2 rounded-full ${STATUS_COLOR_MAP[series.status as keyof typeof STATUS_COLOR_MAP]}`}
                            />
                            <span className="text-sm font-medium">
                                {STATUS_DISPLAY_MAP[STATUS_MAP[series.status as keyof typeof STATUS_MAP]]}
                            </span>
                        </div>
                    </div>
                    {isTablet && (
                        <span className="absolute right-2 top-2">
                            {series.isNotified ? (
                                <HiBellAlert className="h-6 w-6 text-blue-500 hover:text-neutral-300" />
                            ) : (
                                <HiBellSlash className="h-6 w-6 text-neutral-400 hover:text-neutral-300" />
                            )}
                        </span>
                    )}
                    <div className="flex flex-1 flex-col justify-end">
                        <p className="pb-2 text-sm text-neutral-400">
                            Bạn đã đọc {series.progress}/{series.totalChapters}
                        </p>
                        <Progress
                            value={(series.progress / series.totalChapters) * 100}
                            className="h-1 bg-neutral-700"
                        />
                        <div className="flex items-center gap-2 pt-4">
                            {!isTablet && (
                                <>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="w-full border-neutral-700 bg-blue-500 text-white hover:bg-blue-600"
                                    >
                                        <Link href={getContinueReadingLink(series)} className="flex items-center">
                                            <svg
                                                className="mr-2 h-4 w-4"
                                                viewBox="0 0 24 24"
                                                fill="none"
                                                xmlns="http://www.w3.org/2000/svg"
                                            >
                                                <path
                                                    d="M5 5.2V18.8C5 19.9201 6.21309 20.6 7.2 20L18.4 13.2C19.3869 12.6 19.3869 11.4 18.4 10.8L7.2 4C6.21309 3.4 5 4.07989 5 5.2Z"
                                                    fill="currentColor"
                                                />
                                            </svg>
                                            {getReadingButtonText(series)}
                                        </Link>
                                    </Button>
                                    {/* <div
                                        className={`flex cursor-pointer items-center gap-2 rounded-md px-3 py-1.5 text-sm transition-colors hover:bg-neutral-800 ${
                                            series.isNotified
                                                ? "border border-blue-500 text-blue-500"
                                                : "border border-neutral-700 text-neutral-200"
                                        }`}
                                        onClick={() => {
                                        }}
                                    >
                                        {series.isNotified ? (
                                            <HiBellAlert className="h-4 w-4" />
                                        ) : (
                                            <HiBellSlash className="h-4 w-4 text-neutral-400" />
                                        )}
                                        {series.isNotified ? "Tắt thông báo" : "Bật thông báo"}
                                    </div> */}
                                </>
                            )}
                            {isTablet ? (
                                <>
                                    <div className="ml-auto">
                                        <MoreActions />
                                    </div>
                                </>
                            ) : (
                                <Button
                                    size="icon"
                                    variant="ghost"
                                    className="absolute right-2 top-2 h-8 w-8 text-neutral-400 hover:bg-blue-500 hover:text-white"
                                    onClick={handleUnfollow}
                                >
                                    <X className="h-4 w-4" />
                                </Button>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
