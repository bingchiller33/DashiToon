"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Home, Plus, Search } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import * as env from "@/utils/env";
import Image from "next/image";
import { Input } from "@/components/ui/input";
import { ConfirmationModal } from "../_components/DeleteConfirmationModal";
import { toast } from "sonner";
import AddVolumeDialog from "../_components/AddVolumeDialog";
import Link from "next/link";
import { RATING_CONFIG_2, STATUS_COLOR_MAP, STATUS_DISPLAY_MAP, STATUS_MAP } from "@/utils/consts";
import DashiFanOverview from "../_components/DashiFanOverview";

import {
    Breadcrumb,
    BreadcrumbEllipsis,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import SiteLayout from "@/components/SiteLayout";
import { getImage } from "@/utils/api/image";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import DashiFanTable from "../_components/DashiFanTable";

interface Series {
    id: number;
    title: string;
    status: 1 | 2 | 3 | 4;
    synopsis: string;
    alternativeTitles: string[];
    thumbnail: string;
    type: 1 | 2;
    genres: string[];
    contentRating: 1 | 2 | 3 | 4;
    categoryRatings: {
        category: string;
        option: number;
    }[];
}

interface Volume {
    volumeId: number;
    volumeNumber: number;
    name: string;
    introduction: string;
    chapterCount: number;
}

const SeriesDashboardScreen: React.FC<SeriesDashboardScreenProps> = ({ seriesId }) => {
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [thumbnailUrl, setThumbnailUrl] = useState<string>("");

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedVolumeId, setSelectedVolumeId] = useState<number | null>(null);
    const [selectedVolumeName, setSelectedVolumeName] = useState<string>("");
    const [selectedVolumeNumber, setSelectedVolumeNumber] = useState<number | null>(null);
    const [searchTerm, setSearchTerm] = useState("");

    const [isSynopsisExpanded, setIsSynopsisExpanded] = useState(false);
    const [series, setSeries] = useState<Series | null>(null);
    const [volumes, setVolumes] = useState<Volume[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const filteredVolumes = useMemo(() => {
        return volumes.filter((volume) => {
            const searchLower = searchTerm.toLowerCase();
            const nameMatch = volume.name?.toLowerCase().includes(searchLower);
            const numberMatch = volume.volumeNumber?.toString().includes(searchTerm);

            return nameMatch || numberMatch;
        });
    }, [volumes, searchTerm]);

    const fetchVolumes = async () => {
        try {
            const volumesResponse = await fetch(`${env.getBackendHost()}/api/AuthorStudio/series/${seriesId}/volumes`, {
                credentials: "include",
            });
            if (!volumesResponse.ok) {
                throw new Error(`Failed to fetch volumes: ${volumesResponse.statusText}`);
            }
            const volumesData = await volumesResponse.json();
            setVolumes(volumesData);
        } catch (err) {
            toast.error("Thất bại khi lấy danh sách tập");
        }
    };

    const handleDeleteClick = (volumeId: number, volumeName: string, volumeNumber: number) => {
        setSelectedVolumeId(volumeId);
        setSelectedVolumeName(volumeName);
        setSelectedVolumeNumber(volumeNumber);
        setIsModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        try {
            if (!selectedVolumeId) return;

            const response = await fetch(
                `${env.getBackendHost()}/api/AuthorStudio/series/${seriesId}/volumes/${selectedVolumeId}`,
                {
                    method: "DELETE",
                    credentials: "include",
                },
            );

            if (!response.ok) {
                throw new Error("Failed to delete volume");
            }

            await fetchVolumes();

            toast.success("Volume deleted successfully");
        } catch (err) {
            toast.error(err instanceof Error ? err.message : "Failed to delete volume");
            console.error("Error deleting volume:", err);
        } finally {
            setIsModalOpen(false);
            setSelectedVolumeId(null);
            setSelectedVolumeName("");
        }
    };

    useEffect(() => {
        const fetchData = async () => {
            setIsLoading(true);
            setError(null);

            try {
                const seriesResponse = await fetch(`${env.getBackendHost()}/api/AuthorStudio/series/${seriesId}`, {
                    credentials: "include",
                });
                if (!seriesResponse.ok) {
                    throw new Error(`Failed to fetch series: ${seriesResponse.statusText}`);
                }
                const seriesData = await seriesResponse.json();
                setSeries(seriesData);

                if (seriesData.thumbnail) {
                    const [imageResult, _] = await getImage(seriesData.thumbnail, "thumbnails");
                    if (imageResult) {
                        setThumbnailUrl(imageResult.imageUrl);
                    }
                }

                await fetchVolumes();
            } catch (err) {
                setError(err instanceof Error ? err.message : "An error occurred");
            } finally {
                setIsLoading(false);
            }
        };
        fetchData();
    }, [seriesId]);

    const handleVolumeAdded = (newVolume: Volume) => {
        setVolumes((prevVolumes) => [...prevVolumes, newVolume]);
    };

    if (error) {
        return <div className="rounded-md bg-red-50 p-4 text-red-500">Error: {error}</div>;
    }

    return (
        <SiteLayout>
            <div className="container min-h-screen space-y-4 p-4">
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
                            <BreadcrumbLink href="/author-studio/series">Bộ truyện</BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>{series?.title ?? "Đang tải"}</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <div className="flex flex-col gap-4 lg:flex-row">
                    <div className="flex-grow space-y-4">
                        <div className="flex flex-col gap-4 md:flex-row">
                            <Card className="w-full md:w-1/3 md:max-w-[300px]">
                                <CardContent className="p-4">
                                    {isLoading || !series ? (
                                        <Skeleton className="mb-4 h-6 w-24" />
                                    ) : (
                                        <div className="mb-4 flex items-center gap-2">
                                            <div
                                                className={`h-2 w-2 rounded-full ${STATUS_COLOR_MAP[series.status as keyof typeof STATUS_COLOR_MAP]}`}
                                            />
                                            <span className="text-sm font-medium">
                                                {
                                                    STATUS_DISPLAY_MAP[
                                                        STATUS_MAP[series.status as keyof typeof STATUS_MAP]
                                                    ]
                                                }
                                            </span>
                                        </div>
                                    )}
                                    <div className="relative w-full pb-[133%]">
                                        {isLoading || !series ? (
                                            <Skeleton className="absolute inset-0 rounded-lg" />
                                        ) : (
                                            <Image
                                                src={thumbnailUrl || "https://placehold.jp/300x400.png"}
                                                alt={series.title}
                                                className="inset-0 rounded-lg object-cover"
                                                fill
                                            />
                                        )}
                                    </div>
                                </CardContent>
                            </Card>

                            <Card className="w-full flex-grow md:w-2/3">
                                <CardContent className="space-y-4 p-4 lg:max-h-[400px] lg:overflow-y-auto">
                                    {isLoading || !series ? (
                                        <>
                                            <Skeleton className="h-8 w-3/4" />
                                            <div className="space-y-4">
                                                <Skeleton className="h-16" />
                                                <Skeleton className="h-16" />
                                                <Skeleton className="h-32" />
                                            </div>
                                        </>
                                    ) : (
                                        <>
                                            <Link href={`/series/${series.id}`} className="mb-6 text-2xl font-bold">
                                                {series.title}
                                            </Link>
                                            <div className="space-y-6">
                                                {series.alternativeTitles && series.alternativeTitles.length > 0 && (
                                                    <div>
                                                        <h3 className="mb-2 text-sm font-medium text-gray-500">
                                                            Tên khác
                                                        </h3>
                                                        <ul className="space-y-1">
                                                            {series.alternativeTitles.map((title, index) => (
                                                                <li key={index} className="list-none text-sm">
                                                                    {title}
                                                                </li>
                                                            ))}
                                                        </ul>
                                                    </div>
                                                )}
                                                <div>
                                                    <h3 className="mb-2 text-sm font-medium text-gray-500">Thể loại</h3>
                                                    <div className="flex flex-wrap gap-2">
                                                        {series.genres.map((genre, index) => (
                                                            <span
                                                                key={index}
                                                                className="rounded-md bg-gray-500 px-2 py-1 text-sm"
                                                            >
                                                                {genre}
                                                            </span>
                                                        ))}
                                                    </div>
                                                </div>
                                                <div className="flex gap-4">
                                                    <div>
                                                        <h3 className="mb-2 text-sm font-medium text-gray-500">
                                                            Phân loại độ tuổi
                                                        </h3>
                                                        <span
                                                            className={`font-bold ${RATING_CONFIG_2[series.contentRating + 1]?.color || "text-gray-500"}`}
                                                        >
                                                            {RATING_CONFIG_2[series.contentRating + 1]?.label}
                                                        </span>
                                                    </div>
                                                </div>
                                                <div>
                                                    <h3 className="mb-2 text-sm font-medium text-gray-500">Tóm tắt</h3>
                                                    <div className="space-y-2">
                                                        <p
                                                            className={`whitespace-pre-line break-words text-sm ${
                                                                isSynopsisExpanded ? "" : "line-clamp-4"
                                                            }`}
                                                        >
                                                            {series.synopsis}
                                                        </p>
                                                        <button
                                                            onClick={() => setIsSynopsisExpanded(!isSynopsisExpanded)}
                                                            className="text-sm text-primary hover:underline"
                                                        >
                                                            {isSynopsisExpanded ? "Thu gọn" : "Xem thêm"}
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        </>
                                    )}
                                </CardContent>
                            </Card>
                        </div>
                        <Tabs defaultValue="account">
                            <TabsList className="grid w-full grid-cols-2">
                                <TabsTrigger value="account">Nội dung</TabsTrigger>
                                <TabsTrigger value="password">DashiFan</TabsTrigger>
                            </TabsList>
                            <TabsContent value="account">
                                <Card>
                                    <CardHeader className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
                                        <h2 className="text-lg font-bold text-blue-300">Tập Truyện</h2>
                                        <div className="flex w-full flex-col items-stretch gap-2 sm:w-auto sm:flex-row sm:items-center">
                                            <div className="flex flex-grow gap-2 sm:flex-grow-0">
                                                <div className="relative flex-grow">
                                                    <Search className="absolute left-2 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-500" />
                                                    <Input
                                                        type="text"
                                                        placeholder="Tìm kiếm tập..."
                                                        className="py-2 pl-8 pr-4"
                                                        value={searchTerm}
                                                        onChange={(e) => setSearchTerm(e.target.value)}
                                                    />
                                                </div>
                                            </div>
                                            <Button className="shrink-0" onClick={() => setIsDialogOpen(true)}>
                                                <Plus className="mr-2 h-4 w-4" />
                                                Thêm Tập
                                            </Button>
                                        </div>
                                    </CardHeader>
                                    <CardContent>
                                        <div className="space-y-4">
                                            {isLoading ? (
                                                <>
                                                    <Skeleton className="h-16" />
                                                    <Skeleton className="h-16" />
                                                </>
                                            ) : volumes.length === 0 ? (
                                                <div className="py-8 text-center">
                                                    <p className="mb-4 text-gray-500">Chưa có tập nào</p>
                                                    <Button onClick={() => setIsDialogOpen(true)}>
                                                        <Plus className="mr-2 h-4 w-4" />
                                                        Tạo tập đầu tiên của bạn
                                                    </Button>
                                                </div>
                                            ) : (
                                                <>
                                                    <div className="flex items-center justify-between gap-4">
                                                        <div className="relative flex-grow"></div>
                                                        <p className="whitespace-nowrap text-sm text-gray-500">
                                                            {searchTerm
                                                                ? `${filteredVolumes.length} trong ${volumes.length}`
                                                                : `${volumes.length} tập`}
                                                        </p>
                                                    </div>

                                                    <div className="space-y-2">
                                                        {filteredVolumes.length === 0 ? (
                                                            <div className="py-8 text-center">
                                                                <p className="text-gray-500">
                                                                    Không tìm thấy tập nào khớp với &quot;
                                                                    {searchTerm}&quot;
                                                                </p>
                                                                <Button
                                                                    variant="link"
                                                                    onClick={() => setSearchTerm("")}
                                                                    className="mt-2"
                                                                >
                                                                    Xóa tìm kiếm
                                                                </Button>
                                                            </div>
                                                        ) : (
                                                            filteredVolumes.map((volume) => (
                                                                <div
                                                                    key={volume.volumeId}
                                                                    className="flex flex-col items-start justify-between gap-4 border-b border-neutral-700 p-4 hover:bg-neutral-700 sm:flex-row sm:items-center"
                                                                >
                                                                    <div>
                                                                        <h3 className="font-medium">
                                                                            Tập {volume.volumeNumber}: {volume.name}
                                                                        </h3>
                                                                        <p className="text-sm text-gray-500">
                                                                            {volume.chapterCount} Chương
                                                                        </p>
                                                                    </div>
                                                                    <div className="flex gap-2">
                                                                        <Link
                                                                            href={`/author-studio/series/${seriesId}/vol/${volume.volumeId}`}
                                                                        >
                                                                            <Button variant="outline" size="sm">
                                                                                Chỉnh sửa
                                                                            </Button>
                                                                        </Link>
                                                                        <Button
                                                                            variant="destructive"
                                                                            size="sm"
                                                                            onClick={() => {
                                                                                handleDeleteClick(
                                                                                    volume.volumeId,
                                                                                    volume.name,
                                                                                    volume.volumeNumber,
                                                                                );
                                                                            }}
                                                                        >
                                                                            Xóa
                                                                        </Button>
                                                                    </div>
                                                                </div>
                                                            ))
                                                        )}
                                                    </div>
                                                </>
                                            )}
                                        </div>
                                        <AddVolumeDialog
                                            isOpen={isDialogOpen}
                                            onClose={() => setIsDialogOpen(false)}
                                            seriesId={seriesId}
                                            onVolumeAdded={handleVolumeAdded}
                                            onSuccess={fetchVolumes}
                                        />
                                        <ConfirmationModal
                                            isOpen={isModalOpen}
                                            onClose={() => {
                                                setIsModalOpen(false);
                                                setSelectedVolumeId(null);
                                                setSelectedVolumeName("");
                                            }}
                                            onConfirm={handleConfirmDelete}
                                            itemName={"Tập " + selectedVolumeNumber + ": " + selectedVolumeName}
                                            entityType="tập"
                                            requiredText={"XÓA " + selectedVolumeName}
                                        />
                                    </CardContent>
                                </Card>
                            </TabsContent>
                            <TabsContent value="password">
                                <Card>
                                    <CardHeader className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
                                        <h2 className="text-lg font-bold text-blue-300">Danh sách hạng DashiFan</h2>
                                    </CardHeader>
                                    <CardContent>
                                        <DashiFanTable seriesId={seriesId} />
                                    </CardContent>
                                </Card>
                            </TabsContent>
                        </Tabs>

                        <div className="block lg:hidden">
                            <Card>
                                <CardTitle>
                                    <CardHeader className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
                                        <h2 className="text-lg font-bold text-blue-300">Tập Truyện</h2>
                                        <div className="flex w-full flex-col items-stretch gap-2 sm:w-auto sm:flex-row sm:items-center">
                                            <div className="flex flex-grow gap-2 sm:flex-grow-0">
                                                <div className="relative flex-grow">
                                                    <Search className="absolute left-2 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-500" />
                                                    <Input
                                                        type="text"
                                                        placeholder="Tìm kiếm tập..."
                                                        className="py-2 pl-8 pr-4"
                                                        value={searchTerm}
                                                        onChange={(e) => setSearchTerm(e.target.value)}
                                                    />
                                                </div>
                                            </div>
                                            <Button className="shrink-0" onClick={() => setIsDialogOpen(true)}>
                                                <Plus className="mr-2 h-4 w-4" />
                                                Thêm Tập
                                            </Button>
                                        </div>
                                    </CardHeader>
                                    <CardContent>
                                        <DashiFanOverview seriesId={seriesId} />
                                    </CardContent>
                                </CardTitle>
                            </Card>
                        </div>
                    </div>
                </div>
            </div>
        </SiteLayout>
    );
};

export default SeriesDashboardScreen;

export interface SeriesDashboardScreenProps {
    seriesId: string;
}
