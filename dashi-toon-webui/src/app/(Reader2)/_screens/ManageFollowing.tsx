"use client";

import React, { useCallback, useEffect } from "react";
import { LayoutGrid, LayoutList, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import FollowingCard from "@/app/(Reader2)/components/FollowingCard";
import { useMediaQuery } from "usehooks-ts";
import SiteLayout from "@/components/SiteLayout";
import { FollowedSeries, getFollowedSeries } from "@/utils/api/user";
import { toast } from "sonner";
import { CustomPagination } from "@/app/(Reader2)/components/CustomPagination";
import { CiRead, CiUnread } from "react-icons/ci";
import { TbListDetails } from "react-icons/tb";
import { FaSortAlphaDown, FaSortAlphaUp } from "react-icons/fa";
import { LoadingSpinner } from "@/components/LoadingSpinenr";

export default function Bookmarks() {
    const [view, setView] = React.useState<"grid" | "list">("grid");
    const isMobile = useMediaQuery("(max-width: 640px)");
    const [isLoading, setIsLoading] = React.useState(true);
    const [followedSeries, setFollowedSeries] = React.useState<FollowedSeries[]>([]);
    const [pageInfo, setPageInfo] = React.useState({
        currentPage: 1,
        totalPages: 1,
        hasNextPage: false,
    });
    const [sortConfig, setSortConfig] = React.useState<{
        sortBy: "LastRead" | "Title";
        sortOrder: "ASC" | "DESC";
    }>({
        sortBy: "LastRead",
        sortOrder: "DESC",
    });
    const [hasRead, setHasRead] = React.useState<boolean | null>(null);

    const fetchFollowedSeries = useCallback(async () => {
        setIsLoading(true);
        const [data, error] = await getFollowedSeries(
            pageInfo.currentPage,
            10,
            hasRead,
            sortConfig.sortBy,
            sortConfig.sortOrder,
        );

        if (data) {
            setFollowedSeries(data.items);
            setPageInfo({
                currentPage: data.pageNumber,
                totalPages: data.totalPages,
                hasNextPage: data.hasNextPage,
            });
        } else {
            toast.error("Lỗi khi tải danh sách truyện", error);
        }
        setIsLoading(false);
    }, [pageInfo.currentPage, hasRead, sortConfig.sortBy, sortConfig.sortOrder]);

    useEffect(() => {
        fetchFollowedSeries();
    }, [fetchFollowedSeries]);

    const handleUnfollow = useCallback(() => {
        fetchFollowedSeries();
    }, [fetchFollowedSeries]);

    return (
        <SiteLayout>
            <div className="min-h-screen bg-neutral-900 text-neutral-200">
                <div className="mx-auto max-w-5xl space-y-4">
                    <div className="sticky top-0 z-10 border-b border-neutral-800 bg-neutral-900">
                        <div className="p-4">
                            <h1 className="mb-4 text-4xl font-bold text-white">Theo Dõi</h1>
                            <Tabs defaultValue="current" className="space-y-4">
                                <div className="flex flex-col gap-4">
                                    {/* <TabsList className="grid w-full grid-cols-2 bg-neutral-800">
                                        <TabsTrigger
                                            value="current"
                                            className="data-[state=active]:bg-blue-500"
                                        >
                                            Đang Đọc
                                        </TabsTrigger>
                                        <TabsTrigger
                                            value="favorite"
                                            className="data-[state=active]:bg-blue-500"
                                        >
                                            Chương Yêu Thích
                                        </TabsTrigger>
                                    </TabsList> */}
                                    <div className="flex items-center justify-between">
                                        <div className="flex gap-2 rounded-lg bg-neutral-800 p-1">
                                            <Button
                                                size="icon"
                                                variant={view === "grid" ? "secondary" : "ghost"}
                                                onClick={() => setView("grid")}
                                                className={`h-8 w-8 ${
                                                    view === "grid" ? "bg-blue-500 text-white" : "text-neutral-400"
                                                }`}
                                            >
                                                <LayoutGrid className="h-4 w-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant={view === "list" ? "secondary" : "ghost"}
                                                onClick={() => setView("list")}
                                                className={`h-8 w-8 ${
                                                    view === "list" ? "bg-blue-500 text-white" : "text-neutral-400"
                                                }`}
                                            >
                                                <LayoutList className="h-4 w-4" />
                                            </Button>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <Button
                                                variant="outline"
                                                size="icon"
                                                onClick={() => {
                                                    if (hasRead === null) setHasRead(true);
                                                    else if (hasRead === true) setHasRead(false);
                                                    else setHasRead(null);
                                                }}
                                                className={`h-8 w-8 border-neutral-700 ${
                                                    hasRead === null
                                                        ? "bg-neutral-800 text-neutral-200"
                                                        : hasRead
                                                          ? "border-blue-500 text-blue-500"
                                                          : "border-neutral-700 text-neutral-400"
                                                }`}
                                                title={hasRead === null ? "Tất Cả" : hasRead ? "Đã Đọc" : "Chưa Đọc"}
                                            >
                                                {hasRead === null ? (
                                                    <TbListDetails className="h-4 w-4" />
                                                ) : hasRead ? (
                                                    <CiRead className="h-4 w-4" />
                                                ) : (
                                                    <CiUnread className="h-4 w-4" />
                                                )}
                                            </Button>
                                            <DropdownMenu>
                                                <DropdownMenuTrigger asChild>
                                                    <Button
                                                        variant="outline"
                                                        size="sm"
                                                        className="border-neutral-700 bg-neutral-800 text-neutral-200"
                                                    >
                                                        {sortConfig.sortBy === "LastRead"
                                                            ? "Thời Gian Đọc"
                                                            : "Tên Truyện"}
                                                        <ChevronDown className="ml-2 h-4 w-4" />
                                                    </Button>
                                                </DropdownMenuTrigger>
                                                <DropdownMenuContent
                                                    align="end"
                                                    className="border-neutral-700 bg-neutral-800 text-neutral-200"
                                                >
                                                    <DropdownMenuItem
                                                        className="hover:bg-blue-500 hover:text-white"
                                                        onClick={() =>
                                                            setSortConfig((prev) => ({
                                                                ...prev,
                                                                sortBy: "Title",
                                                            }))
                                                        }
                                                    >
                                                        Tên Truyện
                                                    </DropdownMenuItem>
                                                    <DropdownMenuItem
                                                        className="hover:bg-blue-500 hover:text-white"
                                                        onClick={() =>
                                                            setSortConfig((prev) => ({
                                                                ...prev,
                                                                sortBy: "LastRead",
                                                            }))
                                                        }
                                                    >
                                                        Thời Gian Đọc
                                                    </DropdownMenuItem>
                                                </DropdownMenuContent>
                                            </DropdownMenu>
                                            <Button
                                                variant="outline"
                                                size="icon"
                                                onClick={() =>
                                                    setSortConfig((prev) => ({
                                                        ...prev,
                                                        sortOrder: prev.sortOrder === "ASC" ? "DESC" : "ASC",
                                                    }))
                                                }
                                                className="h-8 w-8 border-neutral-700 bg-neutral-800 text-neutral-200"
                                            >
                                                {sortConfig.sortOrder === "ASC" ? (
                                                    <FaSortAlphaUp className="h-4 w-4" />
                                                ) : (
                                                    <FaSortAlphaDown className="h-4 w-4" />
                                                )}
                                            </Button>
                                        </div>
                                    </div>
                                </div>
                            </Tabs>
                        </div>
                    </div>

                    <Tabs defaultValue="current">
                        <TabsContent value="current">
                            <div className="p-4 pt-0">
                                {isLoading ? (
                                    <div className="flex items-center justify-center pt-8">
                                        <LoadingSpinner
                                            size={64}
                                            className="text-blue-500 opacity-75 transition-opacity ease-in-out"
                                        />
                                    </div>
                                ) : followedSeries.length === 0 ? (
                                    <div className="flex flex-col items-center justify-center gap-4 py-8 text-neutral-400">
                                        <svg
                                            className="h-16 w-16 text-neutral-700"
                                            fill="none"
                                            viewBox="0 0 24 24"
                                            stroke="currentColor"
                                        >
                                            <path
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                strokeWidth={2}
                                                d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
                                            />
                                        </svg>
                                        <div className="text-center">
                                            <h3 className="text-lg font-semibold text-neutral-200">
                                                Chưa có truyện nào được theo dõi
                                            </h3>
                                            <p className="mt-1 text-sm text-neutral-400">
                                                Bắt đầu theo dõi truyện để xem chúng ở đây.
                                            </p>
                                        </div>
                                    </div>
                                ) : (
                                    <>
                                        <div
                                            className={
                                                view === "list"
                                                    ? "flex flex-col space-y-2"
                                                    : `grid grid-cols-1 gap-4 ${isMobile ? "" : "sm:grid-cols-2"}`
                                            }
                                        >
                                            {followedSeries.map((series) => (
                                                <FollowingCard
                                                    key={series.seriesId}
                                                    series={series}
                                                    view={view}
                                                    onUnfollow={handleUnfollow}
                                                />
                                            ))}
                                        </div>
                                        <div className="mt-4 flex justify-center pb-4">
                                            <CustomPagination
                                                currentPage={pageInfo.currentPage}
                                                totalPages={pageInfo.totalPages}
                                                hasNextPage={pageInfo.hasNextPage}
                                                onPageChange={(page) =>
                                                    setPageInfo((prev) => ({
                                                        ...prev,
                                                        currentPage: page,
                                                    }))
                                                }
                                            />
                                        </div>
                                    </>
                                )}
                            </div>
                        </TabsContent>
                        <TabsContent value="favorite">
                            <div className="p-4 pt-0">
                                <div className="text-center text-neutral-400">Chưa có chương yêu thích nào</div>
                            </div>
                        </TabsContent>
                    </Tabs>
                </div>
            </div>
        </SiteLayout>
    );
}
