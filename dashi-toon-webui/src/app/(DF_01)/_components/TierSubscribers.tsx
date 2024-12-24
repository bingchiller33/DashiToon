"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    Sheet,
    SheetContent,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
    ChevronLeft,
    ChevronRight,
    ArrowUpDown,
    AlertCircle,
    Search,
} from "lucide-react";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer,
} from "recharts";
import {
    ChartContainer,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import { Toggle } from "@/components/ui/toggle";
import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";
import { ScrollArea } from "@/components/ui/scroll-area";

interface Subscriber {
    id: string;
    name: string;
    joinDate: string;
    currentTierDuration: number;
    totalDuration: number;
    earlyChapterViews: {
        current: number;
        delta: number;
    };
    isActive: boolean;
    subscriptionHistory: {
        startDate: string;
        endDate: string;
        duration: number;
        tier: string;
        monthlyViews: number;
    }[];
    totalRevenue: number;
    viewsHistory: { date: string; views: number }[];
    revenueHistory: { date: string; revenue: number }[];
}

const subscribers: Subscriber[] = [
    {
        id: "1",
        name: "John Doe",
        joinDate: "2023-01-15",
        currentTierDuration: 3,
        totalDuration: 5,
        earlyChapterViews: { current: 25, delta: 5 },
        isActive: true,
        subscriptionHistory: [
            {
                startDate: "2023-01-15",
                endDate: "2023-03-14",
                duration: 2,
                tier: "Silver",
                monthlyViews: 15,
            },
            {
                startDate: "2023-03-15",
                endDate: "Present",
                duration: 3,
                tier: "Gold",
                monthlyViews: 25,
            },
        ],
        totalRevenue: 49.95,
        viewsHistory: [
            { date: "2023-01", views: 15 },
            { date: "2023-02", views: 18 },
            { date: "2023-03", views: 22 },
            { date: "2023-04", views: 25 },
            { date: "2023-05", views: 30 },
        ],
        revenueHistory: [
            { date: "2023-01", revenue: 9.99 },
            { date: "2023-02", revenue: 9.99 },
            { date: "2023-03", revenue: 9.99 },
            { date: "2023-04", revenue: 9.99 },
            { date: "2023-05", revenue: 9.99 },
        ],
    },
    // Add more subscriber objects here...
];

type SortField = "name" | "totalDuration" | "earlyChapterViews" | "active";

export default function CurrentSubscribers() {
    const [nameFilter, setNameFilter] = useState("");
    const [sortField, setSortField] = useState<SortField>("name");
    const [sortOrder, setSortOrder] = useState<"asc" | "desc">("asc");
    const [showActiveOnly, setShowActiveOnly] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 10;

    const handleSort = (field: SortField) => {
        if (field === sortField) {
            setSortOrder(sortOrder === "asc" ? "desc" : "asc");
        } else {
            setSortField(field);
            setSortOrder("asc");
        }
    };

    const filteredSubscribers = subscribers
        .filter((sub) =>
            sub.name.toLowerCase().includes(nameFilter.toLowerCase()),
        )
        .filter((sub) => !showActiveOnly || sub.isActive)
        .sort((a, b) => {
            const order = sortOrder === "asc" ? 1 : -1;
            if (sortField === "name") {
                return a.name.localeCompare(b.name) * order;
            } else if (sortField === "totalDuration") {
                return (a.totalDuration - b.totalDuration) * order;
            } else if (sortField === "earlyChapterViews") {
                return (
                    (a.earlyChapterViews.current -
                        b.earlyChapterViews.current) *
                    order
                );
            }
            return 0;
        });

    const pageCount = Math.ceil(filteredSubscribers.length / itemsPerPage);
    const paginatedSubscribers = filteredSubscribers.slice(
        (currentPage - 1) * itemsPerPage,
        currentPage * itemsPerPage,
    );

    return (
        <Card className="border-neutral-700 bg-neutral-800">
            <CardHeader>
                <CardTitle className="text-blue-300">
                    Người đăng ký hiện tại
                </CardTitle>
            </CardHeader>
            <CardContent>
                <div className="mb-4 flex items-center gap-2">
                    <Input
                        placeholder="Lọc theo tên"
                        value={nameFilter}
                        onChange={(e) => setNameFilter(e.target.value)}
                        className="border-neutral-600 bg-neutral-700 text-neutral-100"
                    />
                    <Toggle
                        variant="outline"
                        aria-label="Chuyển đổi chữ nghiêng"
                        className="w-[280px]"
                    >
                        Chỉ đăng ký hoạt động
                    </Toggle>

                    <Button
                        className="bg-blue-600 text-white hover:bg-blue-700"
                        onClick={() => {
                            /* Thực hiện chức năng tìm kiếm */
                        }}
                    >
                        <Search className="h-4 w-4" />
                        <span className="sr-only">Tìm</span>
                    </Button>
                </div>
                <Pagination className="justify-end">
                    <PaginationContent>
                        <PaginationItem>
                            <PaginationPrevious href="#" />
                        </PaginationItem>
                        <PaginationItem>
                            <PaginationLink href="#">1</PaginationLink>
                        </PaginationItem>
                        <PaginationItem>
                            <PaginationLink href="#" isActive>
                                2
                            </PaginationLink>
                        </PaginationItem>
                        <PaginationItem>
                            <PaginationLink href="#">3</PaginationLink>
                        </PaginationItem>
                        <PaginationItem>
                            <PaginationEllipsis />
                        </PaginationItem>
                        <PaginationItem>
                            <PaginationNext href="#" />
                        </PaginationItem>
                    </PaginationContent>
                </Pagination>
                <div className="overflow-x-auto">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead className="text-neutral-300">
                                    <Button
                                        variant="ghost"
                                        onClick={() => handleSort("name")}
                                        className="h-auto p-0 font-bold text-neutral-300 hover:text-blue-300"
                                    >
                                        Tên
                                        <ArrowUpDown className="ml-2 h-4 w-4" />
                                    </Button>
                                </TableHead>
                                <TableHead className="text-neutral-300">
                                    <Button
                                        variant="ghost"
                                        onClick={() =>
                                            handleSort("totalDuration")
                                        }
                                        className="h-auto p-0 font-bold text-neutral-300 hover:text-blue-300"
                                    >
                                        Tổng thời gian
                                        <ArrowUpDown className="ml-2 h-4 w-4" />
                                    </Button>
                                </TableHead>
                                <TableHead className="text-neutral-300">
                                    <Button
                                        variant="ghost"
                                        onClick={() => handleSort("active")}
                                        className="h-auto p-0 font-bold text-neutral-300 hover:text-blue-300"
                                    >
                                        Hạng hoạt động
                                        <ArrowUpDown className="ml-2 h-4 w-4" />
                                    </Button>
                                </TableHead>
                                <TableHead className="text-neutral-300">
                                    <Button
                                        variant="ghost"
                                        onClick={() =>
                                            handleSort("earlyChapterViews")
                                        }
                                        className="h-auto p-0 font-bold text-neutral-300 hover:text-blue-300"
                                    >
                                        Lượt xem chương sớm
                                        <ArrowUpDown className="ml-2 h-4 w-4" />
                                    </Button>
                                </TableHead>
                                <TableHead className="text-neutral-300">
                                    Hành động
                                </TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {paginatedSubscribers.map((sub) => (
                                <TableRow key={sub.id}>
                                    <TableCell className="text-neutral-100">
                                        {sub.name}
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        {sub.currentTierDuration} /{" "}
                                        {sub.totalDuration} tháng
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        Vàng
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        {sub.earlyChapterViews.current}{" "}
                                        <span
                                            className={
                                                sub.earlyChapterViews.delta > 0
                                                    ? "text-green-500"
                                                    : "text-red-500"
                                            }
                                        >
                                            {sub.earlyChapterViews.delta > 0
                                                ? "+"
                                                : ""}
                                            {sub.earlyChapterViews.delta}
                                        </span>
                                    </TableCell>

                                    <TableCell className="flex items-center">
                                        <Button
                                            variant="outline"
                                            className="mr-2 bg-red-600 text-white hover:bg-red-700"
                                        >
                                            <AlertCircle className="mr-2 h-4 w-4" />
                                            Báo cáo
                                        </Button>
                                        <Sheet>
                                            <SheetTrigger asChild>
                                                <Button
                                                    variant="outline"
                                                    className="bg-blue-600 text-white hover:bg-blue-700"
                                                >
                                                    Chi tiết
                                                </Button>
                                            </SheetTrigger>
                                            <SheetContent className="w-[90vw] overflow-auto bg-neutral-800 text-neutral-100 sm:max-w-[800px]">
                                                <SubscriberDetails
                                                    subscriber={sub}
                                                />
                                            </SheetContent>
                                        </Sheet>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>
            </CardContent>
        </Card>
    );
}

function SubscriberDetails({ subscriber }: { subscriber: Subscriber }) {
    return (
        <div className="space-y-6">
            <SheetHeader>
                <SheetTitle className="text-blue-300">
                    Chi tiết người đăng ký: {subscriber.name}
                </SheetTitle>
            </SheetHeader>
            <div className="grid grid-cols-2 gap-4">
                <div>
                    <h3 className="text-sm font-medium text-neutral-400">
                        Ngày tham gia
                    </h3>
                    <p className="text-neutral-100">{subscriber.joinDate}</p>
                </div>
                <div>
                    <h3 className="text-sm font-medium text-neutral-400">
                        Trạng thái đăng ký
                    </h3>
                    <p className="text-neutral-100">
                        {subscriber.isActive
                            ? "Đang hoạt động"
                            : "Ngưng hoạt động"}
                    </p>
                </div>
                <div>
                    <h3 className="text-sm font-medium text-neutral-400">
                        Tổng doanh thu
                    </h3>
                    <p className="text-neutral-100">
                        ${subscriber.totalRevenue.toFixed(2)}
                    </p>
                </div>
                <div>
                    <h3 className="text-sm font-medium text-neutral-400">
                        Hạng hiện tại
                    </h3>
                    <p className="text-neutral-100">
                        {
                            subscriber.subscriptionHistory[
                                subscriber.subscriptionHistory.length - 1
                            ].tier
                        }
                    </p>
                </div>
            </div>

            <div className="space-y-4">
                <h3 className="text-lg font-semibold text-blue-300">
                    Lượt xem chương sớm
                </h3>
                <ChartContainer
                    config={{
                        views: {
                            label: "Lượt xem",
                            color: "hsl(var(--chart-1))",
                        },
                    }}
                    className="h-[200px]"
                >
                    <ResponsiveContainer width="100%" height="100%">
                        <LineChart data={subscriber.viewsHistory}>
                            <CartesianGrid
                                strokeDasharray="3 3"
                                stroke="#374151"
                            />
                            <XAxis dataKey="date" stroke="#9CA3AF" />
                            <YAxis stroke="#9CA3AF" />
                            <ChartTooltip content={<ChartTooltipContent />} />
                            <Legend />
                            <Line
                                type="monotone"
                                dataKey="views"
                                stroke="var(--color-views)"
                                strokeWidth={2}
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </ChartContainer>
            </div>

            <div className="space-y-4">
                <h3 className="text-lg font-semibold text-blue-300">
                    Doanh thu
                </h3>
                <ChartContainer
                    config={{
                        revenue: {
                            label: "Doanh thu",
                            color: "hsl(var(--chart-2))",
                        },
                    }}
                    className="h-[200px]"
                >
                    <ResponsiveContainer width="100%" height="100%">
                        <LineChart data={subscriber.revenueHistory}>
                            <CartesianGrid
                                strokeDasharray="3 3"
                                stroke="#374151"
                            />
                            <XAxis dataKey="date" stroke="#9CA3AF" />
                            <YAxis stroke="#9CA3AF" />
                            <ChartTooltip content={<ChartTooltipContent />} />
                            <Legend />
                            <Line
                                type="monotone"
                                dataKey="revenue"
                                stroke="var(--color-revenue)"
                                strokeWidth={2}
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </ChartContainer>
            </div>

            <div className="space-y-4">
                <h3 className="text-lg font-semibold text-blue-300">
                    Lịch sử đăng ký
                </h3>
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead className="text-neutral-300">
                                Ngày bắt đầu
                            </TableHead>
                            <TableHead className="text-neutral-300">
                                Ngày kết thúc
                            </TableHead>
                            <TableHead className="text-neutral-300">
                                Thời gian
                            </TableHead>
                            <TableHead className="text-neutral-300">
                                Hạng
                            </TableHead>
                            <TableHead className="text-neutral-300">
                                Lượt xem hàng tháng
                            </TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {subscriber.subscriptionHistory
                            .sort(
                                (a, b) =>
                                    -Date.parse(a.startDate) +
                                    Date.parse(b.startDate),
                            )
                            .map((history, index) => (
                                <TableRow key={index}>
                                    <TableCell className="text-neutral-100">
                                        {history.startDate}
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        {history.endDate}
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        {history.duration}
                                        tháng
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        {history.tier}
                                    </TableCell>
                                    <TableCell className="text-neutral-100">
                                        {history.monthlyViews}
                                    </TableCell>
                                </TableRow>
                            ))}
                    </TableBody>
                </Table>
            </div>

            <Button
                variant="destructive"
                className="w-full bg-red-600 text-white hover:bg-red-700"
            >
                <AlertCircle className="mr-2 h-4 w-4" />
                Báo cáo người dùng
            </Button>
        </div>
    );
}
