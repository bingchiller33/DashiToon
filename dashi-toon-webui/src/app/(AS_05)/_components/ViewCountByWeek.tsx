"use client";

import { Eye, TrendingDown, TrendingUp } from "lucide-react";
import { PolarAngleAxis, PolarGrid, Radar, RadarChart } from "recharts";

import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import {
    ChartConfig,
    ChartContainer,
    ChartLegend,
    ChartLegendContent,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import { InsightRaw } from "./Insight";
import { ViewAnalyticsResp } from "@/utils/api/analytics";
import { Skeleton } from "@/components/ui/skeleton";

export interface ViewCountByWeekProps {
    data?: ViewAnalyticsResp;
    current: string;
    compare?: string;
}

export function ViewCountByWeek(props: ViewCountByWeekProps) {
    const { data, current, compare } = props;

    const chartConfig = {
        current: {
            label: current,
            color: "hsl(var(--chart-1))",
        },
        compare: {
            label: compare,
            color: "hsl(var(--chart-2))",
        },
    } satisfies ChartConfig;

    return (
        <Card className="flex-grow">
            <CardHeader>
                <CardTitle className="flex gap-2">
                    <Eye /> Lượt xem theo ngày trong tuần
                </CardTitle>
                <CardDescription>Đánh giá thời gian cao điểm trong tuần khán giả đọc truyện của bạn</CardDescription>
            </CardHeader>
            <CardContent className="pb-0">
                {!data ? (
                    <Skeleton className="h-[350px] w-full" />
                ) : (
                    <ChartContainer config={chartConfig} className="mx-auto aspect-square max-h-[350px]">
                        <RadarChart data={data.dayOfWeeks}>
                            <ChartTooltip cursor={false} content={<ChartTooltipContent indicator="line" />} />
                            <PolarAngleAxis dataKey="time" />
                            <PolarGrid />
                            <Radar
                                dataKey="compare"
                                fill="var(--color-compare)"
                                fillOpacity={0.2}
                                stroke="var(--color-compare)"
                                strokeDasharray={8}
                                strokeOpacity={1}
                                strokeWidth={2}
                            />
                            <Radar
                                dataKey="current"
                                fill="var(--color-current)"
                                fillOpacity={0.2}
                                stroke="var(--color-current)"
                                strokeOpacity={1}
                                strokeWidth={2}
                            />
                            <ChartLegend content={<ChartLegendContent />} />
                        </RadarChart>
                    </ChartContainer>
                )}
            </CardContent>
            <CardFooter className="grid grid-cols-2">
                <InsightRaw
                    icon={<TrendingUp className="h-6 w-6 text-green-400" />}
                    title="Ngày cao điểm"
                    current={data?.bestDayOfWeek}
                    isLoading={!data}
                    isGood
                />
                <InsightRaw
                    icon={<TrendingDown className="h-6 w-6 text-red-400" />}
                    title="Ngày thấp điểm"
                    isLoading={!data}
                    current={data?.worstDayOfWeek}
                    isGood
                />
            </CardFooter>
        </Card>
    );
}
