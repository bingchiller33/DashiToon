"use client";

import {
    ArrowUpDown,
    ChartNoAxesColumn,
    CreditCard,
    DollarSign,
    Eye,
    Hash,
    TrendingUp,
} from "lucide-react";
import { Area, AreaChart, CartesianGrid, Pie, PieChart, XAxis } from "recharts";

import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    ChartConfig,
    ChartContainer,
    ChartLegend,
    ChartLegendContent,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import { Skeleton } from "@/components/ui/skeleton";
import { formatCurrency } from "@/utils";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import cx from "classnames";
import ChapterPagination from "@/app/(AS_02)/_components/ChapterPagination";
import { ReactNode } from "react";
import Link from "next/link";

const chartData = [
    { time: "January", current: 186, compare: 80 },
    { time: "February", current: 305, compare: 200 },
    { time: "March", current: 237, compare: 120 },
    { time: "April", current: 73, compare: 190 },
    { time: "May", current: 209, compare: 130 },
    { time: "June", current: 214, compare: 140 },
];

import React from "react";
import { ChartSeries } from "@/utils/api/analytics";

export interface OmniAreaSubChartProps {
    compare: string;
    current: string;
    data: ChartSeries[];
}

export default function OmniAreaSubChart(props: OmniAreaSubChartProps) {
    const { compare, current, data } = props;
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
        <ChartContainer
            className="aspect-[unset] h-[300px] w-full"
            config={chartConfig}
        >
            <AreaChart
                accessibilityLayer
                data={data}
                margin={{
                    left: 12,
                    right: 12,
                }}
            >
                <CartesianGrid vertical={false} />
                <XAxis
                    dataKey="time"
                    tickLine={false}
                    axisLine={false}
                    tickMargin={8}
                />
                <ChartTooltip
                    cursor={false}
                    content={<ChartTooltipContent />}
                />
                <defs>
                    <linearGradient
                        id="fillDesktop"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                    >
                        <stop
                            offset="5%"
                            stopColor="var(--color-current)"
                            stopOpacity={0.8}
                        />
                        <stop
                            offset="95%"
                            stopColor="var(--color-current)"
                            stopOpacity={0.1}
                        />
                    </linearGradient>
                    <linearGradient id="fillMobile" x1="0" y1="0" x2="0" y2="1">
                        <stop
                            offset="5%"
                            stopColor="var(--color-compare)"
                            stopOpacity={0.8}
                        />
                        <stop
                            offset="95%"
                            stopColor="var(--color-compare)"
                            stopOpacity={0.1}
                        />
                    </linearGradient>
                </defs>
                <Area
                    dataKey="compare"
                    type="natural"
                    fill="url(#fillMobile)"
                    strokeDasharray={4}
                    fillOpacity={0.4}
                    stroke="var(--color-compare)"
                    stackId="a"
                />
                <Area
                    dataKey="current"
                    type="natural"
                    fill="url(#fillDesktop)"
                    fillOpacity={0.4}
                    stroke="var(--color-current)"
                    stackId="a"
                />
                <ChartLegend content={<ChartLegendContent />} />
            </AreaChart>
        </ChartContainer>
    );
}
