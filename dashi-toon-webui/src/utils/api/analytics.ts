import { DateRange } from "react-day-picker";
import { fakeApi, fetchApi } from ".";
import { Result } from "../err";

export interface Analytics {
    total: number;
    totalCompare?: number;
    growth: number; // 0 -> 100
    growthCompare?: number; // 0 -> 100
    ranking: number;
    rankingCompare?: number; // 0 -> 100
    data: ChartSeries[];
    topChapters: ChapterRanking[];
}

export interface ChapterRanking {
    id: string;
    name: string;
    chapterNumber: number;
    volumeId: string;
    volumeName: string;
    data: number;
    compare?: number;
    rank: number;
}

export interface ChartSeries {
    time: string;
    current: number;
    compare?: number;
}

export interface DayOfWeekViewSeries {
    time: string;
    current: number;
    compare?: number;
}

export interface ReviewSeries {
    time: string;
    currentPositive: number;
    currentNegative: number;
    comparePositive?: number;
    compareNegative?: number;
}

export interface AnalyticsBaseQuery {
    current: DateRange;
    compare?: DateRange;
    seriesId: string;
}

export interface GeneralAnalyticsResp {
    currentView: number;
    compareView?: number;
    currentRating: number;
    compareRating?: number;
    currentRevenue: number;
    compareRevenue?: number;
    currentDashiFan: number;
    compareDashiFan?: number;
}

export function processQuery(query: AnalyticsBaseQuery) {
    if (!query.current.to) {
        query.current.to = query.current.from;
    }

    if (query.compare) {
        if (!query.compare.to) {
            query.compare.to = query.compare.from;
        }
    }

    return query;
}

export function getGeneralAnalytics(query: AnalyticsBaseQuery): Promise<Result<GeneralAnalyticsResp>> {
    // return fakeApi({
    //     currentView: 1200,
    //     compareView: 1100,
    //     currentRating: 45,
    //     compareRating: 43,
    //     currentRevenue: 5000,
    //     compareRevenue: 4500,
    //     currentDashiFan: 300,
    //     compareDashiFan: 280,
    // } satisfies GeneralAnalyticsResp as GeneralAnalyticsResp);
    return fetchApi("POST", `/api/AuthorStudio/series/${query.seriesId}/analytics/general`, processQuery(query));
}

export interface ViewAnalyticsResp extends Analytics {
    bestDayOfWeek: string;
    worstDayOfWeek: string;
    dayOfWeeks: ChartSeries[];
}

export function getViewAnalytics(query: AnalyticsBaseQuery): Promise<Result<ViewAnalyticsResp>> {
    // return fakeApi({
    //     total: 1000,
    //     totalCompare: 100,
    //     growth: 20,
    //     growthCompare: 12,
    //     ranking: 5,
    //     rankingCompare: -2,
    //     data: [
    //         { time: "01-2024", current: 186, compare: 80 },
    //         { time: "02-2024", current: 305, compare: 200 },
    //         { time: "03-2024", current: 237, compare: 120 },
    //         { time: "04-2024", current: 73, compare: 190 },
    //         { time: "05-2024", current: 209, compare: 130 },
    //         { time: "01-2024", current: 214, compare: 140 },
    //     ],
    //     topChapters: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             chapterNumber: 1,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             chapterNumber: 3,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapte23r 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "12w38",
    //             name: "Test cha3pter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    //     bestDayOfWeek: "Thứ 2",
    //     worstDayOfWeek: "Thứ 3",
    //     dayOfWeeks: [
    //         { time: "T2", current: 186, compare: 80 },
    //         { time: "T3", current: 305, compare: 200 },
    //         { time: "T4", current: 237, compare: 120 },
    //         { time: "T5", current: 73, compare: 190 },
    //         { time: "T6", current: 209, compare: 130 },
    //         { time: "T7", current: 214, compare: 140 },
    //         { time: "CN", current: 214, compare: 140 },
    //     ],
    // } satisfies ViewAnalyticsResp as ViewAnalyticsResp);

    return fetchApi("POST", `/api/AuthorStudio/series/${query.seriesId}/analytics/view`, processQuery(query));
}

export interface RankingQuery extends AnalyticsBaseQuery {
    pageNumber?: number;
    pageSize?: number;
    query?: string;
    desc: boolean;
}

export interface RankingResp<T = ChapterRanking> {
    items: T[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
}

export function getViewRanking(query: RankingQuery): Promise<Result<RankingResp>> {
    // return fakeApi({
    //     items: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             chapterNumber: 1,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             chapterNumber: 3,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1238",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    //     pageNumber: 2,
    //     totalPages: 10,
    //     totalCount: 200,
    // } satisfies RankingResp as RankingResp);

    return fetchApi(
        "POST",
        `/api/AuthorStudio/series/${query.seriesId}/analytics/chapter/view-rankings`,
        processQuery(query),
    );
}

export interface CommentAnalyticsResp extends Analytics {}

export function getCommentAnalytics(query: AnalyticsBaseQuery): Promise<Result<CommentAnalyticsResp>> {
    // return fakeApi({
    //     total: 1000,
    //     totalCompare: 100,
    //     growth: 20,
    //     growthCompare: 12,
    //     ranking: 5,
    //     rankingCompare: -2,
    //     data: [
    //         { time: "01-2024", current: 186, compare: 80 },
    //         { time: "02-2024", current: 305, compare: 200 },
    //         { time: "03-2024", current: 237, compare: 120 },
    //         { time: "04-2024", current: 73, compare: 190 },
    //         { time: "05-2024", current: 209, compare: 130 },
    //         { time: "06-2024", current: 214, compare: 140 },
    //     ],
    //     topChapters: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             chapterNumber: 1,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             chapterNumber: 3,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapte23r 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "12w38",
    //             name: "Test cha3pter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    // } satisfies CommentAnalyticsResp as CommentAnalyticsResp);

    return fetchApi("POST", `/api/AuthorStudio/series/${query.seriesId}/analytics/comment`, processQuery(query));
}

export function getCommentRanking(query: RankingQuery): Promise<Result<RankingResp>> {
    // return fakeApi({
    //     items: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             chapterNumber: 1,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             chapterNumber: 3,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1238",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    //     pageNumber: 2,
    //     totalPages: 10,
    //     totalCount: 200,
    // } satisfies RankingResp as RankingResp);

    return fetchApi(
        "POST",
        `/api/AuthorStudio/series/${query.seriesId}/analytics/chapter/comment-rankings`,
        processQuery(query),
    );
}

export interface ReviewAnalyticsResp {
    total: number;
    totalCompare?: number;
    positive: number;
    positiveCompare?: number;
    ranking: number;
    rankingCompare?: number;
    data: ReviewSeries[];
}

export function getReviewAnalytics(query: AnalyticsBaseQuery): Promise<Result<ReviewAnalyticsResp>> {
    // return fakeApi({
    //     total: 1000,
    //     totalCompare: 100,
    //     positive: 75,
    //     positiveCompare: 50,
    //     ranking: 10,
    //     rankingCompare: 12,
    //     data: [
    //         {
    //             time: "January",
    //             currentPositive: 186,
    //             currentNegative: 80,
    //             comparePositive: 305,
    //             compareNegative: 200,
    //         },
    //         {
    //             time: "February",
    //             currentPositive: 305,
    //             currentNegative: 200,
    //             comparePositive: 237,
    //             compareNegative: 120,
    //         },
    //         {
    //             time: "March",
    //             currentPositive: 237,
    //             currentNegative: 120,
    //             comparePositive: 73,
    //             compareNegative: 190,
    //         },
    //         {
    //             time: "April",
    //             currentPositive: 73,
    //             currentNegative: 190,
    //             comparePositive: 209,
    //             compareNegative: 130,
    //         },
    //         {
    //             time: "May",
    //             currentPositive: 209,
    //             currentNegative: 130,
    //             comparePositive: 214,
    //             compareNegative: 140,
    //         },
    //         {
    //             time: "June",
    //             currentPositive: 214,
    //             currentNegative: 140,
    //             comparePositive: 546,
    //             compareNegative: 234,
    //         },
    //         {
    //             time: "July",
    //             currentPositive: 186,
    //             currentNegative: 80,
    //             comparePositive: 305,
    //             compareNegative: 200,
    //         },
    //         {
    //             time: "August",
    //             currentPositive: 305,
    //             currentNegative: 200,
    //             comparePositive: 237,
    //             compareNegative: 120,
    //         },
    //         {
    //             time: "September",
    //             currentPositive: 237,
    //             currentNegative: 120,
    //             comparePositive: 73,
    //             compareNegative: 190,
    //         },
    //         {
    //             time: "October",
    //             currentPositive: 209,
    //             currentNegative: 130,
    //             comparePositive: 214,
    //             compareNegative: 140,
    //         },
    //         {
    //             time: "November",
    //             currentPositive: 214,
    //             currentNegative: 140,
    //             comparePositive: 546,
    //             compareNegative: 234,
    //         },
    //         {
    //             time: "December",
    //             currentPositive: 186,
    //             currentNegative: 80,
    //             comparePositive: 305,
    //             compareNegative: 200,
    //         },
    //     ],
    // } satisfies ReviewAnalyticsResp as ReviewAnalyticsResp);

    return fetchApi("POST", `/api/AuthorStudio/series/${query.seriesId}/analytics/review`, processQuery(query));
}

export interface KanaAnalyticsResp extends Analytics {}
export function getKanaAnalytics(query: AnalyticsBaseQuery): Promise<Result<KanaAnalyticsResp>> {
    // return fakeApi({
    //     total: 1000,
    //     totalCompare: 100,
    //     growth: 20,
    //     growthCompare: 12,
    //     ranking: 5,
    //     rankingCompare: -2,
    //     data: [
    //         { time: "01-2024", current: 186, compare: 80 },
    //         { time: "02-2024", current: 305, compare: 200 },
    //         { time: "03-2024", current: 237, compare: 120 },
    //         { time: "04-2024", current: 73, compare: 190 },
    //         { time: "05-2024", current: 209, compare: 130 },
    //         { time: "06-2024", current: 214, compare: 140 },
    //     ],
    //     topChapters: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             chapterNumber: 1,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             chapterNumber: 3,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapte23r 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "12w38",
    //             name: "Test cha3pter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    // } satisfies KanaAnalyticsResp as KanaAnalyticsResp);
    return fetchApi("POST", `/api/AuthorStudio/series/${query.seriesId}/analytics/kana`, processQuery(query));
}

export function getKanaRanking(query: RankingQuery): Promise<Result<RankingResp>> {
    // return fakeApi({
    //     items: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             chapterNumber: 1,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             chapterNumber: 3,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1238",
    //             name: "Test chapter 2",
    //             chapterNumber: 2,
    //             volumeId: "123",
    //             volumeName: "Test volume",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    //     pageNumber: 2,
    //     totalPages: 10,
    //     totalCount: 200,
    // } satisfies RankingResp as RankingResp);

    return fetchApi(
        "POST",
        `/api/AuthorStudio/series/${query.seriesId}/analytics/chapter/kana-rankings`,
        processQuery(query),
    );
}

export interface DashiFanAnalyticsResp extends Omit<Analytics, "topChapters"> {
    topTiers: TierRanking[];
}
export function getDashiFanAnalytics(query: AnalyticsBaseQuery): Promise<Result<DashiFanAnalyticsResp>> {
    // return fakeApi({
    //     total: 1000,
    //     totalCompare: 100,
    //     growth: 20,
    //     growthCompare: 12,
    //     ranking: 5,
    //     rankingCompare: -2,
    //     data: [
    //         { time: "01-2024", current: 186, compare: 80 },
    //         { time: "02-2024", current: 305, compare: 200 },
    //         { time: "03-2024", current: 237, compare: 120 },
    //         { time: "04-2024", current: 73, compare: 190 },
    //         { time: "05-2024", current: 209, compare: 130 },
    //         { time: "06-2024", current: 214, compare: 140 },
    //     ],
    //     topTiers: [
    //         {
    //             id: "123",
    //             name: "Test chapter ",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapte23r 2",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "12w38",
    //             name: "Test cha3pter 2",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    // } satisfies DashiFanAnalyticsResp as DashiFanAnalyticsResp);

    return fetchApi("POST", `/api/AuthorStudio/series/${query.seriesId}/analytics/dashi-fan`, processQuery(query));
}

export interface TierRanking {
    id: string;
    name: string;
    data: number;
    compare?: number;
    rank: number;
}

export function getDashiFanRanking(query: RankingQuery): Promise<Result<RankingResp<TierRanking>>> {
    // return fakeApi({
    //     items: [
    //         {
    //             id: "123",
    //             name: "Test tier ",
    //             data: 123,
    //             compare: 23,
    //             rank: 1,
    //         },
    //         {
    //             id: "1234",
    //             name: "Test chapter 3",
    //             data: 123,
    //             compare: 34,
    //             rank: 1,
    //         },
    //         {
    //             id: "1236",
    //             name: "Test chapter 2",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1237",
    //             name: "Test chapter 2",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //         {
    //             id: "1238",
    //             name: "Test chapter 2",
    //             data: 123,
    //             compare: 45,
    //             rank: 1,
    //         },
    //     ],
    //     pageNumber: 2,
    //     totalPages: 10,
    //     totalCount: 200,
    // } satisfies RankingResp<TierRanking> as RankingResp<TierRanking>);

    return fetchApi(
        "POST",
        `/api/AuthorStudio/series/${query.seriesId}/analytics/dashi-fan-rankings`,
        processQuery(query),
    );
}
