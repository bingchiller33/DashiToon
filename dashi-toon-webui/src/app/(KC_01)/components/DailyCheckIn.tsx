import { Button } from "@/components/ui/button";
import { checkIn } from "@/utils/api/reader/kana";
import { Coins } from "lucide-react";
import { toast } from "sonner";

type DailyCheckInProps = {
    isCheckedIn: boolean;
    onCheckIn: () => void;
};

export function DailyCheckIn({ isCheckedIn, onCheckIn }: DailyCheckInProps) {
    const handleCheckIn = async () => {
        const [data, error] = await checkIn();
        if (error) {
            console.error("Failed to check in:", error);
            toast.error("Không thể điểm danh. Vui lòng thử lại sau.");
        } else {
            toast.success("Điểm danh thành công!");
            onCheckIn();
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex flex-col space-y-1.5">
                <div className="flex items-center justify-between">
                    <h3 className="text-xl font-semibold tracking-tight text-gray-200">
                        Điểm Danh Hàng Ngày
                    </h3>
                    <div className="flex">
                        <Coins className="mr-2 h-6 w-6 flex-shrink-0 text-gray-400" />
                        <p className="text-2xl font-bold">
                            100{" "}
                            <span className="text-sm text-muted-foreground">
                                / lần
                            </span>
                        </p>
                    </div>
                </div>
                <p className="text-sm text-muted-foreground">
                    Đăng nhập mỗi ngày để nhận Kana Coin.
                </p>
            </div>
            <Button
                onClick={handleCheckIn}
                disabled={isCheckedIn}
                className="w-full bg-blue-600 text-white hover:bg-blue-700"
            >
                {isCheckedIn
                    ? "Đã Nhận Thưởng Hôm Nay"
                    : "Nhận KanaCoin Hàng Ngày"}
            </Button>
        </div>
    );
}
