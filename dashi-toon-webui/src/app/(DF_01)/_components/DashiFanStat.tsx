"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
    ChartContainer,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer,
} from "recharts";

const tierData = [
    {
        month: "Jan",
        BronzeSubscribers: 100,
        SilverSubscribers: 75,
        GoldSubscribers: 50,
        BronzeRevenue: 499,
        SilverRevenue: 749.25,
        GoldRevenue: 999,
    },
    {
        month: "Feb",
        BronzeSubscribers: 120,
        SilverSubscribers: 90,
        GoldSubscribers: 60,
        BronzeRevenue: 598.8,
        SilverRevenue: 899.1,
        GoldRevenue: 1198.8,
    },
    {
        month: "Mar",
        BronzeSubscribers: 150,
        SilverSubscribers: 100,
        GoldSubscribers: 75,
        BronzeRevenue: 748.5,
        SilverRevenue: 999,
        GoldRevenue: 1498.5,
    },
    {
        month: "Apr",
        BronzeSubscribers: 180,
        SilverSubscribers: 120,
        GoldSubscribers: 90,
        BronzeRevenue: 898.2,
        SilverRevenue: 1198.8,
        GoldRevenue: 1798.2,
    },
    {
        month: "May",
        BronzeSubscribers: 200,
        SilverSubscribers: 150,
        GoldSubscribers: 100,
        BronzeRevenue: 998,
        SilverRevenue: 1498.5,
        GoldRevenue: 1998,
    },
];

const tiers = ["Bronze", "Silver", "Gold"];
const colors = [
    "hsl(var(--chart-1))",
    "hsl(var(--chart-2))",
    "hsl(var(--chart-3))",
];

export default function TierStatistics() {
    const calculateStatistics = () => {
        const lastMonth = tierData[tierData.length - 1];
        const totalSubscribers = tiers.reduce(
            (sum, tier) => sum + (lastMonth as any)[`${tier}Subscribers`],
            0,
        );
        const totalRevenue = tiers.reduce(
            (sum, tier) => sum + (lastMonth as any)[`${tier}Revenue`],
            0,
        );
        const averageRevenuePerSubscriber = totalRevenue / totalSubscribers;

        return {
            totalSubscribers,
            totalRevenue: totalRevenue.toLocaleString("vi-VN", {
                maximumFractionDigits: 2,
            }),
            averageRevenuePerSubscriber:
                averageRevenuePerSubscriber.toLocaleString("vi-VN", {
                    maximumFractionDigits: 2,
                }),
        };
    };

    const stats = calculateStatistics();

    return (
        <div className="space-y-6">
            <Card className="border-neutral-700 bg-neutral-800">
                <CardHeader>
                    <CardTitle className="text-blue-300">
                        Số lượng người đăng ký theo hạng
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex flex-col gap-4 lg:flex-row">
                        <div className="mb-4 w-full lg:mb-0 lg:w-3/4 lg:pr-4">
                            <ChartContainer
                                config={{
                                    Bronze: {
                                        label: "Đồng",
                                        color: colors[0],
                                    },
                                    Silver: {
                                        label: "Bạc",
                                        color: colors[1],
                                    },
                                    Gold: { label: "Vàng", color: colors[2] },
                                }}
                                className="h-[300px]"
                            >
                                <ResponsiveContainer width="100%" height="100%">
                                    <LineChart data={tierData}>
                                        <CartesianGrid
                                            strokeDasharray="3 3"
                                            stroke="#374151"
                                        />
                                        <XAxis
                                            dataKey="month"
                                            stroke="#9CA3AF"
                                        />
                                        <YAxis stroke="#9CA3AF" />
                                        <ChartTooltip
                                            content={<ChartTooltipContent />}
                                        />
                                        <Legend />
                                        {tiers.map((tier, index) => (
                                            <Line
                                                key={`${tier}Subscribers`}
                                                type="monotone"
                                                dataKey={`${tier}Subscribers`}
                                                name={`${tier}`}
                                                stroke={colors[index]}
                                                strokeWidth={2}
                                                dot={{ r: 4 }}
                                                activeDot={{ r: 6 }}
                                            />
                                        ))}
                                    </LineChart>
                                </ResponsiveContainer>
                            </ChartContainer>
                        </div>
                        <div className="w-full space-y-4 lg:w-1/4">
                            <StatCard
                                title="Tổng số người đăng ký"
                                value={stats.totalSubscribers}
                            />
                            <StatCard title="Hạng phổ biến nhất" value="Đồng" />
                            <StatCard
                                title="Tăng trưởng số người đăng ký"
                                value="+15% so với tháng trước"
                            />
                        </div>
                    </div>
                </CardContent>
            </Card>

            <Card className="border-neutral-700 bg-neutral-800">
                <CardHeader>
                    <CardTitle className="text-blue-300">
                        Doanh thu theo hạng
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex flex-col gap-4 lg:flex-row">
                        <div className="mb-4 w-full lg:mb-0 lg:w-3/4 lg:pr-4">
                            <ChartContainer
                                config={{
                                    Bronze: {
                                        label: "Đồng",
                                        color: colors[0],
                                    },
                                    Silver: {
                                        label: "Bạc",
                                        color: colors[1],
                                    },
                                    Gold: { label: "Vàng", color: colors[2] },
                                }}
                                className="h-[300px]"
                            >
                                <ResponsiveContainer width="100%" height="100%">
                                    <LineChart data={tierData}>
                                        <CartesianGrid
                                            strokeDasharray="3 3"
                                            stroke="#374151"
                                        />
                                        <XAxis
                                            dataKey="month"
                                            stroke="#9CA3AF"
                                        />
                                        <YAxis stroke="#9CA3AF" />
                                        <ChartTooltip
                                            content={<ChartTooltipContent />}
                                        />
                                        <Legend />
                                        {tiers.map((tier, index) => (
                                            <Line
                                                key={`${tier}Revenue`}
                                                type="monotone"
                                                dataKey={`${tier}Revenue`}
                                                name={`${tier}`}
                                                stroke={colors[index]}
                                                strokeWidth={2}
                                                dot={{ r: 4 }}
                                                activeDot={{ r: 6 }}
                                            />
                                        ))}
                                    </LineChart>
                                </ResponsiveContainer>
                            </ChartContainer>
                        </div>
                        <div className="w-full space-y-4 lg:w-1/4">
                            <StatCard
                                title="Tổng doanh thu"
                                value={`${stats.totalRevenue}đ`}
                            />
                            <StatCard
                                title="Doanh thu trung bình trên mỗi người đăng ký"
                                value={`${stats.averageRevenuePerSubscriber}đ`}
                            />
                            <StatCard
                                title="Tăng trưởng doanh thu"
                                value="+20% so với tháng trước"
                            />
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

function StatCard({ title, value }: { title: string; value: string | number }) {
    return (
        <div className="rounded-lg border border-neutral-600 bg-neutral-700 p-4">
            <h3 className="mb-1 text-sm font-medium text-blue-300">{title}</h3>
            <p className="text-2xl font-bold text-neutral-300">{value}</p>
        </div>
    );
}
