"use client";

import Link from "next/link";
import React, { useCallback, useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
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

import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Textarea } from "@/components/ui/textarea";
import { Separator } from "@/components/ui/separator";
import { FaSave, FaEye, FaTrash, FaCalendar, FaPlus, FaEdit } from "react-icons/fa";
import { FaGear } from "react-icons/fa6";
import { IoSend } from "react-icons/io5";

import {
    ColumnDef,
    ColumnFiltersState,
    SortingState,
    VisibilityState,
    flexRender,
    getCoreRowModel,
    getFilteredRowModel,
    getPaginationRowModel,
    getSortedRowModel,
    useReactTable,
} from "@tanstack/react-table";
import MemoImage from "@/app/(AS_02)/_components/MemoImage";

import DeleteVolumeButton from "@/app/(AS_02)/_components/DeleteVolumeButton";

import { Checkbox } from "@/components/ui/checkbox";

import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

import { CHAPTER_STATUS } from "@/utils/chapterStatus";

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuRadioGroup,
    DropdownMenuRadioItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { BsFillSignStopFill } from "react-icons/bs";
import {
    ChapterItem,
    GetChapterListResponse,
    getChapters,
    getVolumeInfo,
    updateVolumeInfo,
    VolumeInfoResponse,
} from "@/utils/api/author-studio/volume";
import { toast } from "sonner";
import {
    bulkPublishChapter,
    bulkRemoveChapter,
    bulkUnpublishChapter,
    createChapter,
    removeChapter,
} from "@/utils/api/author-studio/chapter";

import { useRouter } from "next/navigation";
import DeleteChapterButton from "@/app/(AS_02)/_components/DeleteChapterButton";
import ChapterPagination from "@/app/(AS_02)/_components/ChapterPagination";
import { getSeriesInfo, SeriesInfo } from "@/utils/api/author-studio/series";
import { RiListOrdered2 } from "react-icons/ri";
import ReorderDialog from "@/components/ReorderDialog";
import PreviewButton from "@/app/(Reader)/components/PreviewButton";
import { Home } from "lucide-react";
import SiteLayout from "@/components/SiteLayout";

// export const payments: ChapterInfo[] = [
//     {
//         id: 1,
//         thumbnail: "https://placehold.co/720x1080.png",
//         title: "The Beginning of Adventure",
//         status: 1, // Draft
//         publishDate: "2024-09-30",
//     },
//     {
//         id: 2,
//         thumbnail: "https://placehold.co/720x1080.png",
//         title: "The Hero's Journey",
//         status: 2, // Scheduled
//         publishDate: "2024-10-05",
//     },
//     {
//         id: 3,
//         thumbnail: "https://placehold.co/720x1080.png",
//         title: "Battle at the Dark Forest",
//         status: 3, // Published
//         publishDate: "2024-09-28",
//     },
//     {
//         id: 4,
//         thumbnail: "https://placehold.co/720x1080.png",
//         title: "The Lost Kingdom",
//         status: 4, // Trashed
//         publishDate: "2024-09-25",
//     },
//     {
//         id: 5,
//         thumbnail: "https://placehold.co/720x1080.png",
//         title: "Rescue Mission",
//         status: 1, // Draft
//         publishDate: "2024-09-29",
//     },
// ];

export const emptyArr = [];
export function DataTable({ seriesId, volumeId, page, search: pSearch, status: pStatus }: ChapterListScreenProps) {
    const router = useRouter();
    const [chapters, setChapters] = useState<GetChapterListResponse>();
    const [search, setSearch] = useState<string>(pSearch);
    const [status, setStatus] = useState<number>(pStatus);
    const [reload, setReload] = useState(1);

    const columns: ColumnDef<ChapterItem>[] = [
        {
            id: "select",
            header: ({ table }) => (
                <Checkbox
                    checked={table.getIsAllPageRowsSelected() || (table.getIsSomePageRowsSelected() && "indeterminate")}
                    onCheckedChange={(value: boolean) => table.toggleAllPageRowsSelected(!!value)}
                    aria-label="Chọn tất cả"
                />
            ),
            cell: ({ row }) => (
                <Checkbox
                    checked={row.getIsSelected()}
                    onCheckedChange={(value: boolean) => row.toggleSelected(!!value)}
                    aria-label="Chọn hàng"
                />
            ),
            enableSorting: false,
            enableHiding: false,
        },
        {
            accessorKey: "thumbnail",
            header: "Hình thu nhỏ",
            cell: ({ row }) => {
                return (
                    <Link href={`/author-studio/series/${seriesId}/vol/${volumeId}/chap/${row.original.id}`}>
                        <div className="text-center">
                            <MemoImage src={row.getValue("thumbnail")} />
                        </div>
                    </Link>
                );
            },
        },
        {
            accessorKey: "title",
            header: () => <p className="w-40">Tiêu đề</p>,
            cell: ({ row }) => {
                const title = row.getValue("title") as string;
                const numberChap = row.original.chapterNumber;

                return (
                    <Link href={`/author-studio/series/${seriesId}/vol/${volumeId}/chap/${row.original.id}`}>
                        Chương {numberChap}: {title}
                    </Link>
                );
            },
        },
        {
            accessorKey: "status",
            header: "Trạng thái",
            cell: ({ row }) => {
                const status = row.getValue("status");
                // BE will return status as 3 (Published) even if a publish date is a future value
                const actualStatus =
                    status === 3 && +new Date((row.original as any).publishedDate) - +new Date() > 0 ? 2 : status;
                return <div>{CHAPTER_STATUS.find((s) => s.value === actualStatus)?.content}</div>;
            },
        },
        {
            accessorKey: "publishedDate",
            header: () => <p className="w-32">Ngày xuất bản</p>,
            cell: ({ row }) => {
                const dateStr = row.getValue("publishedDate") as string;
                const val = dateStr === null ? "" : new Date(Date.parse(dateStr)).toLocaleString();
                return <p>{val}</p>;
            },
        },
        {
            accessorKey: "price",
            header: () => <p className="w-32">Giá</p>,
            cell: ({ row }) => {
                const price = row.getValue("price") as string;
                const val = price === null ? "Miễn Phí" : price;
                return <p>{val}</p>;
            },
        },
        {
            accessorKey: "actions",
            header: "Hành động",
            cell: ({ row }) => {
                return (
                    <div className="flex justify-start gap-2">
                        <Link href={`/author-studio/series/${seriesId}/vol/${volumeId}/chap/${row.original.id}`}>
                            <Button className="bg-blue-600 text-white">
                                <FaEdit />
                            </Button>
                        </Link>
                        <ReorderDialog
                            seriesId={seriesId}
                            volumeId={volumeId}
                            chapterId={row.original.id.toString()}
                            onReordered={() => setReload(Math.random())}
                        >
                            <Button className="bg-neutral-600 text-white">
                                <RiListOrdered2 />
                            </Button>
                        </ReorderDialog>

                        <DeleteChapterButton
                            seriesId={seriesId}
                            volumeId={volumeId}
                            chapterId={row.original.id.toString()}
                            onDeleted={() => setReload(Math.random())}
                        />
                    </div>
                );
            },
        },
    ];

    function buildUrl(search: string, status: number, page: number) {
        const url = new URL(`https://whatever.com/author-studio/series/${seriesId}/vol/${volumeId}`);
        url.searchParams.set("search", search);
        url.searchParams.set("status", status.toString());
        url.searchParams.set("page", page.toString());
        return `${url.pathname}${url.search}`;
    }

    useEffect(() => {
        async function work() {
            const [chapters, err] = await getChapters(
                seriesId,
                volumeId,
                page,
                20,
                search,
                status === 0 ? undefined : status.toString(),
            );
            if (err) {
                console.error(err);
                toast.error("Failed to load chapters. Please reload the page.");
                return;
            }
            setChapters(chapters);
        }

        work();
    }, [page, search, seriesId, status, volumeId, reload]);

    const [rowSelection, setRowSelection] = React.useState({});
    const table = useReactTable({
        data: chapters?.items ?? emptyArr,
        columns,
        getCoreRowModel: getCoreRowModel(),
        onRowSelectionChange: setRowSelection,
        state: {
            rowSelection,
        },
    });

    async function handleBulkRemove() {
        const [_, err] = await bulkRemoveChapter(
            seriesId,
            volumeId,
            Object.keys(rowSelection)
                .map((x) => chapters?.items[parseInt(x)].id)
                .filter((x) => x !== undefined) as string[],
        );

        if (err) {
            toast.error("Không thể xóa chương");
            return;
        }

        toast.success("Các chương đã được xóa thành công");
        table.setRowSelection({});
        setReload(Math.random());
    }

    async function handleBulkPublish() {
        const [_, err] = await bulkPublishChapter(
            seriesId,
            volumeId,
            Object.keys(rowSelection)
                .map((x) => chapters?.items[parseInt(x)].id)
                .filter((x) => x !== undefined) as string[],
        );

        if (err) {
            toast.error("Không thể xuất bản chương");
            return;
        }

        toast.success("Các chương đã được xuất bản thành công");
        table.setRowSelection({});
        setReload(Math.random());
    }

    async function handleBulkUnpublish() {
        const [_, err] = await bulkUnpublishChapter(
            seriesId,
            volumeId,
            Object.keys(rowSelection)
                .map((x) => chapters?.items[parseInt(x)].id)
                .filter((x) => x !== undefined) as string[],
        );

        if (err) {
            toast.error("Không thể hủy xuất bản chương");
            return;
        }

        toast.success("Các chương đã được hủy xuất bản thành công");
        table.setRowSelection({});
        setReload(Math.random());
    }

    return (
        <>
            <div className="flex justify-end">
                <Input
                    placeholder="Lọc tiêu đề..."
                    value={search}
                    onChange={(e) => {
                        const { value } = e.target;
                        if (value === search) return;
                        setSearch(value);
                        router.replace(buildUrl(value, status, 1));
                    }}
                    className="max-w-sm"
                />
            </div>

            <div className="flex flex-wrap items-center justify-end gap-2 py-4">
                <Select value={status.toString()} onValueChange={(e) => setStatus(parseInt(e))}>
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Trạng thái" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="0">Tất cả trạng thái</SelectItem>
                        <SelectItem value="1">Nháp</SelectItem>
                        <SelectItem value="2">Đã lên lịch</SelectItem>
                        <SelectItem value="3">Đã xuất bản</SelectItem>
                    </SelectContent>
                </Select>

                <Dialog>
                    <DialogTrigger asChild>
                        <Button variant={"destructive"} className="flex gap-2">
                            <FaTrash /> Xóa hàng loạt
                        </Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Xác nhận xóa hàng loạt</DialogTitle>
                            <DialogDescription>
                                Bạn có chắc chắn muốn xóa các chương đã chọn không? Hành động này không thể hoàn tác.
                            </DialogDescription>
                        </DialogHeader>
                        <DialogFooter className="gap-2 sm:justify-end">
                            <DialogClose asChild>
                                <Button type="button" variant="ghost">
                                    Hủy
                                </Button>
                            </DialogClose>
                            <DialogClose asChild>
                                <Button variant={"destructive"} className="flex gap-2" onClick={handleBulkRemove}>
                                    <FaTrash /> Xóa chương đã chọn!
                                </Button>
                            </DialogClose>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>

                <DropdownMenu aria-label="Xuất bản">
                    <DropdownMenuTrigger asChild>
                        <Button className="flex gap-2 bg-blue-600 text-white">
                            <IoSend /> Xuất bản hàng loạt
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent className="w-56">
                        <DropdownMenuLabel>Tùy chọn xuất bản</DropdownMenuLabel>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onClick={handleBulkPublish}>
                            <IoSend className="mr-2 h-4 w-4" />
                            <span>Ngay lập tức</span>
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={handleBulkUnpublish}>
                            <BsFillSignStopFill className="mr-2 h-4 w-4" />
                            <span>Hủy xuất bản</span>
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>

                <Button
                    className="flex gap-2 bg-blue-600 px-4 text-white"
                    onClick={async () => {
                        const [chapterId, err] = await createChapter(seriesId, volumeId);

                        if (err) {
                            toast.error("Tạo chương thất bại");
                            return;
                        }

                        toast.success("Chương được tạo thành công");
                        router.push(`/author-studio/series/${seriesId}/vol/${volumeId}/chap/${chapterId}`);
                    }}
                >
                    <FaPlus /> Chương mới
                </Button>
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
            <div className="mt-2 flex justify-end">
                <ChapterPagination
                    currentPage={chapters?.pageNumber ?? 1}
                    totalPages={chapters?.totalPages ?? 1}
                    pageLink={(page) => buildUrl(search, status, page)}
                />
            </div>
        </>
    );
}

const volumeEditSchema = z.object({
    name: z.string(),
    introduction: z.string().optional(),
});

export function VolumeDetails(props: ChapterListScreenProps) {
    const form = useForm<z.infer<typeof volumeEditSchema>>({
        resolver: zodResolver(volumeEditSchema),
        defaultValues: {},
    });

    useEffect(() => {
        async function work() {
            const [volume, err] = await getVolumeInfo(props.seriesId, props.volumeId);
            if (err) {
                console.error(err);
                toast.error("Failed to load volume details! Please reload the page.");
                return;
            }

            form.reset(volume);
        }

        work();
    }, [props.seriesId, props.volumeId, form]);

    async function onSubmit(values: z.infer<typeof volumeEditSchema>) {
        const [_, err] = await updateVolumeInfo(props.seriesId, props.volumeId, values);

        if (err) {
            toast.error("Không cập nhật được chi tiết về tập truyện");
            return;
        }

        toast.success("Chi tiết tập truyện đã được cập nhật thành công");
    }

    return (
        <div>
            <Form {...form}>
                <form onSubmit={form.handleSubmit(onSubmit)} className="px-0">
                    <FormField
                        name="name"
                        control={form.control}
                        render={({ field }) => (
                            <FormItem className="mt-4">
                                <FormLabel>Tiêu đề tập truyện</FormLabel>
                                <FormControl>
                                    <Input
                                        className="w-full"
                                        placeholder="Nhập tiêu đề tập truyện của bạn..."
                                        {...field}
                                    />
                                </FormControl>
                                <FormDescription>Dưới 256 ký tự</FormDescription>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        name="introduction"
                        control={form.control}
                        render={({ field }) => (
                            <FormItem className="mt-4">
                                <FormLabel>Giới thiệu (Tùy chọn)</FormLabel>
                                <FormControl>
                                    <Textarea placeholder="Nhập phần mô tả..." {...field} />
                                </FormControl>
                                <FormDescription>Độ dài tối đa 2000 ký tự</FormDescription>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <div className="flex gap-2">
                        <Button className="mt-4 bg-blue-600 text-white">
                            <FaSave className="mr-2" /> Lưu
                        </Button>

                        <DeleteVolumeButton seriesId={props.seriesId} volumeId={props.volumeId} />
                    </div>
                </form>
            </Form>
        </div>
    );
}

export default function ChapterListScreen(props: ChapterListScreenProps) {
    const { seriesId, volumeId } = props;
    const [seriesName, setSeriesName] = useState<string>();
    const [volumeName, setVolumeName] = useState<string>();
    const [volumeInfo, setVolumeInfo] = useState<VolumeInfoResponse | null>(null);

    useEffect(() => {
        async function work() {
            const [volume, err] = await getVolumeInfo(seriesId, volumeId);

            if (err) {
                console.error(err);
                toast.error("Failed to load volume details! Please reload the page.");
                return;
            }

            setVolumeName(volume.name);

            const [series, err2] = await getSeriesInfo(seriesId);
            if (err2) {
                toast.error("Failed to load series details! Please reload the page.");
                return;
            }

            setSeriesName(series.title);
            setVolumeInfo(volume);
        }

        work();
    }, [seriesId, volumeId]);

    return (
        <SiteLayout>
            <div className="container w-screen justify-center px-2 pb-4">
                <Breadcrumb className="my-4">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/" className="flex items-center">
                                <Home className="mr-2 h-4 w-4" />
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio">Xưởng Truyện</BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/author-studio/series">Bộ truyện</BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink href={`/author-studio/series/${seriesId}`}>
                                {seriesName ?? "Đang tải"}
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>
                                Tập {volumeInfo?.volumeNumber ?? 1}: {volumeName ?? "Đang tải..."}
                            </BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="mt-4 gap-4 lg:flex">
                    <div>
                        <h2 className="text-lg">Chi tiết tập truyện</h2>
                        <VolumeDetails {...props} />
                    </div>

                    <Separator className="my-4 lg:hidden" />
                    <Separator orientation="vertical" className="hidden lg:block" />

                    <div className="flex-grow">
                        <h2 className="text-lg">Danh sách chương</h2>
                        <DataTable
                            seriesId={seriesId}
                            volumeId={volumeId}
                            page={props.page}
                            search={props.search}
                            status={props.status}
                        />
                    </div>
                </div>
            </div>
        </SiteLayout>
    );
}

export interface ChapterListScreenProps {
    seriesId: string;
    volumeId: string;
    page: number;
    search: string;
    status: number;
}
