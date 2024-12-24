import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { CATEGORY_TRANSLATIONS } from "@/utils/consts";
import { formatScore, getScoreColor } from "@/lib/moderation";

interface Report {
    reportedByUsername: string;
    reason: string;
    reportedAt: string;
    flagged: boolean;
    flaggedCategories?: Record<string, number>;
}

interface ModReportDialogProps {
    reports: Report[];
    itemType: string;
    reportedBy: string;
    contentLocation: string;
    reportCount: number;
}

export function ModReportDialog({
    reports,
    itemType,
    reportedBy,
    contentLocation,
    reportCount,
}: ModReportDialogProps) {
    return (
        <Dialog>
            <DialogTrigger asChild>
                <Button
                    variant="ghost"
                    className="flex items-center gap-2 text-red-500"
                >
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        className="lucide lucide-flag"
                    >
                        <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z" />
                        <line x1="4" x2="4" y1="22" y2="15" />
                    </svg>
                    <span>({reportCount})</span>
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Chi tiết báo cáo</DialogTitle>
                    <DialogDescription>
                        {itemType} bởi {reportedBy} trong {contentLocation}
                    </DialogDescription>
                </DialogHeader>
                <div className="mt-4 space-y-4">
                    {reports.map((report, index) => (
                        <div key={index} className="rounded-lg border p-4">
                            <p>
                                <strong>Người báo cáo:</strong>{" "}
                                {report.reportedByUsername}
                            </p>
                            <p>
                                <strong>Lý do:</strong> {report.reason}
                            </p>
                            <p>
                                <strong>Thời gian:</strong>{" "}
                                {new Date(report.reportedAt).toLocaleString()}
                            </p>
                            <p>
                                <strong>Đã gắn cờ:</strong>{" "}
                                {report.flagged ? "Có" : "Không"}
                            </p>
                            {report.flagged && report.flaggedCategories && (
                                <div>
                                    <strong>Phân tích nội dung:</strong>
                                    <div className="mt-2 grid gap-2">
                                        {Object.entries(
                                            report.flaggedCategories,
                                        )
                                            .filter(
                                                ([_, score]) => score > 0.001,
                                            )
                                            .sort(([_1, a], [_2, b]) => b - a)
                                            .map(([category, score]) => (
                                                <div
                                                    key={category}
                                                    className="flex items-center justify-between rounded-md p-2 text-sm"
                                                >
                                                    <span>
                                                        {CATEGORY_TRANSLATIONS[
                                                            category
                                                        ] || category}
                                                    </span>
                                                    <Badge
                                                        variant="secondary"
                                                        className={`ml-2 ${getScoreColor(score)}`}
                                                    >
                                                        {formatScore(score)}
                                                        <span className="ml-1 text-xs opacity-75">
                                                            (
                                                            {(
                                                                score * 100
                                                            ).toFixed(1)}
                                                            %)
                                                        </span>
                                                    </Badge>
                                                </div>
                                            ))}
                                    </div>
                                    {Object.entries(
                                        report.flaggedCategories,
                                    ).every(([_, score]) => score <= 0.001) && (
                                        <p className="text-sm italic text-muted-foreground">
                                            Không phát hiện vi phạm nghiêm trọng
                                        </p>
                                    )}
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            </DialogContent>
        </Dialog>
    );
}
