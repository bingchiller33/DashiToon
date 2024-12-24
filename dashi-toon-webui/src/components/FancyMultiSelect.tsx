import * as React from "react";
import { X } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Command, CommandGroup, CommandItem, CommandList } from "@/components/ui/command";
import { Command as CommandPrimitive } from "cmdk";

type SelectableItem = {
    value: number;
    label: string;
};

interface FancyMultiSelectProps {
    items: SelectableItem[];
    onSelectionChange: (selectedItems: SelectableItem[]) => void; // Callback to pass selected items
    selectedItems: SelectableItem[];
}

export function FancyMultiSelect({ items, onSelectionChange, selectedItems }: FancyMultiSelectProps) {
    const inputRef = React.useRef<HTMLInputElement>(null);
    const [open, setOpen] = React.useState(false);
    const [inputValue, setInputValue] = React.useState("");

    const handleUnselect = (framework: SelectableItem) => {
        const newSelected = selectedItems.filter((s) => s.value !== framework.value);
        onSelectionChange(newSelected);
    };
    const handleKeyDown = React.useCallback(
        (e: React.KeyboardEvent<HTMLDivElement>) => {
            const input = inputRef.current;
            if (input) {
                if (e.key === "Delete" || e.key === "Backspace") {
                    if (input.value === "") {
                        const updatedSelected = selectedItems.slice(0, -1);
                        onSelectionChange(updatedSelected);
                    }
                }
                if (e.key === "Escape") {
                    input.blur();
                }
            }
        },
        [onSelectionChange, selectedItems],
    );

    const selectables = items.filter(
        (framework) =>
            !selectedItems.some((s) => s.value === framework.value) &&
            framework.label.toLowerCase().includes(inputValue.toLowerCase()),
    );

    return (
        <Command onKeyDown={handleKeyDown} className="overflow-visible bg-transparent">
            <div className="group rounded-md border border-input px-3 py-2 text-sm ring-offset-background focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2">
                <div className="flex flex-wrap gap-1">
                    {selectedItems.map((framework) => (
                        <Badge key={framework.value} variant="secondary">
                            {framework.label}
                            <button
                                className="ml-1 rounded-full outline-none ring-offset-background focus:ring-2 focus:ring-ring focus:ring-offset-2"
                                onMouseDown={(e) => {
                                    e.preventDefault();
                                    e.stopPropagation();
                                }}
                                onClick={() => handleUnselect(framework)}
                            >
                                <X className="h-3 w-3 text-muted-foreground hover:text-foreground" />
                            </button>
                        </Badge>
                    ))}
                    <input
                        ref={inputRef}
                        value={inputValue}
                        onChange={(e) => setInputValue(e.target.value)}
                        onBlur={() => setOpen(false)}
                        onFocus={() => setOpen(true)}
                        placeholder="Select Genres..."
                        className="ml-2 flex-1 bg-transparent outline-none placeholder:text-muted-foreground"
                    />
                </div>
            </div>
            <div className="relative mt-2">
                {open && selectables.length > 0 && (
                    <div className="absolute top-0 z-10 w-full rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in">
                        <CommandList>
                            <CommandGroup className="h-full overflow-auto">
                                {selectables.map((framework) => (
                                    <CommandItem
                                        key={framework.value}
                                        onMouseDown={(e) => {
                                            e.preventDefault();
                                            e.stopPropagation();
                                        }}
                                        onSelect={() => {
                                            setInputValue(""); // Clear input value
                                            onSelectionChange([...selectedItems, framework]); // Notify parent of selection
                                        }}
                                        className="cursor-pointer"
                                    >
                                        {framework.label}
                                    </CommandItem>
                                ))}
                            </CommandGroup>
                        </CommandList>
                    </div>
                )}
            </div>
        </Command>
    );
}
