import { Skeleton } from "@/components/ui/skeleton";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import React from "react";

export interface ToggleListItem<Id> {
    id: Id;
}

export interface ListToggleProps<T extends ToggleListItem<Id>, Id = string> {
    items: T[];
    isLoading?: boolean;
    skeletonCount?: number;
    value?: string;
    labelKey?: keyof T;
    onChange?: (value: string) => void;
}

export default function ListToggle<T extends ToggleListItem<Id>, Id>(
    props: ListToggleProps<T, Id>,
) {
    const { items, isLoading, value, onChange, labelKey: lableKey } = props;

    if (isLoading) {
        return (
            <div className="flex flex-wrap justify-start gap-2">
                {[...Array(props.skeletonCount ?? 5)].map((_, index) => (
                    <Skeleton key={index} className="h-10 w-20 rounded-md" />
                ))}
            </div>
        );
    }

    return (
        <ToggleGroup
            type="single"
            className="flex-wrap justify-start"
            value={value}
            onValueChange={(e) => e && onChange?.(e)}
        >
            {items.map((item) => (
                <ToggleGroupItem
                    key={`${item.id}`}
                    value={`${item.id}`}
                    aria-label={`${item[lableKey ?? "id"]}`}
                >
                    {`${item[lableKey ?? "id"]}`}
                </ToggleGroupItem>
            ))}
        </ToggleGroup>
    );
}
