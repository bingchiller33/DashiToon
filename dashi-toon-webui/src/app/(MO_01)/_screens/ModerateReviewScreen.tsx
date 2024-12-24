"use client";

import { useState, useCallback, useEffect } from "react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import ModeratorSettingLayout from "@/components/ModeratorSettingLayout";
import SiteLayout from "@/components/SiteLayout";
import { ROLES } from "@/utils/consts";
import { getReportedReviews, ReportedReview, resolveReviewReport } from "@/utils/api/moderator/moderation";
import { ModReportDialog } from "../components/ModReportDialog";
import { DismissButton } from "../components/DismissButton";
import { BanDurationDialog } from "../components/BanDurationDialog";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";

export default function ModerateReviewScreen() {
    const [reviews, setReviews] = useState<ReportedReview[]>([]);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        totalCount: 0,
        hasNextPage: false,
        hasPreviousPage: false,
    });

    const fetchReportedReviews = useCallback(async () => {
        const [response, err] = await getReportedReviews(pagination.pageNumber, pagination.pageSize);
        if (err) {
            toast.error("Không thể tải các đánh giá bị báo cáo");
            return;
        }
        setReviews(response.items);
        setPagination((prev) => ({
            ...prev,
            totalPages: response.totalPages,
            totalCount: response.totalCount,
            hasNextPage: response.hasNextPage,
            hasPreviousPage: response.hasPreviousPage,
        }));
    }, [pagination.pageNumber, pagination.pageSize]);

    useEffect(() => {
        fetchReportedReviews();
    }, [fetchReportedReviews]);

    const handleResolve = async (reviewId: string, duration: number) => {
        try {
            const [_, err] = await resolveReviewReport(reviewId, duration);
            if (err) throw err;

            await fetchReportedReviews();

            toast.success(`Báo cáo đánh giá đã được giải quyết và người dùng đã bị cấm trong ${duration} ngày.`);
        } catch (error) {
            toast.error("Không thể giải quyết báo cáo đánh giá");
        }
    };

    return (
        <SiteLayout allowedRoles={[ROLES.Moderator]} hiddenUntilLoaded>
            <div className="container min-h-screen pt-6">
                <ModeratorSettingLayout>
                    <h1 className="mb-5 text-2xl font-bold">Kiểm duyệt đánh giá</h1>

                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Đánh giá</TableHead>
                                <TableHead>Người dùng</TableHead>
                                <TableHead>Truyện</TableHead>
                                <TableHead>Báo cáo</TableHead>
                                <TableHead>Thao tác</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {reviews.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-24 text-center text-muted-foreground">
                                        Không có báo cáo đánh giá nào cần xử lý
                                    </TableCell>
                                </TableRow>
                            ) : (
                                reviews.map((review) => (
                                    <TableRow key={review.reviewId}>
                                        <TableCell className="max-w-xs">
                                            <Dialog>
                                                <DialogTrigger asChild>
                                                    <Button
                                                        variant="link"
                                                        className="h-auto w-full max-w-xs p-0 text-left hover:no-underline"
                                                    >
                                                        <span className="block overflow-hidden text-ellipsis whitespace-nowrap">
                                                            {review.reviewContent}
                                                        </span>
                                                    </Button>
                                                </DialogTrigger>
                                                <DialogContent className="max-w-2xl">
                                                    <DialogHeader>
                                                        <DialogTitle>Đánh giá của {review.reviewUser}</DialogTitle>
                                                    </DialogHeader>
                                                    <div className="mt-4 max-h-[60vh] overflow-y-auto whitespace-pre-wrap break-words">
                                                        {review.reviewContent}
                                                    </div>
                                                </DialogContent>
                                            </Dialog>
                                        </TableCell>
                                        <TableCell>{review.reviewUser}</TableCell>
                                        <TableCell>{review.seriesTitle}</TableCell>
                                        <TableCell>
                                            <ModReportDialog
                                                reports={review.reports}
                                                itemType="Đánh giá"
                                                reportedBy={review.reviewUser}
                                                contentLocation={`Truyện ${review.seriesTitle}`}
                                                reportCount={review.reports.length}
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex space-x-2">
                                                <BanDurationDialog
                                                    onConfirm={(duration) => handleResolve(review.reviewId, duration)}
                                                    triggerText="Giải quyết"
                                                    title={`Giải quyết báo cáo đánh giá của "${review.reviewUser}" cho truyện "${review.seriesTitle}"`}
                                                    description="Chọn thời gian cấm người dùng và giải quyết báo cáo đánh giá này."
                                                />
                                                <DismissButton
                                                    entityId={review.reviewId}
                                                    type="review"
                                                    onSuccess={fetchReportedReviews}
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
