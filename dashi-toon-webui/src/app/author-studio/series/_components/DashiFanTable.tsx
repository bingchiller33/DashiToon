"use client";

import Link from "next/link";
import React, { ReactNode, useCallback, useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTitle, DialogTrigger } from "@/components/ui/dialog";

import { Input } from "@/components/ui/input";

import { Separator } from "@/components/ui/separator";
import { FaPlus } from "react-icons/fa";

import {
    ColumnDef,
    flexRender,
    getCoreRowModel,
    useReactTable,
    SortingState,
    getSortedRowModel,
    ColumnFiltersState,
    getFilteredRowModel,
} from "@tanstack/react-table";

import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

import { useRouter } from "next/navigation";

import { getSeriesInfo } from "@/utils/api/author-studio/series";
import UwU from "@/components/UwU";
import { ArrowUpDown, Home, ListCollapse } from "lucide-react";
import DeleteTierButton from "@/app/(DF_01)/_components/DeleteTierButton";
import FanTierForm, { FormValues } from "@/app/(DF_01)/_components/FanTierForm";
import { toast } from "sonner";

import SiteLayout from "@/components/SiteLayout";
import { createDashiFanTier, getDashiFanTiers } from "@/utils/api/dashifan";
import EditDashiFanDialog from "./EditDashiFanDialog";

export interface ChapterListScreenProps {
    seriesId: string;
}

export interface SubscriptionTierInfo {
    id: string;
    title: string;
    price: number;
    earlyChapter: number;
    subscriberCount: number;
    prevCycleDiff: number;
    active: boolean;
}

export const emptyArr = [];
export default function DashiFanTable({ seriesId }: ChapterListScreenProps) {
    const columns: ColumnDef<SubscriptionTierInfo>[] = [
        {
            accessorKey: "title",
            header: ({ column }) => (
                <Button
                    className="ms-[-16px]"
                    variant="ghost"
                    onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                >
                    Tiêu đề
                    <ArrowUpDown className="ml-2 h-4 w-4" />
                </Button>
            ),
            cell: ({ row }) => row.getValue("title"),
        },
        {
            accessorKey: "price",
            header: ({ column }) => {
                return (
                    <Button
                        className="ms-[-16px]"
                        variant="ghost"
                        onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                    >
                        Giá
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                    </Button>
                );
            },
            cell: ({ row }) => {
                const price = row.getValue("price") as number;

                return (
                    <div>
                        {price.toLocaleString("vi-VN", {})}
                        {"đ "}
                        <UwU>Ủa sao lại scam vậy?</UwU>
                    </div>
                );
            },
        },
        {
            accessorKey: "earlyChapter",
            header: ({ column }) => {
                return (
                    <Button
                        className="ms-[-16px]"
                        variant="ghost"
                        onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                    >
                        Chương truy cập sớm
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                    </Button>
                );
            },
            cell: ({ row }) => {
                const val = row.getValue("earlyChapter") as number;
                return <p>{val}</p>;
            },
        },
        // {
        //     accessorKey: "subscriberCount",
        //     header: ({ column }) => {
        //         return (
        //             <Button
        //                 className="ms-[-16px]"
        //                 variant="ghost"
        //                 onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
        //             >
        //                 Số người đăng ký hoạt động
        //                 <ArrowUpDown className="ml-2 h-4 w-4" />
        //             </Button>
        //         );
        //     },
        //     cell: ({ row }) => {
        //         const current = row.getValue("subscriberCount") as number;
        //         const delta = row.original.prevCycleDiff;
        //         return (
        //             <p>
        //                 {current}{" "}
        //                 {delta > 0 && (
        //                     <UwU gate="analytics">
        //                         <span className="text-green-500">+{delta}</span>
        //                     </UwU>
        //                 )}
        //             </p>
        //         );
        //     },
        // },
        {
            accessorKey: "actions",
            header: "Hành động",
            cell: ({ row }) => {
                return (
                    <div className="flex items-center justify-start gap-2">
                        <UwU>
                            <Link href={`/author-studio/series/${seriesId}/dashifan/${row.original.id}`}>
                                <Button aria-label="Xem" className="bg-neutral-600 text-white">
                                    <ListCollapse /> Chi tiết
                                </Button>
                            </Link>
                        </UwU>
                        <EditDashiFanDialog seriesId={seriesId} tierId={row.original.id} />
                        <DeleteTierButton
                            tierId={row.original.id}
                            seriesId={seriesId}
                            onDeleted={() => {
                                const newTiers = tiers.filter((x) => x.id !== row.original.id);
                                setTiers(newTiers);
                            }}
                        ></DeleteTierButton>
                    </div>
                );
            },
        },
    ];

    const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>([]);
    const [sorting, setSorting] = React.useState<SortingState>([]);
    const [tiers, setTiers] = useState<SubscriptionTierInfo[]>([]);
    const table = useReactTable({
        data: tiers,
        columns,
        onSortingChange: setSorting,
        getSortedRowModel: getSortedRowModel(),
        getCoreRowModel: getCoreRowModel(),
        onColumnFiltersChange: setColumnFilters,
        getFilteredRowModel: getFilteredRowModel(),
        state: {
            sorting,
            columnFilters,
        },
    });

    const router = useRouter();
    const [isNewTierDialogOpen, setIsNewTierDialogOpen] = useState(false);
    const [isNewTierDialogLoading, setIsNewTierDialogLoading] = useState(false);

    useEffect(() => {
        async function work() {
            const [data, err] = await getDashiFanTiers(seriesId);

            if (err) {
                toast.error("Không thể tải danh sách hạng DashiFan!");
                return;
            }

            const mapped = data
                .filter((x) => x.isActive)
                .map((tier) => ({
                    id: tier.id,
                    title: tier.name,
                    price: tier.price.amount,
                    earlyChapter: tier.perks,
                    subscriberCount: Math.floor(Math.random() * 1000),
                    prevCycleDiff: Math.floor(Math.random() * 1000),
                    active: true,
                }));
            setTiers(mapped);
        }

        work();
    }, [seriesId]);

    async function handleCreate(values: FormValues) {
        try {
            setIsNewTierDialogLoading(true);
            const [id, err] = await createDashiFanTier(seriesId, {
                name: values.title,
                perks: values.earlyChapterCount,
                amount: values.price,
            });

            if (err) {
                toast.error("Không thể tạo gói DashiFan mới!");
                return;
            }

            toast.success("Tạo gói DashiFan mới thành công!");
            router.refresh();
            // router.push(`/author-studio/series/${seriesId}/dashifan/${id}`);
        } finally {
            setIsNewTierDialogOpen(false);
            setIsNewTierDialogLoading(false);
        }
    }

    return (
        <>
            <div className="mb-4 flex justify-end gap-2">
                <Input
                    placeholder="Lọc tiêu đề..."
                    value={(table.getColumn("title")?.getFilterValue() as string) ?? ""}
                    onChange={(e) => {
                        table.getColumn("title")?.setFilterValue(e.target.value);
                    }}
                    className="max-w-sm"
                />

                <Dialog open={isNewTierDialogOpen} onOpenChange={setIsNewTierDialogOpen}>
                    <DialogTrigger>
                        <Button className="flex gap-2 bg-blue-600 px-4 text-white">
                            <FaPlus /> Gói mới
                        </Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogTitle>
                            <h3 className="text-xl">Tạo gói DashiFan mới</h3>
                        </DialogTitle>
                        <FanTierForm
                            onSubmit={handleCreate}
                            onCancel={() => setIsNewTierDialogOpen(false)}
                            isLoading={isNewTierDialogLoading}
                        />
                    </DialogContent>
                </Dialog>
            </div>

            <div className="w-full overflow-auto rounded-md border">
                <Table>
                    <TableHeader>
                        {table.getHeaderGroups().map((headerGroup) => (
                            <TableRow key={headerGroup.id}>
                                {headerGroup.headers.map((header) => {
                                    return (
                                        <TableHead key={header.id}>
                                            {header.isPlaceholder
                                                ? null
                                                : flexRender(header.column.columnDef.header, header.getContext())}
                                        </TableHead>
                                    );
                                })}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {table.getRowModel().rows?.length ? (
                            table.getRowModel().rows.map((row) => (
                                <TableRow key={row.id} data-state={row.getIsSelected() && "selected"}>
                                    {row.getVisibleCells().map((cell) => (
                                        <TableCell key={cell.id}>
                                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={columns.length} className="h-24 text-center">
                                    Không có kết quả.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>
        </>
    );
}
