import { Skeleton } from "@/components/ui/skeleton";
import { ReactNode } from "react";
import StatNum from "./StatNum";

export interface InsightProps {
    isLoading?: boolean;
    icon: ReactNode;
    title: string;
    current: number;
    compare?: number;
    formatter?: (value: number) => string;
    prefix?: string;
}

export default function Insight(props: InsightProps) {
    const { current, compare, formatter } = props;

    const fmt = formatter ?? ((v) => v.toString());
    return (
        <InsightRaw
            {...props}
            isGood={compare ? compare > 0 : true}
            current={fmt(current)}
            compare={compare ? fmt(compare) : undefined}
        />
    );
}

export interface InsightRawProps {
    isLoading?: boolean;
    icon: ReactNode;
    title: string;
    current: ReactNode;
    compare?: ReactNode;
    isGood?: boolean;
    prefix?: string;
}

export function InsightRaw(pros: InsightRawProps) {
    const { isLoading, icon, title, current, compare, prefix, isGood } = pros;

    return (
        <div className="flex items-center space-x-4">
            <div className="flex-shrink-0">{icon}</div>
            <div>
                <p className="text-sm font-medium text-gray-400">{title}</p>
                <div className="text-2xl font-bold text-gray-100">
                    {isLoading ? (
                        <Skeleton className="h-8 w-full" />
                    ) : (
                        <>
                            {prefix}
                            <StatNum
                                current={current}
                                compare={compare ? compare : undefined}
                                isGood={!!isGood}
                            />
                        </>
                    )}
                </div>
            </div>
        </div>
    );
}
