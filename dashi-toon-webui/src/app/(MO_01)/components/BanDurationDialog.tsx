"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface BanDurationDialogProps {
    onConfirm: (duration: number) => void;
    durations?: number[]; // durations in days
    triggerText?: string;
    title?: string;
    description?: string;
}

export function BanDurationDialog({
    onConfirm,
    durations = [1, 3, 7, 14, 30, 90, 365],
    triggerText = "Resolve",
    title = "Select Ban Duration",
    description = "Choose a duration or enter a custom number of days to ban the user.",
}: BanDurationDialogProps) {
    const [selectedDuration, setSelectedDuration] = useState<string>("");
    const [customDuration, setCustomDuration] = useState<string>("");
    const [open, setOpen] = useState(false);

    const handleConfirm = () => {
        const duration = selectedDuration === "custom" ? parseInt(customDuration, 10) : parseInt(selectedDuration, 10);

        if (!isNaN(duration) && duration > 0) {
            onConfirm(duration);
            setOpen(false);
            setSelectedDuration("");
            setCustomDuration("");
        }
    };

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
                <Button variant="destructive" className="text-white">
                    {triggerText}
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>{title}</DialogTitle>
                    <DialogDescription>{description}</DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                    <div className="grid grid-cols-4 items-center gap-4">
                        <Label htmlFor="duration" className="text-right">
                            Thời gian
                        </Label>
                        <Select value={selectedDuration} onValueChange={setSelectedDuration}>
                            <SelectTrigger className="col-span-3">
                                <SelectValue placeholder="Chọn thời gian" />
                            </SelectTrigger>
                            <SelectContent>
                                {durations.map((duration) => (
                                    <SelectItem key={duration} value={duration.toString()}>
                                        {duration} {duration === 1 ? "ngày" : "ngày"}
                                    </SelectItem>
                                ))}
                                <SelectItem value="custom">Tùy chỉnh</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    {selectedDuration === "custom" && (
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="customDuration" className="text-right">
                                Số ngày tùy chỉnh
                            </Label>
                            <Input
                                id="customDuration"
                                type="number"
                                value={customDuration}
                                onChange={(e) => setCustomDuration(e.target.value)}
                                className="col-span-3"
                                min="1"
                            />
                        </div>
                    )}
                </div>
                <DialogFooter>
                    <Button type="submit" onClick={handleConfirm}>
                        Xác nhận cấm
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
