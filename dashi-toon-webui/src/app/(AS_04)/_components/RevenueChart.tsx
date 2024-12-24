"use client";
import { Area, AreaChart, CartesianGrid, XAxis, YAxis } from "recharts";

import {
    ChartConfig,
    ChartContainer,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import React from "react";
const chartData = [
    {
        month: "Th12 23",
        revenue: 100.0,
        withdrawal: 0.0,
    },
    {
        month: "Th1 24",
        revenue: 200.0,
        withdrawal: 10.0,
    },
    {
        month: "Th2 24",
        revenue: 150.0,
        withdrawal: 20.0,
    },
    {
        month: "Th3 24",
        revenue: 180.0,
        withdrawal: 0.0,
    },
    {
        month: "Th4 24",
        revenue: 220.0,
        withdrawal: 30.0,
    },
    {
        month: "Th5 24",
        revenue: 250.0,
        withdrawal: 0.0,
    },
    {
        month: "Th6 24",
        revenue: 280.0,
        withdrawal: 40.0,
    },
    {
        month: "Th7 24",
        revenue: 300.0,
        withdrawal: 0.0,
    },
    {
        month: "Th8 24",
        revenue: 320.0,
        withdrawal: 50.0,
    },
    {
        month: "Th9 24",
        revenue: 350.0,
        withdrawal: 0.0,
    },
    {
        month: "Th10 24",
        revenue: 380.0,
        withdrawal: 60.0,
    },
    {
        month: "Th11 24",
        revenue: 400.0,
        withdrawal: 0.0,
    },
];
const chartConfig = {
    revenue: {
        label: "Thu nhập",
        color: "hsl(var(--chart-2))",
    },
    withdrawal: {
        label: "Rút tiền",
        color: "hsl(0deg 90.6% 70.78%)",
    },
} satisfies ChartConfig;

export interface RevenueChartProps {
    data: {
        month: string;
        revenue: number;
        withdrawal: number;
    }[];
}

export function RevenueChart(props: RevenueChartProps) {
    return (
        <ChartContainer
            config={chartConfig}
            className="aspect-[unset] h-[250px]"
        >
            <AreaChart accessibilityLayer data={props.data}>
                <CartesianGrid vertical={false} />
                <XAxis
                    dataKey="month"
                    tickLine={false}
                    axisLine={false}
                    tickMargin={8}
                />
                <YAxis
                    tickLine={false}
                    axisLine={false}
                    tickFormatter={(value) => `${value / 1000}k`}
                />
                <ChartTooltip
                    cursor={false}
                    content={<ChartTooltipContent indicator="dot" />}
                />
                <defs>
                    <linearGradient
                        id="fillRevenue"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                    >
                        <stop
                            offset="5%"
                            stopColor="var(--color-revenue)"
                            stopOpacity={0.8}
                        />
                        <stop
                            offset="95%"
                            stopColor="var(--color-revenue)"
                            stopOpacity={0.1}
                        />
                    </linearGradient>
                    <linearGradient
                        id="fillWithdrawal"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                    >
                        <stop
                            offset="5%"
                            stopColor="var(--color-withdrawal)"
                            stopOpacity={0.8}
                        />
                        <stop
                            offset="95%"
                            stopColor="var(--color-withdrawal)"
                            stopOpacity={0.1}
                        />
                    </linearGradient>
                </defs>
                <Area
                    dataKey="withdrawal"
                    type="natural"
                    fill="url(#fillWithdrawal)"
                    fillOpacity={0.4}
                    stroke="var(--color-withdrawal)"
                />
                <Area
                    dataKey="revenue"
                    type="natural"
                    fill="url(#fillRevenue)"
                    fillOpacity={0.4}
                    stroke="var(--color-revenue)"
                />
            </AreaChart>
        </ChartContainer>
    );
}
