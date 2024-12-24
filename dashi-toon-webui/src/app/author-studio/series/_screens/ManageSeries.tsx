"use client";

import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { EllipsisVertical, Home, Plus, Search } from "lucide-react";
import Image from "next/image";
import React, { useEffect, useState } from "react";
import * as env from "@/utils/env";
import { ConfirmationModal } from "../_components/DeleteConfirmationModal";
import { toast } from "sonner";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { formatUpdatedAt } from "@/lib/date-fns";
import { format, parseISO } from "date-fns";
import {
    RATING_CONFIG,
    STATUS_CONFIG,
    STATUS_MAP,
    STATUS_MAPPING,
    APIRatingType,
    VNRatingType,
    RATING_MAPPING,
} from "@/utils/consts";
import shimmer from "@/components/Shimmer";
import Link from "next/link";
import SiteLayout from "@/components/SiteLayout";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { getSeries } from "@/utils/api/author-studio/series";
import { LoadingSpinner } from "@/components/LoadingSpinenr";

interface Series {
    id: number;
    title: string;
    status: "Ongoing" | "Hiatus" | "Complete" | "Draft" | "Trashed";
    thumbnail: string;
    thumbnailUrl: string;
    type: "Novel" | "Comic";
    genres: string[];
    contentRating: APIRatingType;
    updatedAt: string;
}

interface APISeriesResponse {
    id: number;
    title: string;
    status: number;
    thumbnail: string;
    type: number;
    genres: string[];
    contentRating: number;
    updatedAt: string;
}

const typeMapping = {
    1: "Novel",
    2: "Comic",
} as const;

const contentRatingMapping = {
    1: "All Ages",
    2: "Teen",
    3: "Young Adult",
    4: "Mature",
} as const;

function transformAPIResponse(apiSeries: APISeriesResponse): Series {
    return {
        id: apiSeries.id,
        title: apiSeries.title,
        status: STATUS_MAP[apiSeries.status as keyof typeof STATUS_MAP],
        thumbnail: apiSeries.thumbnail,
        thumbnailUrl: "",
        type: typeMapping[apiSeries.type as keyof typeof typeMapping],
        genres: apiSeries.genres,
        contentRating: contentRatingMapping[apiSeries.contentRating as keyof typeof contentRatingMapping],
        updatedAt: apiSeries.updatedAt,
    };
}

type SeriesType = "all" | "Novel" | "Comic";
type SeriesStatus = "all" | "Ongoing" | "Hiatus" | "Complete" | "Draft" | "Trashed";
// type StatusType = "Ongoing" | "Hiatus" | "Complete" | "Draft" | "Trashed";
// type ContentRatingType = "All Ages" | "Teen" | "Young Adult" | "Mature";

const ManageSeriesScreen: React.FC = () => {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedSeriesId, setSelectedSeriesId] = useState<number | null>(null);
    const [selectedSeriesName, setSelectedSeriesName] = useState<string>("");

    const [series, setSeries] = useState<Series[]>([]);
    const [selectedType, setSelectedType] = useState<SeriesType>("all");
    const [selectedStatus, setSelectedStatus] = useState<SeriesStatus>("all");
    const [searchQuery, setSearchQuery] = useState<string>("");
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetchSeries();
    }, []);

    const fetchSeries = async (): Promise<void> => {
        try {
            setIsLoading(true);
            setError(null);
            const [apiData, error] = await getSeries();
            if (error) {
                throw new Error("Failed to fetch series!");
            }
            if (apiData) {
                const transformedData = await Promise.all(
                    apiData.map(async (apiSeries) => {
                        const series = transformAPIResponse(apiSeries);
                        const thumbnailUrl = await fetchThumbnailUrl(series.thumbnail);
                        return { ...series, thumbnailUrl };
                    }),
                );
                setSeries(transformedData);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : "An error occurred");
            toast.error("Failed to fetch series");
        } finally {
            setIsLoading(false);
        }
    };

    const fetchThumbnailUrl = async (fileName: string): Promise<string> => {
        try {
            const response = await fetch(`${env.getBackendHost()}/api/Images/${fileName}?type=thumbnails`, {
                credentials: "include",
            });
            if (response.ok) {
                const thumbnailResponse = await response.json();
                return thumbnailResponse.imageUrl;
            } else {
                console.error("Failed to fetch thumbnail URL", await response.text());
                return "https://placehold.jp/225x300.png";
            }
        } catch (error) {
            console.error("Error fetching thumbnail URL", error);
            return "https://placehold.jp/225x300.png";
        }
    };

    const filteredSeries = series.filter((item) => {
        const typeMatch = selectedType === "all" || item.type === selectedType;
        const statusMatch = selectedStatus === "all" || item.status === selectedStatus;
        const searchMatch = item.title.toLowerCase().includes(searchQuery.toLowerCase());
        return typeMatch && statusMatch && searchMatch;
    });

    const handleDeleteClick = (seriesId: number, seriesName: string) => {
        setSelectedSeriesId(seriesId);
        setSelectedSeriesName(seriesName);
        setIsModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        try {
            if (!selectedSeriesId) return;

            setIsLoading(true);
            const response = await fetch(`${env.getBackendHost()}/api/AuthorStudio/series/${selectedSeriesId}`, {
                method: "DELETE",
                credentials: "include",
            });

            if (!response.ok) {
                throw new Error("Failed to delete series");
            }

            toast.success("Series deleted successfully");
            await fetchSeries();
        } catch (err) {
            toast.error(err instanceof Error ? err.message : "Failed to delete series");
            console.error("Error deleting series:", err);
        } finally {
            setIsLoading(false);
            setIsModalOpen(false);
            setSelectedSeriesId(null);
            setSelectedSeriesName("");
        }
    };

    const SkeletonCard = () => (
        <div className="rounded-lg border p-4">
            <Skeleton className="mb-4 h-[200px] w-full" />
            <Skeleton className="mb-2 h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
        </div>
    );

    const EmptyState = () => (
        // <div className="relative w-full">
        <div className="col-span-full flex flex-col items-center justify-center p-8 text-center">
            <h3 className="mb-2 text-xl font-semibold">Không tìm thấy bộ truyện</h3>
            <p className="mb-4 text-gray-500">Bắt đầu bằng cách tạo bộ truyện đầu tiên của bạn</p>
            <Link href="/author-studio/series/create" passHref legacyBehavior>
                <a>
                    <Button>Tạo bộ truyện</Button>
                </a>
            </Link>
        </div>
        // </div>
    );

    const toBase64 = (str: string) =>
        typeof window === "undefined" ? Buffer.from(str).toString("base64") : window.btoa(str);

    const SeriesCard: React.FC<{ item: Series }> = ({ item }) => (
        <Card className="overflow-hidden text-white">
            <div className="sm:hidden">
                <Link href={`/series/${item.id}`}>
                    <div className="relative w-full cursor-pointer pt-[133.33%]">
                        <Image
                            src={item.thumbnailUrl || "https://placehold.jp/225x300.png"}
                            alt={item.title}
                            fill
                            className="object-cover"
                            sizes="(max-width: 640px) 100vw"
                            placeholder={`data:image/svg+xml;base64,${toBase64(shimmer(225, 300))}`}
                        />
                    </div>
                </Link>
                <div className="p-4">
                    <div className="flex items-start justify-between">
                        <div className="min-w-0 flex-1">
                            <p className="truncate text-sm text-gray-400">{item.genres.join(", ")}</p>
                            <Link href={`/series/${item.id}`} className="block">
                                <h3
                                    className="cursor-pointer truncate text-lg font-semibold transition-colors hover:text-blue-500"
                                    title={item.title}
                                >
                                    {item.title}
                                </h3>
                            </Link>
                            <div className="mt-1 flex flex-wrap gap-2">
                                <Badge
                                    variant="secondary"
                                    className={`${
                                        STATUS_CONFIG[STATUS_MAPPING[item.status]].color
                                    } flex items-center gap-1`}
                                >
                                    {(() => {
                                        const StatusIcon = STATUS_CONFIG[STATUS_MAPPING[item.status]].icon;
                                        return <StatusIcon className="h-3 w-3" />;
                                    })()}
                                    {STATUS_MAPPING[item.status]}
                                </Badge>
                                <Badge
                                    variant="secondary"
                                    className={RATING_CONFIG[RATING_MAPPING[item.contentRating] as VNRatingType].color}
                                >
                                    {RATING_MAPPING[item.contentRating]}
                                </Badge>
                            </div>
                        </div>
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="ghost" className="ml-2 h-8 w-8 flex-shrink-0 p-0">
                                    •••
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent>
                                <DropdownMenuItem onClick={() => handleDeleteClick(item.id, item.title)}>
                                    Xóa
                                </DropdownMenuItem>
                                <DropdownMenuItem>Phân tích</DropdownMenuItem>
                                <DropdownMenuItem>Cài đặt</DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                    <p
                        className="mt-2 flex items-center truncate text-sm text-gray-400"
                        title={format(parseISO(item.updatedAt), "PPPpp")}
                    >
                        <span className="mr-1">Cập nhật:</span>
                        {formatUpdatedAt(item.updatedAt)}
                    </p>
                    <Link href={`/author-studio/series/${item.id}`} passHref legacyBehavior>
                        <a className="flex-1">
                            <Button size="sm" className="w-full">
                                Khám phá
                            </Button>
                        </a>
                    </Link>
                </div>
            </div>

            <div className="hidden h-[240px] sm:flex">
                <Link href={`/series/${item.id}`} className="relative w-[180px] flex-shrink-0 cursor-pointer">
                    <div className="relative h-full w-full">
                        <Image
                            src={item.thumbnailUrl}
                            alt={item.title}
                            fill
                            className="object-cover"
                            sizes="180px"
                            placeholder={`data:image/svg+xml;base64,${toBase64(shimmer(225, 300))}`}
                        />
                    </div>
                </Link>
                <div className="flex min-w-0 flex-1 flex-col p-4">
                    <div className="flex-1">
                        <div className="flex items-start justify-between">
                            <div className="min-w-0 flex-1">
                                <p className="truncate text-sm text-gray-400">{item.genres.join(", ")}</p>
                                <Link href={`/series/${item.id}`}>
                                    <h3
                                        className="cursor-pointer truncate text-lg font-semibold transition-colors hover:text-blue-500"
                                        title={item.title}
                                    >
                                        {item.title}
                                    </h3>
                                </Link>
                                <div className="mt-1 flex flex-wrap gap-2">
                                    <Badge
                                        variant="secondary"
                                        className={`${
                                            STATUS_CONFIG[STATUS_MAPPING[item.status]].color
                                        } flex items-center gap-1`}
                                    >
                                        {(() => {
                                            const StatusIcon = STATUS_CONFIG[STATUS_MAPPING[item.status]].icon;
                                            return <StatusIcon className="h-3 w-3" />;
                                        })()}
                                        {STATUS_MAPPING[item.status]}
                                    </Badge>
                                    <Badge
                                        variant="secondary"
                                        className={
                                            RATING_CONFIG[RATING_MAPPING[item.contentRating] as VNRatingType].color
                                        }
                                    >
                                        {RATING_MAPPING[item.contentRating]}
                                    </Badge>
                                </div>
                            </div>
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" className="ml-2 h-8 w-8 flex-shrink-0 p-0">
                                        <EllipsisVertical />
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent>
                                    <DropdownMenuItem onClick={() => handleDeleteClick(item.id, item.title)}>
                                        Xóa
                                    </DropdownMenuItem>
                                    <DropdownMenuItem asChild>
                                        <Link href={`/author-studio/series/${item.id}/analytics`}>Phân tích</Link>
                                    </DropdownMenuItem>
                                    <DropdownMenuItem asChild>
                                        <Link href={`/author-studio/series/${item.id}/edit`}>Chỉnh sửa</Link>
                                    </DropdownMenuItem>
                                </DropdownMenuContent>
                            </DropdownMenu>
                        </div>
                    </div>
                    <div>
                        <p
                            className="mt-2 flex items-center truncate text-sm text-gray-400"
                            title={format(parseISO(item.updatedAt), "PPPpp")}
                        >
                            <span className="mr-1">Cập nhật:</span>
                            {formatUpdatedAt(item.updatedAt)}
                        </p>
                        <div className="flex gap-2">
                            <Link href={`/author-studio/series/${item.id}`} passHref legacyBehavior>
                                <a className="flex-1">
                                    <Button size="sm" className="w-full">
                                        Khám phá
                                    </Button>
                                </a>
                            </Link>
                        </div>
                    </div>
                </div>
            </div>
            <ConfirmationModal
                isOpen={isModalOpen}
                onClose={() => {
                    setIsModalOpen(false);
                    setSelectedSeriesId(null);
                    setSelectedSeriesName("");
                }}
                onConfirm={handleConfirmDelete}
                itemName={selectedSeriesName}
                entityType="bộ truyện"
                requiredText={`XÓA ${selectedSeriesName} / XOÁ ${selectedSeriesName}`}
            />
        </Card>
    );

    if (isLoading) {
        return (
            <SiteLayout>
                <div className="flex h-screen items-center justify-center">
                    <LoadingSpinner size={64} className="text-blue-500 opacity-75 transition-opacity ease-in-out" />
                </div>
            </SiteLayout>
        );
    }

    if (error) {
        return <div className="text-center text-red-500">Error: {error}</div>;
    }

    return (
        <SiteLayout>
            <div
                className={`mx-auto min-h-screen py-4 ${
                    filteredSeries.length === 0
                        ? "min-w-[320px] max-w-[360px] sm:max-w-[640px] md:max-w-[768px] lg:min-w-[1152px] xl:max-w-[1280px]"
                        : "max-w-6xl"
                }`}
            >
                <Breadcrumb className="mb-4">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/" className="flex items-center">
                                <Home className="mr-2 h-4 w-4" />
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio">Xưởng Truyện</BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Bộ truyện</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <div className="mb-4 flex flex-wrap gap-2 sm:mb-6 sm:gap-4">
                    {(["all", "Novel", "Comic"] as const).map((type) => (
                        <Button
                            key={type}
                            variant={selectedType === type ? "default" : "secondary"}
                            onClick={() => setSelectedType(type)}
                            className="flex-1 sm:flex-none"
                        >
                            {type === "all" ? "Tất cả" : type === "Novel" ? "Tiểu thuyết" : "Truyện tranh"}
                        </Button>
                    ))}

                    <Button className="ms-auto">
                        <Link href="/author-studio/revenue">Quản lý tài chính</Link>
                    </Button>
                </div>

                <div className="mb-6 flex flex-col gap-4 sm:flex-row">
                    <div className="relative flex-1">
                        <Search className="absolute left-2 top-2.5 h-4 w-4 text-gray-500" />
                        <Input
                            placeholder="Tìm kiếm"
                            className="pl-8"
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                        />
                    </div>
                    <Select value={selectedStatus} onValueChange={(value: SeriesStatus) => setSelectedStatus(value)}>
                        <SelectTrigger className="w-full sm:w-48">
                            <SelectValue placeholder="Lọc theo trạng thái" />
                        </SelectTrigger>
                        <SelectContent>
                            {(["all", "Draft", "Ongoing", "Hiatus", "Complete", "Trashed"] as const).map((status) => (
                                <SelectItem key={status} value={status}>
                                    {status === "all"
                                        ? "Tất cả trạng thái"
                                        : status === "Draft"
                                          ? "Bản thảo"
                                          : status === "Ongoing"
                                            ? "Đang tiến hành"
                                            : status === "Hiatus"
                                              ? "Tạm ngưng"
                                              : status === "Complete"
                                                ? "Hoàn thành"
                                                : "Đã xóa"}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                    <Link href="/author-studio/series/create" passHref legacyBehavior>
                        <a className="sm:flex-none">
                            <Button className="sm:w-full">
                                <Plus className="mr-2 h-4 w-4" />
                                Thêm Truyện Mới
                            </Button>
                        </a>
                    </Link>
                </div>

                {isLoading ? (
                    <div className="grid grid-cols-1 gap-4 sm:gap-6 lg:grid-cols-2">
                        {Array.from({ length: 4 }).map((_, index) => (
                            <SkeletonCard key={index} />
                        ))}
                    </div>
                ) : filteredSeries.length === 0 ? (
                    <EmptyState />
                ) : (
                    <div className="grid grid-cols-1 gap-4 sm:gap-6 lg:grid-cols-2">
                        {filteredSeries.map((item) => (
                            <SeriesCard key={item.id} item={item} />
                        ))}
                    </div>
                )}
            </div>
        </SiteLayout>
    );
};

export default ManageSeriesScreen;
