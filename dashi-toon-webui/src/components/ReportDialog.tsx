import { useState } from "react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Flag } from "lucide-react";
import { toast } from "sonner";
import { RadioGroup, RadioGroupItem } from "./ui/radio-group";
import { Label } from "./ui/label";

interface ReportDialogProps {
    contentId: string;
    contentType: ContentType;
    username?: string; // Optional since series/chapters might not have a specific user
    contentTitle?: string; // For series/chapter titles
    onReport: (contentId: string, reason: string, contentType: ContentType) => Promise<void>;
    trigger?: React.ReactNode; // Custom trigger button
    iconSize?: number;
}

type ContentType = "series" | "chapter" | "review" | "comment";

const getReportOptions = (contentType: ContentType) => {
    const commonOptions = [
        { value: "harassment", label: "Quấy rối hoặc lăng mạ" },
        { value: "spam", label: "Spam" },
        { value: "copyright", label: "Vi phạm bản quyền" },
        { value: "discrimination", label: "Phân biệt đối xử (phân biệt chủng tộc, giới tính,...)" },
        { value: "custom", label: "Lý do khác" },
    ];

    const specificOptions = {
        series: [
            { value: "wrong_info", label: "Thông tin không chính xác" },
            { value: "duplicate", label: "Trùng lặp nội dung" },
        ],
        chapter: [
            { value: "wrong_content", label: "Nội dung sai" },
            { value: "bad_quality", label: "Chất lượng kém" },
        ],
        review: [
            { value: "spoilers", label: "Tiết lộ nội dung truyện" },
            { value: "irrelevant", label: "Không liên quan đến nội dung" },
        ],
        comment: [
            { value: "spoilers", label: "Tiết lộ nội dung truyện" },
            { value: "delete_request", label: "Yêu cầu xóa bình luận của bạn" },
        ],
    };

    return [...specificOptions[contentType], ...commonOptions];
};

const getDialogTitle = (contentType: ContentType) => {
    const titles = {
        series: "Báo cáo series",
        chapter: "Báo cáo chapter",
        review: "Báo cáo đánh giá",
        comment: "Báo cáo bình luận",
    };
    return titles[contentType];
};

export function ReportDialog({
    contentId,
    contentType,
    username,
    contentTitle,
    onReport,
    trigger,
    iconSize = 4,
}: ReportDialogProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [selectedReason, setSelectedReason] = useState("");
    const [customReason, setCustomReason] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);

    const reportOptions = getReportOptions(contentType);

    const handleSubmit = async () => {
        const finalReason =
            selectedReason === "custom"
                ? customReason
                : reportOptions.find((opt) => opt.value === selectedReason)?.label || "";

        if (!finalReason.trim()) {
            toast.error("Vui lòng chọn hoặc nhập lý do báo cáo");
            return;
        }

        setIsSubmitting(true);
        try {
            await onReport(contentId, finalReason, contentType);
            setIsOpen(false);
            setSelectedReason("");
            setCustomReason("");
            toast.success("Đã gửi báo cáo thành công");
        } catch (error) {
            toast.error("Không thể gửi báo cáo. Vui lòng thử lại sau");
        } finally {
            setIsSubmitting(false);
        }
    };
    return (
        <Dialog open={isOpen} onOpenChange={setIsOpen}>
            <DialogTrigger asChild>
                {trigger || (
                    <button className="text-base text-muted-foreground transition-colors hover:text-red-500">
                        <Flag className={`h-${iconSize} w-${iconSize}`} />
                    </button>
                )}
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{getDialogTitle(contentType)}</DialogTitle>
                    <DialogDescription>
                        {contentTitle && `Báo cáo "${contentTitle}"`}
                        {username && `Báo cáo nội dung của ${username}`}
                    </DialogDescription>
                </DialogHeader>
                <div className="space-y-4 py-4">
                    <RadioGroup value={selectedReason} onValueChange={setSelectedReason} className="space-y-2">
                        {reportOptions.map((option) => (
                            <div key={option.value} className="flex items-center space-x-2">
                                <RadioGroupItem value={option.value} id={option.value} />
                                <Label htmlFor={option.value}>{option.label}</Label>
                            </div>
                        ))}
                    </RadioGroup>

                    {selectedReason === "custom" && (
                        <Textarea
                            placeholder="Nhập lý do báo cáo của bạn..."
                            value={customReason}
                            onChange={(e) => setCustomReason(e.target.value)}
                            className="min-h-[100px]"
                        />
                    )}

                    <div className="flex justify-end gap-2">
                        <Button variant="ghost" onClick={() => setIsOpen(false)}>
                            Hủy
                        </Button>
                        <Button onClick={handleSubmit} disabled={isSubmitting}>
                            Gửi báo cáo
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}
