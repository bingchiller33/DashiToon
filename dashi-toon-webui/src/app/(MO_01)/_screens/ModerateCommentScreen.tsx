"use client";

import { useState, useCallback, useEffect } from "react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import SiteLayout from "@/components/SiteLayout";
import ModeratorSettingLayout from "@/components/ModeratorSettingLayout";
import { ROLES } from "@/utils/consts";
import { ReportedComment, getReportedComments, resolveCommentReport } from "@/utils/api/moderator/moderation";
import { ModReportDialog } from "@/app/(MO_01)/components/ModReportDialog";
import { BanDurationDialog } from "../components/BanDurationDialog";
import { DismissButton } from "../components/DismissButton";

export default function ModerateCommentScreen() {
    const [comments, setComments] = useState<ReportedComment[]>([]);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        totalCount: 0,
        hasNextPage: false,
        hasPreviousPage: false,
    });

    const fetchReportedComments = useCallback(async () => {
        const [response, err] = await getReportedComments(pagination.pageNumber, pagination.pageSize);
        if (err) {
            toast.error("Không thể tải bình luận bị báo cáo");
            return;
        }
        setComments(response.items);
        setPagination((prev) => ({
            ...prev,
            totalPages: response.totalPages,
            totalCount: response.totalCount,
            hasNextPage: response.hasNextPage,
            hasPreviousPage: response.hasPreviousPage,
        }));
    }, [pagination.pageNumber, pagination.pageSize]);

    useEffect(() => {
        fetchReportedComments();
    }, [fetchReportedComments]);

    const handleResolve = async (commentId: string, duration: number) => {
        try {
            const [_, err] = await resolveCommentReport(commentId, duration);
            if (err) throw err;

            await fetchReportedComments();

            toast.success(`Báo cáo của bộ truyện đã được giải quyết và người dùng đã bị cấm trong ${duration} ngày.`);
        } catch (error) {
            toast.error("Không thể giải quyết báo cáo của bộ truyện");
        }
    };

    return (
        <SiteLayout allowedRoles={[ROLES.Moderator]} hiddenUntilLoaded>
            <div className="container min-h-screen pt-6">
                <ModeratorSettingLayout>
                    <h1 className="mb-5 text-2xl font-bold">Kiểm duyệt bình luận</h1>

                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Truyện</TableHead>
                                <TableHead>Chương</TableHead>
                                <TableHead>Bình luận</TableHead>
                                <TableHead>Báo cáo</TableHead>
                                <TableHead>Hành động</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {comments.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-24 text-center text-muted-foreground">
                                        Không có báo cáo bình luận nào cần xử lý
                                    </TableCell>
                                </TableRow>
                            ) : (
                                comments.map((comment) => (
                                    <TableRow key={comment.commentId}>
                                        <TableCell className="font-medium">{comment.seriesTitle}</TableCell>
                                        <TableCell>
                                            Tập {comment.volumeNumber}, Chương {comment.chapterNumber}
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex flex-col gap-1">
                                                <span className="font-bold">{comment.commentUser}</span>
                                                <span>{comment.commentContent}</span>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <ModReportDialog
                                                reports={comment.reports}
                                                itemType="Bình luận"
                                                reportedBy={comment.commentUser}
                                                contentLocation={`Truyện ${comment.seriesTitle}, Tập ${comment.volumeNumber}, Chương ${comment.chapterNumber}`}
                                                reportCount={comment.reports.length}
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex space-x-2">
                                                <BanDurationDialog
                                                    onConfirm={(duration) => handleResolve(comment.commentId, duration)}
                                                    triggerText="Giải quyết"
                                                    title={`Xử lý báo cáo bình luận của "${comment.commentUser}"`}
                                                    description="Chọn thời gian cấm người dùng và giải quyết báo cáo bình luận này."
                                                />
                                                <DismissButton
                                                    entityId={comment.commentId}
                                                    type="comment"
                                                    onSuccess={fetchReportedComments}
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
                            Page {pagination.pageNumber} of {pagination.totalPages}
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
