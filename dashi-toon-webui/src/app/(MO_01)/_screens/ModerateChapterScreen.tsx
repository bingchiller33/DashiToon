"use client";

import { useCallback, useEffect, useState } from "react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import SiteLayout from "@/components/SiteLayout";
import ModeratorSettingLayout from "@/components/ModeratorSettingLayout";
import { ROLES } from "@/utils/consts";
import { ModReportDialog } from "../components/ModReportDialog";
import { getReportedChapters, ReportedChapter, resolveChapterReport } from "@/utils/api/moderator/moderation";
import { BanDurationDialog } from "../components/BanDurationDialog";
import { DismissButton } from "../components/DismissButton";
import Link from "next/link";

export default function ModerateChapterScreen() {
    const [chapters, setChapters] = useState<ReportedChapter[]>([]);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        totalCount: 0,
        hasNextPage: false,
        hasPreviousPage: false,
    });

    const fetchReportedChapters = useCallback(async () => {
        const [response, err] = await getReportedChapters(pagination.pageNumber, pagination.pageSize);
        if (err) {
            toast.error("Không thể tải các chương bị báo cáo");
            return;
        }
        setChapters(response.items);
        setPagination((prev) => ({
            ...prev,
            totalPages: response.totalPages,
            totalCount: response.totalCount,
            hasNextPage: response.hasNextPage,
            hasPreviousPage: response.hasPreviousPage,
        }));
    }, [pagination.pageNumber, pagination.pageSize]);

    useEffect(() => {
        fetchReportedChapters();
    }, [fetchReportedChapters]);

    const handleResolve = async (chapterId: number, duration: number) => {
        try {
            const [_, err] = await resolveChapterReport(chapterId.toString(), duration);
            if (err) throw err;

            await fetchReportedChapters();

            toast.success(`Báo cáo chương đã được giải quyết và người dùng đã bị cấm trong ${duration} ngày.`);
        } catch (error) {
            toast.error("Không thể giải quyết báo cáo chương");
        }
    };

    return (
        <SiteLayout allowedRoles={[ROLES.Moderator]} hiddenUntilLoaded>
            <div className="container min-h-screen pt-6">
                <ModeratorSettingLayout>
                    <h1 className="mb-5 text-2xl font-bold">Kiểm duyệt chương</h1>

                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Truyện</TableHead>
                                <TableHead>Chương</TableHead>
                                <TableHead>Tác giả</TableHead>
                                <TableHead>Báo cáo</TableHead>
                                <TableHead>Thao tác</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {chapters.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-24 text-center text-muted-foreground">
                                        Không có báo cáo chương nào cần xử lý
                                    </TableCell>
                                </TableRow>
                            ) : (
                                chapters.map((chapter) => (
                                    <TableRow key={chapter.chapterId}>
                                        <TableCell className="font-medium">{chapter.seriesTitle}</TableCell>
                                        <TableCell>
                                            Tập {chapter.volumeNumber}, Chương {chapter.chapterNumber}
                                        </TableCell>
                                        <TableCell>{chapter.seriesAuthor}</TableCell>
                                        <TableCell>
                                            <ModReportDialog
                                                reports={chapter.reports}
                                                itemType="Chương"
                                                reportedBy={chapter.seriesAuthor}
                                                contentLocation={`Truyện ${chapter.seriesTitle}, Tập ${chapter.volumeNumber}, Chương ${chapter.chapterNumber}`}
                                                reportCount={chapter.reports.length}
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex space-x-2">
                                                <BanDurationDialog
                                                    onConfirm={(duration) => handleResolve(chapter.chapterId, duration)}
                                                    triggerText="Giải quyết"
                                                    title={`Xử lý báo cáo chương ${chapter.chapterNumber} tập ${chapter.volumeNumber} của "${chapter.seriesTitle}"`}
                                                    description="Chọn thời gian cấm người dùng và giải quyết báo cáo này."
                                                />
                                                <DismissButton
                                                    entityId={chapter.chapterId.toString()}
                                                    type="content"
                                                    onSuccess={fetchReportedChapters}
                                                />
                                                <Link
                                                    href={`/series/${chapter.seriesId}/vol/${chapter.volumeId}/chap/${chapter.chapterId}/${chapter.seriesType === 1 ? "novel" : "comic"}`}
                                                >
                                                    <Button variant="link">Đọc</Button>
                                                </Link>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>

                    <div className="mt-4 flex items-center justify-between">
                        <div>
                            Trang {pagination.pageNumber} / {pagination.totalPages}
                        </div>
                        <div className="flex gap-2">
                            <Button
                                onClick={() =>
                                    setPagination((prev) => ({
                                        ...prev,
                                        pageNumber: Math.max(1, prev.pageNumber - 1),
                                    }))
                                }
                                disabled={!pagination.hasPreviousPage}
                            >
                                Trước
                            </Button>
                            <Button
                                onClick={() =>
                                    setPagination((prev) => ({
                                        ...prev,
                                        pageNumber: prev.pageNumber + 1,
                                    }))
                                }
                                disabled={!pagination.hasNextPage}
                            >
                                Sau
                            </Button>
                        </div>
                    </div>
                </ModeratorSettingLayout>
            </div>
        </SiteLayout>
    );
}
