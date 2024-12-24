import { fetchApi } from "@/utils/api";
import { Result } from "@/utils/err";

export interface User {
    userId: string;
    userName: string;
    email: string;
    phoneNumber: string;
    roles: string[];
}

export interface PaginatedUsers {
    items: User[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export interface RoleAssignmentResult {
    succeeded: boolean;
    errors: string[];
}

export async function getUsers(params: {
    userId?: string;
    username?: string;
    role?: string;
    pageNumber?: number;
    pageSize?: number;
}): Promise<Result<PaginatedUsers>> {
    return await fetchApi("GET", `/api/Administrator/users`, undefined, {
        params,
    });
}

export async function assignRole(
    userId: string,
    role: string,
): Promise<Result<RoleAssignmentResult>> {
    return await fetchApi(
        "POST",
        `/api/Administrator/users/${userId}/assign-role`,
        { userId, role },
    );
}

export async function getRoles(): Promise<Result<string[]>> {
    return await fetchApi("GET", `/api/Administrator/roles`);
}
