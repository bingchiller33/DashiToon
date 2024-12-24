import { Result } from "../../err";
import { fetchApi } from "..";
import * as env from "@/utils/env";
import * as gtag from "@/utils/gtag";

export async function unlockChapter(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    price?: number,
): Promise<Result<void, { error: any; status: number }>> {
    try {
        const response = await fetch(
            `${env.BACKEND_HOST}/api/ReadContent/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/unlock`,
            {
                credentials: "include",
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    seriesId,
                    volumeId,
                    chapterId,
                }),
            },
        );

        const status = response.status;
        if (!response.ok) {
            const text = await response.text();
            return [undefined, { error: text, status }];
        }

        return [undefined, undefined];
    } catch (error) {
        return [undefined, { error, status: 500 }];
    }
}

export async function getChapterPrice(seriesId: string, volumeId: string, chapterId: string): Promise<Result<number>> {
    return await fetchApi("GET", `/api/ReadContent/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/price`);
}
