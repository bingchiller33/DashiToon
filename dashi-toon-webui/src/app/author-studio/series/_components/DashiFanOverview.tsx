"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ChartContainer, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { ArrowRight, TrendingUp, TrendingDown } from "lucide-react";
import Link from "next/link";

const dashifanData = [
    { month: "Jan", totalRevenue: 2_247_000, totalSubscribers: 225 },
    { month: "Feb", totalRevenue: 2_697_000, totalSubscribers: 260 },
    { month: "Mar", totalRevenue: 3_246_000, totalSubscribers: 310 },
    { month: "Apr", totalRevenue: 3_895_000, totalSubscribers: 355 },
    { month: "May", totalRevenue: 4_495_000, totalSubscribers: 420 },
];

export default function DashiFanOverviewSlim(props: { seriesId: string }) {
    const { seriesId } = props;
    const lastMonth = dashifanData[dashifanData.length - 1];
    const previousMonth = dashifanData[dashifanData.length - 2];

    const calculateChange = (current: number, previous: number) => {
        const change = ((current - previous) / previous) * 100;
        return change.toFixed(1);
    };

    const revenueChange = calculateChange(lastMonth.totalRevenue, previousMonth.totalRevenue);
    const subscribersChange = calculateChange(lastMonth.totalSubscribers, previousMonth.totalSubscribers);

    return (
        <Card className="border-neutral-700 bg-neutral-800">
            <CardHeader className="pb-2">
                <div className="flex items-center justify-between">
                    <CardTitle className="text-lg font-bold text-blue-300">DashiFan</CardTitle>
                    <Link
                        href={`/author-studio/series/${seriesId}/dashifan`}
                        className="flex items-center text-sm text-sky-400 hover:text-blue-300"
                    >
                        Xem chi tiết
                        <ArrowRight className="ml-1 h-4 w-4" />
                    </Link>
                </div>
            </CardHeader>
            <CardContent className="space-y-4">
                <ChartContainer
                    config={{
                        totalRevenue: {
                            label: "Revenue",
                            color: "hsl(var(--chart-1))",
                        },
                        totalSubscribers: {
                            label: "Subscribers",
                            color: "hsl(var(--chart-2))",
                        },
                    }}
                    className="h-[160px]"
                >
                    <ResponsiveContainer width="100%" height="100%">
                        <LineChart data={dashifanData}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                            <XAxis dataKey="month" stroke="#9CA3AF" tick={{ fontSize: 10 }} />
                            <YAxis yAxisId="left" stroke="#9CA3AF" tick={{ fontSize: 10 }} />
                            <YAxis yAxisId="right" orientation="right" stroke="#9CA3AF" tick={{ fontSize: 10 }} />
                            <ChartTooltip content={<ChartTooltipContent />} />
                            <Legend iconSize={8} wrapperStyle={{ fontSize: "10px" }} />
                            <Line
                                yAxisId="left"
                                type="monotone"
                                dataKey="totalRevenue"
                                name="Revenue"
                                stroke="var(--color-totalRevenue)"
                                strokeWidth={2}
                                dot={false}
                            />
                            <Line
                                yAxisId="right"
                                type="monotone"
                                dataKey="totalSubscribers"
                                name="Subscribers"
                                stroke="var(--color-totalSubscribers)"
                                strokeWidth={2}
                                dot={false}
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </ChartContainer>
                <div className="grid grid-cols-2 gap-4 text-sm">
                    <StatCard
                        title="Revenue"
                        value={`${lastMonth.totalRevenue.toLocaleString("vi-VN", {})}đ`}
                        change={revenueChange}
                    />
                    <StatCard title="Subscribers" value={lastMonth.totalSubscribers} change={subscribersChange} />
                </div>
            </CardContent>
        </Card>
    );
}

function StatCard({ title, value, change }: { title: string; value: string | number; change: string }) {
    const isPositive = parseFloat(change) >= 0;
    return (
        <div className="rounded-lg border border-neutral-600 bg-neutral-700 p-2">
            <h3 className="text-xs font-medium text-neutral-300">{title}</h3>
            <p className="text-lg font-bold text-blue-300">{value}</p>
            <p className={`flex items-center text-xs ${isPositive ? "text-green-400" : "text-red-400"}`}>
                {isPositive ? <TrendingUp className="mr-1 h-3 w-3" /> : <TrendingDown className="mr-1 h-3 w-3" />}
                {isPositive ? "+" : ""}
                {change}%
            </p>
        </div>
    );
}
