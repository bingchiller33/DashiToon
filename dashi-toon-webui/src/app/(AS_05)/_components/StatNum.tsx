import { ReactNode } from "react";
import cx from "classnames";
import { Skeleton } from "@/components/ui/skeleton";

export interface StatNumProps {
    current: ReactNode;
    compare?: ReactNode;
    isGood: boolean;
    isLoading?: boolean;
}

export default function StatNum({ current, compare, isGood, isLoading }: StatNumProps) {
    if (isLoading) {
        return <Skeleton className="h-9 w-32" />;
    }

    if (compare) {
        return (
            <span>
                {current}{" "}
                <span
                    className={cx("text-sm", {
                        "text-green-400": isGood,
                        "text-red-400": !isGood,
                    })}
                >
                    ({isGood ? "+" : ""}
                    {compare})
                </span>
            </span>
        );
    }

    return <span>{current}</span>;
}
