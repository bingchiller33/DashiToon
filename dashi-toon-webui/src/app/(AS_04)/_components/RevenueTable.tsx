import React from "react";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { TransactionHistory } from "@/utils/api/author-studio/revenue";
import { formatCurrency, formatDateTime } from "@/utils";

export interface RevenueTableProps {
    historyData?: TransactionHistory[];
}

export default function RevenueTable({ historyData }: RevenueTableProps) {
    return (
        <>
            <Table>
                <TableHeader>
                    <TableRow className="border-gray-700">
                        <TableHead className="w-[175px] text-gray-400">
                            Thời gian
                        </TableHead>
                        <TableHead className="w-[200px] text-gray-400">
                            Số tiền
                        </TableHead>
                        <TableHead className="text-gray-400">Lý do</TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    {historyData?.map((item, index) => (
                        <TableRow key={index} className="border-gray-700">
                            <TableCell className="text-gray-300">
                                {formatDateTime(item.timestamp)}
                            </TableCell>
                            <TableCell
                                className={`${item.transactionType === 0 ? "text-green-400" : "text-red-400"}`}
                            >
                                {formatCurrency(item.amount)}
                            </TableCell>
                            <TableCell className="text-gray-300">
                                {item.reason}
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
            {!historyData?.length && (
                <p className="mt-4 text-center italic text-muted-foreground">
                    Không có bản ghi trên hệ thống
                </p>
            )}
        </>
    );
}
