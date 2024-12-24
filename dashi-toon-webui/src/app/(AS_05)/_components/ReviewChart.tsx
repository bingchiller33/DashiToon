import { Bar, BarChart, CartesianGrid, XAxis } from "recharts";

import {
    ChartConfig,
    ChartContainer,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import { ReviewSeries } from "@/utils/api/analytics";

export interface ReviewChartProps {
    current: string;
    compare: string;
    data: ReviewSeries[];
}

export function ReviewChart(props: ReviewChartProps) {
    const { current, compare, data } = props;
    const chartConfig = {
        currentPositive: {
            label: `${current} tích cực`,
            color: "#15803d",
        },
        currentNegative: {
            label: `${current} tiêu cực`,
            color: "#be123c",
        },
        comparePositive: {
            label: `${compare} tích cực`,
            color: "#15803d70",
        },
        compareNegative: {
            label: `${compare} tiêu cực`,
            color: "#be123c70",
        },
    } satisfies ChartConfig;

    return (
        <ChartContainer
            config={chartConfig}
            className="aspect-[unset] h-[300px] w-full"
        >
            <BarChart accessibilityLayer data={data}>
                <CartesianGrid vertical={false} />
                <XAxis
                    dataKey="time"
                    tickLine={false}
                    tickMargin={10}
                    axisLine={false}
                    tickFormatter={(value) => value.slice(0, 3)}
                />
                <ChartTooltip
                    cursor={false}
                    content={<ChartTooltipContent indicator="dashed" />}
                />
                <Bar
                    dataKey="compareNegative"
                    fill="var(--color-compareNegative)"
                    stackId="compare"
                />

                <Bar
                    dataKey="comparePositive"
                    fill="var(--color-comparePositive)"
                    radius={[4, 4, 0, 0]}
                    stackId="compare"
                />

                <Bar
                    dataKey="currentNegative"
                    fill="var(--color-currentNegative)"
                    stackId="current"
                />

                <Bar
                    dataKey="currentPositive"
                    fill="var(--color-currentPositive)"
                    radius={[4, 4, 0, 0]}
                    stackId="current"
                />
            </BarChart>
        </ChartContainer>
    );
}
