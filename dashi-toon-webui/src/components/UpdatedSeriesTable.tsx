"use client";

import { Skeleton } from "@/components/ui/skeleton";

import { formatDistanceToNow } from "date-fns";
import { vi } from "date-fns/locale";
import Image from "next/image";
import { Card, CardContent } from "./ui/card";
import { UpdatedSeries } from "@/utils/api/series";
import useUwU from "@/hooks/useUwU";
import Link from "next/link";

interface NovelChaptersTableProps {
    chapters: UpdatedSeries[];
    isLoading: boolean;
}

export const placeholder =
    "https://cdn.wuxiaworld.com/images/covers/atg.webp?v=302f530596d7ba61dde2042585ca667fb2984114";

const UpdatedSeriesTable: React.FC<NovelChaptersTableProps> = ({ chapters, isLoading }) => {
    const [uwu] = useUwU();

    const formatRelativeTime = (date: Date) => {
        return formatDistanceToNow(date, { addSuffix: true, locale: vi });
    };

    return (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
            {isLoading ? (
                <ListLoadingSkeleton />
            ) : (
                chapters.map((chapter) => (
                    <Link key={chapter.id} href={`/series/${chapter.id}`}>
                        <Card className="flex h-full flex-col">
                            <CardContent className="flex h-full flex-col p-4">
                                <div className="mb-4 flex items-center space-x-4">
                                    <Image
                                        src={uwu ? placeholder : chapter.thumbnail}
                                        alt={chapter.chapterName}
                                        width={60}
                                        height={90}
                                        className="rounded"
                                    />
                                    <div className="min-w-0 flex-1 self-start">
                                        <h2 className="text-lg font-semibold text-blue-600 dark:text-blue-400">
                                            {chapter.title}
                                        </h2>
                                        <p className="truncate text-sm text-gray-500 dark:text-gray-400">
                                            {chapter.authors.join(", ")}
                                        </p>
                                    </div>
                                </div>
                                <div className="flex-1">
                                    <p className="text-sm text-gray-600 dark:text-gray-300">
                                        Chương {chapter.chapterNumber}: {chapter.chapterName}
                                    </p>
                                </div>
                                <p className="mt-2 text-xs text-gray-400 dark:text-gray-500">
                                    {formatRelativeTime(new Date(chapter.updatedAt))}
                                </p>
                            </CardContent>
                        </Card>
                    </Link>
                ))
            )}
        </div>
    );
};

const ListLoadingSkeleton: React.FC = () => (
    <>
        {[...Array(8)].map((_, i) => (
            <Card key={i} className="flex h-full flex-col">
                <CardContent className="flex h-full flex-col p-4">
                    <div className="mb-4 flex items-center space-x-4">
                        <Skeleton className="h-[90px] w-[60px] rounded" />
                        <div className="flex-1 space-y-2">
                            <Skeleton className="h-5 w-3/4" />
                            <Skeleton className="h-4 w-1/2" />
                        </div>
                    </div>
                    <Skeleton className="h-4 w-full flex-1" />
                    <Skeleton className="mt-2 h-3 w-1/4" />
                </CardContent>
            </Card>
        ))}
    </>
);

export default UpdatedSeriesTable;
