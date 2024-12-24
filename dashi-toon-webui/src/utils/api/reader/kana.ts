import { Result } from "@/utils/err";
import { fetchApi } from "..";

export interface Transaction {
    amount: number;
    currency: 1 | 2;
    type: 1 | 2 | 3 | 4;
    reason: string;
    time: string;
}

export interface TransactionResponse {
    items: Transaction[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getTransactionHistory(
    type: "ACQUIRED" | "SPENT",
    page: number,
): Promise<Result<TransactionResponse>> {
    return await fetchApi("GET", `/api/Users/kana-transactions`, undefined, {
        params: {
            type: type,
            pageNumber: page,
            pageSize: 5,
        },
    });
}

export interface KanaTotal {
    kanaType: 1 | 2;
    amount: number;
}

export interface UserKanas {
    totals: KanaTotal[];
}

export interface UserMetadata {
    isCheckedIn: boolean;
    currentDateChapterRead: number;
}

export async function getUserKanas(): Promise<Result<UserKanas>> {
    return await fetchApi("GET", `/api/Users/kanas`, undefined, {});
}

export async function getUserMetadata(): Promise<Result<UserMetadata>> {
    return await fetchApi("GET", `/api/Users/metadata`, undefined, {});
}

export async function checkIn(): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Users/checkin`, undefined, {});
}

export interface Mission {
    missionId: string;
    amount: number;
    readCount: number;
    isCompleted: boolean;
    isCompletable: boolean;
}

export async function getUserMissions(): Promise<Result<Mission[]>> {
    return await fetchApi("GET", `/api/Users/missions`, undefined, {});
}

export async function claimMissionReward(
    missionId: string,
): Promise<Result<void>> {
    return await fetchApi("PUT", `/api/Users/missions/${missionId}`, undefined);
}

export interface KanaPack {
    id: string;
    amount: number;
    price: {
        amount: number;
        currency: string;
    };
}

export async function getKanaPacks(): Promise<Result<KanaPack[]>> {
    return await fetchApi("GET", `/api/Subscriptions/kana-packs`, undefined, {
        preventAutoLogin: true,
    });
}

export interface PurchaseKanaPackRequest {
    packId: string;
    paymentMethod: string;
    returnUrl: string;
}

export interface PurchaseKanaPackResponse {
    redirectUrl: string;
}

export async function purchaseKanaPack(
    id: string,
    request: PurchaseKanaPackRequest,
): Promise<Result<PurchaseKanaPackResponse>> {
    return await fetchApi(
        "POST",
        `/api/Subscriptions/kana-packs/${id}/purchase`,
        request,
    );
}
