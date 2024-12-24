"use client";

import { useState, useCallback, useEffect } from "react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import Image from "next/image";
import ModeratorSettingLayout from "@/components/ModeratorSettingLayout";
import { ROLES } from "@/utils/consts";
import SiteLayout from "@/components/SiteLayout";
import { getReportedSeries, ReportedSeries, resolveSeriesReport } from "@/utils/api/moderator/moderation";
import { ModReportDialog } from "../components/ModReportDialog";
import { BanDurationDialog } from "../components/BanDurationDialog";
import { DismissButton } from "../components/DismissButton";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";

export default function ModerateSeriesScreen() {
    const [series, setSeries] = useState<ReportedSeries[]>([]);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        totalCount: 0,
        hasNextPage: false,
        hasPreviousPage: false,
    });

    const fetchReportedSeries = useCallback(async () => {
        const [response, err] = await getReportedSeries(pagination.pageNumber, pagination.pageSize);
        if (err) {
            toast.error("Không thể tải danh sách truyện bị báo cáo");
            return;
        }
        setSeries(response.items);
        setPagination((prev) => ({
            ...prev,
            totalPages: response.totalPages,
            totalCount: response.totalCount,
            hasNextPage: response.hasNextPage,
            hasPreviousPage: response.hasPreviousPage,
        }));
    }, [pagination.pageNumber, pagination.pageSize]);

    useEffect(() => {
        fetchReportedSeries();
    }, [fetchReportedSeries]);

    const handleResolve = async (seriesId: number, duration: number) => {
        try {
            const [_, err] = await resolveSeriesReport(seriesId.toString(), duration);
            if (err) throw err;

            await fetchReportedSeries();

            toast.success(`Báo cáo truyện đã được giải quyết và người dùng đã bị cấm trong ${duration} ngày.`);
        } catch (error) {
            toast.error("Không thể giải quyết báo cáo truyện");
        }
    };

    return (
        <SiteLayout allowedRoles={[ROLES.Moderator]} hiddenUntilLoaded>
            <div className="container min-h-screen pt-6">
                <ModeratorSettingLayout>
                    <h1 className="mb-5 text-2xl font-bold">Truyện bị báo cáo</h1>

                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Ảnh bìa</TableHead>
                                <TableHead>Tên truyện</TableHead>
                                <TableHead>Tác giả</TableHead>
                                <TableHead>Tóm tắt</TableHead>
                                <TableHead>Báo cáo</TableHead>
                                <TableHead>Thao tác</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {series.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={6} className="h-24 text-center text-muted-foreground">
                                        Không có báo cáo truyện nào cần xử lý
                                    </TableCell>
                                </TableRow>
                            ) : (
                                series.map((s) => (
                                    <TableRow key={s.seriesId}>
                                        <TableCell>
                                            <Image
                                                src={s.seriesThumbnail}
                                                alt={s.seriesTitle}
                                                width={100}
                                                height={150}
                                                className="object-cover"
                                            />
                                        </TableCell>
                                        <TableCell className="font-medium">{s.seriesTitle}</TableCell>
                                        <TableCell>{s.seriesAuthor}</TableCell>
                                        <TableCell className="max-w-xs">
                                            <Dialog>
                                                <DialogTrigger asChild>
                                                    <Button 
                                                        variant="link" 
                                                        className="h-auto w-full max-w-xs p-0 text-left hover:no-underline"
                                                    >
                                                        <span 
                                                            className="block overflow-hidden text-ellipsis whitespace-nowrap"
                                                            title={s.seriesSynopsis}
                                                        >
                                                            {s.seriesSynopsis}
                                                        </span>
                                                    </Button>
                                                </DialogTrigger>
                                                <DialogContent className="max-w-2xl">
                                                    <DialogHeader>
                                                        <DialogTitle>Tóm tắt truyện: {s.seriesTitle}</DialogTitle>
                                                    </DialogHeader>
                                                    <div className="mt-4 max-h-[60vh] overflow-y-auto whitespace-pre-wrap break-words">
                                                        {s.seriesSynopsis}
                                                    </div>
                                                </DialogContent>
                                            </Dialog>
                                        </TableCell>
                                        <TableCell>
                                            <ModReportDialog
                                                reports={s.reports}
                                                itemType="Truyện"
                                                reportedBy={s.seriesAuthor}
                                                contentLocation={`Truyện ${s.seriesTitle}`}
                                                reportCount={s.reports.length}
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex space-x-2">
                                                <BanDurationDialog
                                                    onConfirm={(duration) => handleResolve(s.seriesId, duration)}
                                                    triggerText="Giải quyết"
                                                    title={`Giải quyết báo cáo cho "${s.seriesTitle}"`}
                                                    description="Chọn thời gian cấm người dùng và giải quyết báo cáo này."
                                                />
                                                <DismissButton
                                                    entityId={s.seriesId.toString()}
                                                    type="series"
                                                    onSuccess={fetchReportedSeries}
                                                />
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
