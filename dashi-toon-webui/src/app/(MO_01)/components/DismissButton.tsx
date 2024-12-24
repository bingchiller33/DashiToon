import { Button } from "@/components/ui/button";
import { dismissReport } from "@/utils/api/moderator/moderation";
import { toast } from "sonner";

interface DismissButtonProps {
    entityId: string;
    type: "review" | "comment" | "content" | "series";
    onSuccess?: () => Promise<void>;
    className?: string;
}

export function DismissButton({ entityId, type, onSuccess, className }: DismissButtonProps) {
    const handleDismiss = async () => {
        try {
            const [_, err] = await dismissReport(entityId, type);
            if (err) throw err;

            if (onSuccess) {
                await onSuccess();
            }

            toast.success(`Báo cáo ${type} đã được bỏ qua thành công.`, {
                description: "Báo cáo đã bị bỏ qua",
            });
        } catch (error) {
            toast.error(`Không thể bỏ qua báo cáo ${type}`, {
                description: "Lỗi",
            });
        }
    };

    return (
        <Button onClick={handleDismiss} variant="ghost" className={`text-gray-500 ${className}`}>
            Bỏ qua
        </Button>
    );
}
