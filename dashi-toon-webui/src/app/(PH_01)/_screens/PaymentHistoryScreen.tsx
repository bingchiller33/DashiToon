"use client";

import SiteLayout from "@/components/SiteLayout";
import { Badge } from "@/components/ui/badge";
import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";
import UserSettingLayout from "@/components/UserSettingLayout";
import { getPaymentHistory, PaymentHistoryItem } from "@/utils/api/user";
import { PAYMENT_STATUS } from "@/utils/consts";
import { useEffect, useState } from "react";
import { toast } from "sonner";

interface GroupedTransactions {
    [key: string]: PaymentHistoryItem[];
}

export default function Component() {
    const [groupedTransactions, setGroupedTransactions] =
        useState<GroupedTransactions>({});
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [isLoading, setIsLoading] = useState(false);

    const fetchTransactions = async (page: number) => {
        setIsLoading(true);
        // Simulating API call
        // Replace this with your actual API call
        const [response, err] = await getPaymentHistory(page, 25);
        if (err) {
            setIsLoading(false);
            toast.error(
                "Không thể tải lịch sử giao dịch, vui lòng thử lại sau",
            );
            return;
        }
        response.items = response.items.filter((x) => x.status === 1);
        response.items.reverse();

        const grouped = response.items.reduce((acc, transaction) => {
            const date = new Date(transaction.completedAt);
            const monthYear = date.toLocaleString("vi-VN", {
                month: "long",
                year: "numeric",
            });
            if (!acc[monthYear]) {
                acc[monthYear] = [];
            }
            acc[monthYear].push(transaction);
            return acc;
        }, {} as GroupedTransactions);
        setGroupedTransactions(grouped);
        setCurrentPage(response.pageNumber);
        setTotalPages(response.totalPages);
        setIsLoading(false);
    };

    useEffect(() => {
        fetchTransactions(currentPage);
    }, [currentPage]);

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            setCurrentPage(newPage);
        }
    };

    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleString("vi-VN", {
            timeZone: "Asia/Ho_Chi_Minh",
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    };

    const formatCurrency = (amount: number, currency: string) => {
        return new Intl.NumberFormat("en-US", {
            style: "currency",
            currency,
        }).format(amount);
    };

    return (
        <SiteLayout>
            <div className="container pt-6">
                <UserSettingLayout>
                    <div className="container mx-auto flex-grow space-y-6 p-4 text-neutral-50">
                        <h1 className="mb-6 text-2xl font-bold">
                            Lịch sử giao dịch
                        </h1>
                        <Pagination className="justify-end">
                            <PaginationContent>
                                <PaginationItem>
                                    <PaginationPrevious
                                        href="#"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            handlePageChange(currentPage - 1);
                                        }}
                                        isActive={currentPage > 1}
                                    >
                                        Trước
                                    </PaginationPrevious>
                                </PaginationItem>
                                {[...Array(totalPages)].map((_, index) => {
                                    const pageNumber = index + 1;
                                    if (
                                        pageNumber === 1 ||
                                        pageNumber === totalPages ||
                                        (pageNumber >= currentPage - 1 &&
                                            pageNumber <= currentPage + 1)
                                    ) {
                                        return (
                                            <PaginationItem key={pageNumber}>
                                                <PaginationLink
                                                    href="#"
                                                    onClick={(e) => {
                                                        e.preventDefault();
                                                        handlePageChange(
                                                            pageNumber,
                                                        );
                                                    }}
                                                    isActive={
                                                        pageNumber ===
                                                        currentPage
                                                    }
                                                >
                                                    {pageNumber}
                                                </PaginationLink>
                                            </PaginationItem>
                                        );
                                    } else if (
                                        pageNumber === currentPage - 2 ||
                                        pageNumber === currentPage + 2
                                    ) {
                                        return (
                                            <PaginationEllipsis
                                                key={pageNumber}
                                            />
                                        );
                                    }
                                    return null;
                                })}
                                <PaginationItem>
                                    <PaginationNext
                                        href="#"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            handlePageChange(currentPage + 1);
                                        }}
                                        isActive={currentPage < totalPages}
                                    >
                                        Sau
                                    </PaginationNext>
                                </PaginationItem>
                            </PaginationContent>
                        </Pagination>
                        {groupedTransactions &&
                            Object.keys(groupedTransactions).length === 0 && (
                                <p className="text-center text-muted-foreground">
                                    Không có bản ghi trong hệ thống
                                </p>
                            )}
                        {isLoading ? (
                            <div className="flex h-full items-center justify-center">
                                <p className="text-blue-400">Đang tải...</p>
                            </div>
                        ) : (
                            Object.entries(groupedTransactions).map(
                                ([monthYear, transactions]) => (
                                    <div key={monthYear} className="mb-6">
                                        <h2 className="mb-4 text-xl font-semibold text-muted-foreground">
                                            {monthYear}
                                        </h2>
                                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                                            {transactions.map((transaction) => (
                                                <div
                                                    key={transaction.id}
                                                    className="flex flex-col justify-between rounded-lg border border-neutral-600 p-4"
                                                >
                                                    <div>
                                                        <h3 className="font-semibold text-gray-100">
                                                            {transaction.detail}
                                                        </h3>
                                                        <p className="mt-1 text-sm text-gray-400">
                                                            {formatDate(
                                                                transaction.completedAt,
                                                            )}
                                                        </p>
                                                    </div>
                                                    <div className="mt-4 flex items-center justify-between">
                                                        <Badge className="bg-blue-900 text-blue-200">
                                                            {
                                                                PAYMENT_STATUS[
                                                                    transaction
                                                                        .status
                                                                ].label
                                                            }
                                                        </Badge>
                                                        <span className="font-bold text-blue-300">
                                                            {formatCurrency(
                                                                transaction
                                                                    .price
                                                                    .amount,
                                                                transaction
                                                                    .price
                                                                    .currency,
                                                            )}
                                                        </span>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                ),
                            )
                        )}
                    </div>
                </UserSettingLayout>
            </div>
        </SiteLayout>
    );
}
