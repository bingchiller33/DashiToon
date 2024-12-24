import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
    Pagination,
    PaginationContent,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";
import {
    Table,
    TableBody,
    TableCaption,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { formatUpdatedAt } from "@/lib/date-fns";
import {
    getTransactionHistory,
    TransactionResponse,
} from "@/utils/api/reader/kana";
import { currencyMap, typeMap } from "@/utils/consts";
import { format, parseISO } from "date-fns";
import { useEffect, useState } from "react";
import { toast } from "sonner";

export function TransactionHistory() {
    const [transactions, setTransactions] =
        useState<TransactionResponse | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [currentType, setCurrentType] = useState<"ACQUIRED" | "SPENT">(
        "ACQUIRED",
    );

    const fetchTransactions = async (
        type: "ACQUIRED" | "SPENT",
        page: number,
    ) => {
        const [data, error] = await getTransactionHistory(type, page);
        if (error) {
            console.error("Failed to fetch transactions:", error);
            toast.error(
                "Không thể tải lịch sử giao dịch. Vui lòng thử lại sau.",
            );
        } else {
            setTransactions(data);
        }
    };

    useEffect(() => {
        fetchTransactions(currentType, currentPage);
    }, [currentType, currentPage]);

    const handlePageChange = (newPage: number) => {
        setCurrentPage(newPage);
    };

    const handleTypeChange = (type: "ACQUIRED" | "SPENT") => {
        setCurrentType(type);
        setCurrentPage(1);
    };

    return (
        <Accordion type="single" collapsible defaultValue="transaction-history">
            <AccordionItem value="transaction-history">
                <Card className="border-none bg-neutral-900 shadow-lg shadow-black/50 backdrop-blur-sm">
                    <CardHeader>
                        <AccordionTrigger className="hover:no-underline">
                            <CardTitle className="text-2xl font-bold">
                                Lịch Sử Giao Dịch
                            </CardTitle>
                        </AccordionTrigger>
                    </CardHeader>
                    <AccordionContent>
                        <CardContent>
                            <Tabs defaultValue="ACQUIRED" className="w-full">
                                <TabsList className="grid w-full grid-cols-2">
                                    <TabsTrigger
                                        value="ACQUIRED"
                                        onClick={() =>
                                            handleTypeChange("ACQUIRED")
                                        }
                                    >
                                        Nhận
                                    </TabsTrigger>
                                    <TabsTrigger
                                        value="SPENT"
                                        onClick={() =>
                                            handleTypeChange("SPENT")
                                        }
                                    >
                                        Chi tiêu
                                    </TabsTrigger>
                                </TabsList>
                                <TabsContent value="ACQUIRED">
                                    <TransactionTable
                                        transactions={transactions}
                                        currentPage={currentPage}
                                        onPageChange={handlePageChange}
                                        type="ACQUIRED"
                                    />
                                </TabsContent>
                                <TabsContent value="SPENT">
                                    <TransactionTable
                                        transactions={transactions}
                                        currentPage={currentPage}
                                        onPageChange={handlePageChange}
                                        type="SPENT"
                                    />
                                </TabsContent>
                            </Tabs>
                        </CardContent>
                    </AccordionContent>
                </Card>
            </AccordionItem>
        </Accordion>
    );
}

function TransactionTable({
    transactions,
    currentPage,
    onPageChange,
    type,
}: {
    transactions: TransactionResponse | null;
    currentPage: number;
    onPageChange: (page: number) => void;
    type: "ACQUIRED" | "SPENT";
}) {
    return (
        <>
            <Table>
                <TableCaption className="text-gray-400">
                    Danh sách giao dịch Kana Coin và Kana Gold của bạn.
                </TableCaption>
                <TableHeader>
                    <TableRow className="border-neutral-700">
                        <TableHead className="text-gray-300">
                            Số lượng
                        </TableHead>
                        <TableHead className="text-gray-300">
                            Loại Kana
                        </TableHead>
                        <TableHead className="text-gray-300">
                            Loại giao dịch
                        </TableHead>
                        <TableHead className="text-gray-300">Lý do</TableHead>
                        <TableHead className="text-gray-300">
                            Thời gian
                        </TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    {transactions?.items.map((transaction, index) => (
                        <TableRow key={index} className="border-neutral-700">
                            <TableCell
                                className={`font-bold ${
                                    transaction.currency === 1
                                        ? "text-yellow-500"
                                        : type === "ACQUIRED"
                                          ? "text-blue-500"
                                          : "text-red-500"
                                }`}
                            >
                                {type === "ACQUIRED" ? "+" : ""}
                                {transaction.amount}
                            </TableCell>
                            <TableCell className="text-gray-300">
                                {currencyMap[transaction.currency]}
                            </TableCell>
                            <TableCell className="text-gray-300">
                                {typeMap[transaction.type]}
                            </TableCell>
                            <TableCell className="text-gray-300">
                                {transaction.reason}
                            </TableCell>
                            <TableCell
                                className="text-gray-300"
                                title={format(
                                    parseISO(transaction.time),
                                    "PPPpp",
                                )}
                            >
                                {formatUpdatedAt(transaction.time)}
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
            {transactions && (
                <Pagination className="mt-4">
                    <PaginationContent>
                        {transactions.hasPreviousPage ? (
                            <PaginationItem>
                                <PaginationPrevious
                                    onClick={() =>
                                        onPageChange(currentPage - 1)
                                    }
                                />
                            </PaginationItem>
                        ) : (
                            <PaginationItem>
                                <PaginationPrevious className="pointer-events-none opacity-50" />
                            </PaginationItem>
                        )}
                        {Array.from(
                            { length: transactions.totalPages },
                            (_, i) => i + 1,
                        ).map((page) => (
                            <PaginationItem key={page}>
                                <PaginationLink
                                    onClick={() => onPageChange(page)}
                                    isActive={page === currentPage}
                                >
                                    {page}
                                </PaginationLink>
                            </PaginationItem>
                        ))}
                        {transactions.hasNextPage ? (
                            <PaginationItem>
                                <PaginationNext
                                    onClick={() =>
                                        onPageChange(currentPage + 1)
                                    }
                                />
                            </PaginationItem>
                        ) : (
                            <PaginationItem>
                                <PaginationNext className="pointer-events-none opacity-50" />
                            </PaginationItem>
                        )}
                    </PaginationContent>
                </Pagination>
            )}
        </>
    );
}
