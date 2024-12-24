"use client";
import cx from "classnames";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { ChartContainer, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart";
import { ChevronDown, Edit, Home } from "lucide-react";
import {
    Breadcrumb,
    BreadcrumbEllipsis,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import SiteLayout from "@/components/SiteLayout";
import { editDashiFanTier, getDashiFanTiers } from "@/utils/api/dashifan";
import { toast } from "sonner";
import { useRouter } from "next/navigation";
import { getSeriesInfo } from "@/utils/api/author-studio/series";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import FanTierForm, { FormValues } from "@/app/(DF_01)/_components/FanTierForm";
import UpdateTierConfirmButton from "@/app/(DF_01)/_components/UpdateTierConfirmButton";

export interface TierManagementProps {
    seriesId: string;
    tierId: string;
}

interface TierData {
    title: string;
    price: number;
    earlyChapterCount: number;
    modifiedAt: Date;
}

const initialTierData: TierData = {
    title: "Gold",
    price: 9.99,
    earlyChapterCount: 5,
    modifiedAt: new Date(),
};

const subscriptionData = [
    { month: "Jan", subscribers: 100 },
    { month: "Feb", subscribers: 120 },
    { month: "Mar", subscribers: 150 },
    { month: "Apr", subscribers: 180 },
    { month: "May", subscribers: 200 },
];

export default function EditDashiFanDialog(props: TierManagementProps) {
    const { seriesId, tierId } = props;
    const router = useRouter();
    const [tierData, setTierData] = useState<TierData>(initialTierData);
    const [lastEditSubmit, setLastEditSubmit] = useState<FormValues | null>(null);

    const currentSubscribers = subscriptionData.map((x) => x.subscribers).reduce((a, b) => a + b, 0);

    const currentMonthGrowth =
        subscriptionData[subscriptionData.length - 1].subscribers -
        subscriptionData[subscriptionData.length - 2].subscribers;

    const growthRate = ((currentMonthGrowth / subscriptionData[subscriptionData.length - 2].subscribers) * 100).toFixed(
        2,
    );

    const handleEditTier = async () => {
        if (!lastEditSubmit) return;
        const now = new Date();
        const lastEditDate = tierData.modifiedAt;
        const diff = now.getTime() - lastEditDate.getTime();
        const days = diff / (1000 * 60 * 60 * 24);
        if (days < 30) {
            toast.error("Bạn chỉ có thể thay đổi hạng sau 30 ngày!");
            return;
        }

        const [_, err] = await editDashiFanTier(seriesId, tierId, {
            amount: lastEditSubmit.price,
            name: lastEditSubmit.title,
            perks: lastEditSubmit.earlyChapterCount,
        });

        if (err) {
            toast.error("Không thể cập nhật hạng! Vui lòng thử lại.");
            return;
        }

        toast.success("Cập nhật hạng thành công!");
        setLastEditSubmit(null);
        tierData.modifiedAt = new Date();
    };

    useEffect(() => {
        async function work() {
            const [series, err2] = await getSeriesInfo(seriesId);
            if (err2) {
                toast.error("Không thể tải thông tin bộ truyện! Vui lòng tải lại trang.");
                return;
            }

            const [data, err] = await getDashiFanTiers(seriesId);

            if (err) {
                router.push(`author-studio/series/`);
                return;
            }

            const curTier = data.find((x) => x.id === tierId);
            if (!curTier) {
                router.push(`author-studio/series/${seriesId}/dashifan`);
                return;
            }

            setTierData({
                title: curTier.name,
                price: curTier.price.amount,
                earlyChapterCount: curTier.perks,
                modifiedAt: new Date(curTier.lastModified),
            });
        }

        work();
    }, [seriesId, router, tierId]);

    return (
        <Dialog>
            <DialogTrigger>
                <Button aria-label="Xem" className="flex gap-2 bg-neutral-600 text-white">
                    <Edit /> Thay đổi
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Chỉnh sửa chi tiết hạng</DialogTitle>
                    <DialogDescription>
                        Lưu ý: Bạn chỉ có thể thay đổi chi tiết hạng sau 30 ngày kể từ lần thay đổi cuối cùng!
                        <p className="mb-4">Lần thay đổi cuối cùng: {tierData.modifiedAt.toLocaleDateString()}</p>
                    </DialogDescription>
                </DialogHeader>
                <UpdateTierConfirmButton
                    seriesId={seriesId}
                    tierId={tierId}
                    isOpen={!!lastEditSubmit}
                    onConfirmed={handleEditTier}
                    onOpenChanged={() => setLastEditSubmit(null)}
                />

                <FanTierForm
                    onSubmit={(e) => setLastEditSubmit(e)}
                    isLoading={false}
                    onCancel={() => {}}
                    hideCancelButton={true}
                    defaultValue={tierData}
                    actionButton={"Thay đổi"}
                />
            </DialogContent>
        </Dialog>
    );
}
