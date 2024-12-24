"use client";

import { useState, useEffect, useMemo } from "react";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";

import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import {
    CreditCard,
    DollarSign,
    Home,
    TrendingUp,
    WalletMinimal,
} from "lucide-react";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

import SiteLayout from "@/components/SiteLayout";
import { RevenueChart } from "../_components/RevenueChart";
import Link from "next/link";
import { Skeleton } from "@/components/ui/skeleton";

import RevenuePagination from "../_components/RevenuePagination";
import RevenueTable from "../_components/RevenueTable";
import { formatCurrency } from "@/utils";
import WithdrawDialog from "../_components/WithdrawDialog";
import {
    getRevenueData,
    getTransactionHistory,
    RevenueData,
} from "@/utils/api/author-studio/revenue";
import { toast } from "sonner";
import useUwU from "@/hooks/useUwU";
const fakeData = [
    { month: "11-2023", revenue: 100000.0, withdrawal: 0.0 },
    { month: "1-2024", revenue: 200000.0, withdrawal: 10000.0 },
    { month: "2-2024", revenue: 150000.0, withdrawal: 20000.0 },
    { month: "3-2024", revenue: 180000.0, withdrawal: 10000.0 },
    { month: "4-2024", revenue: 220000.0, withdrawal: 30000.0 },
    { month: "5-2024", revenue: 250000.0, withdrawal: 10000.0 },
    { month: "6-2024", revenue: 280000.0, withdrawal: 40000.0 },
    { month: "7-2024", revenue: 300000.0, withdrawal: 10000.0 },
    { month: "8-2024", revenue: 320000.0, withdrawal: 50000.0 },
    { month: "9-2024", revenue: 350000.0, withdrawal: 10000.0 },
    { month: "10-2024", revenue: 320000.0, withdrawal: 60000.0 },
];
const fetchBalanceData = async () => {
    return {
        balance: 100.0,
        totalRevenue: 200.0,
        momGrowth: 0.5,
        data: fakeData,
    };
};

const fetchHistoryData = async (page: number, filter: string) => {
    return {
        items: [
            {
                amount: 160,
                revenueType: 1,
                transactionType: 0,
                reason: "Doanh thu từ chương truyện [80699]",
                timestamp: "2024-11-14T15:31:28.7740790+00:00",
            },
            {
                amount: 210,
                revenueType: 1,
                transactionType: 0,
                reason: "Doanh thu từ chương truyện [80692]",
                timestamp: "2024-11-14T15:30:33.6607220+00:00",
            },
            {
                amount: 190,
                revenueType: 1,
                transactionType: 0,
                reason: "Doanh thu từ chương truyện [80691]",
                timestamp: "2024-11-14T15:30:21.8955490+00:00",
            },
            {
                amount: 500,
                revenueType: 3,
                transactionType: 1,
                reason: "Rút tiền qua PayPal",
                timestamp: "2024-11-13T10:15:00.0000000+00:00",
            },
        ],
        pageNumber: page,
        totalPages: 5,
        totalCount: 50,
        hasPreviousPage: page > 1,
        hasNextPage: page < 5,
    };
};

export default function AuthorRevenue() {
    const [balanceData, setBalanceData] = useState<RevenueData | null>(null);
    const [inData, setInData] = useState<any>(null);
    const [outData, setOutData] = useState<any>(null);
    const [currentInPage, setCurrentInPage] = useState(1);
    const [currentOutPage, setCurrentOutPage] = useState(1);
    const [isLoading, setIsLoading] = useState(true);
    const [isHistoryLoading, setIsHistoryLoading] = useState(true);
    const [uwu] = useUwU();

    useEffect(() => {
        let mounted = true;
        setIsLoading(true);
        async function fetchBalanceData() {
            const [data, err] = await getRevenueData();
            if (err) {
                toast.error("Lỗi khi tải dữ liệu tài chính");
                return;
            }

            if (!mounted) {
                return;
            }
            console.log({ uwu });
            if (uwu) {
                data.data = [...fakeData, ...data.data];
                data.data.splice(12);
            }
            setBalanceData(data);
            setIsLoading(false);
        }

        fetchBalanceData();

        // fetchBalanceData().then((x) => {
        //     setBalanceData(x);
        //     setIsLoading(false);
        // });

        return () => {
            mounted = false;
        };
    }, [uwu]);

    useEffect(() => {
        setIsHistoryLoading(true);
        async function fetchHistoryData() {
            const [data, err] = await getTransactionHistory(
                0,
                currentInPage,
                20,
            );

            if (err) {
                toast.error("Lỗi khi tải lịch sử doanh thu");
                return;
            }

            setInData(data);
            setIsHistoryLoading(false);
        }
        fetchHistoryData();
        // fetchHistoryData(currentInPage, filter).then((data) => {
        //     setInData(data);
        //     setIsHistoryLoading(false);
        // });
    }, [currentInPage]);

    useEffect(() => {
        setIsHistoryLoading(true);
        async function fetchHistoryData() {
            const [data, err] = await getTransactionHistory(
                1,
                currentOutPage,
                20,
            );

            if (err) {
                toast.error("Lỗi khi tải lịch sử rút tiền");
                return;
            }

            setOutData(data);
            setIsHistoryLoading(false);
        }
        fetchHistoryData();

        // fetchHistoryData(currentOutPage, filter).then((data) => {
        //     setOutData(data);
        //     setIsHistoryLoading(false);
        // });
    }, [currentOutPage]);

    return (
        <SiteLayout>
            <div className="container mx-auto space-y-6 p-4">
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
                            <BreadcrumbPage>Quản lý tài chính</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <h1 className="mb-6 text-2xl font-bold text-blue-400">
                    Quản lý tài chính tác giả
                </h1>

                <div className="grid gap-6 md:grid-cols-2">
                    <Card className="bg-neutral-800">
                        <CardHeader>
                            <CardTitle className="text-gray-100">
                                Biểu đồ doanh thu
                            </CardTitle>
                            <CardDescription className="text-gray-400">
                                Doanh thu và rút tiền trong 12 tháng qua
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            {isLoading ? (
                                <div className="space-y-2">
                                    <Skeleton className="h-[250px] w-full" />
                                </div>
                            ) : (
                                <RevenueChart data={balanceData?.data ?? []} />
                            )}
                        </CardContent>
                    </Card>

                    <Card className="flex flex-col bg-neutral-800">
                        <CardHeader>
                            <CardTitle className="text-gray-100">
                                Tổng quan tài chính
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="relative flex h-full flex-col">
                            <div className="flex-1 space-y-4">
                                <div className="flex items-center space-x-4">
                                    <CreditCard className="h-6 w-6 text-blue-400" />
                                    <div>
                                        <p className="text-sm font-medium text-gray-400">
                                            Số dư hiện tại
                                        </p>
                                        <div className="text-2xl font-bold text-gray-100">
                                            {isLoading ? (
                                                <Skeleton className="h-8 w-52" />
                                            ) : (
                                                formatCurrency(
                                                    balanceData?.balance || 0,
                                                )
                                            )}
                                        </div>
                                    </div>
                                </div>
                                <div className="flex items-center space-x-4">
                                    <DollarSign className="h-6 w-6 text-green-400" />
                                    <div>
                                        <p className="text-sm font-medium text-gray-400">
                                            Tổng doanh thu
                                        </p>
                                        <div className="text-2xl font-bold text-gray-100">
                                            {isLoading ? (
                                                <Skeleton className="h-8 w-52" />
                                            ) : (
                                                formatCurrency(
                                                    balanceData?.totalRevenue ||
                                                        0,
                                                )
                                            )}
                                        </div>
                                    </div>
                                </div>
                                <div className="flex items-center space-x-4">
                                    <TrendingUp className="h-6 w-6 text-orange-400" />
                                    <div>
                                        <p className="text-sm font-medium text-gray-400">
                                            Tăng trưởng tháng qua
                                        </p>
                                        <div className="text-2xl font-bold text-gray-100">
                                            {isLoading ? (
                                                <Skeleton className="h-8 w-32" />
                                            ) : (
                                                (balanceData?.momGrowth || 0) *
                                                    100 +
                                                "%"
                                            )}
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <WithdrawDialog balance={balanceData?.balance ?? 0}>
                                <Button
                                    className="mt-4 flex w-full gap-2 bg-blue-500 text-white hover:bg-blue-600"
                                    disabled={isLoading}
                                >
                                    <WalletMinimal />
                                    Rút tiền
                                </Button>
                            </WithdrawDialog>
                        </CardContent>
                    </Card>
                </div>

                <Card className="bg-neutral-800">
                    <CardHeader>
                        <CardTitle className="text-gray-100">
                            Lịch sử doanh thu
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="flex items-center justify-between space-x-2 py-4">
                            {isHistoryLoading ? (
                                <Skeleton className="ms-auto h-10 w-[300px]" />
                            ) : (
                                <RevenuePagination
                                    currentPage={inData?.pageNumber}
                                    totalPages={inData?.totalPages}
                                    onClick={(page) => setCurrentInPage(page)}
                                />
                            )}
                        </div>
                        {isHistoryLoading ? (
                            <div className="space-y-2">
                                <Skeleton className="h-10 w-full" />
                                <Skeleton className="h-10 w-full" />
                                <Skeleton className="h-10 w-full" />
                                <Skeleton className="h-10 w-full" />
                            </div>
                        ) : (
                            <RevenueTable historyData={inData?.items} />
                        )}
                    </CardContent>
                </Card>

                <Card className="bg-neutral-800">
                    <CardHeader>
                        <CardTitle className="text-gray-100">
                            Lịch sử rút tiền
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="flex items-center justify-between space-x-2 py-4">
                            {isHistoryLoading ? (
                                <Skeleton className="ms-auto h-10 w-[300px]" />
                            ) : (
                                <RevenuePagination
                                    currentPage={outData?.pageNumber}
                                    totalPages={outData?.totalPages}
                                    onClick={(page) => setCurrentOutPage(page)}
                                />
                            )}
                        </div>
                        {isHistoryLoading ? (
                            <div className="space-y-2">
                                <Skeleton className="h-10 w-full" />
                                <Skeleton className="h-10 w-full" />
                                <Skeleton className="h-10 w-full" />
                                <Skeleton className="h-10 w-full" />
                            </div>
                        ) : (
                            <RevenueTable historyData={outData?.items} />
                        )}
                    </CardContent>
                </Card>
            </div>
        </SiteLayout>
    );
}
