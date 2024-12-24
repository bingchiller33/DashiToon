import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    Analytics,
    getViewAnalytics,
    getViewRanking,
    ViewAnalyticsResp,
    RankingResp,
} from "@/utils/api/analytics";
import {
    ChartNoAxesColumn,
    Eye,
    Hash,
    MessageSquareText,
    Star,
} from "lucide-react";
import Insight from "../_components/Insight";
import { OmniChart } from "../_components/OmniChart";
import { ReviewChart } from "../_components/ReviewChart";
import { ViewCountByWeek } from "../_components/ViewCountByWeek";
import { useEffect, useState } from "react";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import ViewAnalytics from "./ViewAnalytics";
import CommentAnalytics from "./CommentAnalytics";
import ReviewAnalytics from "./ReviewAnalytics";

const testReview = {
    total: 1000,
    totalCompare: 100,
    positive: 75,
    positiveCompare: 50,
    ranking: 10,
    rankingCompare: 12,
    data: [
        {
            time: "January",
            currentPositive: 186,
            currentNegative: 80,
            comparePositive: 305,
            compareNegative: 200,
        },
        {
            time: "February",
            currentPositive: 305,
            currentNegative: 200,
            comparePositive: 237,
            compareNegative: 120,
        },
        {
            time: "March",
            currentPositive: 237,
            currentNegative: 120,
            comparePositive: 73,
            compareNegative: 190,
        },
        {
            time: "April",
            currentPositive: 73,
            currentNegative: 190,
            comparePositive: 209,
            compareNegative: 130,
        },
        {
            time: "May",
            currentPositive: 209,
            currentNegative: 130,
            comparePositive: 214,
            compareNegative: 140,
        },
        {
            time: "June",
            currentPositive: 214,
            currentNegative: 140,
            comparePositive: 546,
            compareNegative: 234,
        },
        {
            time: "July",
            currentPositive: 186,
            currentNegative: 80,
            comparePositive: 305,
            compareNegative: 200,
        },
        {
            time: "August",
            currentPositive: 305,
            currentNegative: 200,
            comparePositive: 237,
            compareNegative: 120,
        },
        {
            time: "September",
            currentPositive: 237,
            currentNegative: 120,
            comparePositive: 73,
            compareNegative: 190,
        },
        {
            time: "October",
            currentPositive: 209,
            currentNegative: 130,
            comparePositive: 214,
            compareNegative: 140,
        },
        {
            time: "November",
            currentPositive: 214,
            currentNegative: 140,
            comparePositive: 546,
            compareNegative: 234,
        },
        {
            time: "December",
            currentPositive: 186,
            currentNegative: 80,
            comparePositive: 305,
            compareNegative: 200,
        },
    ],
};

export interface EngagementAnalyticsProps {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function EngagementAnalytics(props: EngagementAnalyticsProps) {
    return (
        <section>
            <div className="container mt-8">
                <h2 className="text-2xl">Thống kê tương tác</h2>
                <p className="mb-4 text-muted-foreground">
                    Thống kê chi tiết về sự tương tác của bộ truyện
                </p>
            </div>
            <ViewAnalytics {...props} />
            <CommentAnalytics {...props} />
            <ReviewAnalytics {...props} />
        </section>
    );
}
