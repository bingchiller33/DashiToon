"use client";

import Link from "next/link";
import React, {
    ReactNode,
    useCallback,
    useEffect,
    useMemo,
    useState,
} from "react";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";

import {
    Breadcrumb,
    BreadcrumbEllipsis,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

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

import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";

import { useRouter } from "next/navigation";

import { getSeriesInfo } from "@/utils/api/author-studio/series";
import UwU from "@/components/UwU";
import { ArrowUpDown, Home, ListCollapse } from "lucide-react";
import DeleteTierButton from "@/app/(DF_01)/_components/DeleteTierButton";
import FanTierForm, { FormValues } from "@/app/(DF_01)/_components/FanTierForm";
import { toast } from "sonner";
import CurrentSubscribers from "../../_components/TierSubscribers";
import TierStatistics from "../../_components/DashiFanStat";
import SiteLayout from "@/components/SiteLayout";
import { createDashiFanTier, getDashiFanTiers } from "@/utils/api/dashifan";

export interface SubscriptionTierInfo {
    id: string;
    title: string;
    price: number;
    earlyChapter: number;
    subscriberCount: number;
    prevCycleDiff: number;
    active: boolean;
}

const fakeSubscriptionTiers: SubscriptionTierInfo[] = [
    {
        id: "tier1",
        title: "Bronze",
        price: 5.99,
        earlyChapter: 2,
        subscriberCount: 120,
        prevCycleDiff: 69,
        active: true,
    },
    {
        id: "tier2",
        title: "Silver",
        price: 9.99,
        earlyChapter: 5,
        subscriberCount: 340,
        prevCycleDiff: 69,
        active: false,
    },
    {
        id: "tier3",
        title: "Gold",
        price: 14.99,
        earlyChapter: 10,
        subscriberCount: 670,
        prevCycleDiff: 69,
        active: true,
    },
    {
        id: "tier4",
        title: "Platinum",
        price: 19.99,
        earlyChapter: 15,
        subscriberCount: 920,
        prevCycleDiff: 69,
        active: true,
    },
];

export const emptyArr = [];
export function DataTable({ seriesId }: ChapterListScreenProps) {
    const columns: ColumnDef<SubscriptionTierInfo>[] = [
        {
            accessorKey: "title",
            header: ({ column }) => (
                <Button
                    className="ms-[-16px]"
                    variant="ghost"
                    onClick={() =>
                        column.toggleSorting(column.getIsSorted() === "asc")
                    }
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
                        onClick={() =>
                            column.toggleSorting(column.getIsSorted() === "asc")
                        }
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
                        onClick={() =>
                            column.toggleSorting(column.getIsSorted() === "asc")
                        }
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
        {
            accessorKey: "subscriberCount",
            header: ({ column }) => {
                return (
                    <Button
                        className="ms-[-16px]"
                        variant="ghost"
                        onClick={() =>
                            column.toggleSorting(column.getIsSorted() === "asc")
                        }
                    >
                        Số người đăng ký hoạt động
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                    </Button>
                );
            },
            cell: ({ row }) => {
                const current = row.getValue("subscriberCount") as number;
                const delta = row.original.prevCycleDiff;
                return (
                    <p>
                        {current}{" "}
                        {delta > 0 && (
                            <UwU gate="analytics">
                                <span className="text-green-500">+{delta}</span>
                            </UwU>
                        )}
                    </p>
                );
            },
        },

        {
            accessorKey: "actions",
            header: "Hành động",
            cell: ({ row }) => {
                return (
                    <div className="flex items-center justify-start gap-2">
                        <Link
                            href={`/author-studio/series/${seriesId}/dashifan/${row.original.id}`}
                        >
                            <Button
                                aria-label="Xem"
                                className="bg-neutral-600 text-white"
                            >
                                <ListCollapse /> Chi tiết
                            </Button>
                        </Link>
                        <DeleteTierButton
                            tierId={row.original.id}
                            seriesId={seriesId}
                            onDeleted={() => {
                                const newTiers = tiers.filter(
                                    (x) => x.id !== row.original.id,
                                );
                                setTiers(newTiers);
                            }}
                        ></DeleteTierButton>
                    </div>
                );
            },
        },
    ];

    const [columnFilters, setColumnFilters] =
        React.useState<ColumnFiltersState>([]);
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
            router.push(`/author-studio/series/${seriesId}/dashifan/${id}`);
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
                    value={
                        (table
                            .getColumn("title")
                            ?.getFilterValue() as string) ?? ""
                    }
                    onChange={(e) => {
                        table
                            .getColumn("title")
                            ?.setFilterValue(e.target.value);
                    }}
                    className="max-w-sm"
                />

                <Dialog
                    open={isNewTierDialogOpen}
                    onOpenChange={setIsNewTierDialogOpen}
                >
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
                                                : flexRender(
                                                      header.column.columnDef
                                                          .header,
                                                      header.getContext(),
                                                  )}
                                        </TableHead>
                                    );
                                })}
                            </TableRow>
                        ))}
                    </TableHeader>
                    <TableBody>
                        {table.getRowModel().rows?.length ? (
                            table.getRowModel().rows.map((row) => (
                                <TableRow
                                    key={row.id}
                                    data-state={
                                        row.getIsSelected() && "selected"
                                    }
                                >
                                    {row.getVisibleCells().map((cell) => (
                                        <TableCell key={cell.id}>
                                            {flexRender(
                                                cell.column.columnDef.cell,
                                                cell.getContext(),
                                            )}
                                        </TableCell>
                                    ))}
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell
                                    colSpan={columns.length}
                                    className="h-24 text-center"
                                >
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

export default function ManageDashiFanScreen(props: ChapterListScreenProps) {
    const { seriesId } = props;
    const [seriesName, setSeriesName] = useState<string>("Đang tải...");

    useEffect(() => {
        async function work() {
            const [series, err2] = await getSeriesInfo(seriesId);
            if (err2) {
                toast.error(
                    "Không thể tải thông tin bộ truyện! Vui lòng tải lại trang.",
                );
                return;
            }

            setSeriesName(series.title);
        }

        work();
    }, [seriesId]);

    return (
        <SiteLayout>
            <div className="container w-screen justify-center px-2 pb-4 md:p-4">
                <Breadcrumb className="mb-4">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink
                                href="/"
                                className="flex items-center"
                            >
                                <Home className="mr-2 h-4 w-4" />
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio">
                                Xưởng Truyện
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio/series">
                                Bộ truyện
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink
                                href={`/author-studio/series/${seriesId}`}
                            >
                                {seriesName ?? "Đang tải"}
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>
                                Quản lý hạng DashiFan
                            </BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <div className="mt-4">
                    <h1 className="text-3xl">Quản lý hạng DashiFan </h1>
                    <DataTable seriesId={seriesId} />
                </div>

                <Separator className="my-4" />
                <h1 className="text-3xl">Phân tích </h1>

                <div className="mt-4">
                    <TierStatistics />
                </div>
                <div className="mt-4">
                    <CurrentSubscribers />
                </div>
            </div>
        </SiteLayout>
    );
}

export interface ChapterListScreenProps {
    seriesId: string;
}
