import { Result } from "@/utils/err";
import { fetchApi } from "..";

export interface WithdrawRequest {
    paypalAccountId: string;
    amount: number;
}

export async function withdraw(
    request: WithdrawRequest,
): Promise<Result<void>> {
    return await fetchApi(
        "POST",
        `/api/AuthorStudio/revenue/withdraw`,
        request,
    );
}

export interface TransactionHistory {
    amount: number;
    revenueType: number;
    transactionType: number;
    reason: string;
    timestamp: string;
}

export interface TransactionHistoryResponse {
    items: TransactionHistory[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getTransactionHistory(
    type: number,
    pageNumber: number,
    pageSize: number,
): Promise<Result<TransactionHistoryResponse>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/revenue/transactions`,
        undefined,
        {
            params: {
                pageNumber,
                pageSize,
                type,
            },
        },
    );
}

export interface RevenueData {
    balance: number;
    totalRevenue: number;
    momGrowth: number;
    data: RevenueDataPoint[];
}

export interface RevenueDataPoint {
    month: string;
    revenue: number;
    withdrawal: number;
}

export async function getRevenueData(): Promise<Result<RevenueData>> {
    return await fetchApi("GET", `/api/AuthorStudio/revenue`);
}
