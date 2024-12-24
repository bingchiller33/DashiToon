"use client";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import { CoinsIcon } from "lucide-react";
import { useEffect, useState, useCallback } from "react";
import { getChapterPrice, unlockChapter } from "@/utils/api/reader/chapter";
import { toast } from "sonner";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { BuyKanaGold } from "@/app/(KC_01)/components/BuyKanaGold";
import Link from "next/link";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";

interface UnlockChapterProps {
    isAuthenticated: boolean;
    timeUntilFree: string;
    seriesId: string;
    volumeId: string;
    chapterId: string;
    onUnlockSuccess?: () => void;
    autoUnlock: boolean;
    onAutoUnlockChange: (checked: boolean) => void;
}

export const UnlockChapter: React.FC<UnlockChapterProps> = ({
    isAuthenticated,
    timeUntilFree,
    seriesId,
    volumeId,
    chapterId,
    onUnlockSuccess,
    autoUnlock,
    onAutoUnlockChange,
}) => {
    const [isUnlocking, setIsUnlocking] = useState(false);
    const [showBuyCoins, setShowBuyCoins] = useState(false);
    const [chapterPrice, setChapterPrice] = useState<number>(45);

    const handleUnlock = useCallback(async () => {
        setIsUnlocking(true);
        try {
            const [data, error] = await unlockChapter(seriesId, volumeId, chapterId, chapterPrice);
            if (error) {
                if (error.status === 409) {
                    toast.warning("Không đủ xu để mở khóa chương này!", {
                        id: "insufficient-coins",
                        action: {
                            label: "Mua Xu",
                            onClick: () => setShowBuyCoins(true),
                        },
                        position: "bottom-center",
                    });
                    return;
                }
                toast.error("Không thể mở khóa chương. Vui lòng thử lại sau!", {
                    id: "unlock-error",
                });
                return;
            }

            toast.success("Mở khóa chương thành công!", {
                id: "unlock-success",
            });
            onUnlockSuccess?.();
        } catch (err) {
            console.error("Error unlocking chapter:", err);
            toast.error("Đã xảy ra lỗi không mong muốn", {
                id: "unexpected-error",
            });
        } finally {
            setIsUnlocking(false);
        }
    }, [seriesId, volumeId, chapterId, onUnlockSuccess, chapterPrice]);

    useEffect(() => {
        if (autoUnlock && isAuthenticated) {
            handleUnlock();
        }
    }, [autoUnlock, isAuthenticated, handleUnlock]);

    useEffect(() => {
        async function fetchChapterPrice() {
            const [price, error] = await getChapterPrice(seriesId, volumeId, chapterId);
            if (!error) {
                setChapterPrice(price);
            }
        }

        fetchChapterPrice();
    }, [seriesId, volumeId, chapterId]);

    const handleAutoUnlockChange = (checked: boolean) => {
        onAutoUnlockChange(checked);
    };

    return (
        <div className="relative flex min-h-[70vh] items-center justify-center">
            <div className="absolute inset-0">
                <div className="h-full w-full space-y-4 px-4 py-10">
                    <div className="flex justify-center">
                        <Skeleton className="h-8 w-64" />
                    </div>

                    <div className="space-y-4">
                        <Skeleton className="h-4 w-full" />
                        <Skeleton className="h-4 w-5/6" />
                        <Skeleton className="h-4 w-full" />
                        <Skeleton className="h-4 w-4/5" />
                        <Skeleton className="h-4 w-full" />
                    </div>

                    <div className="space-y-6">
                        <div className="space-y-4">
                            <Skeleton className="h-6 w-full" />
                            <Skeleton className="h-6 w-11/12" />
                            <Skeleton className="h-6 w-4/5" />
                        </div>
                        <div className="space-y-4">
                            <Skeleton className="h-6 w-full" />
                            <Skeleton className="h-6 w-11/12" />
                            <Skeleton className="h-6 w-3/4" />
                        </div>
                    </div>
                </div>
            </div>
            <div
                className={cn(
                    "relative z-10 w-full max-w-md rounded-xl p-8",
                    "duration-500 animate-in fade-in-0 slide-in-from-bottom-4",
                    "bg-white/95 shadow-[0_8px_30px_rgb(0,0,0,0.12)] backdrop-blur-sm dark:bg-gray-900/95",
                    "dark:shadow-[0_8px_30px_rgb(0,0,0,0.3)]",
                )}
            >
                {!isAuthenticated ? (
                    <>
                        <h2 className="text-center text-2xl font-bold tracking-tight">
                            Đăng nhập để tiếp tục đọc truyện
                        </h2>
                        <p className="mt-4 text-center text-muted-foreground">Những lợi ích khác bạn sẽ nhận được</p>
                        <div className="mt-8 space-y-4">
                            <div className="flex items-center space-x-3">
                                <span className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-primary">
                                    ✓
                                </span>
                                <span className="text-base">Mở khóa chương miễn phí mỗi ngày</span>
                            </div>
                            <div className="flex items-center space-x-3">
                                <span className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-primary">
                                    ✓
                                </span>
                                <span className="text-base">Lưu tiến độ đọc truyện</span>
                            </div>
                            <div className="flex items-center space-x-3">
                                <span className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-primary">
                                    ✓
                                </span>
                                <span className="text-base">Nhận thông báo chương mới</span>
                            </div>
                        </div>
                        <div className="mt-10 flex flex-col gap-3">
                            <Link href="/login?redirect=back" className="w-full">
                                <Button className="w-full text-base font-medium" size="lg">
                                    ĐĂNG NHẬP
                                </Button>
                            </Link>
                            <Link href="/signup?redirect=back" className="w-full">
                                <Button variant="outline" className="w-full text-base font-medium" size="lg">
                                    ĐĂNG KÝ
                                </Button>
                            </Link>
                        </div>
                    </>
                ) : (
                    <>
                        {/* <h2 className="text-center text-2xl font-bold tracking-tight">
                            Thời gian chờ mở khóa miễn phí
                        </h2>
                        <div className="mt-8 flex justify-center">
                            <div className="text-center">
                                <div className="text-4xl font-bold text-primary">{timeUntilFree}</div>
                                <div className="mt-3 text-sm text-muted-foreground">hoặc</div>
                            </div>
                        </div> */}
                        <div className="mt-10 space-y-4">
                            <Button
                                onClick={handleUnlock}
                                disabled={isUnlocking}
                                className="w-full text-base font-medium"
                                size="lg"
                            >
                                {isUnlocking ? (
                                    "Đang mở khóa..."
                                ) : (
                                    <>
                                        MỞ KHÓA NGAY <CoinsIcon className="ml-2" /> {chapterPrice}
                                    </>
                                )}
                            </Button>
                            <Link href={`/series/7?tab=dashifan`} className="mt-2 w-full">
                                <Button
                                    variant="secondary"
                                    className="w-full bg-black text-base font-medium text-white hover:bg-black/90"
                                    size="lg"
                                >
                                    Trở thành DashiFan - mở khóa tất cả các chương
                                </Button>
                            </Link>
                            <div className="mt-6 flex items-center justify-center space-x-2">
                                <Checkbox
                                    id="auto-unlock"
                                    checked={autoUnlock}
                                    onCheckedChange={handleAutoUnlockChange}
                                />
                                <Label htmlFor="auto-unlock" className="cursor-pointer text-sm text-muted-foreground">
                                    Tự động mở khóa chương mới
                                </Label>
                            </div>
                            <p className="mt-2 text-center text-xs text-muted-foreground">
                                Tự động sử dụng xu để mở khóa chương mới
                            </p>
                        </div>
                        <Dialog open={showBuyCoins} onOpenChange={setShowBuyCoins}>
                            <DialogContent className="max-w-3xl border-none bg-transparent p-0">
                                <BuyKanaGold />
                            </DialogContent>
                        </Dialog>
                    </>
                )}
            </div>
        </div>
    );
};
