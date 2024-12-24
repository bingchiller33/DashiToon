"use client";

import * as React from "react";
import { Check, ChevronsUpDown } from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList,
} from "@/components/ui/command";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover";

export interface ComboBoxProps {
    value?: string;
    onChange: (value: string) => void;
    placeholder?: string;
    searchPlaceHolder?: string;
    data: { value: string; label: string }[];
    onSearch?: (value: React.FormEvent<HTMLInputElement>) => void;
}

export function ComboBox(props: ComboBoxProps) {
    const { value, onChange, data, placeholder, searchPlaceHolder, onSearch } =
        props;
    const [open, setOpen] = React.useState(false);

    return (
        <Popover open={open} onOpenChange={setOpen} modal={true}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={open}
                    className="w-full justify-between"
                >
                    {value
                        ? data.find((d) => d.value === value)?.label
                        : placeholder}
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[300px] p-0">
                <Command shouldFilter={false}>
                    <CommandInput
                        placeholder={searchPlaceHolder}
                        onInput={onSearch}
                    />
                    <CommandList>
                        <CommandEmpty>Not found.</CommandEmpty>
                        <CommandGroup>
                            {data.map((x) => (
                                <CommandItem
                                    key={x.value}
                                    value={x.value}
                                    onSelect={(currentValue) => {
                                        onChange(
                                            currentValue === value
                                                ? ""
                                                : currentValue,
                                        );
                                        setOpen(false);
                                    }}
                                >
                                    <Check
                                        className={cn(
                                            "mr-2 h-4 w-4",
                                            value === x.value
                                                ? "opacity-100"
                                                : "opacity-0",
                                        )}
                                    />
                                    {x.label}
                                </CommandItem>
                            ))}
                        </CommandGroup>
                    </CommandList>
                </Command>
            </PopoverContent>
        </Popover>
    );
}
