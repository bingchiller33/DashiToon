"use client";
import cx from "classnames";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { ChartContainer, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart";
import { ChevronDown, Home } from "lucide-react";
import FanTierForm, { FormValues } from "../../_components/FanTierForm";
import {
    Breadcrumb,
    BreadcrumbEllipsis,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import CurrentSubscribers from "../../_components/TierSubscribers";
import SiteLayout from "@/components/SiteLayout";
import { editDashiFanTier, getDashiFanTiers } from "@/utils/api/dashifan";
import { toast } from "sonner";
import { useRouter } from "next/navigation";
import { getSeriesInfo } from "@/utils/api/author-studio/series";
import UpdateTierConfirmButton from "../../_components/UpdateTierConfirmButton";

interface SubscriptionHistory {
    startDate: string;
    endDate: string;
    tier: string;
}

interface Subscriber {
    id: string;
    name: string;
    joinDate: string;
    subscriptionHistory: SubscriptionHistory[];
}

interface TierData {
    title: string;
    price: number;
    earlyChapterCount: number;
    subscribers: Subscriber[];
    modifiedAt: Date;
}

const initialTierData: TierData = {
    title: "Gold",
    price: 9.99,
    earlyChapterCount: 5,
    modifiedAt: new Date(),
    subscribers: [
        {
            id: "1",
            name: "John Doe",
            joinDate: "2023-01-15",
            subscriptionHistory: [
                {
                    startDate: "2023-01-15",
                    endDate: "2023-03-14",
                    tier: "Silver",
                },
                { startDate: "2023-03-15", endDate: "Present", tier: "Gold" },
            ],
        },
        {
            id: "2",
            name: "Jane Smith",
            joinDate: "2023-02-20",
            subscriptionHistory: [{ startDate: "2023-02-20", endDate: "Present", tier: "Gold" }],
        },
        {
            id: "3",
            name: "Bob Johnson",
            joinDate: "2023-03-10",
            subscriptionHistory: [
                {
                    startDate: "2023-03-10",
                    endDate: "2023-04-09",
                    tier: "Bronze",
                },
                { startDate: "2023-04-10", endDate: "Present", tier: "Gold" },
            ],
        },
    ],
};

const subscriptionData = [
    { month: "Jan", subscribers: 100 },
    { month: "Feb", subscribers: 120 },
    { month: "Mar", subscribers: 150 },
    { month: "Apr", subscribers: 180 },
    { month: "May", subscribers: 200 },
];

export interface TierManagementProps {
    seriesId: string;
    tierId: string;
}

export default function TierManagement(props: TierManagementProps) {
    const { seriesId, tierId } = props;
    const [tierData, setTierData] = useState<TierData>(initialTierData);
    const [seriesName, setSeriesName] = useState("Đang tải...");
    const router = useRouter();
    const [lastEditSubmit, setLastEditSubmit] = useState<FormValues | null>(null);

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

    const currentSubscribers = subscriptionData.map((x) => x.subscribers).reduce((a, b) => a + b, 0);

    const currentMonthGrowth =
        subscriptionData[subscriptionData.length - 1].subscribers -
        subscriptionData[subscriptionData.length - 2].subscribers;

    const growthRate = ((currentMonthGrowth / subscriptionData[subscriptionData.length - 2].subscribers) * 100).toFixed(
        2,
    );

    useEffect(() => {
        async function work() {
            const [series, err2] = await getSeriesInfo(seriesId);
            if (err2) {
                toast.error("Không thể tải thông tin bộ truyện! Vui lòng tải lại trang.");
                return;
            }

            setSeriesName(series.title);

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
                subscribers: [],
                modifiedAt: new Date(curTier.lastModified),
            });
        }

        work();
    }, [seriesId, router, tierId]);

    return (
        <SiteLayout>
            <UpdateTierConfirmButton
                seriesId={seriesId}
                tierId={tierId}
                isOpen={!!lastEditSubmit}
                onConfirmed={handleEditTier}
                onOpenChanged={() => setLastEditSubmit(null)}
            />
            <div className="container mx-auto p-2 text-neutral-50 md:p-4">
                <Breadcrumb className="mb-4">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/" className="flex items-center">
                                <Home className="mr-2 h-4 w-4" />
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio">Xưởng Truyện</BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio/series">Bộ truyện</BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href={`/author-studio/series/${seriesId}`}>
                                {seriesName ?? "Đang tải"}
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href={`/author-studio/series/${seriesId}/dashifan`}>
                                Quản lý hạng DashiFan
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Hạng: {tierData.title}</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <h1 className="mb-6 mt-6 text-3xl">Quản lý hạng: {tierData.title}</h1>

                <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
                    <div className="lg:col-span-1">
                        <Card className="border-neutral-700 bg-neutral-800">
                            <CardHeader>
                                <CardTitle className="text-blue-300">Chỉnh sửa chi tiết hạng</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="mb-4 text-muted-foreground">
                                    Lưu ý: Bạn chỉ có thể thay đổi chi tiết hạng sau 30 ngày kể từ lần thay đổi cuối
                                    cùng!
                                </p>

                                <p className="mb-4">
                                    Lần thay đổi cuối cùng: {tierData.modifiedAt.toLocaleDateString()}
                                </p>

                                <FanTierForm
                                    onSubmit={(e) => setLastEditSubmit(e)}
                                    isLoading={false}
                                    onCancel={() => {}}
                                    hideCancelButton={true}
                                    defaultValue={tierData}
                                    actionButton={"Thay đổi"}
                                />
                            </CardContent>
                        </Card>
                    </div>

                    <div className="space-y-6 lg:col-span-3">
                        <Card className="border-neutral-700 bg-neutral-800">
                            <CardHeader>
                                <CardTitle className="text-blue-300">Tăng trưởng người đăng ký</CardTitle>
                            </CardHeader>
                            <CardContent className="flex flex-col gap-4 md:flex-row">
                                <div className="h-[300px] w-full md:w-3/4">
                                    <ChartContainer
                                        config={{
                                            subscribers: {
                                                label: "Người đăng ký",
                                                color: "hsl(var(--chart-1))",
                                            },
                                        }}
                                        className="h-full"
                                    >
                                        <ResponsiveContainer width="100%" height="100%">
                                            <LineChart data={subscriptionData} className="w-full">
                                                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                                <XAxis dataKey="month" stroke="#9CA3AF" />
                                                <YAxis stroke="#9CA3AF" />
                                                <ChartTooltip content={<ChartTooltipContent />} />
                                                <Legend />
                                                <Line
                                                    type="monotone"
                                                    dataKey="subscribers"
                                                    stroke="var(--color-subscribers)"
                                                    strokeWidth={2}
                                                />
                                            </LineChart>
                                        </ResponsiveContainer>
                                    </ChartContainer>
                                </div>
                                <div className="mt-4 flex w-full flex-col justify-center space-y-4 md:ml-4 md:mt-0 md:w-1/4">
                                    <div className="rounded-lg bg-neutral-700 p-4">
                                        <h3 className="text-lg font-semibold text-blue-300">Người đăng ký hiện tại</h3>
                                        <p className="text-2xl font-bold">
                                            {currentSubscribers}{" "}
                                            <span
                                                className={cx(
                                                    "text-sm",
                                                    currentMonthGrowth > 0 ? "text-green-500" : "text-red-500",
                                                )}
                                            >
                                                {currentMonthGrowth > 0 ? "+" : ""}
                                                {currentMonthGrowth}
                                            </span>
                                        </p>
                                    </div>
                                    <div className="rounded-lg bg-neutral-700 p-4">
                                        <h3 className="text-lg font-semibold text-blue-300">Tỷ lệ tăng trưởng</h3>
                                        <p className="text-2xl font-bold">{growthRate}%</p>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                        <CurrentSubscribers />
                    </div>
                </div>
            </div>
        </SiteLayout>
    );
}
