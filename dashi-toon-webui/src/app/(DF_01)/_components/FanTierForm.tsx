import React, { ReactNode, useEffect, useState } from "react";
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

const formSchema = z.object({
    title: z
        .string()
        .min(1, "Tiêu đề hạng DashiFan là bắt buộc")
        .max(255, "Tiêu đề hạng DashiFan phải ít hơn 255 ký tự"),
    price: z.coerce
        .number()
        .min(10_000, "Giá phải ít nhất là 10.000")
        .max(100_000_000, "Giá phải ít hơn 100.000.000"),
    earlyChapterCount: z.coerce
        .number()
        .min(1, "Số lượng chương sớm phải ít nhất là 1")
        .max(1_000, "Số lượng chương sớm phải ít hơn 1000"),
});

export type FormValues = z.infer<typeof formSchema>;
interface AddVolumeDialogProps {
    onSubmit: (vals: FormValues) => void;
    onCancel: () => void;
    isLoading: boolean;
    defaultValue?: FormValues;
    actionButton?: ReactNode;
    hideCancelButton?: boolean;
}

export default function FanTierForm({
    onSubmit,
    onCancel,
    isLoading,
    defaultValue,
    actionButton,
    hideCancelButton,
}: AddVolumeDialogProps) {
    const form = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: defaultValue ?? {
            title: "",
            price: 10_000,
            earlyChapterCount: 5,
        },
    });

    useEffect(() => {
        form.reset(
            defaultValue ?? {
                title: "",
                price: 10_000,
                earlyChapterCount: 5,
            },
        );
    }, [form, defaultValue]);

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="title"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Tiêu đề hạng DashiFan</FormLabel>
                            <FormControl>
                                <Input
                                    placeholder="Vui lòng giữ tiêu đề hạng DashiFan dưới 255 ký tự"
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="price"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Giá hạng DashiFan</FormLabel>
                            <FormControl>
                                <div className="flex items-center gap-2">
                                    <Input
                                        min={10_000}
                                        max={100_000_000}
                                        step={1}
                                        type="number"
                                        placeholder="Chi phí cho một tháng của hạng này"
                                        {...field}
                                    />
                                    <span>VND</span>
                                </div>
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="earlyChapterCount"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Số lượng chương truy cập sớm</FormLabel>
                            <FormControl>
                                <Input
                                    min={1}
                                    max={1000}
                                    step={1}
                                    type="number"
                                    placeholder="Số lượng chương sớm bao gồm trong hạng này"
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <DialogFooter
                    className={hideCancelButton ? "sm:space-x-0" : "gap-2"}
                >
                    <Button
                        type="button"
                        variant="outline"
                        onClick={onCancel}
                        className={hideCancelButton ? "hidden" : ""}
                    >
                        Hủy
                    </Button>

                    {typeof actionButton === "string" || !actionButton ? (
                        <Button
                            type="submit"
                            disabled={isLoading}
                            className="bg-blue-600 text-white hover:bg-blue-700"
                        >
                            {isLoading
                                ? "Đang tải..."
                                : (actionButton ?? "Tạo")}
                        </Button>
                    ) : (
                        actionButton
                    )}
                </DialogFooter>
            </form>
        </Form>
    );
}
