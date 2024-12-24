import { ChartConfig, ChartContainer, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart";
import { ChapterRanking } from "@/utils/api/analytics";
import React from "react";
import { Pie, PieChart } from "recharts";

export interface OmniPieSubChartProps {
    compare: string;
    current: string;
    data: ChapterRanking[];
}

export default function OmniPieSubChart(props: OmniPieSubChartProps) {
    const { compare, current, data } = props;
    const currentData = data.map((d) => ({
        chapter: d.id,
        current: d.data,
        fill: `var(--color-${d.id})`,
    }));

    const compareData = data.map((d) => ({
        chapter: d.id,
        compare: d.data + (d?.compare ?? 0),
        fill: `var(--color-${d.id})`,
    }));

    const chartConfig: ChartConfig = {
        visitors: {
            label: "Visitors",
        },
        current: {
            label: current,
        },
        compare: {
            label: compare,
        },

        may: {
            label: "May",
            color: "hsl(var(--chart-5))",
        },
    };

    let i = 0;
    for (const item of data) {
        chartConfig[item.id] = {
            label: item.name,
            color: `hsl(var(--chart-${++i % 5}))`,
        };
    }

    const showCompare = compareData.some((d) => d.compare > 0);

    return (
        <ChartContainer config={chartConfig} className="mx-auto mb-4 h-[300px] w-[300px] md:mb-0">
            <PieChart>
                <ChartTooltip
                    content={
                        <ChartTooltipContent
                            labelKey="visitors"
                            nameKey="chapter"
                            indicator="line"
                            style={{ gap: 200, backgroundColor: "red" }}
                            labelFormatter={(_, payload) => {
                                return chartConfig[payload?.[0].dataKey as keyof typeof chartConfig]?.label;
                            }}
                        />
                    }
                />
                <Pie data={currentData} dataKey="current" outerRadius={110} />
                {showCompare && (
                    <Pie data={compareData} dataKey="compare" innerRadius={120} outerRadius={150} fillOpacity={0.4} />
                )}
            </PieChart>
        </ChartContainer>
    );
}
