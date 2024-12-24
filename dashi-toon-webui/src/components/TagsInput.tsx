import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input, InputProps } from "@/components/ui/input";
import { Delete, Plus, PlusIcon } from "lucide-react";
import { Dispatch, SetStateAction, forwardRef, useState } from "react";

type InputTagsProps = InputProps & {
    value: string[];
    onChange: Dispatch<SetStateAction<string[]>>;
};

export default function TagsInput({
    value,
    onChange,
    ...props
}: InputTagsProps) {
    const [pendingDataPoint, setPendingDataPoint] = useState("");

    const addPendingDataPoint = () => {
        if (pendingDataPoint) {
            const newDataPoints = new Set([...value, pendingDataPoint]);
            onChange(Array.from(newDataPoints));
            setPendingDataPoint("");
        }
    };

    return (
        <>
            <div className="flex">
                <Input
                    value={pendingDataPoint}
                    onChange={(e) => setPendingDataPoint(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === "Enter") {
                            e.preventDefault();
                            addPendingDataPoint();
                        }
                    }}
                    className="rounded-r-none"
                    {...props}
                />
                <Button
                    type="button"
                    variant="secondary"
                    className="rounded-l-none border border-l-0"
                    onClick={addPendingDataPoint}
                >
                    <Plus className="h-4 w-4" />
                </Button>
            </div>
            {value.length > 0 && (
                <div className="flex min-h-[2.5rem] flex-wrap items-center gap-2 overflow-y-auto rounded-md py-2">
                    {value.map((item, idx) => (
                        <Badge key={idx} variant="secondary">
                            {item}
                            <button
                                type="button"
                                className="ml-2 w-3"
                                onClick={() => {
                                    onChange(value.filter((i) => i !== item));
                                }}
                            >
                                <Delete className="w-3" />
                            </button>
                        </Badge>
                    ))}
                </div>
            )}
        </>
    );
}
