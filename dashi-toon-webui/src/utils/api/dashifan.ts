import { fetchApi } from ".";
import { Result } from "../err";

export interface DashiFanTier {
    id: string;
    name: string;
    price: {
        amount: number;
        currency: string;
    };
    description: string;
    perks: number;
    isActive: boolean;
    lastModified: string;
}

export async function getDashiFanTiers(
    id: string,
): Promise<Result<DashiFanTier[]>> {
    return await fetchApi("GET", `/api/AuthorStudio/series/${id}/dashi-fans`);
}

export interface DashiFanTierCreateReq {
    name: string;
    perks: number;
    amount: number;
}

export async function createDashiFanTier(
    id: string,
    data: DashiFanTierCreateReq,
): Promise<Result<string>> {
    return await fetchApi("POST", `/api/AuthorStudio/series/${id}/dashi-fans`, {
        ...data,
        seriesId: id,
        description: "A description",
        currency: "VND",
    });
}

export interface DashiFanTierUpdateReq {
    name: string;
    perks: number;
    amount: number;
}

export async function editDashiFanTier(
    id: string,
    tierId: string,
    data: DashiFanTierUpdateReq,
): Promise<Result<string>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${id}/dashi-fans/${tierId}`,
        {
            ...data,
            tierId: tierId,
            seriesId: id,
            description: "A description",
            currency: "VND",
        },
    );
}

export async function editDashiFanTierStatus(
    id: string,
    tierId: string,
): Promise<Result<string>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${id}/dashi-fans/${tierId}/status`,
        {
            tierId: tierId,
            seriesId: id,
        },
    );
}
