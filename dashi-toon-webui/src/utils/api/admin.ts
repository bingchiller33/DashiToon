import { fetchApi } from ".";
import { Result } from "../err";

export async function updateMission(
    id: string,
    readCount: number,
    reward: number,
    isActive: boolean,
): Promise<Result<Mission>> {
    return await fetchApi("PUT", `/api/Administrator/missions/${id}`, {
        id,
        readCount,
        reward,
        isActive,
    });
}

export async function createMission(
    readCount: number,
    reward: number,
): Promise<Result<Mission>> {
    return await fetchApi("POST", `/api/Administrator/missions`, {
        readCount,
        reward,
        isActive: true,
    });
}

export async function getMissions(): Promise<Result<Mission[]>> {
    return await fetchApi("GET", `/api/Administrator/missions`);
}

export interface Mission {
    id: string;
    readCount: number;
    reward: number;
    isActive: boolean;
}

export async function updateKanaPack(
    id: string,
    amount: number,
    price: number,
    isActive: boolean,
): Promise<Result<KanaPackItem>> {
    return await fetchApi("PUT", `/api/Administrator/kana-packs/${id}`, {
        id,
        amount,
        priceAmount: price,
        currency: "VND",
        isActive,
    });
}

export async function createKanaPack(
    amount: number,
    price: number,
): Promise<Result<KanaPackItem>> {
    return await fetchApi("POST", `/api/Administrator/kana-packs`, {
        amount,
        priceAmount: price,
        currency: "VND",
    });
}

export async function getKanaPacks(): Promise<Result<KanaPackItem[]>> {
    return await fetchApi("GET", `/api/Administrator/kana-packs`);
}

export interface KanaPackItem {
    id: "2cdcfe05-688b-4dcb-8ec3-98e24830a6e0";
    amount: number;
    price: {
        amount: number;
        currency: string;
    };
    isActive: boolean;
}

export async function updateKanaExchangeRate(
    rate: number,
): Promise<Result<void>> {
    return await fetchApi("PUT", `/api/Administrator/kana-exchange-rates`, {
        rate,
    });
}

export async function getKanaExchangeRates(): Promise<
    Result<KanaExchangeRate>
> {
    return await fetchApi("GET", `/api/Administrator/kana-exchange-rates`);
}

export interface KanaExchangeRate {
    currency: string;
    rate: number;
}

export async function updateCommissionRate(
    commissionRate: CommissionRate,
): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/Administrator/commission-rates`,
        commissionRate,
    );
}

export async function getCommissionRates(
    type: number,
): Promise<Result<CommissionRate>> {
    return await fetchApi(
        "GET",
        `/api/Administrator/commission-rates`,
        undefined,
        {
            params: {
                type,
            },
        },
    );
}

export interface CommissionRate {
    type: number;
    rate: number;
}
