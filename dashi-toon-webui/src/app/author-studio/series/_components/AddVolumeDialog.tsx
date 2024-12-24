import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import * as env from "@/utils/env";
import { toast } from "sonner";

const formSchema = z.object({
    name: z
        .string()
        .min(1, "Tên tập không được để trống")
        .max(100, "Tên tập không được vượt quá 100 ký tự"),
    introduction: z
        .string()
        .max(2000, "Giới thiệu không được vượt quá 2000 ký tự")
        .optional(),
});

type FormValues = z.infer<typeof formSchema>;

interface Volume {
    volumeId: number;
    volumeNumber: number;
    name: string;
    introduction: string;
    chapterCount: number;
}

interface AddVolumeDialogProps {
    isOpen: boolean;
    onClose: () => void;
    seriesId: string;
    onVolumeAdded: (newVolume: Volume) => void;
    onSuccess?: () => Promise<void>;
}

export default function AddVolumeDialog({
    isOpen,
    onClose,
    seriesId,
    onVolumeAdded,
    onSuccess,
}: AddVolumeDialogProps) {
    const [isCreating, setIsCreating] = useState(false);

    const form = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            name: "",
            introduction: "",
        },
    });

    const onSubmit = async (values: FormValues) => {
        setIsCreating(true);
        try {
            const response = await fetch(
                `${env.getBackendHost()}/api/AuthorStudio/series/${seriesId}/volumes`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        seriesId,
                        name: values.name,
                        introduction: values.introduction || "",
                    }),
                    credentials: "include",
                },
            );

            if (!response.ok) {
                throw new Error("Failed to create volume");
            }

            const newVolume: Volume = await response.json();

            toast.success("Volume created successfully!");

            onVolumeAdded(newVolume);

            if (onSuccess) {
                await onSuccess();
            }

            onClose();
            form.reset();
        } catch (error) {
            console.error("Error creating volume:", error);
            toast.error("Error creating volume!");
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <div className="flex items-center justify-between">
                        <DialogTitle>THÊM TẬP MỚI</DialogTitle>
                    </div>
                </DialogHeader>
                <Form {...form}>
                    <form
                        onSubmit={form.handleSubmit(onSubmit)}
                        className="space-y-4"
                    >
                        <FormField
                            control={form.control}
                            name="name"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>TÊN TẬP</FormLabel>
                                    <FormControl>
                                        <Input
                                            placeholder="Vui lòng giữ tên tập trong 100 ký tự"
                                            {...field}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="introduction"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>GIỚI THIỆU</FormLabel>
                                    <FormControl>
                                        <Textarea
                                            placeholder="Vui lòng giữ phần mô tả trong 2000 ký tự"
                                            className="h-32 resize-none"
                                            {...field}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <DialogFooter className="gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={onClose}
                            >
                                HỦY
                            </Button>
                            <Button type="submit" disabled={isCreating}>
                                {isCreating ? "ĐANG TẠO..." : "TẠO"}
                            </Button>
                        </DialogFooter>
                    </form>
                </Form>
            </DialogContent>
        </Dialog>
    );
}
